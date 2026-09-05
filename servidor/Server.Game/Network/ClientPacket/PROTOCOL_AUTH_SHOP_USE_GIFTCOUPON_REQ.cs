// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_SHOP_USE_GIFTCOUPON_REQ
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
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_SHOP_USE_GIFTCOUPON_REQ : GameClientPacket
    {
        private uint Field0;
        private string Field1;
        private TicketType Field2;

        public override void Read()
        {
            this.Field1 = this.ReadS((int)this.ReadC());
            this.Field2 = (TicketType)this.ReadC();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                TicketModel ticket = RedeemCodeXML.GetTicket(this.Field1, this.Field2);
                if (ticket != null)
                {
                    if ((long)ComDiv.CountDB($"SELECT COUNT(used_count) FROM base_redeem_history WHERE used_token = '{ticket.Token}'") >= (long)ticket.TicketCount)
                    {
                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_USE_GIFTCOUPON_ACK(2147483648U /*0x80000000*/));
                        return;
                    }
                    int usedTicket = DaoManagerSQL.GetUsedTicket(player.PlayerId, ticket.Token);
                    if ((long)usedTicket < (long)ticket.PlayerRation)
                    {
                        int A_3 = usedTicket + 1;
                        if (ticket.Type == TicketType.COUPON)
                        {
                            List<GoodsItem> goods = this.GetGoods(ticket);
                            if (goods.Count > 0 && this.Method0(player, ticket, A_3))
                            {
                                foreach (GoodsItem goodsItem in goods)
                                {
                                    if (ComDiv.GetIdStatics(goodsItem.Item.Id, 1) == 6 && player.Character.GetCharacter(goodsItem.Item.Id) == null)
                                        AllUtils.CreateCharacter(player, goodsItem.Item);
                                    else
                                        player.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, goodsItem.Item));
                                }
                            }
                        }
                        else if (ticket.Type == TicketType.VOUCHER && (ticket.GoldReward != 0 || ticket.CashReward != 0 || ticket.TagsReward != 0) && this.Method0(player, ticket, A_3))
                        {
                            if (DaoManagerSQL.UpdateAccountValuable(player.PlayerId, player.Gold + ticket.GoldReward, player.Cash + ticket.CashReward, player.Tags + ticket.TagsReward))
                            {
                                player.Gold += ticket.GoldReward;
                                player.Cash += ticket.CashReward;
                                player.Tags += ticket.TagsReward;
                            }
                            this.Client.SendPacket(new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, player));
                        }
                    }
                    else
                        this.Field0 = 2147483648U /*0x80000000*/;
                }
                else
                    this.Field0 = 2147483648U /*0x80000000*/;
                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_USE_GIFTCOUPON_ACK(this.Field0));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_SHOP_USE_GIFTCOUPON_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        public List<GoodsItem> GetGoods(TicketModel Ticket)
        {
            List<GoodsItem> goods = new List<GoodsItem>();
            if (Ticket.Rewards.Count == 0)
                return goods;
            foreach (int reward in Ticket.Rewards)
            {
                GoodsItem good = ShopManager.GetGood(reward);
                if (good != null)
                    goods.Add(good);
            }
            return goods;
        }

        
        private bool Method0(Account A_1, TicketModel A_2, int A_3)
        {
            if (!DaoManagerSQL.IsTicketUsedByPlayer(A_1.PlayerId, A_2.Token))
                return DaoManagerSQL.CreatePlayerRedeemHistory(A_1.PlayerId, A_2.Token, A_3);
            return ComDiv.UpdateDB("base_redeem_history", "owner_id", (object)A_1.PlayerId, "used_token", (object)A_2.Token, new string[1]
            {
      "used_count"
            }, (object)A_3);
        }
    }
}