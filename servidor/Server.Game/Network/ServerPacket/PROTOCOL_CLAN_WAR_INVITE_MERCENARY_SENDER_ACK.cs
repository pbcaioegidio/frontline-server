namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, handler 0xEF0D5D): opcode 6943 derives
    // S2MOPacketBaseResultT<0x1B1F> with no field nodes -> body is the u32 result.
    // Sent back to the team leader that issued INVITE_MERCENARY_REQ (6942).
    public class PROTOCOL_CLAN_WAR_INVITE_MERCENARY_SENDER_ACK : GameServerPacket
    {
        private readonly uint Result;

        public PROTOCOL_CLAN_WAR_INVITE_MERCENARY_SENDER_ACK(uint result = 0U) => this.Result = result;

        public override void Write()
        {
            this.WriteH((short)6943);
            this.WriteD(this.Result);
        }
    }
}
