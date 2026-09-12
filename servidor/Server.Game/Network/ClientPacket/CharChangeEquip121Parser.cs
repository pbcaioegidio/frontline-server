using System;

namespace Server.Game.Network.ClientPacket
{
    internal struct CharChangeEquip121ItemPair
    {
        internal readonly uint ItemId;
        internal readonly uint ObjectId;

        internal CharChangeEquip121ItemPair(uint itemId, uint objectId)
        {
            ItemId = itemId;
            ObjectId = objectId;
        }
    }

    internal sealed class CharChangeEquip121Info
    {
        internal readonly byte Flag;
        internal readonly CharChangeEquip121ItemPair[] Pairs;

        internal CharChangeEquip121Info(byte flag, CharChangeEquip121ItemPair[] pairs)
        {
            Flag = flag;
            Pairs = pairs;
        }
    }

    internal sealed class CharChangeEquip121Request
    {
        internal readonly int BytesConsumed;
        internal readonly CharChangeEquip121ItemPair[] ItemInfo6;
        internal readonly bool ItemInfo6Changed;
        internal readonly CharChangeEquip121ItemPair Accessory;
        internal readonly bool AccessoryChanged;
        internal readonly uint[] UInt39A;
        internal readonly ushort UInt39AMarker;
        internal readonly uint[] UInt39B;
        internal readonly ushort UInt39BMarker;
        internal readonly CharChangeEquip121Info CharacterInfo;
        internal readonly bool CharacterEquipmentChanged;
        internal readonly CharChangeEquip121ItemPair[] ItemInfo3;
        internal readonly bool ItemEquipmentChanged;
        internal readonly byte[] CharacterSlots;

        internal int ItemInfo6Count
        {
            get { return ItemInfo6.Length; }
        }

        internal int ItemInfo3Count
        {
            get { return ItemInfo3.Length; }
        }

        internal CharChangeEquip121Request(
            int bytesConsumed,
            CharChangeEquip121ItemPair[] itemInfo6,
            bool itemInfo6Changed,
            CharChangeEquip121ItemPair accessory,
            bool accessoryChanged,
            uint[] uint39A,
            ushort uint39AMarker,
            uint[] uint39B,
            ushort uint39BMarker,
            CharChangeEquip121Info characterInfo,
            bool characterEquipmentChanged,
            CharChangeEquip121ItemPair[] itemInfo3,
            bool itemEquipmentChanged,
            byte[] characterSlots)
        {
            BytesConsumed = bytesConsumed;
            ItemInfo6 = itemInfo6;
            ItemInfo6Changed = itemInfo6Changed;
            Accessory = accessory;
            AccessoryChanged = accessoryChanged;
            UInt39A = uint39A;
            UInt39AMarker = uint39AMarker;
            UInt39B = uint39B;
            UInt39BMarker = uint39BMarker;
            CharacterInfo = characterInfo;
            CharacterEquipmentChanged = characterEquipmentChanged;
            ItemInfo3 = itemInfo3;
            ItemEquipmentChanged = itemEquipmentChanged;
            CharacterSlots = characterSlots;
        }

        internal void CopyToLegacyFields(int[] equipmentList, int[] characterTemps, int[] characterSlots)
        {
            if (equipmentList == null || equipmentList.Length < 13)
                throw new ArgumentException("equipmentList must have at least 13 slots", "equipmentList");
            if (characterTemps == null || characterTemps.Length < 2)
                throw new ArgumentException("characterTemps must have at least 2 slots", "characterTemps");
            if (characterSlots == null || characterSlots.Length < 2)
                throw new ArgumentException("characterSlots must have at least 2 slots", "characterSlots");

            CharChangeEquip121ItemPair[] pairs = CharacterInfo.Pairs;
            for (int i = 0; i < 5; i++)
                equipmentList[i] = ToLegacyItemId(pairs[i].ItemId);

            characterTemps[0] = ToLegacyItemId(pairs[8].ItemId);

            for (int i = 5; i < 13; i++)
                equipmentList[i] = ToLegacyItemId(pairs[i + 4].ItemId);

            characterTemps[1] = ToLegacyItemId(pairs[17].ItemId);

            characterSlots[0] = CharacterSlots.Length > 0 ? CharacterSlots[0] : 0;
            characterSlots[1] = CharacterSlots.Length > 1 ? CharacterSlots[1] : 0;
        }

        /// <summary>
        /// pairs[5] = Arma Especial 2 (slot extra de special no client 121/122).
        /// </summary>
        internal int GetWeaponSpecial2()
        {
            CharChangeEquip121ItemPair[] pairs = CharacterInfo.Pairs;
            if (pairs == null || pairs.Length <= 5)
                return 0;
            return ToLegacyItemId(pairs[5].ItemId);
        }

        internal static int ToLegacyItemId(uint value)
        {
            if (value > int.MaxValue)
                return 0;
            return (int)value;
        }
    }

    internal static class CharChangeEquip121Parser
    {
        internal static CharChangeEquip121Request ParsePacket(byte[] packet)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");
            if (packet.Length < 4)
                throw new FormatException("CHAR_CHANGE_EQUIP packet is shorter than opcode+seed");

            byte[] body = new byte[packet.Length - 4];
            Buffer.BlockCopy(packet, 4, body, 0, body.Length);
            return ParseBody(body);
        }

