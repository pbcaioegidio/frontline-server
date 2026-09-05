// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CHAR_CHANGE_EQUIP_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CHAR_CHANGE_EQUIP_REQ : GameClientPacket
    {
        private int Field0;
        private bool Field1;
        private bool Field2;
        private bool Field3;
        private bool Field4;
        private bool Field5;
        // ITEM_INFO[6] = the 6 emoticon slots (first block of the 121 CHANGE_EQUIP packet)
        // and its "changed" flag. Maps to loadout+164 on the client.
        private int[] EmoticonIds = new int[0];
        private bool EmoticonsChanged;
        private readonly int[] Field6 = new int[2];
        private readonly int[] Field7 = new int[2];
        private readonly int[] Field8 = new int[13];
        private readonly SortedList<int, int> Field9 = new SortedList<int, int>();
        private readonly SortedList<int, int> Field10 = new SortedList<int, int>();
        private readonly SortedList<int, int> Field11 = new SortedList<int, int>();

        public override void Read()
        {
            CharChangeEquip121Request packet = CharChangeEquip121Parser.ParsePacket(this._raw);

            // ITEM_INFO[6] = emoticons; take the id of each pair the client sent.
            this.EmoticonsChanged = packet.ItemInfo6Changed;
            this.EmoticonIds = new int[packet.ItemInfo6.Length];
            for (int i = 0; i < packet.ItemInfo6.Length; i++)
                this.EmoticonIds[i] = CharChangeEquip121Request.ToLegacyItemId(packet.ItemInfo6[i].ItemId);

            this.Field0 = CharChangeEquip121Request.ToLegacyItemId(packet.Accessory.ItemId);
            this.Field1 = packet.AccessoryChanged;

            FillList(this.Field9, packet.UInt39A);
            FillList(this.Field10, packet.UInt39B);
            this.Field2 = this.Field9.Count > 0;
            this.Field3 = this.Field10.Count > 0;

            packet.CopyToLegacyFields(this.Field8, this.Field6, this.Field7);
            this.Field4 = packet.CharacterEquipmentChanged;

            for (int i = 0; i < packet.ItemInfo3.Length; i++)
                this.Field11.Add(i, CharChangeEquip121Request.ToLegacyItemId(packet.ItemInfo3[i].ItemId));
            this.Field5 = packet.ItemEquipmentChanged;
        }

        private static void FillList(SortedList<int, int> target, uint[] source)
        {
            for (int i = 0; i < source.Length; i++)
                target.Add(i, CharChangeEquip121Request.ToLegacyItemId(source[i]));
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                if (player.Character.Characters.Count > 0)
                {
                    if (this.Field1)
                        AllUtils.ValidateAccesoryEquipment(player, this.Field0);
                    if (this.Field2)
                        AllUtils.ValidateDisabledCoupon(player, this.Field9);
                    if (this.Field3)
                        AllUtils.ValidateEnabledCoupon(player, this.Field10);
                    if (this.Field4)
                        AllUtils.ValidateCharacterEquipment(player, player.Equipment, this.Field8, this.Field6, this.Field7);
                    if (this.Field5)
                        AllUtils.ValidateItemEquipment(player, this.Field11);
                    if (this.EmoticonsChanged)
                        AllUtils.ValidateEmoticonEquipment(player, this.EmoticonIds);
                    AllUtils.ValidateCharacterSlot(player, player.Equipment, this.Field7);
                }
                RoomModel room = player.Room;
                if (room != null)
                    AllUtils.UpdateSlotEquips(player, room);
                this.Client.SendPacket(new PROTOCOL_CHAR_CHANGE_EQUIP_ACK(0U));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
