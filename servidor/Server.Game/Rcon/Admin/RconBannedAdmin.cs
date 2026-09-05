using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Core.Enums;
using Plugin.Core;
using Plugin.Core.Utility;
using Server.Game.Rcon;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;

namespace Server.Game.Rcon.Admin
{
    public class RconBannedAdmin : RconReceive
    {
        private string Token, Option;
        private long Id, gm_id;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                Id = PopLong("player_id");
                Option = PopString("reason");
                gm_id = PopLong("gm");
            }

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
            Account GM = AccountManager.GetAccount(gm_id, 0);
            Account victim = AccountManager.GetAccount(Id, 0);
            if (victim == null)
            {
                RconLogger.LogsPanel($"Player {Id} not found", 1);
                return;
            }
            string gmName = GM != null ? GM.Username : ("rcon:" + gm_id);
            if (Option == "Banned Permanent")
            {
                if (victim.Access == AccessLevel.BANNED)
                {
                    RconLogger.Logs($"This PlayerID {victim.NameCard} Already Banned Permanently");
                }
                // FL GUARD: hard ban (conta + MAC + IP + fingerprint + hardware)
                Plugin.Core.Security.SecurityDao.HardBanResult hard = Plugin.Core.Security.SecurityDao.ApplyHardBan(
                    victim.PlayerId, "RCON: Banned Permanent", TimeSpan.Zero, "rcon:" + gmName,
                    Plugin.Core.Security.SecurityDao.SourceRcon, gm_id, "GM_BAN",
                    "{\"gm\":\"" + gmName + "\",\"gm_id\":" + gm_id + ",\"duration\":\"permanent\",\"via\":\"rcon\"}",
                    ConfigLoader.HardBanSubnet);
                if (hard.BanId > 0)
                {
                    ComDiv.UpdateDB("accounts", "ban_object_id", hard.BanId, "player_id", victim.PlayerId);
                    victim.BanObjectId = hard.BanId;
                    using (PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK packet = new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("PlayerBannedWarning", victim.Nickname)))
                    {
                        GameXender.BroadcastToAll(packet);
                    }
                    victim.Access = AccessLevel.BANNED;
                    victim.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                    victim.Close(1000, true);

                    RconLogger.LogsPanel($"[!] {victim.Nickname} has been successfully banned ({hard.Identifiers} identifiers, {hard.Devices} devices). [{gmName}]", 0);
                }
                else
                {
                    RconLogger.Logs(Translation.GetLabel("PlayerBanFail"));
                }
            }
            else if (Option == "Un-Banned Permanent")
            {
                if (victim.Access != AccessLevel.BANNED)
                {
                    RconLogger.LogsPanel($"Player {victim.Nickname} Status is not Banned", 1);
                    return;
                }
                else
                {
                    // FL GUARD: libera conta, identificadores e HWIDs
                    if (Plugin.Core.Security.SecurityDao.LiftHardBan(victim.PlayerId, "rcon:" + gmName, gm_id))
                    {
                        victim.Access = AccessLevel.NORMAL;
                        victim.BanObjectId = 0;
                        RconLogger.LogsPanel($"[!] {victim.Nickname} has been successfully un-banned.", 0);
                    }
                    else
                    {
                        RconLogger.LogsPanel($"Gagal Proses Un-Banned", 1);
                    }
                }
            }
            else
            {
                RconLogger.LogsPanel($"Invalid Option", 1);
                return;
            }
        }
    }
}