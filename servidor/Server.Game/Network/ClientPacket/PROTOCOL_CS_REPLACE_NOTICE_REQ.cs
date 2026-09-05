// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_REPLACE_NOTICE_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_REPLACE_NOTICE_REQ : GameClientPacket
    {
        private string Field0;
        private EventErrorEnum Field1;

        public override void Read() => this.Field0 = this.ReadU((int)this.ReadC() * 2);

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ClanModel clan = ClanManager.GetClan(player.ClanId);
                if (clan.Id > 0 && clan.News != this.Field0 && (clan.OwnerId == this.Client.PlayerId || player.ClanAccess >= 1 && player.ClanAccess <= 2))
                {
                    if (!ComDiv.UpdateDB("system_clan", "news", (object)this.Field0, "id", (object)clan.Id))
                        this.Field1 = EventErrorEnum.CLAN_REPLACE_NOTICE_ERROR;
                    else
                        clan.News = this.Field0;
                }
                else
                    this.Field1 = EventErrorEnum.CLAN_FAILED_CHANGE_OPTION;
                this.Client.SendPacket(new PROTOCOL_CS_REPLACE_NOTICE_ACK(this.Field1));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}