        internal static CharChangeEquip121Request ParseBody(byte[] body)
        {
            if (body == null)
                throw new ArgumentNullException("body");

            int offset = 0;
            CharChangeEquip121ItemPair[] itemInfo6 = ReadItemPairs(body, ref offset, 6, "ITEM_INFO[6]");
            bool itemInfo6Changed = ReadBool(body, ref offset, "ITEM_INFO[6] changed");
            CharChangeEquip121ItemPair accessory = ReadItemPair(body, ref offset, "ITEM_INFO[1]");
            bool accessoryChanged = ReadBool(body, ref offset, "ITEM_INFO[1] changed");
            uint[] uint39A = ReadUIntArray(body, ref offset, 39, "uint[39] A");
            ushort uint39AMarker = ReadUShort(body, ref offset, "uint[39] A marker");
            uint[] uint39B = ReadUIntArray(body, ref offset, 39, "uint[39] B");
            ushort uint39BMarker = ReadUShort(body, ref offset, "uint[39] B marker");
            CharChangeEquip121Info characterInfo = ReadCharacterInfo(body, ref offset);
            bool characterEquipmentChanged = ReadBool(body, ref offset, "S2_CHAR_CHANGE_EQUIP_INFO changed");
            CharChangeEquip121ItemPair[] itemInfo3 = ReadItemPairs(body, ref offset, 3, "ITEM_INFO[3]");
            bool itemEquipmentChanged = ReadBool(body, ref offset, "ITEM_INFO[3] changed");
            byte[] slots = ReadByteArray(body, ref offset, 2, "uchar[2]");

            if (offset != body.Length)
                throw new FormatException("CHAR_CHANGE_EQUIP body has " + (body.Length - offset) + " trailing bytes");

            return new CharChangeEquip121Request(
                offset,
                itemInfo6,
                itemInfo6Changed,
                accessory,
                accessoryChanged,
                uint39A,
                uint39AMarker,
                uint39B,
                uint39BMarker,
                characterInfo,
                characterEquipmentChanged,
                itemInfo3,
                itemEquipmentChanged,
                slots);
        }

        private static CharChangeEquip121ItemPair[] ReadItemPairs(byte[] body, ref int offset, int maxCount, string label)
        {
            int count = ReadCount(body, ref offset, maxCount, label);
            CharChangeEquip121ItemPair[] values = new CharChangeEquip121ItemPair[count];
            for (int i = 0; i < count; i++)
                values[i] = ReadItemPair(body, ref offset, label + "[" + i + "]");
            return values;
        }

        private static CharChangeEquip121ItemPair ReadItemPair(byte[] body, ref int offset, string label)
        {
            uint itemId = ReadUInt(body, ref offset, label + ".itemId");
            uint objectId = ReadUInt(body, ref offset, label + ".objectId");
            return new CharChangeEquip121ItemPair(itemId, objectId);
        }

        private static uint[] ReadUIntArray(byte[] body, ref int offset, int maxCount, string label)
        {
            int count = ReadCount(body, ref offset, maxCount, label);
            uint[] values = new uint[count];
            for (int i = 0; i < count; i++)
                values[i] = ReadUInt(body, ref offset, label + "[" + i + "]");
            return values;
        }

        private static byte[] ReadByteArray(byte[] body, ref int offset, int maxCount, string label)
        {
            int count = ReadCount(body, ref offset, maxCount, label);
            Ensure(body, offset, count, label);
            byte[] values = new byte[count];
            Buffer.BlockCopy(body, offset, values, 0, count);
            offset += count;
            return values;
        }

        private static CharChangeEquip121Info ReadCharacterInfo(byte[] body, ref int offset)
        {
            Ensure(body, offset, 145, "S2_CHAR_CHANGE_EQUIP_INFO");
            byte flag = body[offset++];
            CharChangeEquip121ItemPair[] pairs = new CharChangeEquip121ItemPair[18];
            for (int i = 0; i < pairs.Length; i++)
                pairs[i] = ReadItemPair(body, ref offset, "S2_CHAR_CHANGE_EQUIP_INFO[" + i + "]");
            return new CharChangeEquip121Info(flag, pairs);
        }

        private static int ReadCount(byte[] body, ref int offset, int maxCount, string label)
        {
            int count;
            if (maxCount < 255)
            {
                Ensure(body, offset, 1, label + ".count");
                count = body[offset++];
            }
            else if (maxCount < 0xFFFF)
            {
                count = ReadUShort(body, ref offset, label + ".count");
            }
            else
            {
                throw new FormatException(label + " has unsupported count capacity " + maxCount);
            }

            if (count > maxCount)
                throw new FormatException(label + " count " + count + " exceeds max " + maxCount);
            return count;
        }

        private static bool ReadBool(byte[] body, ref int offset, string label)
        {
            Ensure(body, offset, 1, label);
            return body[offset++] != 0;
        }

        private static ushort ReadUShort(byte[] body, ref int offset, string label)
        {
            Ensure(body, offset, 2, label);
            ushort value = (ushort)(body[offset] | (body[offset + 1] << 8));
            offset += 2;
            return value;
        }

        private static uint ReadUInt(byte[] body, ref int offset, string label)
        {
            Ensure(body, offset, 4, label);
            uint value =
                (uint)(body[offset]
                | (body[offset + 1] << 8)
                | (body[offset + 2] << 16)
                | (body[offset + 3] << 24));
            offset += 4;
            return value;
        }

        private static void Ensure(byte[] body, int offset, int length, string label)
        {
            if (offset < 0 || length < 0 || offset + length > body.Length)
                throw new FormatException("CHAR_CHANGE_EQUIP body ended while reading " + label);
        }
    }
}
