using Plugin.Core;
using Plugin.Core.Managers;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Rcon.UserArea
{
    public class RconChangenickUser : RconReceive
    {
        private static Dictionary<long, HashSet<string>> processedCommands = new Dictionary<long, HashSet<string>>();
        private string Token, name, LauncherKey;
        private long player_id;
        private int cost = 50;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                name = PopString("new_nickname");
                LauncherKey = PopString("launcher_key");
                player_id = PopLong("player_id");
            }

            //int cost = 50;

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
            if (!RconManager.CheckLauncherKey(player_id, LauncherKey) || LauncherKey == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
            if (processedCommands.ContainsKey(player_id) && processedCommands[player_id].Contains(LauncherKey))
            {
                RconLogger.LogsPanel("This redeem with the same LauncherKey has already been processed for this user.", 1);
                return;
            }
            Account pL = AccountManager.GetAccount(player_id, 0);
            int Webcoin = RconManager.GetWebcoin(player_id);
            if (Webcoin < cost)
            {
                RconLogger.LogsPanel($"{pL.Nickname} dont Have Enough Webcoin {Webcoin}", 1);
                return;
            }
            else if (name.Length > ConfigLoader.MaxNickSize || name.Length < ConfigLoader.MinNickSize)
            {
                RconLogger.LogsPanel(Translation.GetLabel("FakeNickWrongLength"), 1);
                return;
            }
            else if (DaoManagerSQL.IsPlayerNameExist(name))
            {
                RconLogger.LogsPanel(Translation.GetLabel("FakeNickAlreadyExist"), 1);
                return;
            }
            else if ((name.Contains("GM") || name.Contains("DEV") || name.Contains("MOD") || name.Contains("OWN")) && pL.PlayerId > 10)
            {
                RconLogger.LogsPanel("Tidak Dapat Menggunakan Nick ini.", 1);
                return;
            }
            else if (ComDiv.UpdateDB("accounts", "nickname", name, "player_id", pL.PlayerId))
            {
                pL.Nickname = name;
                pL.SendPacket(new PROTOCOL_AUTH_CHANGE_NICKNAME_ACK(name));

                int WebcoinPlayer = RconManager.GetWebcoin(pL.PlayerId);
                int webcoin = WebcoinPlayer - cost;
                ComDiv.UpdateDB("accounts", "progressive_coin", webcoin, "player_id", pL.PlayerId);

                if (pL.Room != null)
                {
                    using (PROTOCOL_ROOM_GET_NICKNAME_ACK packet = new PROTOCOL_ROOM_GET_NICKNAME_ACK(pL.SlotId, pL.Nickname, pL.NickColor))
                    {
                        pL.Room.SendPacketToPlayers(packet);
                    }
                }
                if (pL.ClanId > 0)
                {
                    using (PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK packet = new PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(pL))
                    {
                        ClanManager.SendPacket(packet, pL.ClanId, -1, true, true);
                    }
                }
                AllUtils.SyncPlayerToFriends(pL, true);
                DaoManagerSQL.CreatePlayerNickHistory(pL.PlayerId, pL.Nickname, name, "Name changed (Webcoin)");

                Randomwinload();
                ComDiv.UpdateDB("accounts", "token", finalString, "player_id", pL.PlayerId);

                if (!processedCommands.ContainsKey(player_id))
                {
                    processedCommands[player_id] = new HashSet<string>();
                }
                processedCommands[player_id].Add(LauncherKey);

                RconLogger.LogsPanel(Translation.GetLabel("FakeNickSuccess", name), 0);
                return;
            }
            RconLogger.LogsPanel(Translation.GetLabel("ChangePlyRankFailPlayer"), 1);
            return;
        }

        private static string finalString;

        private static void Randomwinload()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[32];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            finalString = new string(stringChars);
        }
    }
}