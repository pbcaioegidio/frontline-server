// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_REPLACE_MANAGEMENT_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_REPLACE_MANAGEMENT_REQ : GameClientPacket
    {
        private int Field0;
        private int Field1;
        private int Field2;
        private int Field3;
        private JoinClanType Field4;
        private uint Field5;

        public override void Read()
        {
            this.Field3 = (int)this.ReadC();
            this.Field0 = (int)this.ReadC();
            this.Field2 = (int)this.ReadC();
            this.Field1 = (int)this.ReadC();
            this.Field4 = (JoinClanType)this.ReadC();
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ClanModel clan = ClanManager.GetClan(player.ClanId);
                if (clan.Id > 0 && clan.OwnerId == player.PlayerId)
                {
                    if (clan.Authority != this.Field3)
                        clan.Authority = this.Field3;
                    if (clan.RankLimit != this.Field0)
                        clan.RankLimit = this.Field0;
                    if (clan.MinAgeLimit != this.Field1)
                        clan.MinAgeLimit = this.Field1;
                    if (clan.MaxAgeLimit != this.Field2)
                        clan.MaxAgeLimit = this.Field2;
                    if (clan.JoinType != this.Field4)
                        clan.JoinType = this.Field4;
                    DaoManagerSQL.UpdateClanInfo(clan.Id, clan.Authority, clan.RankLimit, clan.MinAgeLimit, clan.MaxAgeLimit, (int)clan.JoinType);
                }
                else
                    this.Field5 = 2147483648U /*0x80000000*/;
                this.Client.SendPacket(new PROTOCOL_CS_REPLACE_MANAGEMENT_ACK(this.Field5));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}