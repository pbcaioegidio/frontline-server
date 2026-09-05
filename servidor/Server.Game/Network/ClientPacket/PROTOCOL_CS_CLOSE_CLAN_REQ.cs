// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_CLOSE_CLAN_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_CLOSE_CLAN_REQ : GameClientPacket
    {
        private uint Field0;

        public override void Read()
        {
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ClanModel clan = ClanManager.GetClan(player.ClanId);
                if (clan.Id > 0 && clan.OwnerId == this.Client.PlayerId && ComDiv.DeleteDB("system_clan", "id", (object)clan.Id))
                {
                    if (ComDiv.UpdateDB("accounts", "player_id", (object)player.PlayerId, new string[2]
                    {
          "clan_id",
          "clan_access"
                    }, (object)0, (object)0))
                    {
                        if (ComDiv.UpdateDB("player_stat_clans", "owner_id", (object)player.PlayerId, new string[2]
                        {
            "clan_matches",
            "clan_match_wins"
                        }, (object)0, (object)0) && ClanManager.RemoveClan(clan))
                        {
                            player.ClanId = 0;
                            player.ClanAccess = 0;
                            SendClanInfo.Load(clan, 1, player.ServerId);
                            goto label_6;
                        }
                    }
                }
                this.Field0 = 2147487850U;
            label_6:
                this.Client.SendPacket(new PROTOCOL_CS_CLOSE_CLAN_ACK(this.Field0));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_CLOSE_CLAN_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}