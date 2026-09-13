using Npgsql;
using Plugin.Core.Enums;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Plugin.Core.Security
{
    /// <summary>
    /// Persistência das camadas de segurança (FL Guard) usada por Auth e Game:
    /// token OTP, auditoria de login, security_events e ban multi-identificador.
    /// </summary>
    public static class SecurityDao
    {
        public const string SourceAuth = "auth";
        public const string SourceGame = "game";
        public const string SourceMatch = "match";
        public const string SourceGm = "gm";
        public const string SourceRcon = "rcon";

        // ------------------------------------------------------------------
        // Token OTP
        // ------------------------------------------------------------------

        /// <summary>Lê token_expires da conta. null = nunca emitido pelo launcher.</summary>
        public static DateTime? GetTokenExpires(long playerId)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("SELECT token_expires FROM accounts WHERE player_id = @p", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", playerId);
                        object o = cmd.ExecuteScalar();
                        if (o == null || o == DBNull.Value) return null;
                        return Convert.ToDateTime(o);
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] GetTokenExpires: " + ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        /// <summary>Queima o token: valor aleatório novo + expiração no passado.</summary>
        public static void InvalidateToken(long playerId)
        {
            try
            {
                var bytes = new byte[16];
                using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
                string burned = Convert.ToBase64String(bytes);

                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "UPDATE accounts SET token = @t, token_expires = now() - interval '1 second' WHERE player_id = @p", conn))
                    {
                        cmd.Parameters.AddWithValue("@t", burned);
                        cmd.Parameters.AddWithValue("@p", playerId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] InvalidateToken: " + ex.Message, LoggerType.Error, ex);
            }
        }

        /// <summary>Após login OK no Auth: grava IP, HWID que o exe mandou e último login.</summary>
        public static void TouchLoginSuccess(long playerId, string ip, string hwidExe)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "UPDATE accounts SET ip4_address = @ip, hwid_exe = @h WHERE player_id = @p", conn))
                    {
                        cmd.Parameters.AddWithValue("@ip", Trunc(ip ?? "", 64));
                        cmd.Parameters.AddWithValue("@h", Trunc(hwidExe ?? "", 128));
                        cmd.Parameters.AddWithValue("@p", playerId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] TouchLoginSuccess: " + ex.Message, LoggerType.Error, ex);
            }
        }

        // ------------------------------------------------------------------
        // Auditoria
        // ------------------------------------------------------------------

        public static void LogLogin(string source, string username, long playerId, string result, string ip, string fingerprint, string reason)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO login_audit (source, username, player_id, result, ip, fingerprint, reason)
                        VALUES (@s, @u, @p, @r, @ip, @f, @reason)", conn))
                    {
                        cmd.Parameters.AddWithValue("@s", Trunc(source, 16));
                        cmd.Parameters.AddWithValue("@u", Trunc(username ?? "", 32));
                        cmd.Parameters.AddWithValue("@p", playerId);
                        cmd.Parameters.AddWithValue("@r", Trunc(result ?? "", 32));
                        cmd.Parameters.AddWithValue("@ip", Trunc(ip ?? "", 64));
                        cmd.Parameters.AddWithValue("@f", Trunc(fingerprint ?? "", 64));
                        cmd.Parameters.AddWithValue("@reason", Trunc(reason ?? "", 255));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] login_audit: " + ex.Message, LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Registro obrigatório de flag/kick/ban. <paramref name="evidenceJson"/> deve ser JSON válido.
        /// </summary>
        public static long LogEvent(string source, long playerId, string username, string nickname, string action,
            string category, string reason, string evidenceJson, int severity, string clientCode,
            long banId = 0, bool auto = true, long gmId = 0)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO security_events
                            (player_id, username, nickname, action, source, category, reason, evidence_json, severity, auto, gm_id, ban_id, client_code)
                        VALUES (@p, @u, @n, @a, @s, @c, @r, @e::jsonb, @sev, @auto, @gm, @ban, @code)
                        RETURNING id", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", playerId);
                        cmd.Parameters.AddWithValue("@u", Trunc(username ?? "", 32));
                        cmd.Parameters.AddWithValue("@n", Trunc(nickname ?? "", 32));
                        cmd.Parameters.AddWithValue("@a", Trunc(action, 24));
                        cmd.Parameters.AddWithValue("@s", Trunc(source, 16));
                        cmd.Parameters.AddWithValue("@c", Trunc(category ?? "", 32));
                        cmd.Parameters.AddWithValue("@r", Trunc(string.IsNullOrWhiteSpace(reason) ? category : reason, 255));
                        cmd.Parameters.AddWithValue("@e", string.IsNullOrWhiteSpace(evidenceJson) ? "{}" : evidenceJson);
                        cmd.Parameters.AddWithValue("@sev", Math.Max(1, Math.Min(5, severity)));
                        cmd.Parameters.AddWithValue("@auto", auto);
                        cmd.Parameters.AddWithValue("@gm", gmId);
                        cmd.Parameters.AddWithValue("@ban", banId);
                        cmd.Parameters.AddWithValue("@code", Trunc(clientCode ?? "", 12));
                        object o = cmd.ExecuteScalar();
                        return o == null ? 0 : Convert.ToInt64(o);
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] security_events: " + ex.Message, LoggerType.Error, ex);
                return 0;
            }
        }

        // ------------------------------------------------------------------
        // Hard ban: conta + todos os identificadores conhecidos
        // ------------------------------------------------------------------

        public sealed class HardBanResult
        {
            public long BanId;
            public int Identifiers;
            public int Devices;
        }

        /// <summary>
        /// Ban "tudo de uma vez": access_level BANNED, base_ban_history (ACCOUNT, MAC, IP4),
        /// base_ban_hwid (fingerprint do launcher + hwid do exe), ban_identifiers de todos os
        /// componentes de todos os devices da conta, e security_events.
        /// </summary>
        public static HardBanResult ApplyHardBan(long playerId, string reason, TimeSpan duration, string createdBy,
            string source, long gmId, string category, string evidenceJson, bool banSubnet)
        {
            var result = new HardBanResult();
            if (playerId <= 0) return result;
            reason = string.IsNullOrWhiteSpace(reason) ? "Cheat" : reason;
            DateTime end = DateTimeUtil.Now().Add(duration <= TimeSpan.Zero ? TimeSpan.FromDays(3650) : duration);

            string username = "", nickname = "", mac = "", ip = "", hwidLauncher = "", hwidExe = "";
            var identifiers = new List<KeyValuePair<string, string>>();

            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        // dados da conta
                        using (var cmd = new NpgsqlCommand(
                            "SELECT username, nickname, mac_address::text, ip4_address, hwid, hwid_exe FROM accounts WHERE player_id = @p", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@p", playerId);
                            using (var r = cmd.ExecuteReader())
                            {
                                if (!r.Read()) { tx.Rollback(); return result; }
                                username = r.IsDBNull(0) ? "" : r.GetString(0);
                                nickname = r.IsDBNull(1) ? "" : r.GetString(1);
                                mac = r.IsDBNull(2) ? "" : r.GetString(2);
                                ip = r.IsDBNull(3) ? "" : r.GetString(3);
                                hwidLauncher = r.IsDBNull(4) ? "" : r.GetString(4);
                                hwidExe = r.IsDBNull(5) ? "" : r.GetString(5);
                            }
                        }

                        // conta
                        using (var cmd = new NpgsqlCommand("UPDATE accounts SET access_level = @a WHERE player_id = @p", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@a", (int)AccessLevel.BANNED);
                            cmd.Parameters.AddWithValue("@p", playerId);
                            cmd.ExecuteNonQuery();
                        }

                        // base_ban_history: ACCOUNT / MAC / IP4 (Auth e Game já leem)
                        result.BanId = InsertBanHistory(conn, tx, playerId, "ACCOUNT", playerId.ToString(), reason, end);
                        if (!string.IsNullOrEmpty(mac) && mac != "00:00:00:00:00:00")
                            InsertBanHistory(conn, tx, playerId, "MAC", mac, reason, end);
                        if (!string.IsNullOrEmpty(ip) && ip != "0")
                            InsertBanHistory(conn, tx, playerId, "IP4", ip, reason, end);

                        // base_ban_hwid (cache do Auth)
                        foreach (string h in new[] { hwidLauncher, hwidExe })
                        {
                            if (string.IsNullOrWhiteSpace(h)) continue;
                            using (var cmd = new NpgsqlCommand(@"
                                INSERT INTO base_ban_hwid (id, hardware_id, username, process_name, ip_address, ban_date, ban_reason, ban_type)
                                VALUES (nextval('base_ban_hwid_id_seq'), @h, @u, 'FL GUARD', @ip, now(), @r, 'cheat')", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@h", Trunc(h, 64));
                                cmd.Parameters.AddWithValue("@u", Trunc(username, 32));
                                cmd.Parameters.AddWithValue("@ip", Trunc(ip, 45));
                                cmd.Parameters.AddWithValue("@r", Trunc(reason, 255));
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // identificadores: conta, mac, ip, hwid_exe, fingerprint(s) e componentes de todos os devices
                        identifiers.Add(new KeyValuePair<string, string>("account", playerId.ToString()));
                        if (!string.IsNullOrEmpty(mac) && mac != "00:00:00:00:00:00")
                            identifiers.Add(new KeyValuePair<string, string>("mac_raw", mac.ToUpperInvariant()));
                        if (!string.IsNullOrEmpty(ip) && ip != "0")
                        {
                            identifiers.Add(new KeyValuePair<string, string>("ip", ip));
                            if (banSubnet)
                            {
                                string[] parts = ip.Split('.');
                                if (parts.Length == 4)
                                    identifiers.Add(new KeyValuePair<string, string>("ip_subnet24", parts[0] + "." + parts[1] + "." + parts[2] + ".0/24"));
                            }
                        }
                        if (!string.IsNullOrEmpty(hwidExe))
                            identifiers.Add(new KeyValuePair<string, string>("hwid_exe", hwidExe));
                        if (!string.IsNullOrEmpty(hwidLauncher))
                            identifiers.Add(new KeyValuePair<string, string>("fingerprint", hwidLauncher));

                        using (var cmd = new NpgsqlCommand(
                            "SELECT fingerprint, components_json::text FROM account_devices WHERE player_id = @p", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@p", playerId);
                            using (var r = cmd.ExecuteReader())
                            {
                                while (r.Read())
                                {
                                    result.Devices++;
                                    string fp = r.IsDBNull(0) ? "" : r.GetString(0);
                                    if (!string.IsNullOrEmpty(fp))
                                        identifiers.Add(new KeyValuePair<string, string>("fingerprint", fp));
                                    string json = r.IsDBNull(1) ? "" : r.GetString(1);
                                    identifiers.AddRange(ComponentsFromJson(json));
                                }
                            }
                        }

                        foreach (var kv in identifiers)
                        {
                            string kind = kv.Key == "mac_raw" ? "mac" : kv.Key;
                            using (var cmd = new NpgsqlCommand(@"
                                INSERT INTO ban_identifiers (kind, value, player_id_origem, reason, expire_at, created_by)
                                VALUES (@k, @v, @o, @r, @e, @by)
                                ON CONFLICT (kind, value) DO UPDATE
                                  SET expire_at = GREATEST(ban_identifiers.expire_at, EXCLUDED.expire_at),
                                      reason = EXCLUDED.reason", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@k", kind);
                                cmd.Parameters.AddWithValue("@v", Trunc(kv.Value, 128));
                                cmd.Parameters.AddWithValue("@o", playerId);
                                cmd.Parameters.AddWithValue("@r", Trunc(reason, 255));
                                cmd.Parameters.AddWithValue("@e", end);
                                cmd.Parameters.AddWithValue("@by", Trunc(createdBy ?? "system", 64));
                                cmd.ExecuteNonQuery();
                                result.Identifiers++;
                            }
                        }

                        tx.Commit();
                    }
                }

                HwIdBanCache.Invalidate();

                LogEvent(source ?? SourceGm, playerId, username, nickname, "hardban", category ?? "HARDBAN", reason,
                    string.IsNullOrWhiteSpace(evidenceJson)
                        ? "{\"identifiers\":" + result.Identifiers + ",\"devices\":" + result.Devices + ",\"until\":\"" + end.ToString("yyyy-MM-dd HH:mm:ss") + "\"}"
                        : evidenceJson,
                    5, "FG-103", result.BanId, gmId == 0, gmId);

                CLogger.Print($"[FL GUARD] Hard ban {username} (#{playerId}) até {end:yyyy-MM-dd} — {result.Identifiers} identificador(es), {result.Devices} device(s). Motivo: {reason}", LoggerType.Warning);
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] ApplyHardBan: " + ex.Message, LoggerType.Error, ex);
            }
            return result;
        }

        /// <summary>
        /// Desfaz o hard ban: libera a conta, expira base_ban_history/ban_identifiers desta conta
        /// e remove os HWIDs dela de base_ban_hwid.
        /// </summary>
        public static bool LiftHardBan(long playerId, string liftedBy, long gmId)
        {
            if (playerId <= 0) return false;
            try
            {
                string username = "", nickname = "";
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        using (var cmd = new NpgsqlCommand("SELECT username, nickname FROM accounts WHERE player_id = @p", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@p", playerId);
                            using (var r = cmd.ExecuteReader())
                            {
                                if (!r.Read()) { tx.Rollback(); return false; }
                                username = r.IsDBNull(0) ? "" : r.GetString(0);
                                nickname = r.IsDBNull(1) ? "" : r.GetString(1);
                            }
                        }
                        using (var cmd = new NpgsqlCommand(
                            "UPDATE accounts SET access_level = CASE WHEN access_level = @b THEN 0 ELSE access_level END, ban_object_id = 0 WHERE player_id = @p", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@b", (int)AccessLevel.BANNED);
                            cmd.Parameters.AddWithValue("@p", playerId);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new NpgsqlCommand(
                            "UPDATE base_ban_history SET expire_date = now() - interval '1 minute' WHERE (owner_id = @p OR player_id = @p) AND expire_date > now() AND type <> 'MUTE'", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@p", playerId);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new NpgsqlCommand(
                            "UPDATE ban_identifiers SET expire_at = now() - interval '1 minute' WHERE player_id_origem = @p AND (expire_at IS NULL OR expire_at > now())", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@p", playerId);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new NpgsqlCommand("DELETE FROM base_auto_ban WHERE owner_id = @p", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@p", playerId);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new NpgsqlCommand("DELETE FROM base_ban_hwid WHERE username = @u", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@u", username);
                            cmd.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                }
                HwIdBanCache.Invalidate();
                LogEvent(SourceGm, playerId, username, nickname, "info", "UNBAN", "Ban removido por " + liftedBy,
                    "{\"by\":\"" + liftedBy + "\"}", 1, "", 0, false, gmId);
                CLogger.Print($"[FL GUARD] Ban removido {username} (#{playerId}) por {liftedBy}", LoggerType.Info);
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] LiftHardBan: " + ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        private static long InsertBanHistory(NpgsqlConnection conn, NpgsqlTransaction tx, long playerId, string type, string value, string reason, DateTime end)
        {
            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO base_ban_history(owner_id, player_id, type, value, reason, start_date, expire_date)
                VALUES(@p, @p, @t, @v, @r, now(), @e) RETURNING object_id", conn, tx))
            {
                cmd.Parameters.AddWithValue("@p", playerId);
                cmd.Parameters.AddWithValue("@t", type);
                cmd.Parameters.AddWithValue("@v", Trunc(value, 255));
                cmd.Parameters.AddWithValue("@r", Trunc(reason, 255));
                cmd.Parameters.AddWithValue("@e", end);
                object o = cmd.ExecuteScalar();
                return o == null ? 0 : Convert.ToInt64(o);
            }
        }

        /// <summary>Extrai (kind, hash) do components_json sem depender de lib do client.</summary>
        private static IEnumerable<KeyValuePair<string, string>> ComponentsFromJson(string json)
        {
            var list = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrWhiteSpace(json)) return list;
            try
            {
                using (var doc = System.Text.Json.JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    foreach (string kind in new[] { "motherboard", "bios_uuid", "cpu", "disk", "ram", "tpm", "gpu", "machine_guid" })
                    {
                        if (root.TryGetProperty(kind, out var el) && el.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            string v = el.GetString();
                            if (!string.IsNullOrWhiteSpace(v) && v.Length == 64)
                                list.Add(new KeyValuePair<string, string>(kind, v.ToLowerInvariant()));
                        }
                    }
                    if (root.TryGetProperty("macs", out var macs) && macs.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var t in macs.EnumerateArray())
                        {
                            string v = t.ValueKind == System.Text.Json.JsonValueKind.String ? t.GetString() : null;
                            if (!string.IsNullOrWhiteSpace(v) && v.Length == 64)
                                list.Add(new KeyValuePair<string, string>("mac", v.ToLowerInvariant()));
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        // ------------------------------------------------------------------
        // Captura de evidência (screenshot / clip)
        // ------------------------------------------------------------------

        public sealed class CaptureRequest
        {
            public long Id;
            public long PlayerId;
            public string Kind; // screenshot | clip
            public string Reason;
            public long GmId;
            public string RequestedBy;
        }

        /// <summary>
        /// Enfileira captura. Não é aleatória — só GM, RCON ou flag/autoban.
        /// Evita spam: se já houver pending do mesmo kind nos últimos 30s, reutiliza.
        /// </summary>
        public static long RequestCapture(long playerId, string kind, string reason, string requestedBy, long gmId = 0, string source = SourceGame)
        {
            if (playerId <= 0) return 0;
            kind = (kind ?? "screenshot").ToLowerInvariant();
            if (kind != "screenshot" && kind != "clip") kind = "screenshot";

            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var check = new NpgsqlCommand(@"
                        SELECT id FROM capture_requests
                         WHERE player_id = @p AND kind = @k AND status = 'pending'
                           AND created_at > now() - interval '30 seconds'
                         ORDER BY id DESC LIMIT 1", conn))
                    {
                        check.Parameters.AddWithValue("@p", playerId);
                        check.Parameters.AddWithValue("@k", kind);
                        object existing = check.ExecuteScalar();
                        if (existing != null && existing != DBNull.Value)
                            return Convert.ToInt64(existing);
                    }

                    long id;
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO capture_requests (player_id, kind, reason, requested_by, gm_id)
                        VALUES (@p, @k, @r, @by, @gm) RETURNING id", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", playerId);
                        cmd.Parameters.AddWithValue("@k", kind);
                        cmd.Parameters.AddWithValue("@r", Trunc(reason ?? "", 255));
                        cmd.Parameters.AddWithValue("@by", Trunc(requestedBy ?? "system", 64));
                        cmd.Parameters.AddWithValue("@gm", gmId);
                        id = Convert.ToInt64(cmd.ExecuteScalar());
                    }

                    LogEvent(source ?? SourceGame, playerId, "", "", kind == "clip" ? "clip" : "screenshot",
                        "CAPTURE_REQUEST", Trunc(reason ?? "captura solicitada", 255),
                        "{\"request_id\":" + id + ",\"kind\":\"" + kind + "\",\"by\":\"" + Trunc(requestedBy ?? "", 64) + "\"}",
                        2, "", 0, gmId == 0, gmId);
                    return id;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] RequestCapture: " + ex.Message, LoggerType.Error, ex);
                return 0;
            }
        }

        /// <summary>Próximas capturas pendentes do jogador (Socket entrega no heartbeat).</summary>
        public static List<CaptureRequest> PeekPendingCaptures(long playerId, int limit = 3)
        {
            var list = new List<CaptureRequest>();
            if (playerId <= 0) return list;
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT id, player_id, kind, reason, gm_id, requested_by
                          FROM capture_requests
                         WHERE player_id = @p AND status = 'pending'
                           AND created_at > now() - interval '5 minutes'
                         ORDER BY id ASC LIMIT @lim", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", playerId);
                        cmd.Parameters.AddWithValue("@lim", limit);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add(new CaptureRequest
                                {
                                    Id = r.GetInt64(0),
                                    PlayerId = r.GetInt64(1),
                                    Kind = r.GetString(2),
                                    Reason = r.IsDBNull(3) ? "" : r.GetString(3),
                                    GmId = r.GetInt64(4),
                                    RequestedBy = r.IsDBNull(5) ? "" : r.GetString(5)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] PeekPendingCaptures: " + ex.Message, LoggerType.Error, ex);
            }
            return list;
        }

        public static void MarkCaptureDelivered(long requestId)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "UPDATE capture_requests SET status = 'delivered', delivered_at = now() WHERE id = @id AND status = 'pending'", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", requestId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] MarkCaptureDelivered: " + ex.Message, LoggerType.Error, ex);
            }
        }

        public static void CompleteCapture(long requestId, long playerId, string kind, string path, bool blocked, string error, string username, string nickname)
        {
            try
            {
                string status = blocked ? "blocked" : (string.IsNullOrEmpty(path) ? "failed" : "completed");
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE capture_requests
                           SET status = @s, completed_at = now(), evidence_path = @path, error = @err
                         WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@s", status);
                        cmd.Parameters.AddWithValue("@path", Trunc(path ?? "", 512));
                        cmd.Parameters.AddWithValue("@err", Trunc(error ?? "", 255));
                        cmd.Parameters.AddWithValue("@id", requestId);
                        cmd.ExecuteNonQuery();
                    }
                }

                string action = blocked ? "capture_blocked" : (kind == "clip" ? "clip" : "screenshot");
                string code = blocked ? "FG-114" : "";
                LogEvent(SourceGame, playerId, username ?? "", nickname ?? "", action,
                    blocked ? "CAPTURE_BLOCKED" : "CAPTURE",
                    blocked ? ("Captura bloqueada: " + error) : ("Evidência salva: " + path),
                    "{\"request_id\":" + requestId + ",\"path\":\"" + Trunc(path ?? "", 200) + "\",\"blocked\":" + (blocked ? "true" : "false") + "}",
                    blocked ? 4 : 2, code);
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] CompleteCapture: " + ex.Message, LoggerType.Error, ex);
            }
        }

        /// <summary>true se o launcher está vivo (heartbeat recente) OU se a exigência está desligada.</summary>
        public static bool IsLauncherAlive(long playerId, int timeoutSeconds)
        {
            if (playerId <= 0) return true;
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT last_heartbeat, status FROM live_sessions WHERE player_id = @p", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", playerId);
                        using (var r = cmd.ExecuteReader())
                        {
                            // Sem linha ainda: Guard ainda não registrou — não kicka no keep-alive.
                            // HeartbeatGuard (com grace) trata ausência prolongada.
                            if (!r.Read()) return true;
                            string status = r.IsDBNull(1) ? "" : r.GetString(1);
                            if (string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase))
                                return false;
                            DateTime last = r.GetDateTime(0);
                            return (DateTimeUtil.Now() - last).TotalSeconds <= Math.Max(5, timeoutSeconds);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] IsLauncherAlive: " + ex.Message, LoggerType.Error, ex);
                return true; // falha de DB não kicka todo mundo
            }
        }

        /// <summary>Lista player_ids online no Game cujo heartbeat sumiu.</summary>
        public static List<long> ListStaleHeartbeats(IEnumerable<long> onlinePlayerIds, int timeoutSeconds)
        {
            var stale = new List<long>();
            var ids = new List<long>();
            foreach (long id in onlinePlayerIds)
                if (id > 0) ids.Add(id);
            if (ids.Count == 0) return stale;
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT u.pid FROM unnest(@ids) AS u(pid)
                        LEFT JOIN live_sessions s ON s.player_id = u.pid
                        WHERE s.player_id IS NULL
                           OR lower(coalesce(s.status, '')) = 'closed'
                           OR s.last_heartbeat < now() - make_interval(secs => @sec)", conn))
                    {
                        cmd.Parameters.Add(new NpgsqlParameter("ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint) { Value = ids.ToArray() });
                        cmd.Parameters.AddWithValue("@sec", Math.Max(5, timeoutSeconds));
                        using (var r = cmd.ExecuteReader())
                            while (r.Read()) stale.Add(r.GetInt64(0));
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] ListStaleHeartbeats: " + ex.Message, LoggerType.Error, ex);
            }
            return stale;
        }

        /// <summary>
        /// Conta nova em probation? Usa accounts.probation_until se setado;
        /// senão created_at + ProbationHours.
        /// </summary>
        public static bool IsInProbation(long playerId, int probationHours, out string reason)
        {
            reason = "";
            if (playerId <= 0 || probationHours <= 0) return false;
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT probation_until, created_at FROM accounts WHERE player_id = @p", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", playerId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) return false;
                            DateTime now = DateTimeUtil.Now();
                            if (!r.IsDBNull(0))
                            {
                                DateTime until = r.GetDateTime(0);
                                if (until > now)
                                {
                                    reason = "probation até " + until.ToString("yyyy-MM-dd HH:mm");
                                    return true;
                                }
                                return false;
                            }
                            if (!r.IsDBNull(1))
                            {
                                DateTime created = r.GetDateTime(1);
                                DateTime until = created.AddHours(probationHours);
                                if (until > now)
                                {
                                    reason = "conta nova (probation " + probationHours + "h, até " + until.ToString("yyyy-MM-dd HH:mm") + ")";
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] IsInProbation: " + ex.Message, LoggerType.Error, ex);
            }
            return false;
        }

        public static void EnsureProbationOnCreate(long playerId, int hours)
        {
            if (playerId <= 0 || hours <= 0) return;
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE accounts SET probation_until = COALESCE(probation_until, created_at + make_interval(hours => @h))
                         WHERE player_id = @p AND probation_until IS NULL", conn))
                    {
                        cmd.Parameters.AddWithValue("@h", hours);
                        cmd.Parameters.AddWithValue("@p", playerId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        public static int CountMatchesLastHour(long playerId)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT count(*) FROM security_events
                         WHERE player_id = @p AND category = 'MATCH_START'
                           AND ts > now() - interval '1 hour'", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", playerId);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch { return 0; }
        }

        public static void LogShop(long playerId, string op, int goodId, int itemId, string currency, int price, int before, int after, long target, string ip)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO shop_audit (player_id, op, good_id, item_id, currency, price, balance_before, balance_after, target_player, ip)
                        VALUES (@p, @op, @g, @i, @c, @price, @b, @a, @t, @ip)", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", playerId);
                        cmd.Parameters.AddWithValue("@op", Trunc(op, 16));
                        cmd.Parameters.AddWithValue("@g", goodId);
                        cmd.Parameters.AddWithValue("@i", itemId);
                        cmd.Parameters.AddWithValue("@c", Trunc(currency ?? "", 8));
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@b", before);
                        cmd.Parameters.AddWithValue("@a", after);
                        cmd.Parameters.AddWithValue("@t", target);
                        cmd.Parameters.AddWithValue("@ip", Trunc(ip ?? "", 64));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[Security] shop_audit: " + ex.Message, LoggerType.Error, ex);
            }
        }

        public static int CountShopOpsLastMinute(long playerId)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(
                        "SELECT count(*) FROM shop_audit WHERE player_id = @p AND ts > now() - interval '1 minute'", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", playerId);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch { return 0; }
        }

        public static string FlagsSnapshot24h(long playerId)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(@"
                        SELECT action, category, count(*) c
                          FROM security_events
                         WHERE player_id = @p AND ts > now() - interval '24 hours'
                         GROUP BY action, category
                         ORDER BY c DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", playerId);
                        var sb = new System.Text.StringBuilder();
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                if (sb.Length > 0) sb.Append("; ");
                                sb.Append(r.GetString(0)).Append('/').Append(r.GetString(1)).Append('=').Append(r.GetInt64(2));
                            }
                        }
                        return sb.Length == 0 ? "(nenhum evento em 24h)" : sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return "erro: " + ex.Message;
            }
        }

        private static string Trunc(string s, int max)
        {
            if (s == null) return "";
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
