namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, handler 0xEF1302): opcode 6941 derives
    // S2MOPacketBaseResultT<0x1B1D> with no field nodes -> body is the u32 result.
    public class PROTOCOL_CLAN_WAR_REMOVE_MERCENARY_ACK : GameServerPacket
    {
        private readonly uint Result;

        public PROTOCOL_CLAN_WAR_REMOVE_MERCENARY_ACK(uint result = 0U) => this.Result = result;

        public override void Write()
        {
            this.WriteH((short)6941);
            this.WriteD(this.Result);
        }
    }
}
