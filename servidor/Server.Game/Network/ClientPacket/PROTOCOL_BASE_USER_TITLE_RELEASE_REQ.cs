// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_USER_TITLE_RELEASE_REQ
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
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_USER_TITLE_RELEASE_REQ : GameClientPacket
    {
        private int Field0;
        private int Field1;

        public override void Read()
        {
            this.Field0 = (int)this.ReadC();
            this.Field1 = (int)this.ReadC();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || this.Field0 >= 3 || player.Title == null)
                    return;
                PlayerTitles title = player.Title;
                int equip = title.GetEquip(this.Field0);
                if (this.Field0 < 3 && this.Field1 < 45 && equip == this.Field1 && DaoManagerSQL.UpdateEquipedPlayerTitle(title.OwnerId, this.Field0, 0))
                {
                    title.SetEquip(this.Field0, 0);
                    if (TitleAwardXML.Contains(equip, player.Equipment.BeretItem) && ComDiv.UpdateDB("player_equipments", "beret_item_part", (object)0, "owner_id", (object)player.PlayerId))
                    {
                        player.Equipment.BeretItem = 0;
                        RoomModel room = player.Room;
                        if (room != null)
                            AllUtils.UpdateSlotEquips(player, room);
                    }
                    this.Client.SendPacket(new PROTOCOL_BASE_USER_TITLE_RELEASE_ACK(0U));
                }
                else
                    this.Client.SendPacket(new PROTOCOL_BASE_USER_TITLE_RELEASE_ACK(2147483648U /*0x80000000*/));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_USER_TITLE_RELEASE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}