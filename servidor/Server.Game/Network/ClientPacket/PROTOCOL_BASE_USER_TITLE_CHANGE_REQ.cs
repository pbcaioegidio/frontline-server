// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_USER_TITLE_CHANGE_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_USER_TITLE_CHANGE_REQ : GameClientPacket
    {
        private int Field0;
        private uint Field1;

        public override void Read() => this.Field0 = (int)this.ReadC();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || this.Field0 >= 45)
                    return;
                if (player.Title.OwnerId == 0L)
                {
                    DaoManagerSQL.CreatePlayerTitlesDB(player.PlayerId);
                    player.Title = new PlayerTitles()
                    {
                        OwnerId = player.PlayerId
                    };
                }
                TitleModel title = TitleSystemXML.GetTitle(this.Field0);
                if (title != null)
                {
                    TitleModel title1;
                    TitleModel title2;
                    TitleSystemXML.Get2Titles(title.Req1, title.Req2, out title1, out title2, false);
                    if ((title.Req1 == 0 || title1 != null) && (title.Req2 == 0 || title2 != null) && player.Rank >= title.Rank && player.Ribbon >= title.Ribbon && player.Medal >= title.Medal && player.MasterMedal >= title.MasterMedal && player.Ensign >= title.Ensign && !player.Title.Contains(title.Flag) && player.Title.Contains(title1.Flag) && player.Title.Contains(title2.Flag))
                    {
                        player.Ribbon -= title.Ribbon;
                        player.Medal -= title.Medal;
                        player.MasterMedal -= title.MasterMedal;
                        player.Ensign -= title.Ensign;
                        long flags = player.Title.Add(title.Flag);
                        DaoManagerSQL.UpdatePlayerTitlesFlags(player.PlayerId, flags);
                        List<ItemsModel> awards = TitleAwardXML.GetAwards(this.Field0);
                        if (awards.Count > 0)
                            this.Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, awards));
                        this.Client.SendPacket(new PROTOCOL_BASE_MEDAL_GET_INFO_ACK(player));
                        DaoManagerSQL.UpdatePlayerTitleRequi(player.PlayerId, player.Medal, player.Ensign, player.MasterMedal, player.Ribbon);
                        if (player.Title.Slots < title.Slot)
                        {
                            player.Title.Slots = title.Slot;
                            ComDiv.UpdateDB("player_titles", "slots", (object)player.Title.Slots, "owner_id", (object)player.PlayerId);
                        }
                    }
                    else
                        this.Field1 = 2147487875U;
                }
                else
                    this.Field1 = 2147487875U;
                this.Client.SendPacket(new PROTOCOL_BASE_USER_TITLE_CHANGE_ACK(this.Field1, player.Title.Slots));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_USER_TITLE_CHANGE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}