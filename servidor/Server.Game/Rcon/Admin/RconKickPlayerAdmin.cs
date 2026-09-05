using Plugin.Core;
using Plugin.Core.Security;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Rcon.Admin
{
    public class RconKickPlayerAdmin : RconReceive
    {
        private string Token;
        private long Id, gm_id;
        private string Reason;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                Id = PopLong("player_id");
                gm_id = PopLong("gm");
                Reason = PopString("reason");
            }

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }

            Account victim = AccountManager.GetAccount(Id, 0);
            if (victim == null)
            {
                RconLogger.LogsPanel($"Player {Id} not found", 1);
                return;
            }

            string reason = string.IsNullOrWhiteSpace(Reason) ? "Kick via RCON" : Reason;
            ComDiv.UpdateDB("accounts", "online", false, "player_id", Id);
            AllUtils.KickPlayer(victim, reason, "RCON_KICK", SecurityDao.SourceRcon, gm_id, "", requestClip: false);

            RconLogger.LogsPanel($"[!] {victim.Nickname} has been successfully kicked from server. [{gm_id}]", 0);
        }
    }

    /// <summary>
    /// RCON: pede screenshot ou clip (ring buffer 20s) do jogador online.
    /// JSON: { "command":"SCREENSHOT"|"CLIP", "password":"...", "token":"...", "player_id":N, "gm":N, "reason":"..." }
    /// </summary>
    public class RconCaptureAdmin : RconReceive
    {
        private string Token, Reason, Kind;
        private long Id, gm_id;

        public RconCaptureAdmin(string kind)
        {
            Kind = kind;
        }

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                Id = PopLong("player_id");
                gm_id = PopLong("gm");
                Reason = PopString("reason");
            }

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }

            Account victim = AccountManager.GetAccount(Id, 0);
            if (victim == null)
            {
                RconLogger.LogsPanel($"Player {Id} not found", 1);
                return;
            }

            Account gm = AccountManager.GetAccount(gm_id, 0);
            string by = gm != null ? ("rcon:" + gm.Username) : ("rcon:" + gm_id);
            string reason = string.IsNullOrWhiteSpace(Reason) ? ("GM pediu " + Kind + " ao vivo") : Reason;

            long reqId = SecurityDao.RequestCapture(Id, Kind, reason, by, gm_id, SecurityDao.SourceRcon);
            if (reqId <= 0)
            {
                RconLogger.LogsPanel($"Falha ao enfileirar {Kind} de {victim.Nickname}", 1);
                return;
            }

            RconLogger.LogsPanel($"[FL GUARD] {Kind} #{reqId} enfileirado para {victim.Nickname} (player_id={Id}). O launcher entrega no próximo heartbeat.", 0);
        }
    }

    /// <summary>
    /// RCON: lista últimos eventos de segurança de um jogador.
    /// JSON: { "command":"EVENTS", "password":"...", "token":"...", "player_id":N }
    /// </summary>
    public class RconEventsAdmin : RconReceive
    {
        private string Token;
        private long Id;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                Id = PopLong("player_id");
            }

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }

            try
            {
                string snap = SecurityDao.FlagsSnapshot24h(Id);
                RconLogger.LogsPanel($"[FL GUARD] flags_24h player={Id}: {snap}", 0);

                using (var conn = Plugin.Core.SQL.ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(@"
                        SELECT id, ts, action, category, client_code, reason, severity
                          FROM security_events
                         WHERE player_id = @p
                         ORDER BY id DESC LIMIT 20", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", Id);
                        using (var r = cmd.ExecuteReader())
                        {
                            int n = 0;
                            while (r.Read())
                            {
                                n++;
                                long eid = r.GetInt64(0);
                                DateTime ts = r.GetDateTime(1);
                                string action = r.GetString(2);
                                string cat = r.GetString(3);
                                string code = r.IsDBNull(4) ? "" : r.GetString(4);
                                string reason = r.IsDBNull(5) ? "" : r.GetString(5);
                                short sev = r.GetInt16(6);
                                RconLogger.LogsPanel($"#{eid} {ts:HH:mm:ss} [{action}/{cat}] {code} sev={sev} — {reason}", 0);
                            }
                            if (n == 0)
                                RconLogger.LogsPanel($"Nenhum security_event para player_id={Id}", 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RconLogger.LogsPanel("EVENTS failed: " + ex.Message, 1);
            }
        }
    }

    /// <summary>
    /// RCON WHY: detalha o último evento / ban da conta.
    /// JSON: { "command":"WHY", "password":"...", "token":"...", "player_id":N }
    /// </summary>
    public class RconWhyAdmin : RconReceive
    {
        private string Token;
        private long Id;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                Id = PopLong("player_id");
            }
            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }

            try
            {
                Account victim = AccountManager.GetAccount(Id, 0);
                string nick = victim != null ? victim.Nickname : "?";
                RconLogger.LogsPanel($"[WHY] player={Id} nick={nick} access={(victim != null ? (int)victim.Access : -999)}", 0);
                RconLogger.LogsPanel($"[WHY] flags_24h: {SecurityDao.FlagsSnapshot24h(Id)}", 0);

                using (var conn = Plugin.Core.SQL.ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = new Npgsql.NpgsqlCommand(@"
                        SELECT id, ts, action, category, client_code, reason, evidence_json::text, severity
                          FROM security_events WHERE player_id = @p
                         ORDER BY id DESC LIMIT 5", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", Id);
                        using (var r = cmd.ExecuteReader())
                        {
                            int n = 0;
                            while (r.Read())
                            {
                                n++;
                                RconLogger.LogsPanel(
                                    $"[WHY #{r.GetInt64(0)}] {r.GetDateTime(1):yyyy-MM-dd HH:mm} {r.GetString(2)}/{r.GetString(3)} " +
                                    $"{(r.IsDBNull(4) ? "" : r.GetString(4))} sev={r.GetInt16(7)} — {r.GetString(5)} | {r.GetString(6)}", 0);
                            }
                            if (n == 0) RconLogger.LogsPanel("[WHY] sem security_events", 1);
                        }
                    }
                    using (var cmd = new Npgsql.NpgsqlCommand(@"
                        SELECT kind, value, reason, expire_at FROM ban_identifiers
                         WHERE player_id_origem = @p AND (expire_at IS NULL OR expire_at > now())
                         ORDER BY id DESC LIMIT 15", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", Id);
                        using (var r = cmd.ExecuteReader())
                        {
                            int n = 0;
                            while (r.Read())
                            {
                                n++;
                                string exp = r.IsDBNull(3) ? "perm" : r.GetDateTime(3).ToString("yyyy-MM-dd");
                                RconLogger.LogsPanel($"[WHY ban_id] {r.GetString(0)}={r.GetString(1).Substring(0, Math.Min(12, r.GetString(1).Length))}… until={exp} — {r.GetString(2)}", 0);
                            }
                            if (n == 0) RconLogger.LogsPanel("[WHY] sem ban_identifiers ativos", 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RconLogger.LogsPanel("WHY failed: " + ex.Message, 1);
            }
        }
    }
}
