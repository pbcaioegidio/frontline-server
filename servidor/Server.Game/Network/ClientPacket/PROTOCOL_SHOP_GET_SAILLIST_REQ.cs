// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_SHOP_GET_SAILLIST_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_SHOP_GET_SAILLIST_REQ : GameClientPacket
    {
        private string Field0;

        public override void Read() => this.Field0 = this.ReadS(32 /*0x20*/);


        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                if (!player.LoadedShop)
                {
                    player.LoadedShop = true;
                    player.LoadedPackedGoods = true;
                    ShopCatalog121Sender.SendFullCatalog(this.Client, player, true);
                }

                this.Client.SendPacket(new PROTOCOL_SHOP_TAG_INFO_ACK());

                if (ShopManager.ItemLimited.Count > 0)
                {
                    this.Client.SendPacket(new PROTOCOL_SHOP_LIMITED_SALE_LIST_ACK());
                    
                }

                if (Bitwise.ReadFile(Environment.CurrentDirectory + "/Data/Raws/Shop.dat") == this.Field0)
                    this.Client.SendPacket(new PROTOCOL_SHOP_GET_SAILLIST_ACK(false));
                else
                    this.Client.SendPacket(new PROTOCOL_SHOP_GET_SAILLIST_ACK(true));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_SHOP_GET_SAILLIST_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
