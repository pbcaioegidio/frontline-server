using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Core;
using Server.Game.Rcon;
using Server.Game.Data.Models;
using Server.Game.Data.Managers;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;
using Plugin.Core.XML;
using System.Numerics;
using Plugin.Core.Managers;

namespace Server.Game.Rcon.Admin
{
    public class RconRankAdmin : RconReceive
    {
        private string Token;
        private long gm_id, Id;
        private int rank;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                Id = PopLong("player_id");
                gm_id = PopLong("gm");
                rank = PopInt("rank");
            }

            int cost = 50000;
            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }
            Account GM = AccountManager.GetAccount(gm_id, 0);
            Account pE = AccountManager.GetAccount(Id, 0);
            int cashBalance = RconManager.GetCashBalance(gm_id);
            if (cashBalance < cost)
            {
                RconLogger.LogsPanel($"{GM.Nickname} dont Have Enough Cash Balance {cashBalance}", 1);
                return;
            }
            if (!RankDomain.IsValid(rank) || Id <= 0)
            {
                RconLogger.LogsPanel(Translation.GetLabel("ChangePlyRankWrongValue"), 1);
                return;
            }
            else if (rank >= 52 && rank <= 55 && pE.Access < Plugin.Core.Enums.AccessLevel.VIP)
            {
                RconLogger.LogsPanel($"Rank 52 - 55 tidak Bisa dibunakan", 1);
                return;
            }
            else if (pE.Rank == rank)
            {
                RconLogger.LogsPanel($"Rank {pE.Nickname} Sama dengan Rank {rank}", 1);
                return;
            }
            else
            {
                if (pE != null)
                {
                    if (ComDiv.UpdateDB("accounts", "rank", rank, "player_id", pE.PlayerId))
                    {
                        RankModel model = PlayerRankXML.GetRank(rank);
                        int previousRank = pE.Rank;
                        pE.Rank = rank;
                        //pE.Exp = model != null ? model.OnAllExp : pE.Exp;
                        //ComDiv.UpdateDB("accounts", "experience", pE.Exp, "player_id", pE.PlayerId);
                        SendItemInfo.LoadGoldCash(pE);
                        pE.SendPacket(new PROTOCOL_BASE_RANK_UP_ACK(pE.Rank, previousRank, model != null ? model.OnGoldUp : 0), false);
                        pE.Room?.UpdateSlotsInfo();

                        int cost1 = cashBalance - cost;
                        ComDiv.UpdateDB("accounts", "cash_balance", cost1, "player_id", GM.PlayerId);

                        RconLogger.LogsPanel($"[!] Player {pE.Nickname}, New Rank: {rank}", 0);
                    }
                    else
                    {
                        RconLogger.LogsPanel(Translation.GetLabel("ChangePlyRankFailUnk"), 1);
                        return;
                    }
                }
                else
                {
                    RconLogger.LogsPanel(Translation.GetLabel("ChangePlyRankFailPlayer"), 1);
                    return;
                }
            }
        }
    }
}