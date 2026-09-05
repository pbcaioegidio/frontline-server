using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    // LOBBY_STAGE_RULE_ACK (opcode 2566). Client handler
    // RoomInfoOptionBlock__HandleStageRulePacket @0xEC8072 (122 IDB) reads:
    //   status (D, gate <= -1 => empty/no-room, stop) ; mySlotIdx (D => SetMySlotIdx)
    //   ; roomInfoBasic (71B) => ParseStageRuleInfo ; roomInfoOptBlock (175B)
    //   ; if GameMode_IsPVPMode(): 2 trailing bytes (battle round-stat slots).
    // The 246B block is byte-identical to ROOM_CREATE_ACK (3593). Sent in reply to
    // LOBBY_STAGE_RULE_REQ (2565) during the room create/enter phase.
    // The 2-byte PVP tail is always emitted as zeros: PVP clients read them
    // (round stats = 0 pre-match); non-PVP clients ignore the trailing bytes
    // (length-framed protocol), so no bound-check failure in either mode.
    public class PROTOCOL_LOBBY_STAGE_RULE_ACK : GameServerPacket
    {
        private readonly RoomModel Room;
        private readonly int SlotIdx;

        public PROTOCOL_LOBBY_STAGE_RULE_ACK(RoomModel room, int slotIdx)
        {
            this.Room = room;
            this.SlotIdx = slotIdx;
        }

        public override void Write()
        {
            this.WriteH((short)2566);
            if (this.Room == null)
            {
                this.WriteD(-1);
                return;
            }
            this.WriteD(0);
            this.WriteD(this.SlotIdx);
            this.WriteRoomInfoBlock(this.Room);
            this.WriteC(0);
            this.WriteC(0);
        }
    }
}
