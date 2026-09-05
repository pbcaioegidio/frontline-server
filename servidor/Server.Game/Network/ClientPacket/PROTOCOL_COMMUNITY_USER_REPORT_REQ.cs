// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_COMMUNITY_USER_REPORT_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_COMMUNITY_USER_REPORT_REQ : GameClientPacket
    {
        private uint Field0;
        private ReportType Field1;
        private string Field2;
        private string Field3;

        public override void Read()
        {
            this.Field3 = this.ReadU((int)this.ReadC() * 2);
            this.Field1 = (ReportType)this.ReadC();
            this.Field2 = this.ReadU((int)this.ReadC() * 2);
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                player.FindPlayer = this.Field2;
                Account account = AccountManager.GetAccount(player.FindPlayer, 1, 31 /*0x1F*/);
                if (account != null && player.Nickname.Length > 0 && player.Nickname != this.Field2)
                {
                    PlayerReport report = player.Report;
                    if (report != null && report.TicketCount != 0)
                    {
                        if (player.Rank < 7)
                            this.Field0 = 2147487977U;
                        else if (DaoManagerSQL.CreatePlayerReportHistory(account.PlayerId, player.PlayerId, account.Nickname, player.Nickname, this.Field1, this.Field3) && ComDiv.UpdateDB("player_reports", "ticket_count", (object)--report.TicketCount, "owner_id", (object)player.PlayerId))
                            ComDiv.UpdateDB("player_reports", "reported_count", (object)++account.Report.ReportedCount, "owner_id", (object)account.PlayerId);
                    }
                    else
                    {
                        this.Client.SendPacket(new PROTOCOL_COMMUNITY_USER_REPORT_ACK(2147487976U));
                        return;
                    }
                }
                this.Client.SendPacket(new PROTOCOL_COMMUNITY_USER_REPORT_ACK(this.Field0));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}