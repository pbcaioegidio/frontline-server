using Plugin.Core.Models;
using Server.Game.Data.Models;
using System.Collections.Generic;

namespace Server.Game.Network.ServerPacket
{
    // S2MO PACKET_INVENTORY_MAX_UP_ACK, opcode 3337 (0xD09).
    //
    // Client 122 ground truth:
    //   ctor  0xECD583 - members are linked head-first, so the wire order is the REVERSE of
    //         construction: [uint,1]@+0x18, [ushort,1]@+0x28, [ushort,1]@+0x38,
    //         [INVEN_BUFFER,608]@+0x48  ->  wire: array, ushort, ushort, uint.
    //   vf2   0xECD826 - writes the status dword FIRST (size += 4) and stops when status < 0.
    //   vf3   0xECD988 - mirror on the read side (this[5] = *a2; if (v2 < 0) return).
    //   walk  0xECD91F / 0xECDA74 - iterate from head (+0x10) following next (+4).
    //   frame S2MOPacketBase__Serialize 0x1046330 - body starts at +6: [size:H][opcode:H][pad:H].
    //         Writers here emit from the opcode on; the length prefix is added by the transport.
    //   array S2MOValue<INVEN_BUFFER,608>::Deserialize 0xEBCB02 - count then count*13 bytes,
    //         rejected above 608. The count width comes from the CAPACITY, not the value
    //         (0x10462C0: capacity >= 255 -> 2 bytes), so it is a ushort. No padding.
    //   Handler 0xECDF1E consumes: array -> InvenList__ProcessInventoryUpdate,
    //         ushort@+0x34 -> sub_889ECB, which does `v3 += a1` at 0x889EF6, i.e. the field is a
    //         uint@+0x24 -> sub_88AD4A, which writes account-struct offset 121 = the CASH slot
    //         (UITopMenu__RefreshMoney 0x8F4DFE binds 121 to STR_SHOP_INFO_CASH), so this field
    //         carries Cash, not Gold. The remaining ushort@+0x44 is never read by the handler.
    public class PACKET_INVENTORY_MAX_UP_ACK : GameServerPacket
    {
        // S2MOValue<INVEN_BUFFER,608>::Deserialize rejects any count above this.
        private const int MaxBufferRows = 608;

        public const int ERROR_GENERIC = -1;
        public const int ERROR_MAX_CAPACITY_REACHED = -2;
        // 0x80002760: the only error code the 122 handler special-cases, it shows
        // STBL_IDX_EP_SHOP_BUY_FAIL_CAPTION_E (0xECE00B).
        public const int ERROR_SHOP_BUY_FAIL = unchecked((int)0x80002760);

        private readonly int resultCode;
        private readonly ushort slotsDelta;
        private readonly uint cash;
        private readonly List<ItemsModel> items;

        public PACKET_INVENTORY_MAX_UP_ACK(Account player, ushort addedSlots, List<ItemsModel> updatedItems = null)
        {
            this.resultCode = 0;
            this.slotsDelta = addedSlots;
            this.cash = (uint)player.Cash;
            this.items = updatedItems ?? new List<ItemsModel>();
        }

        public PACKET_INVENTORY_MAX_UP_ACK(int errorCode)
        {
            this.resultCode = errorCode;
            this.slotsDelta = 0;
            this.cash = 0;
            this.items = new List<ItemsModel>();
        }

        public override void Write()
        {
            this.WriteH((ushort)3337);
            this.WriteH((ushort)0);
            this.WriteD(this.resultCode);

            // vf2 stops right after the status on failure; anything past it would be trailing junk.
            if (this.resultCode < 0)
                return;

            int count = this.items.Count > MaxBufferRows ? MaxBufferRows : this.items.Count;
            this.WriteH((ushort)count);
            for (int i = 0; i < count; i++)
            {
                ItemsModel item = this.items[i];
                this.WriteD((uint)item.ObjectId);
                this.WriteD(item.Id);
                this.WriteC((byte)item.Equip);
                this.WriteD(item.Count);
            }

            this.WriteH((ushort)0);
            this.WriteH(this.slotsDelta);
            this.WriteD(this.cash);
        }
    }
}
