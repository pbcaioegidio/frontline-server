// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_SHOP_ITEM_CHANGE_DATA_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_SHOP_ITEM_CHANGE_DATA_REQ : GameClientPacket
    {
        private long Field0;
        private uint Field1;
        private uint Field2;
        private byte[] Field3;
        private string Field4;

        public override void Read()
        {
            this.Field0 = (long)this.ReadUD();
            this.Field3 = this.ReadB((int)this.ReadC());
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ItemsModel itemsModel = player.Inventory.GetItem(this.Field0);
                if (itemsModel != null && itemsModel.Id > 1600000)
                {
                    int itemId = ComDiv.CreateItemId(16 /*0x10*/, 0, ComDiv.GetIdStatics(itemsModel.Id, 3));
                    switch (itemId)
                    {
                        case 1600005:
                        case 1610052:
                        case 1600187:
                            this.Field1 = BitConverter.ToUInt32(this.Field3, 0);
                            break;
                        case 1600010:
                        case 1610047:
                        case 1610051:
                            this.Field4 = Bitwise.HexArrayToString(this.Field3, this.Field3.Length);
                            break;
                        default:
                            if (this.Field3.Length != 0)
                            {
                                this.Field1 = (uint)this.Field3[0];
                                break;
                            }
                            break;
                    }
                    this.Method0(itemId, player);
                }
                else
                    this.Field2 = 2147483648U /*0x80000000*/;
                this.Client.SendPacket(new PROTOCOL_AUTH_SHOP_ITEM_CHANGE_DATA_ACK(this.Field2));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_SHOP_ITEM_CHANGE_DATA_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        
        private void Method0(int A_1, Account A_2)
        {
            switch (A_1)
            {
                case 1600005:
                    ClanModel clan1 = ClanManager.GetClan(A_2.ClanId);
                    if (clan1.Id > 0 && clan1.OwnerId == this.Client.PlayerId && ComDiv.UpdateDB("system_clan", "name_color", (object)(int)this.Field1, "id", (object)clan1.Id))
                    {
                        clan1.NameColor = (int)this.Field1;
                        this.Client.SendPacket(new PROTOCOL_CS_REPLACE_COLOR_NAME_RESULT_ACK(clan1.NameColor));
                        this.Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(A_2));
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600006:
                    if (ComDiv.UpdateDB("accounts", "nick_color", (object)(int)this.Field1, "player_id", (object)A_2.PlayerId))
                    {
                        A_2.NickColor = (int)this.Field1;
                        this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(this.Field3.Length, A_2));
                        this.Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(A_2));
                        if (A_2.Room == null)
                            break;
                        using (PROTOCOL_ROOM_GET_NICKNAME_ACK Packet = new PROTOCOL_ROOM_GET_NICKNAME_ACK(A_2.SlotId, A_2.Nickname, A_2.NickColor))
                            A_2.Room.SendPacketToPlayers(Packet);
                        A_2.Room.UpdateSlotsInfo();
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600009:
                    if ((int)this.Field1 < 51 && (int)this.Field1 >= A_2.Rank - 10 && (int)this.Field1 <= A_2.Rank + 10)
                    {
                        if (ComDiv.UpdateDB("player_bonus", "fake_rank", (object)(int)this.Field1, "owner_id", (object)A_2.PlayerId))
                        {
                            A_2.Bonus.FakeRank = (int)this.Field1;
                            this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(this.Field3.Length, A_2));
                            this.Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(A_2));
                            if (A_2.Room == null)
                                break;
                            using (PROTOCOL_ROOM_GET_RANK_ACK Packet = new PROTOCOL_ROOM_GET_RANK_ACK(A_2.SlotId, A_2.GetRank()))
                                A_2.Room.SendPacketToPlayers(Packet);
                            A_2.Room.UpdateSlotsInfo();
                            break;
                        }
                        this.Field2 = 2147483648U /*0x80000000*/;
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600010:
                    if (!string.IsNullOrEmpty(this.Field4) && this.Field4.Length >= ConfigLoader.MinNickSize && this.Field4.Length <= ConfigLoader.MaxNickSize)
                    {
                        if (ComDiv.UpdateDB("player_bonus", "fake_nick", (object)A_2.Nickname, "owner_id", (object)A_2.PlayerId) && ComDiv.UpdateDB("accounts", "nickname", (object)this.Field4, "player_id", (object)A_2.PlayerId))
                        {
                            A_2.Bonus.FakeNick = A_2.Nickname;
                            A_2.Nickname = this.Field4;
                            this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(this.Field3.Length, A_2));
                            this.Client.SendPacket(new PROTOCOL_AUTH_CHANGE_NICKNAME_ACK(A_2.Nickname));
                            this.Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(A_2));
                            if (A_2.Room == null)
                                break;
                            using (PROTOCOL_ROOM_GET_NICKNAME_ACK Packet = new PROTOCOL_ROOM_GET_NICKNAME_ACK(A_2.SlotId, A_2.Nickname, A_2.NickColor))
                                A_2.Room.SendPacketToPlayers(Packet);
                            A_2.Room.UpdateSlotsInfo();
                            break;
                        }
                        this.Field2 = 2147483648U /*0x80000000*/;
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600014:
                    if (ComDiv.UpdateDB("player_bonus", "crosshair_color", (object)(int)this.Field1, "owner_id", (object)A_2.PlayerId))
                    {
                        A_2.Bonus.CrosshairColor = (int)this.Field1;
                        this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(this.Field3.Length, A_2));
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600047:
                    if (!string.IsNullOrEmpty(this.Field4) && this.Field4.Length >= ConfigLoader.MinNickSize && this.Field4.Length <= ConfigLoader.MaxNickSize && A_2.Inventory.GetItem(1600010) == null)
                    {
                        if (!DaoManagerSQL.IsPlayerNameExist(this.Field4))
                        {
                            if (ComDiv.UpdateDB("accounts", "nickname", (object)this.Field4, "player_id", (object)A_2.PlayerId))
                            {
                                DaoManagerSQL.CreatePlayerNickHistory(A_2.PlayerId, A_2.Nickname, this.Field4, "Changed (Coupon)");
                                A_2.Nickname = this.Field4;
                                this.Client.SendPacket(new PROTOCOL_AUTH_CHANGE_NICKNAME_ACK(A_2.Nickname));
                                this.Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(A_2));
                                if (A_2.Room != null)
                                {
                                    using (PROTOCOL_ROOM_GET_NICKNAME_ACK Packet = new PROTOCOL_ROOM_GET_NICKNAME_ACK(A_2.SlotId, A_2.Nickname, A_2.NickColor))
                                        A_2.Room.SendPacketToPlayers(Packet);
                                    A_2.Room.UpdateSlotsInfo();
                                }
                                if (A_2.ClanId > 0)
                                {
                                    using (PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK Packet = new PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(A_2))
                                        ClanManager.SendPacket(Packet, A_2.ClanId, -1L, true, true);
                                }
                                AllUtils.SyncPlayerToFriends(A_2, true);
                                break;
                            }
                            this.Field2 = 2147483648U /*0x80000000*/;
                            break;
                        }
                        this.Field2 = 2147483923U;
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600051:
                    if (!string.IsNullOrEmpty(this.Field4) && this.Field4.Length <= 16 /*0x10*/)
                    {
                        ClanModel clan2 = ClanManager.GetClan(A_2.ClanId);
                        if (clan2.Id > 0 && clan2.OwnerId == this.Client.PlayerId)
                        {
                            if (!ClanManager.IsClanNameExist(this.Field4) && ComDiv.UpdateDB("system_clan", "name", (object)this.Field4, "id", (object)A_2.ClanId))
                            {
                                clan2.Name = this.Field4;
                                using (PROTOCOL_CS_REPLACE_NAME_RESULT_ACK Packet = new PROTOCOL_CS_REPLACE_NAME_RESULT_ACK(this.Field4))
                                {
                                    ClanManager.SendPacket(Packet, A_2.ClanId, -1L, true, true);
                                    break;
                                }
                            }
                            this.Field2 = 2147483648U /*0x80000000*/;
                            break;
                        }
                        this.Field2 = 2147483648U /*0x80000000*/;
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600052:
                    ClanModel clan3 = ClanManager.GetClan(A_2.ClanId);
                    if (clan3.Id > 0 && clan3.OwnerId == this.Client.PlayerId && !ClanManager.IsClanLogoExist(this.Field1) && DaoManagerSQL.UpdateClanLogo(A_2.ClanId, this.Field1))
                    {
                        clan3.Logo = this.Field1;
                        using (PROTOCOL_CS_REPLACE_MARK_RESULT_ACK Packet = new PROTOCOL_CS_REPLACE_MARK_RESULT_ACK(this.Field1))
                        {
                            ClanManager.SendPacket(Packet, A_2.ClanId, -1L, true, true);
                            break;
                        }
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600085:
                    if (A_2.Room == null)
                    {
                        this.Field2 = 2147483648U /*0x80000000*/;
                        break;
                    }
                    Account playerBySlot = A_2.Room.GetPlayerBySlot((int)this.Field1);
                    if (playerBySlot != null)
                    {
                        this.Client.SendPacket(new PROTOCOL_ROOM_GET_USER_ITEM_ACK(playerBySlot));
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600187:
                    if (ComDiv.UpdateDB("player_bonus", "muzzle_color", (object)(int)this.Field1, "owner_id", (object)A_2.PlayerId))
                    {
                        A_2.Bonus.MuzzleColor = (int)this.Field1;
                        this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(this.Field3.Length, A_2));
                        if (A_2.Room == null)
                            break;
                        using (PROTOCOL_ROOM_GET_COLOR_MUZZLE_FLASH_ACK Packet = new PROTOCOL_ROOM_GET_COLOR_MUZZLE_FLASH_ACK(A_2.SlotId, A_2.Bonus.MuzzleColor))
                            A_2.Room.SendPacketToPlayers(Packet);
                        A_2.Room.UpdateSlotsInfo();
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600193:
                    ClanModel clan4 = ClanManager.GetClan(A_2.ClanId);
                    if (clan4.Id > 0 && clan4.OwnerId == this.Client.PlayerId)
                    {
                        if (ComDiv.UpdateDB("system_clan", "effects", (object)(int)this.Field1, "id", (object)A_2.ClanId))
                        {
                            clan4.Effect = (int)this.Field1;
                            using (PROTOCOL_CS_REPLACE_MARKEFFECT_RESULT_ACK Packet = new PROTOCOL_CS_REPLACE_MARKEFFECT_RESULT_ACK((int)this.Field1))
                                ClanManager.SendPacket(Packet, A_2.ClanId, -1L, true, true);
                            this.Client.SendPacket(new PROTOCOL_BASE_GET_MYINFO_BASIC_ACK(A_2));
                            break;
                        }
                        this.Field2 = 2147483648U /*0x80000000*/;
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                case 1600205:
                    if (ComDiv.UpdateDB("player_bonus", "nick_border_color", (object)(int)this.Field1, "owner_id", (object)A_2.PlayerId))
                    {
                        A_2.Bonus.NickBorderColor = (int)this.Field1;
                        this.Client.SendPacket(new PROTOCOL_BASE_INV_ITEM_DATA_ACK(this.Field3.Length, A_2));
                        if (A_2.Room == null)
                            break;
                        using (PROTOCOL_ROOM_GET_NICK_OUTLINE_COLOR_ACK Packet = new PROTOCOL_ROOM_GET_NICK_OUTLINE_COLOR_ACK(A_2.SlotId, A_2.Bonus.NickBorderColor))
                            A_2.Room.SendPacketToPlayers(Packet);
                        A_2.Room.UpdateSlotsInfo();
                        break;
                    }
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
                default:
                    CLogger.Print($"Coupon effect not found! Id: {A_1}", LoggerType.Warning);
                    this.Field2 = 2147483648U /*0x80000000*/;
                    break;
            }
        }
    }
}
