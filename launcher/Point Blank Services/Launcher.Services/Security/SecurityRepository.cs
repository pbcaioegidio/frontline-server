using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Launcher.Services.Security
{
    /// <summary>
    /// Acesso ao banco para as camadas de segurança do launcher:
    /// ban por identificador, evasão, devices, token OTP, auditoria.
    /// </summary>
    public sealed class SecurityRepository
    {
        private readonly string _cs;

        public const int AccessBanned = -1;

        // Peso de cada identificador no cálculo de evasão.
        private static readonly Dictionary<string, int> Weights = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "fingerprint", 5 },
            { "tpm", 4 },
            { "bios_uuid", 3 },
            { "motherboard", 3 },
            { "disk", 2 },
            { "ram", 2 },
            { "cpu", 1 },
            { "gpu", 1 },
            { "machine_guid", 1 },
            { "mac", 1 },
            { "ip", 1 },
            { "ip_subnet24", 1 },
            { "account", 5 },
            { "hwid_exe", 3 }
        };

        public SecurityRepository(string host, int port, string database, string user, string password)
        {
            _cs = string.Format("Host={0};Port={1};Database={2};Username={3};Password={4};", host, port, database, user, password);
        }

        public sealed class AccountRow
        {
            public long PlayerId;
            public string Username;
            public string Password;
            public int AccessLevel;
            public int TosVersion;
            public string Hwid;
        }

        public sealed class BanHit
        {
            public string Kind;
            public string Value;
            public long OriginPlayerId;
            public string Reason;
            public DateTime? ExpireAt;
        }

        public sealed class EvasionResult
        {
            public int Score;
            public long OriginPlayerId;
            public List<BanHit> Hits = new List<BanHit>();
            public bool DirectBan => Hits.Any(h => h.Kind == "fingerprint" || h.Kind == "account" || h.Kind == "ip" || h.Kind == "ip_subnet24");
        }

        // ------------------------------------------------------------------
        // Conta
        // ------------------------------------------------------------------

        public AccountRow FindAccount(string username)
        {
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT player_id, username, password, access_level, tos_version, hwid FROM accounts WHERE username = @u LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("u", username);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return null;
                        return new AccountRow
                        {
                            PlayerId = r.GetInt64(0),
                            Username = r.GetString(1),
                            Password = r.IsDBNull(2) ? "" : r.GetString(2),
                            AccessLevel = r.GetInt32(3),
                            TosVersion = r.GetInt32(4),
                            Hwid = r.IsDBNull(5) ? "" : r.GetString(5)
                        };
                    }
                }
            }
        }

        /// <summary>Ban ativo de conta em base_ban_history (tipos ACCOUNT / PERMANENT / DURATION).</summary>
        public bool HasActiveAccountBan(long playerId, out string reason, out DateTime? expire)
        {
            reason = ""; expire = null;
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT reason, expire_date FROM base_ban_history
                    WHERE (owner_id = @p OR player_id = @p)
                      AND type IN ('ACCOUNT','PERMANENT','DURATION')
                      AND expire_date > now()
                    ORDER BY expire_date DESC LIMIT 1", conn))
                {
                    cmd.Parameters.AddWithValue("p", playerId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return false;
                        reason = r.IsDBNull(0) ? "" : r.GetString(0);
                        expire = r.IsDBNull(1) ? (DateTime?)null : r.GetDateTime(1);
                        return true;
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Ban por identificador + evasão
        // ------------------------------------------------------------------

        /// <summary>
        /// Procura identificadores banidos (ban_identifiers ativos) e calcula score de evasão
        /// agrupando por conta de origem do ban. Retorna o pior caso.
        /// </summary>
        public EvasionResult Evaluate(IEnumerable<KeyValuePair<string, string>> identifiers, string ip)
        {
            var ids = identifiers.Where(kv => !string.IsNullOrEmpty(kv.Value)).ToList();
            if (!string.IsNullOrEmpty(ip))
            {
                ids.Add(new KeyValuePair<string, string>("ip", ip));
                string subnet = Subnet24(ip);
                if (subnet != null)
                    ids.Add(new KeyValuePair<string, string>("ip_subnet24", subnet));
            }

            var result = new EvasionResult();
            if (ids.Count == 0) return result;

            var kinds = ids.Select(k => k.Key).ToArray();
            var values = ids.Select(k => k.Value).ToArray();

            var hits = new List<BanHit>();
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    SELECT b.kind, b.value, b.player_id_origem, b.reason, b.expire_at
                    FROM ban_identifiers b
                    JOIN unnest(@kinds, @values) AS q(kind, value) ON q.kind = b.kind AND q.value = b.value
                    WHERE b.expire_at IS NULL OR b.expire_at > now()", conn))
                {
                    cmd.Parameters.Add(new NpgsqlParameter("kinds", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = kinds });
                    cmd.Parameters.Add(new NpgsqlParameter("values", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = values });
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            hits.Add(new BanHit
                            {
                                Kind = r.GetString(0),
                                Value = r.GetString(1),
                                OriginPlayerId = r.GetInt64(2),
                                Reason = r.IsDBNull(3) ? "" : r.GetString(3),
                                ExpireAt = r.IsDBNull(4) ? (DateTime?)null : r.GetDateTime(4)
                            });
                        }
                    }
                }
            }

            if (hits.Count == 0) return result;

            foreach (var group in hits.GroupBy(h => h.OriginPlayerId))
            {
                int score = group.Select(h => h.Kind).Distinct(StringComparer.OrdinalIgnoreCase)
                    .Sum(k => Weights.TryGetValue(k, out int w) ? w : 1);
                if (score > result.Score)
                {
                    result.Score = score;
                    result.OriginPlayerId = group.Key;
                    result.Hits = group.ToList();
                }
            }
            return result;
        }

        // ------------------------------------------------------------------
        // Devices
        // ------------------------------------------------------------------

        public void UpsertDevice(long playerId, string fingerprint, string componentsJson, string ip)
        {
            if (string.IsNullOrEmpty(fingerprint)) return;
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO account_devices (player_id, fingerprint, components_json, last_ip)
                    VALUES (@p, @f, @c::jsonb, @ip)
                    ON CONFLICT (player_id, fingerprint) DO UPDATE
                      SET last_seen = now(), last_ip = EXCLUDED.last_ip,
                          components_json = EXCLUDED.components_json,
                          login_count = account_devices.login_count + 1", conn))
                {
                    cmd.Parameters.AddWithValue("p", playerId);
                    cmd.Parameters.AddWithValue("f", fingerprint);
                    cmd.Parameters.AddWithValue("c", string.IsNullOrWhiteSpace(componentsJson) ? "{}" : componentsJson);
                    cmd.Parameters.AddWithValue("ip", ip ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>Outras contas que já usaram este fingerprint (multi-conta no mesmo PC).</summary>
        public List<long> OtherAccountsOnDevice(long playerId, string fingerprint)
        {
            var list = new List<long>();
            if (string.IsNullOrEmpty(fingerprint)) return list;
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT player_id FROM account_devices WHERE fingerprint = @f AND player_id <> @p", conn))
                {
                    cmd.Parameters.AddWithValue("f", fingerprint);
                    cmd.Parameters.AddWithValue("p", playerId);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(r.GetInt64(0));
                }
            }
            return list;
        }

        public void LinkAccounts(long a, long b, string reason, int score)
        {
            if (a == b || a <= 0 || b <= 0) return;
            long lo = Math.Min(a, b), hi = Math.Max(a, b);
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO account_links (player_a, player_b, reason, score)
                    VALUES (@a, @b, @r, @s)
                    ON CONFLICT (player_a, player_b) DO UPDATE
                      SET score = GREATEST(account_links.score, EXCLUDED.score), reason = EXCLUDED.reason", conn))
                {
                    cmd.Parameters.AddWithValue("a", lo);
                    cmd.Parameters.AddWithValue("b", hi);
                    cmd.Parameters.AddWithValue("r", Trunc(reason, 255));
                    cmd.Parameters.AddWithValue("s", score);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ------------------------------------------------------------------
        // Token OTP
        // ------------------------------------------------------------------

        /// <summary>
        /// Gera token aleatório (16 bytes → Base64, formato aceito pelo client), com validade curta.
        /// Grava também o fingerprint em accounts.hwid.
        /// </summary>
        public string IssueToken(long playerId, string fingerprint, int minutes)
        {
            for (int attempt = 0; attempt < 3; attempt++)
            {
                string token = NewToken();
                try
                {
                    using (var conn = new NpgsqlConnection(_cs))
                    {
                        conn.Open();
                        using (var cmd = new NpgsqlCommand(@"
                            UPDATE accounts
                               SET token = @t,
                                   token_expires = now() + make_interval(mins => @m),
                                   hwid = CASE WHEN @h = '' THEN hwid ELSE @h END
                             WHERE player_id = @p", conn))
                        {
                            cmd.Parameters.AddWithValue("t", token);
                            cmd.Parameters.AddWithValue("m", minutes);
                            cmd.Parameters.AddWithValue("h", fingerprint ?? "");
                            cmd.Parameters.AddWithValue("p", playerId);
                            if (cmd.ExecuteNonQuery() == 1)
                                return token;
                            return null;
                        }
                    }
                }
                catch (PostgresException ex) when (ex.SqlState == "23505")
                {
                    // colisão no índice único de token — tenta outro
                }
            }
            return null;
        }

        private static string NewToken()
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        // ------------------------------------------------------------------
        // Termo de uso
        // ------------------------------------------------------------------

        public void AcceptTos(long playerId, int version)
        {
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "UPDATE accounts SET tos_version = @v, tos_accepted_at = now() WHERE player_id = @p AND tos_version < @v", conn))
                {
                    cmd.Parameters.AddWithValue("v", version);
                    cmd.Parameters.AddWithValue("p", playerId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ------------------------------------------------------------------
        // Ban automático por evasão
        // ------------------------------------------------------------------

        /// <summary>
        /// Bane a conta nova (access_level, base_ban_history) e registra os identificadores
        /// dela em ban_identifiers apontando para a conta de origem.
        /// </summary>
        public long AutoBanEvasion(long newPlayerId, long originPlayerId, IEnumerable<KeyValuePair<string, string>> identifiers, string ip, string reason)
        {
            long banId = 0;
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = new NpgsqlCommand("UPDATE accounts SET access_level = @a WHERE player_id = @p", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("a", AccessBanned);
                        cmd.Parameters.AddWithValue("p", newPlayerId);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO base_ban_history(owner_id, player_id, type, value, reason, start_date, expire_date)
                        VALUES(@p, @p, 'ACCOUNT', @p::text, @r, now(), now() + interval '3650 days')
                        RETURNING object_id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("p", newPlayerId);
                        cmd.Parameters.AddWithValue("r", Trunc(reason, 255));
                        banId = Convert.ToInt64(cmd.ExecuteScalar());
                    }

                    var all = identifiers.Where(kv => !string.IsNullOrEmpty(kv.Value)).ToList();
                    all.Add(new KeyValuePair<string, string>("account", newPlayerId.ToString()));
                    if (!string.IsNullOrEmpty(ip))
                        all.Add(new KeyValuePair<string, string>("ip", ip));

                    foreach (var kv in all)
                    {
                        using (var cmd = new NpgsqlCommand(@"
                            INSERT INTO ban_identifiers (kind, value, player_id_origem, reason, created_by)
                            VALUES (@k, @v, @o, @r, 'socket:evasion')
                            ON CONFLICT (kind, value) DO NOTHING", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("k", kv.Key);
                            cmd.Parameters.AddWithValue("v", Trunc(kv.Value, 128));
                            cmd.Parameters.AddWithValue("o", originPlayerId);
                            cmd.Parameters.AddWithValue("r", Trunc(reason, 255));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }
            return banId;
        }

        // ------------------------------------------------------------------
        // Auditoria
        // ------------------------------------------------------------------

        public void LogLogin(string username, long playerId, string result, string ip, string fingerprint, string reason)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_cs))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO login_audit (source, username, player_id, result, ip, fingerprint, reason)
                        VALUES ('socket', @u, @p, @r, @ip, @f, @reason)", conn))
                    {
                        cmd.Parameters.AddWithValue("u", Trunc(username ?? "", 32));
                        cmd.Parameters.AddWithValue("p", playerId);
                        cmd.Parameters.AddWithValue("r", Trunc(result ?? "", 32));
                        cmd.Parameters.AddWithValue("ip", Trunc(ip ?? "", 64));
                        cmd.Parameters.AddWithValue("f", Trunc(fingerprint ?? "", 64));
                        cmd.Parameters.AddWithValue("reason", Trunc(reason ?? "", 255));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Security] login_audit falhou: " + ex.Message);
            }
        }

        /// <summary>FileCheck do launcher → integrity_events (staff/IA).</summary>
        public void InsertIntegrityEvent(
            long playerId,
            string username,
            string ip,
            bool ok,
            bool? restored,
            string invalidFilesJson,
            string extrasRemovedJson,
            string launcherVer,
            string message)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_cs))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO integrity_events
                            (player_id, username, ip, ok, restored, invalid_files, extras_removed, launcher_ver, message)
                        VALUES
                            (@p, @u, @ip, @ok, @restored, @inv::jsonb, @ext::jsonb, @ver, @msg)", conn))
                    {
                        cmd.Parameters.AddWithValue("p", playerId);
                        cmd.Parameters.AddWithValue("u", Trunc(username ?? "", 64));
                        cmd.Parameters.AddWithValue("ip", Trunc(ip ?? "", 64));
                        cmd.Parameters.AddWithValue("ok", ok);
                        cmd.Parameters.AddWithValue("restored", (object)restored ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("inv", string.IsNullOrWhiteSpace(invalidFilesJson) ? "[]" : invalidFilesJson);
                        cmd.Parameters.AddWithValue("ext", string.IsNullOrWhiteSpace(extrasRemovedJson) ? "[]" : extrasRemovedJson);
                        cmd.Parameters.AddWithValue("ver", Trunc(launcherVer ?? "", 32));
                        cmd.Parameters.AddWithValue("msg", Trunc(message ?? "", 512));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Security] integrity_events falhou: " + ex.Message);
            }
        }

        public void LogEvent(long playerId, string username, string action, string category, string reason,
            string evidenceJson, int severity, string clientCode, long banId = 0, bool auto = true, long gmId = 0)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_cs))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO security_events
                            (player_id, username, action, source, category, reason, evidence_json, severity, auto, gm_id, ban_id, client_code)
                        VALUES (@p, @u, @a, 'socket', @c, @r, @e::jsonb, @s, @auto, @gm, @ban, @code)", conn))
                    {
                        cmd.Parameters.AddWithValue("p", playerId);
                        cmd.Parameters.AddWithValue("u", Trunc(username ?? "", 32));
                        cmd.Parameters.AddWithValue("a", action);
                        cmd.Parameters.AddWithValue("c", Trunc(category, 32));
                        cmd.Parameters.AddWithValue("r", Trunc(reason, 255));
                        cmd.Parameters.AddWithValue("e", string.IsNullOrWhiteSpace(evidenceJson) ? "{}" : evidenceJson);
                        cmd.Parameters.AddWithValue("s", Math.Max(1, Math.Min(5, severity)));
                        cmd.Parameters.AddWithValue("auto", auto);
                        cmd.Parameters.AddWithValue("gm", gmId);
                        cmd.Parameters.AddWithValue("ban", banId);
                        cmd.Parameters.AddWithValue("code", Trunc(clientCode ?? "", 12));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Security] security_events falhou: " + ex.Message);
            }
        }

        // ------------------------------------------------------------------

        // ------------------------------------------------------------------
        // Heartbeat + captura
        // ------------------------------------------------------------------

        public sealed class CaptureJob
        {
            public long Id;
            public string Kind;
            public string Reason;
        }

        public void UpsertLiveSession(long playerId, string sessionId, string fingerprint, string ip, string status, string statusReason, string modulesHash)
        {
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO live_sessions (player_id, session_id, fingerprint, ip, last_heartbeat, status, status_reason, modules_hash)
                    VALUES (@p, @s, @f, @ip, now(), @st, @sr, @mh)
                    ON CONFLICT (player_id) DO UPDATE
                      SET session_id = EXCLUDED.session_id,
                          fingerprint = EXCLUDED.fingerprint,
                          ip = EXCLUDED.ip,
                          last_heartbeat = now(),
                          status = EXCLUDED.status,
                          status_reason = EXCLUDED.status_reason,
                          modules_hash = EXCLUDED.modules_hash", conn))
                {
                    cmd.Parameters.AddWithValue("p", playerId);
                    cmd.Parameters.AddWithValue("s", Trunc(sessionId ?? "", 64));
                    cmd.Parameters.AddWithValue("f", Trunc(fingerprint ?? "", 64));
                    cmd.Parameters.AddWithValue("ip", Trunc(ip ?? "", 64));
                    cmd.Parameters.AddWithValue("st", Trunc(status ?? "ok", 16));
                    cmd.Parameters.AddWithValue("sr", Trunc(statusReason ?? "", 255));
                    cmd.Parameters.AddWithValue("mh", Trunc(modulesHash ?? "", 64));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Goodbye do FL Guard: invalida a sessão na hora (kick quase imediato no Game).
        /// </summary>
        public void EndLiveSession(long playerId, string statusReason = "guard_stop")
        {
            if (playerId <= 0) return;
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    UPDATE live_sessions
                       SET last_heartbeat = now() - interval '1 day',
                           status = 'closed',
                           status_reason = @sr
                     WHERE player_id = @p", conn))
                {
                    cmd.Parameters.AddWithValue("p", playerId);
                    cmd.Parameters.AddWithValue("sr", Trunc(statusReason ?? "guard_stop", 255));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<CaptureJob> PeekAndDeliverCaptures(long playerId, int limit = 2)
        {
            var list = new List<CaptureJob>();
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT id, kind, reason FROM capture_requests
                         WHERE player_id = @p AND status = 'pending'
                           AND created_at > now() - interval '5 minutes'
                         ORDER BY id ASC LIMIT @lim
                         FOR UPDATE SKIP LOCKED", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("p", playerId);
                        cmd.Parameters.AddWithValue("lim", limit);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add(new CaptureJob
                                {
                                    Id = r.GetInt64(0),
                                    Kind = r.GetString(1),
                                    Reason = r.IsDBNull(2) ? "" : r.GetString(2)
                                });
                            }
                        }
                    }
                    foreach (var job in list)
                    {
                        using (var upd = new NpgsqlCommand(
                            "UPDATE capture_requests SET status = 'delivered', delivered_at = now() WHERE id = @id AND status = 'pending'", conn, tx))
                        {
                            upd.Parameters.AddWithValue("id", job.Id);
                            upd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                }
            }
            return list;
        }

        public void CompleteCapture(long requestId, long playerId, string kind, string path, bool blocked, string error, string username)
        {
            string status = blocked ? "blocked" : (string.IsNullOrEmpty(path) ? "failed" : "completed");
            using (var conn = new NpgsqlConnection(_cs))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(@"
                    UPDATE capture_requests
                       SET status = @s, completed_at = now(), evidence_path = @path, error = @err
                     WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("s", status);
                    cmd.Parameters.AddWithValue("path", Trunc(path ?? "", 512));
                    cmd.Parameters.AddWithValue("err", Trunc(error ?? "", 255));
                    cmd.Parameters.AddWithValue("id", requestId);
                    cmd.ExecuteNonQuery();
                }
            }

            string action = blocked ? "capture_blocked" : (kind == "clip" ? "clip" : "screenshot");
            LogEvent(playerId, username ?? "", action, blocked ? "CAPTURE_BLOCKED" : "CAPTURE",
                blocked ? ("Captura bloqueada: " + error) : ("Evidência: " + path),
                "{\"request_id\":" + requestId + ",\"path\":\"" + Trunc(path ?? "", 200) + "\"}",
                blocked ? 4 : 2, blocked ? "FG-114" : "");
        }

        public void EnsureProbation(long playerId, int hours)
        {
            if (playerId <= 0 || hours <= 0) return;
            try
            {
                using (var conn = new NpgsqlConnection(_cs))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE accounts SET probation_until = COALESCE(probation_until, created_at + make_interval(hours => @h))
                         WHERE player_id = @p AND probation_until IS NULL", conn))
                    {
                        cmd.Parameters.AddWithValue("h", hours);
                        cmd.Parameters.AddWithValue("p", playerId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        public static string Subnet24(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return null;
            string[] parts = ip.Split('.');
            if (parts.Length != 4) return null;
            return parts[0] + "." + parts[1] + "." + parts[2] + ".0/24";
        }

        private static string Trunc(string s, int max)
        {
            if (s == null) return "";
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
