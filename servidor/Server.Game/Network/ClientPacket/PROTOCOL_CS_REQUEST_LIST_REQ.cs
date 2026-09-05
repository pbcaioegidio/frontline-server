// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_REQUEST_LIST_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_REQUEST_LIST_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read() => this.Field0 = (int)this.ReadC();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                if (player.ClanId != 0)
                {
                    List<ClanInvite> clanRequestList = DaoManagerSQL.GetClanRequestList(player.ClanId);
                    using (SyncServerPacket A_2_1 = new SyncServerPacket())
                    {
                        int A_2_2 = 0;
                        for (int index = this.Field0 * (this.Field0 != 0 ? 14 : 13); index < clanRequestList.Count; ++index)
                        {
                            this.Method0(clanRequestList[index], A_2_1);
                            if (++A_2_2 == 13)
                                break;
                        }
                        this.Client.SendPacket(new PROTOCOL_CS_REQUEST_LIST_ACK(0, A_2_2, this.Field0, A_2_1.ToArray()));
                    }
                }
                else
                    this.Client.SendPacket(new PROTOCOL_CS_REQUEST_LIST_ACK(-1));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_REQUEST_LIST_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        
        private void Method0(ClanInvite A_1, SyncServerPacket A_2)
        {
            A_2.WriteQ(A_1.PlayerId);
            Account account = AccountManager.GetAccount(A_1.PlayerId, 31 /*0x1F*/);
            if (account != null)
            {
                A_2.WriteU(account.Nickname, 66);
                A_2.WriteC((byte)account.Rank);
                A_2.WriteC((byte)account.NickColor);
                A_2.WriteD(A_1.InviteDate);
                A_2.WriteD(account.Statistic.Basic.KillsCount);
                A_2.WriteD(account.Statistic.Basic.DeathsCount);
                A_2.WriteD(account.Statistic.Basic.Matches);
                A_2.WriteD(account.Statistic.Basic.MatchWins);
                A_2.WriteD(account.Statistic.Basic.MatchLoses);
                A_2.WriteN(A_1.Text, A_1.Text.Length + 2, "UTF-16LE");
            }
            A_2.WriteD(A_1.InviteDate);
        }
    }
}