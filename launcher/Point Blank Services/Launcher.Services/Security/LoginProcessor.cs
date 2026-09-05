using Launcher.Services.Config;
using Launcher.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Launcher.Services.Security
{
    /// <summary>
    /// Regras do login do launcher (FL Guard):
    /// brute force → conta/senha → termo → ban de identificadores → evasão → device → token OTP.
    /// Toda decisão gera login_audit e, quando bloqueia, security_events.
    /// </summary>
    public sealed class LoginProcessor
    {
        private static readonly Regex Hex64 = new Regex("^[0-9a-f]{64}$", RegexOptions.Compiled);

        private readonly ServerConfig _config;
        private readonly SecurityRepository _repo;

        public LoginProcessor(ServerConfig config)
        {
            _config = config;
            _repo = new SecurityRepository(config.DbHost, config.DbPort, config.DbName, config.DbUser, config.DbPassword);
        }

        public LoginResult Process(LoginRequest request, string ip)
        {
            ip = ip ?? "";
            string username = (request?.Username ?? "").Trim();
            string fingerprint = NormalizeHash(request?.Hwid);
            List<KeyValuePair<string, string>> identifiers = ParseIdentifiers(request?.Components, fingerprint);

            // 1) brute force por IP
            if (LoginThrottle.IsBlocked(ip, out TimeSpan remaining))
            {
                _repo.LogLogin(username, 0, "throttled", ip, fingerprint, "bloqueio brute force");
                return Fail("Muitas tentativas. Aguarde " + Math.Ceiling(remaining.TotalMinutes) + " min.", "FG-142");
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(request?.Password))
            {
                _repo.LogLogin(username, 0, "invalid_request", ip, fingerprint, "campos vazios");
                return Fail("Dados de login inválidos.", "");
            }

            // 2) fingerprint obrigatório?
            if (_config.RequireFingerprint && string.IsNullOrEmpty(fingerprint))
            {
                _repo.LogLogin(username, 0, "no_fingerprint", ip, "", "launcher sem fingerprint");
                return Fail("Atualize o launcher para continuar.", "FG-105");
            }

            // 3) conta + senha
            SecurityRepository.AccountRow account = _repo.FindAccount(username);
            if (account == null || !PasswordMatches(account.Password, request.Password))
            {
                bool blocked = LoginThrottle.RegisterFailure(ip, _config.BruteForceMaxFailures,
                    TimeSpan.FromMinutes(_config.BruteForceWindowMinutes), TimeSpan.FromMinutes(_config.BruteForceBlockMinutes));
                _repo.LogLogin(username, account?.PlayerId ?? 0, "bad_password", ip, fingerprint, "");
                if (blocked)
                {
                    _repo.LogEvent(account?.PlayerId ?? 0, username, "login_denied", "BRUTE_FORCE",
                        "IP bloqueado por excesso de falhas de login",
                        Json(new { ip, max = _config.BruteForceMaxFailures, window_min = _config.BruteForceWindowMinutes }),
                        2, "FG-142");
                }
                return Fail("Usuário ou senha incorretos.", "");
            }

            // 4) conta banida
            if (account.AccessLevel == SecurityRepository.AccessBanned ||
                _repo.HasActiveAccountBan(account.PlayerId, out string banReason, out DateTime? banExpire))
            {
                _repo.LogLogin(username, account.PlayerId, "account_banned", ip, fingerprint, "");
                _repo.LogEvent(account.PlayerId, username, "login_denied", "ACCOUNT_BANNED", "Conta banida tentou logar",
                    Json(new { ip, fingerprint }), 2, "FG-103");
                return Fail("Conta bloqueada. Código FG-103.", "FG-103");
            }

            // 5) identificadores banidos / evasão
            SecurityRepository.EvasionResult evasion = _repo.Evaluate(identifiers, ip);
            if (evasion.Score > 0)
            {
                bool hardHit = evasion.DirectBan || evasion.Score >= _config.EvasionAutoBanScore;
                if (hardHit)
                {
                    string reason = "Evasão de ban: vínculo com conta " + evasion.OriginPlayerId +
                                    " (score " + evasion.Score + ": " + string.Join(",", evasion.Hits.Select(h => h.Kind).Distinct()) + ")";

                    long banId = 0;
                    if (evasion.OriginPlayerId != account.PlayerId)
                    {
                        banId = _repo.AutoBanEvasion(account.PlayerId, evasion.OriginPlayerId, identifiers, ip, reason);
                        _repo.LinkAccounts(account.PlayerId, evasion.OriginPlayerId, reason, evasion.Score);
                    }

                    _repo.LogLogin(username, account.PlayerId, "device_banned", ip, fingerprint, reason);
                    _repo.LogEvent(account.PlayerId, username, evasion.OriginPlayerId != account.PlayerId ? "evasion_link" : "login_denied",
                        evasion.OriginPlayerId != account.PlayerId ? "EVASION" : "DEVICE_BANNED", reason,
                        Json(new
                        {
                            ip,
                            fingerprint,
                            score = evasion.Score,
                            origin_player = evasion.OriginPlayerId,
                            hits = evasion.Hits.Select(h => new { h.Kind, h.Value, h.Reason })
                        }), 5, evasion.OriginPlayerId != account.PlayerId ? "FG-104" : "FG-101", banId);

                    return Fail("Acesso bloqueado. Código " + (evasion.OriginPlayerId != account.PlayerId ? "FG-104" : "FG-101") + ".",
                        evasion.OriginPlayerId != account.PlayerId ? "FG-104" : "FG-101");
                }

                if (evasion.Score >= _config.EvasionSuspectScore)
                {
                    _repo.LogEvent(account.PlayerId, username, "flag", "EVASION",
                        "Componentes de hardware em comum com conta banida " + evasion.OriginPlayerId,
                        Json(new
                        {
                            ip,
                            fingerprint,
                            score = evasion.Score,
                            origin_player = evasion.OriginPlayerId,
                            hits = evasion.Hits.Select(h => new { h.Kind, h.Reason })
                        }), 3, "");
                    _repo.LinkAccounts(account.PlayerId, evasion.OriginPlayerId, "suspeita de evasão (score " + evasion.Score + ")", evasion.Score);
                }
            }

            // 6) termo de uso
            if (_config.TosVersion > 0)
            {
                if (request.TosAccepted >= _config.TosVersion)
                    _repo.AcceptTos(account.PlayerId, _config.TosVersion);
                else if (account.TosVersion < _config.TosVersion)
                {
                    _repo.LogLogin(username, account.PlayerId, "tos_required", ip, fingerprint, "versão " + _config.TosVersion);
                    return new LoginResult
                    {
                        Success = false,
                        Message = "Leia e aceite o termo de uso para continuar.",
                        Code = "FG-105",
                        RequireTos = true,
                        TosVersion = _config.TosVersion
                    };
                }
            }

            // 7) device + multi-conta no mesmo PC (só vínculo, não bloqueia)
            if (!string.IsNullOrEmpty(fingerprint))
            {
                _repo.UpsertDevice(account.PlayerId, fingerprint, request.Components, ip);
                foreach (long other in _repo.OtherAccountsOnDevice(account.PlayerId, fingerprint))
                    _repo.LinkAccounts(account.PlayerId, other, "mesmo dispositivo", 5);
            }

            // Probation: garante coluna preenchida para contas novas
            // (Game bloqueia ranked/clan enquanto estiver dentro da janela)
            _repo.EnsureProbation(account.PlayerId, 48);

            // 8) token OTP
            string token = _repo.IssueToken(account.PlayerId, fingerprint, _config.TokenMinutes);
            if (string.IsNullOrEmpty(token))
            {
                _repo.LogLogin(username, account.PlayerId, "token_error", ip, fingerprint, "falha ao gerar token");
                return Fail("Erro ao iniciar sessão. Tente novamente.", "");
            }

            LoginThrottle.RegisterSuccess(ip);
            _repo.LogLogin(username, account.PlayerId, "ok", ip, fingerprint, "");

            return new LoginResult
            {
                Success = true,
                Message = "Login realizado com sucesso.",
                Token = token,
                TokenTtlSeconds = _config.TokenMinutes * 60,
                SessionId = Guid.NewGuid().ToString("N"),
                PlayerId = account.PlayerId,
                TosVersion = _config.TosVersion
            };
        }

        // ------------------------------------------------------------------

        private static LoginResult Fail(string message, string code)
        {
            return new LoginResult { Success = false, Message = message, Code = code ?? "" };
        }

        /// <summary>Mesma regra do DatabaseService antigo: MD5 igual, texto puro igual ao MD5, ou senha vazia no banco.</summary>
        private static bool PasswordMatches(string dbPass, string sentMd5)
        {
            if (string.IsNullOrEmpty(dbPass)) return true;
            if (string.Equals(dbPass, sentMd5, StringComparison.OrdinalIgnoreCase)) return true;
            return string.Equals(Md5(dbPass), sentMd5, StringComparison.OrdinalIgnoreCase);
        }

        private static string Md5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input ?? ""));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static string NormalizeHash(string h)
        {
            if (string.IsNullOrWhiteSpace(h)) return "";
            h = h.Trim().ToLowerInvariant();
            return Hex64.IsMatch(h) ? h : "";
        }

        /// <summary>Lê o JSON de HardwareComponents sem depender da lib do client (Socket pode rodar em Linux).</summary>
        private static List<KeyValuePair<string, string>> ParseIdentifiers(string componentsJson, string fingerprint)
        {
            var list = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrEmpty(fingerprint))
                list.Add(new KeyValuePair<string, string>("fingerprint", fingerprint));

            if (string.IsNullOrWhiteSpace(componentsJson))
                return list;

            JObject obj;
            try { obj = JObject.Parse(componentsJson); }
            catch { return list; }

            foreach (string kind in new[] { "motherboard", "bios_uuid", "cpu", "disk", "ram", "tpm", "gpu", "machine_guid" })
            {
                string v = NormalizeHash(obj.Value<string>(kind));
                if (v.Length > 0)
                    list.Add(new KeyValuePair<string, string>(kind, v));
            }

            if (obj["macs"] is JArray macs)
            {
                foreach (JToken t in macs)
                {
                    string v = NormalizeHash(t.ToString());
                    if (v.Length > 0)
                        list.Add(new KeyValuePair<string, string>("mac", v));
                }
            }

            // fingerprint do JSON só se o campo Hwid veio vazio
            if (list.All(kv => kv.Key != "fingerprint"))
            {
                string fp = NormalizeHash(obj.Value<string>("fingerprint"));
                if (fp.Length > 0)
                    list.Insert(0, new KeyValuePair<string, string>("fingerprint", fp));
            }

            return list;
        }

        private static string Json(object o) => JsonConvert.SerializeObject(o);
    }
}
