// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_SHOP_REPAIR_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_SHOP_REPAIR_REQ : GameClientPacket
    {
        private int Field0;
        private List<long> Field1 = new List<long>();

        public override void Read()
        {
            this.Field0 = (int)this.ReadC();
            for (int index = 0; index < this.Field0; ++index)
                this.Field1.Add((long)this.ReadUD());
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                int Gold;
                int Cash;
                uint Error;
                List<ItemsModel> itemsModelList = AllUtils.RepairableItems(player, this.Field1, out Gold, out Cash, out Error);
                if (itemsModelList.Count <= 0)
                    return;
                player.Gold -= Gold;
                player.Cash -= Cash;
                if (ComDiv.UpdateDB("accounts", "player_id", (object)player.PlayerId, new string[2]
                {
        "gold",
        "cash"
                }, (object)player.Gold, (object)player.Cash))
                    this.Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(2, player, itemsModelList));
                this.Client.SendPacket(new PROTOCOL_SHOP_REPAIR_ACK(Error, itemsModelList, player));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_SHOP_REPAIR_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}