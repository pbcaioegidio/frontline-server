// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_SHOP_DELETE_ITEM_REQ
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
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_SHOP_DELETE_ITEM_REQ : GameClientPacket
    {
        private long Field0;
        private uint Field1 = 1;

        public override void Read() => this.Field0 = (long)this.ReadUD();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ItemsModel itemsModel = player.Inventory.GetItem(this.Field0);
                PlayerBonus bonus = player.Bonus;
                if (itemsModel == null)
                    this.Field1 = 2147483648U /*0x80000000*/;
                else if (ComDiv.GetIdStatics(itemsModel.Id, 1) == 16 /*0x10*/)
                {
                    if (bonus == null)
                    {
                        this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_DELETE_ITEM_ACK(2147483648U /*0x80000000*/));
                        return;
                    }
                    if (!bonus.RemoveBonuses(itemsModel.Id))
                    {
                        if (itemsModel.Id != 1600014)
                        {
                            if (itemsModel.Id == 1600010)
                            {
                                if (bonus.FakeNick.Length != 0)
                                {
                                    if (ComDiv.UpdateDB("accounts", "nickname", (object)bonus.FakeNick, "player_id", (object)player.PlayerId) && ComDiv.UpdateDB("player_bonus", "fake_nick", (object)"", "owner_id", (object)player.PlayerId))
                                    {
                                        player.Nickname = bonus.FakeNick;
                                        bonus.FakeNick = "";
                                        this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(0, player));
                                        this.Client.SendPacket(new PROTOCOL_AUTH_CHANGE_NICKNAME_ACK(player.Nickname));
                                        this.Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(player));
                                        RoomModel room = player.Room;
                                        if (room != null)
                                        {
                                            using (PROTOCOL_ROOM_GET_NICKNAME_ACK Packet = new PROTOCOL_ROOM_GET_NICKNAME_ACK(player.SlotId, player.Nickname, player.NickColor))
                                                room.SendPacketToPlayers(Packet);
                                            room.UpdateSlotsInfo();
                                        }
                                    }
                                    else
                                        this.Field1 = 2147483648U /*0x80000000*/;
                                }
                                else
                                    this.Field1 = 2147483648U /*0x80000000*/;
                            }
                            else if (itemsModel.Id == 1600009)
                            {
                                if (ComDiv.UpdateDB("player_bonus", "fake_rank", (object)RankDomain.NoFakeRank, "owner_id", (object)player.PlayerId))
                                {
                                    bonus.FakeRank = RankDomain.NoFakeRank;
                                    this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(0, player));
                                    this.Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(player));
                                    RoomModel room = player.Room;
                                    if (room != null)
                                    {
                                        using (PROTOCOL_ROOM_GET_RANK_ACK Packet = new PROTOCOL_ROOM_GET_RANK_ACK(player.SlotId, bonus.MuzzleColor))
                                            room.SendPacketToPlayers(Packet);
                                        room.UpdateSlotsInfo();
                                    }
                                }
                                else
                                    this.Field1 = 2147483648U /*0x80000000*/;
                            }
                            else if (itemsModel.Id == 1600187)
                            {
                                if (ComDiv.UpdateDB("player_bonus", "muzzle_color", (object)0, "owner_id", (object)player.PlayerId))
                                {
                                    bonus.MuzzleColor = 0;
                                    this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(0, player));
                                    RoomModel room = player.Room;
                                    if (room != null)
                                    {
                                        using (PROTOCOL_ROOM_GET_COLOR_MUZZLE_FLASH_ACK Packet = new PROTOCOL_ROOM_GET_COLOR_MUZZLE_FLASH_ACK(player.SlotId, bonus.MuzzleColor))
                                            room.SendPacketToPlayers(Packet);
                                        room.UpdateSlotsInfo();
                                    }
                                }
                                else
                                    this.Field1 = 2147483648U /*0x80000000*/;
                            }
                            else if (itemsModel.Id == 1600006)
                            {
                                if (ComDiv.UpdateDB("accounts", "nick_color", (object)0, "owner_id", (object)player.PlayerId))
                                {
                                    player.NickColor = 0;
                                    this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(0, player));
                                    this.Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(player));
                                    RoomModel room = player.Room;
                                    if (room != null)
                                    {
                                        using (PROTOCOL_ROOM_GET_COLOR_NICK_ACK Packet = new PROTOCOL_ROOM_GET_COLOR_NICK_ACK(player.SlotId, player.NickColor))
                                            room.SendPacketToPlayers(Packet);
                                        room.UpdateSlotsInfo();
                                    }
                                }
                                else
                                    this.Field1 = 2147483648U /*0x80000000*/;
                            }
                        }
                        else if (!ComDiv.UpdateDB("player_bonus", "crosshair_color", (object)4, "owner_id", (object)player.PlayerId))
                        {
                            this.Field1 = 2147483648U /*0x80000000*/;
                        }
                        else
                        {
                            bonus.CrosshairColor = 4;
                            this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(0, player));
                            this.Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(player));
                        }
                    }
                    else
                        DaoManagerSQL.UpdatePlayerBonus(player.PlayerId, bonus.Bonuses, bonus.FreePass);
                    CouponFlag couponEffect = CouponEffectXML.GetCouponEffect(itemsModel.Id);
                    if (couponEffect != null && couponEffect.EffectFlag > (CouponEffects)0 && player.Effects.HasFlag((Enum)couponEffect.EffectFlag))
                    {
                        player.Effects -= couponEffect.EffectFlag;
                        DaoManagerSQL.UpdateCouponEffect(player.PlayerId, player.Effects);
                    }
                }
                if (this.Field1 == 1U && itemsModel != null)
                {
                    if (DaoManagerSQL.DeletePlayerInventoryItem(itemsModel.ObjectId, player.PlayerId))
                        player.Inventory.RemoveItem(itemsModel);
                    else
                        this.Field1 = 2147483648U /*0x80000000*/;
                }
                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_DELETE_ITEM_ACK(this.Field1, this.Field0));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_SHOP_DELETE_ITEM_ACK: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}