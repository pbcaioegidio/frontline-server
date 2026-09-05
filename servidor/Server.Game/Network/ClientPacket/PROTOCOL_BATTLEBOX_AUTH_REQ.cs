// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BATTLEBOX_AUTH_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLEBOX_AUTH_REQ : GameClientPacket
    {
        private long Field0;
        private int Field1;

        public override void Read()
        {
            this.Field0 = (long)this.ReadUD();
            this.Field1 = this.ReadD();
        }


        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ItemsModel A_3 = player.Inventory.GetItem(this.Field0);
                if (A_3 == null)
                {
                    this.Client.SendPacket(new PROTOCOL_BATTLEBOX_AUTH_ACK(2147483648U /*0x80000000*/));
                }
                else
                {
                    BattleBoxModel battleBox = BattleBoxXML.GetBattleBox(A_3.Id);
                    if (battleBox != null && battleBox.RequireTags == this.Field1)
                    {
                        if (this.Field1 <= player.Tags)
                        {
                            if (!DaoManagerSQL.UpdateAccountTags(player.PlayerId, player.Tags - this.Field1))
                            {
                                this.Client.SendPacket(new PROTOCOL_BATTLEBOX_AUTH_ACK(2147483648U /*0x80000000*/));
                            }
                            else
                            {
                                GoodsItem good = ShopManager.GetGood(battleBox.GetItemWithProbabilities<int>(battleBox.Goods, battleBox.Probabilities));
                                if (good != null)
                                {
                                    player.Tags -= this.Field1;
                                    if (ComDiv.UpdateDB("accounts", "tags", (object)player.Tags, "player_id", (object)player.PlayerId))
                                        this.Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, good.Item));
                                    --A_3.Count;
                                    if (A_3.Count > 0U)
                                    {
                                        ComDiv.UpdateDB("player_items", "count", (object)(long)A_3.Count, "owner_id", (object)player.PlayerId, "id", (object)A_3.Id);
                                        this.Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(1, player, A_3));
                                    }
                                    else
                                    {
                                        if (DaoManagerSQL.DeletePlayerInventoryItem(A_3.ObjectId, player.PlayerId))
                                            player.Inventory.RemoveItem(A_3);
                                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_DELETE_ITEM_ACK(1U, this.Field0));
                                    }
                                    this.Client.SendPacket(new PROTOCOL_BATTLEBOX_AUTH_ACK(0U, player, good.Item.Id));
                                }
                                else
                                    this.Client.SendPacket(new PROTOCOL_BATTLEBOX_AUTH_ACK(2147483648U /*0x80000000*/));
                            }
                        }
                        else
                            this.Client.SendPacket(new PROTOCOL_BATTLEBOX_AUTH_ACK(2147483648U /*0x80000000*/));
                    }
                    else
                        this.Client.SendPacket(new PROTOCOL_BATTLEBOX_AUTH_ACK(2147483648U /*0x80000000*/));
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.ToString(), LoggerType.Error);
            }
        }


        public void SendGiftMessage(Account Player, ItemsModel Item)
        {
            string label = Translation.GetLabel("BattleBoxGift");
            MessageModel message = this.CreateMessage("GM", Player.PlayerId, $"{label}\n\n{Item.Name}");
            if (message == null)
                return;
            Player.Connection.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(message));
        }

        public MessageModel CreateMessage(string SenderName, long OwnerId, string Text)
        {
            MessageModel Message = new MessageModel(15.0)
            {
                SenderName = SenderName,
                Text = Text,
                Type = NoteMessageType.Gift,
                State = NoteMessageState.Unreaded
            };
            return !DaoManagerSQL.CreateMessage(OwnerId, Message) ? (MessageModel)null : Message;
        }
    }
}