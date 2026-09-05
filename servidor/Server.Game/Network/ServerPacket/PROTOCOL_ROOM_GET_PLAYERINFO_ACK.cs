using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_GET_PLAYERINFO_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly PlayerInventory Field1;
        private readonly PlayerEquipment Field2;
        private readonly ClanModel Field3;

        public PROTOCOL_ROOM_GET_PLAYERINFO_ACK(Account A_1)
        {
            this.Field0 = A_1;
            if (A_1 == null)
                return;
            this.Field1 = A_1.Inventory;
            this.Field2 = A_1.Equipment;
            this.Field3 = ClanManager.GetClan(A_1.ClanId);
        }

        public override void Write()
        {
            // 122 wire layout (ROOM_GET_PLAYERINFO_BASE, 472B), reconstructed from the official
            // capture (artifacts/official-3597) + client read order (122 IDB, handler
            // Room__HandleGetPlayerInfoAck / S2MOValue_struct_USER_INFO_BASIC deserializer 0xEBD0F3).
            // Order: header(8) ITEM_INFO[1](8) USER_INFO_RECORD(96) ITEM_INFO[3](25)
            //        CHAR_EQUIP_INFO(144) uchar[3](4) USER_INFO_BASIC(185) uchar[1](1) pad(1).
            this.WriteH((short)3597);                 // @0  opcode
            this.WriteH((short)0);                    // @2  pad
            this.WriteD(0);                           // @4  status
            // @8 ITEM_INFO[1] (accessory; zeroed [Id][ObjId] when none)
            this.WriteB(this.Field1.EquipmentDataChara(this.Field2.AccessoryId));
            // @16 USER_INFO_RECORD: Season(12) + Basic(12) dwords
            this.WriteD(this.Field0.Statistic.Season.Matches);
            this.WriteD(this.Field0.Statistic.Season.MatchWins);
            this.WriteD(this.Field0.Statistic.Season.MatchLoses);
            this.WriteD(this.Field0.Statistic.Season.MatchDraws);
            this.WriteD(this.Field0.Statistic.Season.KillsCount);
            this.WriteD(this.Field0.Statistic.Season.HeadshotsCount);
            this.WriteD(this.Field0.Statistic.Season.DeathsCount);
            this.WriteD(this.Field0.Statistic.Season.TotalMatchesCount);
            this.WriteD(this.Field0.Statistic.Season.TotalKillsCount);
            this.WriteD(this.Field0.Statistic.Season.EscapesCount);
            this.WriteD(this.Field0.Statistic.Season.AssistsCount);
            this.WriteD(this.Field0.Statistic.Season.MvpCount);
            this.WriteD(this.Field0.Statistic.Basic.Matches);
            this.WriteD(this.Field0.Statistic.Basic.MatchWins);
            this.WriteD(this.Field0.Statistic.Basic.MatchLoses);
            this.WriteD(this.Field0.Statistic.Basic.MatchDraws);
            this.WriteD(this.Field0.Statistic.Basic.KillsCount);
            this.WriteD(this.Field0.Statistic.Basic.HeadshotsCount);
            this.WriteD(this.Field0.Statistic.Basic.DeathsCount);
            this.WriteD(this.Field0.Statistic.Basic.TotalMatchesCount);
            this.WriteD(this.Field0.Statistic.Basic.TotalKillsCount);
            this.WriteD(this.Field0.Statistic.Basic.EscapesCount);
            this.WriteD(this.Field0.Statistic.Basic.AssistsCount);
            this.WriteD(this.Field0.Statistic.Basic.MvpCount);
            // @112 ITEM_INFO[3] + @137 CHAR_EQUIP_INFO + @281 uchar[3]
            this.WriteRoomEquipBlocks(this.Field0);
            // @285 USER_INFO_BASIC (185B). The official capture puts the nickname exactly
            // here, which is what pins the block's position; the byte map inside it lives in
            // GameServerPacket.WriteUserInfoBasicBlock.
            this.WriteUserInfoBasicBlock(this.Field0, this.Field3);
            // @470 S2MOValue<unsigned char,1>. Its deserializer 0x9B141E consumes exactly one
            // byte and reports 1 to the list walker sub_ED936B; unlike S2MOValue<T,N> for N>1
            // (0xEBC62F, 0x922B5B) it reads no count byte, so the value byte is @470 and @471
            // is past the reader. UNKNOWN semantic, and no consumer: Room__HandleGetPlayerInfoAck
            // keeps only the status at object+20, the 185B block at object+52 and the 96B record
            // at object+480, never the node payload. The official capture sends 0 in one frame
            // and 1 in another, both 472B; we send 0.
            this.WriteC((byte)0);
            // @471 the reader stops at 471 of the 472 bytes the official server sends. Emitted
            // to keep the frame length identical to the official one; nothing reads it.
            this.WriteC((byte)0);
        }

    }
}