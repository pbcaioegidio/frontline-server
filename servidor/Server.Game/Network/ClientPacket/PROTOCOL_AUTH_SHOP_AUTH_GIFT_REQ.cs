// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_SHOP_AUTH_GIFT_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_SHOP_AUTH_GIFT_REQ : GameClientPacket
    {
        private long Field0;

        public override void Read() => this.Field0 = (long)this.ReadUD();

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                if (player.Inventory.Items.Count < 1500)
                {
                    MessageModel message = DaoManagerSQL.GetMessage(this.Field0, player.PlayerId);
                    if (message != null && message.Type == NoteMessageType.Gift)
                    {
                        GoodsItem good = ShopManager.GetGood((int)message.SenderId);
                        if (good == null)
                            return;
                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_AUTH_GIFT_ACK(1U, good.Item, player));
                        DaoManagerSQL.DeleteMessage(this.Field0, player.PlayerId);
                    }
                    else
                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_AUTH_GIFT_ACK(2147483648U /*0x80000000*/));
                }
                else
                {
                    this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_ITEM_AUTH_ACK(2147487785U));
                    this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_AUTH_GIFT_ACK(2147483648U /*0x80000000*/));
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_SHOP_AUTH_GIFT_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}