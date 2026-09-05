using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.Security;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_SHOP_GOODS_BUY_REQ : GameClientPacket
    {
        private List<CartGoods> Field0 = new List<CartGoods>();

        public override void Read()
        {
            byte num1 = this.ReadC();
            for (byte index = 0; (int)index < (int)num1; ++index)
            {
                int num2 = (int)this.ReadC();
                this.Field0.Add(new CartGoods()
                {
                    GoodId = this.ReadD(),
                    BuyType = (int)this.ReadC()
                });
                int num3 = (int)this.ReadC();
                this.ReadQ();
            }
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                if (SecurityDao.CountShopOpsLastMinute(player.PlayerId) >= 20)
                {
                    SecurityDao.LogEvent(SecurityDao.SourceGame, player.PlayerId, player.Username, player.Nickname, "flag", "SHOP",
                        "Rate limit loja (20/min)", "{}", 3, "FG-150");
                    this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U, player));
                    return;
                }
                if (player.Inventory.Items.Count >= 1500)
                {
                    this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487929U, player));
                }
                else
                {
                    int GoldPrice;
                    int CashPrice;
                    int TagsPrice;
                    List<GoodsItem> goods = ShopManager.GetGoods(this.Field0, out GoldPrice, out CashPrice, out TagsPrice);
                    if (goods.Count == 0)
                    {
                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487767U, player));
                        return;
                    }

                    // Reject the whole cart (no charge) if a season good is ineligible.
                    if (!AllUtils.PreflightSeasonGoods(player, goods, out _))
                    {
                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U, player));
                        return;
                    }

                    // Class 36 (inventory expansion) cannot be consumed from the inventory in the
                    // 122 client: UIWeaponStat__GetStatValue (0x98DBC4) routes the whole 3601xxx
                    // family to the single record at index 45, whose auth-name string is "0" (live
                    // RPM: record 0x0B499940 +280 -> wstring len 1, U+0030). None of the 12 entries
                    // in off_1520EB0 match it, so sub_F0404D (0xF0404D) returns -1 and
                    // InvenDB__ValidateAndShowItemUseError (0xDB645F) exits without sending
                    // anything. Delivering the item would strand it, so the slots are applied at
                    // purchase instead, as one aggregated delta that a refund can undo.
                    int extraSlots = 0;
                    foreach (GoodsItem expansion in goods.Where(g => ComDiv.GetIdStatics(g.Item.Id, 1) == 36 && !AllUtils.IsSeasonShopGood(g)))
                    {
                        // An unmapped class-36 good has no known slot count; delivering it would
                        // recreate the dead inventory item, so refuse the cart.
                        int slots = AllUtils.GetInventoryExpansionSlots(expansion.Item.Id);
                        if (slots <= 0)
                        {
                            CLogger.Print($"{this.GetType().Name}; class 36 good {expansion.Item.Id} has no slot mapping", LoggerType.Error);
                            this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U, player));
                            return;
                        }
                        extraSlots += slots;
                    }
                    if (extraSlots > 0 && !AllUtils.CanExtendInventory(player, extraSlots))
                    {
                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U, player));
                        return;
                    }

                    // Season state is persisted by CommitSeasonGoods and cannot be rolled back, so a
                    // cart holding both has no honest failure path. Refuse it instead of guessing.
                    if (extraSlots > 0 && goods.Any(AllUtils.IsSeasonShopGood))
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

                    // Expansion before anything is delivered: it is the only step whose failure is
                    // still fully reversible by refunding.
                    if (extraSlots > 0 && !AllUtils.ExtendInventory(this.Client, player, extraSlots))
                    {
                        if (DaoManagerSQL.UpdateAccountValuable(player.PlayerId, origGold, origCash, origTags))
                        {
                            player.Gold = origGold;
                            player.Cash = origCash;
                            player.Tags = origTags;
                        }
                        else
                            CLogger.Print($"{this.GetType().Name}; refund failed for player {player.PlayerId}, charged with no expansion", LoggerType.Error);
                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U, player));
                        return;
                    }

                    // Apply the (single) season good BEFORE delivering normal goods;
                    // a failed season persist rolls the whole charge back.
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
                            continue; // handled by CommitSeasonGoods

                        if (ComDiv.GetIdStatics(goodsItem.Item.Id, 1) == 36)
                            continue; // applied once, above, as one aggregated delta

                        if (ComDiv.GetIdStatics(goodsItem.Item.Id, 1) == 6 && player.Character.GetCharacter(goodsItem.Item.Id) == null)
                            AllUtils.CreateCharacter(player, goodsItem.Item);
                        else
                            this.Client.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, goodsItem.Item));

                        if (ShopManager.IsLimitedItem(goodsItem.Id))
                        {
                            ShopManager.UpdateLimitedItemStock(goodsItem.Id, 1);
                            GameXender.BroadcastLimitedSaleSync();
                        }
                    }
                    this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(1U, goods, player));
                    if (seasonType != AllUtils.SeasonShopPurchaseType.None)
                        AllUtils.FinalizeSeasonShopPurchase(player, seasonType);

                    string currency = GoldPrice > 0 ? "gold" : (CashPrice > 0 ? "cash" : "tags");
                    int price = GoldPrice > 0 ? GoldPrice : (CashPrice > 0 ? CashPrice : TagsPrice);
                    int before = GoldPrice > 0 ? origGold : (CashPrice > 0 ? origCash : origTags);
                    int after = GoldPrice > 0 ? player.Gold : (CashPrice > 0 ? player.Cash : player.Tags);
                    int firstGood = this.Field0.Count > 0 ? this.Field0[0].GoodId : 0;
                    int firstItem = goods.Count > 0 && goods[0].Item != null ? goods[0].Item.Id : 0;
                    SecurityDao.LogShop(player.PlayerId, "buy", firstGood, firstItem, currency, price, before, after, 0, player.PublicIP?.ToString());
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_SHOP_GOODS_BUY_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}