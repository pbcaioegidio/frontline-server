using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Plugin.Core.Managers;
using Plugin.Core.Utility;
using Server.Game.Rcon;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;

namespace Server.Game.Rcon.Admin
{
    public class RconBalanceAdmin : RconReceive
    {
        private string Token, Id, Type;
        private int Balance;
        private long gm_id;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                Id = PopString("username");
                Balance = PopInt("balance");
                Type = PopString("type");
                gm_id = PopLong("gm");
            }

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
            Balance = Balance < 0 ? 0 : Balance;
            if (gm_id == 1)
            {
                try
                {
                    switch (Type)
                    {
                        case "VIP Balance":
                            TopupVIP(Id, Balance);
                            break;

                        case "Cash Balance":
                            TopupCash(Id, Balance);
                            break;

                        default:
                            RconLogger.LogsPanel("No se pudo ejecutar el comando", 1);
                            break;
                    }
                }
                catch
                {
                    RconLogger.LogsPanel($"Error:", 1); return;
                }
            }
            else
            {
                RconLogger.LogsPanel("No se pudo ejecutar el comando", 1); return;
            }
        }

        private static void TopupVIP(string Id, int Balance)
        {
            long PlayerID = RconManager.GetPlayerId(Id);
            Account GM = AccountManager.GetAccount(PlayerID, 0);
            int vipBalance = RconManager.GetVipBalance(GM.PlayerId);
            if (GM != null)
            {
                int vip = vipBalance + Balance;
                ComDiv.UpdateDB("accounts", "vip_balance", vip, "player_id", GM.PlayerId);

                RconLogger.LogsPanel($"Se agregó correctamente el saldo VIP {Balance} al saldo VIP total {vip} de {GM.Nickname}", 0);
            }
        }

        private static void TopupCash(string Id, int Balance)
        {
            long PlayerID = RconManager.GetPlayerId(Id);
            Account GM = AccountManager.GetAccount(PlayerID, 0);
            int CashBalance = RconManager.GetCashBalance(GM.PlayerId);
            if (GM != null)
            {
                int cash = CashBalance + Balance;
                ComDiv.UpdateDB("accounts", "cash_balance", cash, "player_id", GM.PlayerId);

                RconLogger.LogsPanel($"Se agregó correctamente el saldo de efectivo {Balance} al saldo total de efectivo {GM.Nickname} {cash}", 0);
            }
        }
    }
}