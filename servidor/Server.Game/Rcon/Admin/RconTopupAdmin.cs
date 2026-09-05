using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Core.Managers;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Rcon;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;

namespace Server.Game.Rcon.Admin
{
    public class RconTopupAdmin : RconReceive
    {
        private string Token;
        private long username, gm_id;
        private string topup;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                username = PopLong("player_id");
                topup = PopString("topup");
                gm_id = PopLong("gm");
            }

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
            int Money = 0;
            int Webcoin = 0;
            try
            {
                if (topup == "300K Cash + 30K Webcoin")
                {
                    Money = 300000;
                    Webcoin = 30000;
                }
                else if (topup == "500K Cash + 50K Webcoin")
                {
                    Money = 500000;
                    Webcoin = 50000;
                }
                else if (topup == "1000K Cash + 100K Webcoin")
                {
                    Money = 1000000;
                    Webcoin = 100000;
                }
                //////////////////////////////////////////////////////
                else if (topup == "300 Cash Reward")
                {
                    Money = 300;
                    Webcoin = 0;
                }
                else if (topup == "500 Cash Reward")
                {
                    Money = 500;
                    Webcoin = 0;
                }
                else if (topup == "1000 Cash Reward")
                {
                    Money = 1000;
                    Webcoin = 0;
                }
                else if (topup == "1500 Cash Reward")
                {
                    Money = 1500;
                    Webcoin = 0;
                }
                else if (topup == "820 Cash + 150 Webcoin")
                {
                    Money = 820;
                    Webcoin = 150;
                }
                else if (topup == "1230 Cash + 250 Webcoin")
                {
                    Money = 1230;
                    Webcoin = 250;
                }
                else if (topup == "2460 Cash + 350 Webcoin")
                {
                    Money = 2460;
                    Webcoin = 350;
                }
                else if (topup == "3600 Cash + 550 Webcoin")
                {
                    Money = 3600;
                    Webcoin = 550;
                }
                else if (topup == "5840 Cash + 700 Webcoin")
                {
                    Money = 5840;
                    Webcoin = 700;
                }
                else if (topup == "7720 Cash + 900 Webcoin")
                {
                    Money = 7720;
                    Webcoin = 900;
                }
                else if (topup == "12650 Cash + 1200 Webcoin")
                {
                    Money = 12650;
                    Webcoin = 1200;
                }
                else if (topup == "17860 Cash + 1600 Webcoin")
                {
                    Money = 17860;
                    Webcoin = 1600;
                }
                else
                {
                    RconLogger.LogsPanel("Topup tidak valid.", 1);
                    return;
                }

                Account Player = AccountManager.GetAccount(username, 0);
                Account GM = AccountManager.GetAccount(gm_id, 0);
                int cashBalance = RconManager.GetCashBalance(GM.PlayerId);
                if (cashBalance < Money)
                {
                    RconLogger.LogsPanel($"{GM.Nickname} dont Have Enough VIP Balance {cashBalance}", 1);
                    return;
                }
                if (Player != null)
                {
                    if (DaoManagerSQL.UpdateAccountCash(Player.PlayerId, Player.Cash + Money))
                    {
                        Player.Cash += Money;

                        int WebcoinPlayer = RconManager.GetWebcoin(Player.PlayerId);
                        int addWebcoin = WebcoinPlayer + Webcoin;
                        ComDiv.UpdateDB("accounts", "progressive_coin", addWebcoin, "player_id", Player.PlayerId);

                        int cost = cashBalance - Money;
                        ComDiv.UpdateDB("accounts", "cash_balance", cost, "player_id", GM.PlayerId);

                        Player.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0, Player));
                        SendItemInfo.LoadGoldCash(Player);

                        RconLogger.LogsPanel("Player " + Player.Nickname + " received [" + topup + "]", 0);
                    }
                }
                else
                {
                    RconLogger.LogsPanel($"Gagal Set VIP: Tidak ditemukan pemain dengan username {username}", 1);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                RconLogger.LogsPanel($"TEST ", 1);
                return;
            }
        }
    }
}