namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED): opcode 6935 is dispatched at 0xEF11C7, which builds the
    // PACKET_CLAN_WAR_MATCHMAKING_ACK class (vftable 0x12EE2CC, S2MOPacketBaseResultT
    // with no field nodes) and posts UI message 0x82BF. So the body is a u32 result.
    // The old body was the bare opcode with no result dword.
    public class PROTOCOL_CLAN_WAR_CANCEL_MATCHMAKING_ACK : GameServerPacket
    {
        private readonly uint Result;

        public PROTOCOL_CLAN_WAR_CANCEL_MATCHMAKING_ACK(uint result = 0U) => this.Result = result;

        public override void Write()
        {
            this.WriteH((short)6935);
            this.WriteD(this.Result);
        }
    }
}
