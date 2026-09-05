using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Server.Game.Network.ClientPacket
{
    // Shop "extend goods" purchase. The 122 client builds this in
    // CGameEventHandler__evtShop_BuyExtendGoods (0xD0E751), fired by UI send-event 266
    // (registered at CGameEventHandler__RegShopEvents 0xD0EFE4):
    //
    //   0xD0E782  mov ecx, 0x43A          ; opcode 1082
    //   0xD0E787  mov word ptr [eax], cx
    //   0xD0EA0A  NetBuffer__AppendData(records, 19 * cartCount)
    //
    // There is no count field: the body is a whole number of 19-byte records, so the record
    // count is the remaining body length / 19. Per record, from the same function:
    //   [0]  D  item db index      (0xD0E9F7)
    //   [4]  C  auth type          (0xD0E890)
    //   [5]  D  goods id           (0xD0E89A)
    //   [9]  C  price type         (0xD0E8B1)
    //   [10] D  unused (zeroed)
    //   [14] D  unused (zeroed)
    //   [18] C  buy kind           (0xD0E8A5)
    public class PROTOCOL_AUTH_SHOP_EXTEND_REQ : GameClientPacket
    {
        private const int RecordSize = 19;

        private readonly List<CartGoods> list_0 = new List<CartGoods>();
        private bool malformed;

        public override void Read()
        {
            long body = this.MStream.Length - this.MStream.Position;
            if (body <= 0 || body % RecordSize != 0)
            {
                // The body is a whole number of records and nothing else. A leftover tail means
                // the layout drifted; parse nothing rather than acting on a partial cart.
                this.malformed = true;
                CLogger.Print($"{this.GetType().Name}; body of {body} bytes is not a multiple of {RecordSize}", LoggerType.Error);
                return;
            }

            while (this.MStream.Length - this.MStream.Position >= RecordSize)
            {
                this.ReadD();                       // item db index
                this.ReadC();                       // auth type
                CartGoods goods = new CartGoods()
                {
                    GoodId = this.ReadD(),
                    BuyType = (int)this.ReadC()
                };
                this.ReadD();                       // unused
                this.ReadD();                       // unused
                this.ReadC();                       // buy kind
                this.list_0.Add(goods);
            }
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.Player;
                if (player == null)
                    return;
                if (this.malformed)
                {
                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487767U));
                    return;
                }
                if (player.Inventory.Items.Count >= 1000)
                {
                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487929U));
                    return;
                }

                int num1;
                int num2;
                int num3;
                List<GoodsItem> goods = ShopManager.GetGoods(this.list_0, out num1, out num2, out num3);
                if (goods.Count == 0)
                {
                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487767U));
                    return;
                }

                // Reject the whole cart (no charge) if a season good is ineligible.
                if (!AllUtils.PreflightSeasonGoods(player, goods, out _))
                {
                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U));
                    return;
                }

                // Same rule for the expansions: resolve every class-36 good and check the whole
                // cart against the client's 1000-slot ceiling BEFORE charging, so a cart that
                // cannot be applied never takes the player's money.
                int extraSlots = 0;
                foreach (GoodsItem goodsItem in goods)
                {
                    if (AllUtils.IsSeasonShopGood(goodsItem) || ComDiv.GetIdStatics(goodsItem.Item.Id, 1) != 36)
                        continue;

                    int slots = AllUtils.GetInventoryExpansionSlots(goodsItem.Item.Id);
                    if (slots <= 0)
                    {
                        CLogger.Print($"{this.GetType().Name}; unmapped inventory-expansion good {goodsItem.Item.Id}, cart refused", LoggerType.Warning);
                        this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487767U));
                        return;
                    }
                    extraSlots += slots;
                }
                if (extraSlots > 0 && !AllUtils.CanExtendInventory(player, extraSlots))
                {
                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487929U));
                    return;
                }

                // CommitSeasonGoods persists season state this handler cannot undo, and the
                // expansion is a separate write, so a cart holding both has no rollback that
                // restores everything. Refuse the combination instead of risking a free grant.
                if (extraSlots > 0 && goods.Any(AllUtils.IsSeasonShopGood))
                {
                    CLogger.Print($"{this.GetType().Name}; refused mixed season + inventory-expansion cart", LoggerType.Warning);
                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487767U));
                    return;
                }

                if (0 > player.Gold - num1 || 0 > player.Cash - num2 || 0 > player.Tags - num3)
                {
                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487768U));
                    return;
                }

                int origGold = player.Gold, origCash = player.Cash, origTags = player.Tags;
                if (!DaoManagerSQL.UpdateAccountValuable(player.PlayerId, player.Gold - num1, player.Cash - num2, player.Tags - num3))
                {
                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U));
                    return;
                }
                player.Gold -= num1;
                player.Cash -= num2;
                player.Tags -= num3;

                // Expansion first: it is the only step whose failure is still fully reversible
                // by refunding, since CommitSeasonGoods below persists season state that this
                // handler cannot undo. One single write + one 3337 for the whole cart, so a
                // multi-expansion cart can never end up half applied.
                if (extraSlots > 0 && !AllUtils.ExtendInventory(this.Client, player, extraSlots))
                {
                    // Only mirror the refund in memory once the DB took it.
                    if (DaoManagerSQL.UpdateAccountValuable(player.PlayerId, origGold, origCash, origTags))
                    {
                        player.Gold = origGold;
                        player.Cash = origCash;
                        player.Tags = origTags;
                    }
                    else
                        CLogger.Print($"{this.GetType().Name}; refund failed for player {player.PlayerId}, charged with no expansion", LoggerType.Error);
                    this.Client.SendPacket(new PACKET_INVENTORY_MAX_UP_ACK(PACKET_INVENTORY_MAX_UP_ACK.ERROR_SHOP_BUY_FAIL));
                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U));
                    return;
                }

                // Season pass / level-skip: apply before delivery, refund on failure.
                bool hasSeason = goods.Any(AllUtils.IsSeasonShopGood);
                AllUtils.SeasonShopPurchaseType seasonType = AllUtils.CommitSeasonGoods(player, goods, origGold, origCash, origTags);
                if (hasSeason && seasonType == AllUtils.SeasonShopPurchaseType.None)
                {
                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(2147487769U));
                    return;
                }

                foreach (GoodsItem goodsItem in goods)
                {
                    if (AllUtils.IsSeasonShopGood(goodsItem))
                        continue; // season good handled by CommitSeasonGoods
                    if (ComDiv.GetIdStatics(goodsItem.Item.Id, 1) == 36)
                        continue; // applied above as a single expansion

                    this.Client.SendPacket((GameServerPacket)new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, goodsItem.Item));
                }
                this.Client.SendPacket((GameServerPacket)new PROTOCOL_AUTH_SHOP_GOODS_BUY_ACK(1U, goods, player));
                if (seasonType != AllUtils.SeasonShopPurchaseType.None)
                    AllUtils.FinalizeSeasonShopPurchase(player, seasonType);
            }
            catch (Exception ex)
            {
                CLogger.Print($"{this.GetType().Name}; {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}
