using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_GET_ACEMODE_PLAYERINFO_ACK : GameServerPacket
    {
        private readonly Account account;
        private readonly StatisticAcemode Field1;
        private readonly ClanModel Field2;

        public PROTOCOL_ROOM_GET_ACEMODE_PLAYERINFO_ACK(Account account)
        {
            this.account = account;
            if (account != null)
            {
                Field1 = account.Statistic.Acemode;
                Field2 = ClanManager.GetClan(account.ClanId);
            }
        }

        // 122 wire layout, 404B. Recovered from the S2MO member list: sub_ED86A8 (the packet
        // ctor) prepends S2MOValue<USER_INFO_ACEMODE_RECORD,1> onto the list built by
        // PACKET_ROOM_GET_PLAYERINFO_BASE__ctor 0xED8728, and the serializer walks the list
        // head to tail, i.e. in reverse registration order. The same model applied to 3597
        // reproduces the official capture byte for byte (472B), which is what validates it.
        // Node sizes are confirmed by the handler sub_EDC108: qmemcpy 0x24 out of object+480
        // (the acemode record) and qmemcpy 0xB9 out of object+52 (USER_INFO_BASIC).
        //   Order: header(8) ACEMODE_RECORD(36) ITEM_INFO[3](25) CHAR_EQUIP_INFO(144)
        //          uchar[3](4) USER_INFO_BASIC(185) uchar[1](1) = 403.
        // Node sizes come from each S2MOValue deserializer: 0xEBD0CC reports 36 for the acemode
        // record, 0xEB07C6 144, 0xEBD0F3 185, 0x9B141E 1, and the counted ones 0xEBC62F /
        // 0x922B5B report 1 + count*elem. The walker sub_ED936B only sums those, so the wire is
        // exactly their concatenation after the 8-byte header.
        // 3597 carries one extra byte past its reader, but that is known only because two
        // official 472B frames exist for it (artifacts/official-3597). There is NO official
        // 3682 capture, so emitting a matching pad here would be inventing a field to confirm
        // our own guess. We emit exactly the 403 bytes the reader consumes.
        // The previous writer emitted header(8) + 9 dwords + 122 zero bytes + a 173-byte block,
        // so the client's 185-byte bulk read ran past the end of the struct.
        // UNKNOWN: only the SIZE of USER_INFO_ACEMODE_RECORD is established (36B, from the
        // 0x24 qmemcpy). Which of the nine dwords is matches / wins / kills is not: the order
        // below is the one this writer already had and is carried over unchanged, not a new
        // claim. It stays unverified until the record's consumers are mapped.
        public override void Write()
        {
            WriteH((short)3682);                       // @0
            WriteH((short)0);                          // @2
            WriteD(0);                                 // @4  status; 0xEDC143 requires >= 0
            // @8 USER_INFO_ACEMODE_RECORD (36B)
            WriteD(Field1.Matches);
            WriteD(Field1.MatchWins);
            WriteD(Field1.MatchLoses);
            WriteD(Field1.Kills);
            WriteD(Field1.Deaths);
            WriteD(Field1.Headshots);
            WriteD(Field1.Assists);
            WriteD(Field1.Escapes);
            WriteD(Field1.Winstreaks);
            // @44 ITEM_INFO[3] + @69 CHAR_EQUIP_INFO + @213 uchar[3]
            WriteRoomEquipBlocks(account);
            // @217 USER_INFO_BASIC (185B)
            WriteUserInfoBasicBlock(account, Field2);
            // @402 S2MOValue<unsigned char,1>: 0x9B141E consumes one byte, no count prefix.
            // Last byte of the frame. UNKNOWN semantic, and no consumer: sub_EDC108 keeps only
            // the status at object+20, the 185B block at object+52 and the 36B record at
            // object+480, never the node payload this byte lands in.
            WriteC((byte)0);
        }
    }
}