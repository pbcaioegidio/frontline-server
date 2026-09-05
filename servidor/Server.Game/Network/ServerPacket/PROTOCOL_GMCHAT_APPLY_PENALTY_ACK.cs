using System;
using System.Numerics;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Security;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
//using PointBlank.Game.Rcon;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_GMCHAT_APPLY_PENALTY_ACK : GameServerPacket
    {
        private readonly Account Player;
        private readonly uint Error;
        private readonly int Type;
        private readonly int BanTime;
        private readonly string Reason;
        /// <summary>GM que aplicou a penalidade (auditoria em security_events).</summary>
        public long GmId { get; set; }
        public string GmNick { get; set; } = "";

        public PROTOCOL_GMCHAT_APPLY_PENALTY_ACK(uint Error, Account Player, int Type, int BanTime, string Reason)
        {
            this.Error = Error;
            this.Player = Player;
            this.Type = Type;
            this.BanTime = BanTime;
            this.Reason = Reason;
        }
        public override void Write()
        {
            WriteH(6664);
            WriteH(0);
            WriteD(Error);
            if (Error == 0)
            {
                if (Player != null)
                {
                    CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Type={Type}, BanTime={BanTime}, Target={Player.PlayerId}", LoggerType.Info);
                    switch (Type)
                    {
                        case 0: // Mute (Client might send 0)
                        case 1: // Mute
                            if (BanTime <= 0)
                            {
                                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Processing UN-MUTE for {Player.PlayerId}. BanObjectId={Player.BanObjectId}", LoggerType.Info);
                                // Set expiry in the past to release immediately
                                if (Player.BanObjectId > 0)
                                {
                                    ComDiv.UpdateDB("base_ban_history", "expire_date", DateTime.Now.AddDays(-1), "object_id", Player.BanObjectId);
                                }
                                else
                                {
                                    // If we don't know the exact BanObjectId, we can't easily expire it from here
                                    // But we should at least clear the account link
                                    CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Target {Player.PlayerId} has no active BanObjectId in memory, but clearing link anyway.", LoggerType.Warning);
                                }
                                
                                ComDiv.UpdateDB("accounts", "ban_object_id", 0, "player_id", Player.PlayerId);
                                Player.BanObjectId = 0;
                                
                                string Message = $"Player '{Player.Nickname}' has been un-muted by GM.";
                                using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Message))
                                {
                                    GameXender.BroadcastToAll(Packet);
                                }
                            }
                            else
                            {
                                int Seconds = BanTime;
                                int DisplayMinutes = Seconds / 60;
                                DateTime expiry = DateTimeUtil.Now().AddSeconds(Seconds);
                                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Mute Duration={Seconds} Sec ({DisplayMinutes} Min), Expiry={expiry.ToString("yyyy-MM-dd HH:mm:ss")}", LoggerType.Info);
                                BanHistory Ban = DaoManagerSQL.SaveBanHistory(Player.PlayerId, "MUTE", $"{Player.PlayerId}", expiry, Reason);
                                if (Ban != null)
                                {
                                    string Message = $"Player '{Player.Nickname}' has been muted for {DisplayMinutes} Minutes!";
                                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Message))
                                    {
                                        GameXender.BroadcastToAll(Packet);
                                    }
                                    ComDiv.UpdateDB("accounts", "ban_object_id", Ban.ObjectId, "player_id", Player.PlayerId);
                                    Player.BanObjectId = Ban.ObjectId;
                                    CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Mute successful for {Player.PlayerId}. ObjectId={Ban.ObjectId}", LoggerType.Info);
                                    // Removed kick for mute
                                }
                                else
                                {
                                    CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Failed to save mute history for {Player.PlayerId}.", LoggerType.Error);
                                }
                            }
                            break;
                        case 11: // Some clients send 1 for Ban if 0 is Mute? No, let's stick to observed patterns. 
                                 // Actually, let's just make case 1 both if it's ambiguous, but that's risky.
                                 // Usually Tab 1 = Type 1, Tab 2 = Type 2.
                                 // If Tab 1 = Type 0, Tab 2 = Type 1.
                        case 2: // Ban
                            if (BanTime == -1)
                            {
                                // FL GUARD: hard ban = conta + MAC + IP + fingerprint + todos os componentes de hardware
                                SecurityDao.RequestCapture(Player.PlayerId, "clip", "GM ban permanente: " + Reason, "gm:" + GmNick, GmId, SecurityDao.SourceGm);
                                SecurityDao.HardBanResult hard = SecurityDao.ApplyHardBan(Player.PlayerId, Reason, TimeSpan.Zero,
                                    "gm:" + GmNick, SecurityDao.SourceGm, GmId, "GM_BAN",
                                    "{\"gm\":\"" + GmNick + "\",\"gm_id\":" + GmId + ",\"duration\":\"permanent\"}", ConfigLoader.HardBanSubnet);
                                if (hard.BanId > 0)
                                {
                                    ComDiv.UpdateDB("accounts", "ban_object_id", hard.BanId, "player_id", Player.PlayerId);
                                    Player.BanObjectId = hard.BanId;

                                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("PlayerBannedWarning", Player.Nickname)))
                                    {
                                        GameXender.BroadcastToAll(packet);
                                    }
                                    Player.Access = AccessLevel.BANNED;
                                    if (Player.Connection != null)
                                    {
                                        Player.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                                        Player.Close(1000, true);
                                    }
                                    else
                                    {
                                        CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Target {Player.PlayerId} (Permanent) has no active connection to kick.", LoggerType.Warning);
                                    }
                                }
                                else
                                {
                                    CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Failed to update accounts table for permanent ban of {Player.PlayerId}.", LoggerType.Error);
                                }
                            }
                            else if (BanTime == 0)
                            {
                                SecurityDao.LogEvent(SecurityDao.SourceGm, Player.PlayerId, Player.Username, Player.Nickname, "kick", "GM_KICK",
                                    string.IsNullOrWhiteSpace(Reason) ? "Kick pelo GM" : Reason,
                                    "{\"gm\":\"" + GmNick + "\",\"gm_id\":" + GmId + "}", 2, "", 0, false, GmId);
                                if (Player.Connection != null)
                                {
                                    Player.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                                    Player.Close(1000, true);
                                }
                                else
                                {
                                    CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Target {Player.PlayerId} (Kick) has no active connection.", LoggerType.Warning);
                                }
                            }
                            else
                            {
                                // Fix: If BanTime is less than a day, use minutes
                                DateTime expiry = BanTime >= 1440 ? DateTimeUtil.Now().AddDays(BanTime / 1440) : DateTimeUtil.Now().AddMinutes(BanTime);
                                CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Ban Duration={BanTime}, Expiry={expiry.ToString("yyyy-MM-dd HH:mm:ss")}", LoggerType.Info);
                                // FL GUARD: ban temporário também bloqueia MAC/IP/hardware pelo mesmo período
                                string durationStr = BanTime >= 1440 ? $"{BanTime / 1440} Days" : $"{BanTime} Minutes";
                                SecurityDao.HardBanResult hard = SecurityDao.ApplyHardBan(Player.PlayerId, Reason, expiry - DateTimeUtil.Now(),
                                    "gm:" + GmNick, SecurityDao.SourceGm, GmId, "GM_BAN",
                                    "{\"gm\":\"" + GmNick + "\",\"gm_id\":" + GmId + ",\"duration\":\"" + durationStr + "\"}", ConfigLoader.HardBanSubnet);
                                if (hard.BanId > 0)
                                {
                                    string Message = $"Player '{Player.Nickname}' has been banned for {durationStr}!";
                                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK Packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Message))
                                    {
                                        GameXender.BroadcastToAll(Packet);
                                    }
                                    ComDiv.UpdateDB("accounts", "ban_object_id", hard.BanId, "player_id", Player.PlayerId);
                                    Player.BanObjectId = hard.BanId;
                                    CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Ban successful for {Player.PlayerId}. ObjectId={hard.BanId}", LoggerType.Info);
                                    if (Player.Connection != null)
                                    {
                                        CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Kicking online player {Player.PlayerId}", LoggerType.Info);
                                        Player.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                                        Player.Close(1000, true);
                                    }
                                    else
                                    {
                                        CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Target {Player.PlayerId} (Duration) has no active connection to kick.", LoggerType.Warning);
                                    }
                                }
                                else
                                {
                                    CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Failed to save ban history for {Player.PlayerId}.", LoggerType.Error);
                                }
                            }
                            break;
                        default:
                            CLogger.Print($"PROTOCOL_GMCHAT_APPLY_PENALTY_ACK: Unhandled Type={Type}.", LoggerType.Warning);
                            break;
                    }
                }
            }
        }
    }
}
