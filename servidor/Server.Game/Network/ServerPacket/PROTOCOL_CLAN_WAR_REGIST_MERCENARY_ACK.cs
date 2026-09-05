namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor 0xEF1299, handler 0xEF1299): opcode 6939 derives
    // S2MOPacketBaseResultT<0x1B1B> and registers no field nodes (the ctor builds a
    // 0x18-byte stack object with a null field-list head) -> the whole body is the
    // u32 result. It then posts UI message 0x82C2 and nothing else.
    //
    // The 068 body wrote a full team roster here. In 122 the roster push is
    // JOIN_TEAM_ACK (6921); this packet only acknowledges REGIST_MERCENARY_REQ (6938).
    public class PROTOCOL_CLAN_WAR_REGIST_MERCENARY_ACK : GameServerPacket
    {
        private readonly uint Result;

        public PROTOCOL_CLAN_WAR_REGIST_MERCENARY_ACK(uint result = 0U) => this.Result = result;

        public override void Write()
        {
            this.WriteH((short)6939);
            this.WriteD(this.Result);
        }
    }
}
