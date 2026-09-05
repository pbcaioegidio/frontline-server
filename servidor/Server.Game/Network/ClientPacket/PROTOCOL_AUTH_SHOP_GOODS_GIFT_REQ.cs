// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_SHOP_GOODS_GIFT_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Utility;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Game.Data.Utils;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_SHOP_GOODS_GIFT_REQ : GameClientPacket
    {
        private string Field0;
        private string Field1;
        private List<CartGoods> Field2 = new List<CartGoods>();

        public override void Read()
        {
            byte num1 = this.ReadC();
            for (byte index = 0; (int)index < (int)num1; ++index)
            {
                int num2 = (int)this.ReadC();
                this.Field2.Add(new CartGoods()
                {
                    GoodId = this.ReadD(),
                    BuyType = (int)this.ReadC()
                });
                int num3 = (int)this.ReadC();
                this.ReadQ();
            }
            this.Field0 = this.ReadU((int)this.ReadC() * 2);
            this.Field1 = this.ReadU((int)this.ReadC() * 2);
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;

                // No recipient (or self) => this is a self-purchase, same flow as
                // GOODS_BUY (season goods included). Only a real gift to ANOTHER
                // account skips season activation (handled below).
                if (string.IsNullOrEmpty(this.Field1) || player.Nickname == this.Field1)
                {
                    this.SelfBuy(player);
                    return;
                }
                Account account = AccountManager.GetAccount(this.Field1, 1, 0);
                if (account != null && account.IsOnline && player.Nickname != this.Field1)
                {
                    if (account.Inventory.Items.Count < 1500)
                    {
                        int GoldPrice;
                        int CashPrice;
                        int TagsPrice;
                        List<GoodsItem> goods = ShopManager.GetGoods(this.Field2, out GoldPrice, out CashPrice, out TagsPrice);
                        if (goods.Count != 0)
                        {
                            // Season pass / level-skip goods are not giftable (a pass
                            // applies to an account, not an inventory item). Reject the
                            // whole cart with no charge; buy them via the shop / season UI.
                            if (goods.Any(AllUtils.IsSeasonShopGood))
                            {
                                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U));
                                return;
                            }

                            if (0 <= player.Gold - GoldPrice && 0 <= player.Cash - CashPrice && 0 <= player.Tags - TagsPrice)
                            {
                                if (!DaoManagerSQL.UpdateAccountValuable(player.PlayerId, player.Gold - GoldPrice, player.Cash - CashPrice, player.Tags - TagsPrice))
                                {
                                    this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U));
                                }
                                else
                                {
                                    player.Gold -= GoldPrice;
                                    player.Cash -= CashPrice;
                                    player.Tags -= TagsPrice;
                                    if (DaoManagerSQL.GetMessagesCount(account.PlayerId) >= 100)
                                    {
                                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487929U));
                                    }
                                    else
                                    {
                                        MessageModel A_1 = this.Method0(player.Nickname, account.PlayerId, this.Client.PlayerId);
                                        if (A_1 != null)
                                            account.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(A_1), false);
                                        account.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, account, goods), false);
                                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_GIFT_ACK(1U, goods, account));
                                        this.Client.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, player));
                                    }
                                }
                            }
                            else
                                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487768U));
                        }
                        else
                            this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487767U));
                    }
                    else
                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487929U));
                }
                else
                    this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_SHOP_GOODS_BUY_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        // Self-gift == self-buy: mirrors PROTOCOL_AUTH_SHOP_GOODS_BUY_REQ, including
        // the season pass / level-skip preflight + charge + commit + refund path.
        private void SelfBuy(Account player)
        {
            if (this.Field2.Count == 0)
                return;
            if (player.Inventory.Items.Count >= 1500)
            {
                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487929U, player));
                return;
            }

            int GoldPrice, CashPrice, TagsPrice;
            List<GoodsItem> goods = ShopManager.GetGoods(this.Field2, out GoldPrice, out CashPrice, out TagsPrice);
            if (goods.Count == 0)
            {
                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487767U, player));
                return;
            }
            if (!AllUtils.PreflightSeasonGoods(player, goods, out _))
            {
                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U, player));
                return;
            }
            if (!(0 <= player.Gold - GoldPrice && 0 <= player.Cash - CashPrice && 0 <= player.Tags - TagsPrice))
            {
                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487768U, player));
                return;
            }

            int origGold = player.Gold, origCash = player.Cash, origTags = player.Tags;
            if (!DaoManagerSQL.UpdateAccountValuable(player.PlayerId, player.Gold - GoldPrice, player.Cash - CashPrice, player.Tags - TagsPrice))
            {
                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U, player));
                return;
            }
            player.Gold -= GoldPrice;
            player.Cash -= CashPrice;
            player.Tags -= TagsPrice;

            bool hasSeason = goods.Any(AllUtils.IsSeasonShopGood);
            AllUtils.SeasonShopPurchaseType seasonType = AllUtils.CommitSeasonGoods(player, goods, origGold, origCash, origTags);
            if (hasSeason && seasonType == AllUtils.SeasonShopPurchaseType.None)
            {
                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U, player));
                return;
            }

            foreach (GoodsItem goodsItem in goods)
            {
                if (AllUtils.IsSeasonShopGood(goodsItem))
                    continue;

                if (ComDiv.GetIdStatics(goodsItem.Item.Id, 1) == 6 && player.Character.GetCharacter(goodsItem.Item.Id) == null)
                    AllUtils.CreateCharacter(player, goodsItem.Item);
                else
                    this.Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, goodsItem.Item));
            }
            this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(1U, goods, player));
            if (seasonType != AllUtils.SeasonShopPurchaseType.None)
                AllUtils.FinalizeSeasonShopPurchase(player, seasonType);
        }

        private MessageModel Method0(string A_1, long A_2, long A_3)
        {
            MessageModel Message = new MessageModel(15.0)
            {
                SenderName = A_1,
                SenderId = A_3,
                Text = this.Field0,
                State = NoteMessageState.Unreaded
            };
            if (DaoManagerSQL.CreateMessage(A_2, Message))
                return Message;
            this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147483648U /*0x80000000*/));
            return (MessageModel)null;
        }
    }
}