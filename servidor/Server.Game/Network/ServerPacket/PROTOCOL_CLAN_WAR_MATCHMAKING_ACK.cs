namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, docs/re/CLANWAR_122_CONTRACT.md): opcode 6933, class
    // PACKET_CLAN_WAR_MATCHMAKING_ACK derives S2MOPacketBaseResultT<0x1B15> and
    // registers no field nodes -> the whole body is the u32 result.
    // Handler 0xEF1230. Replaces the 068 PROTOCOL_CLAN_WAR_MATCH_PROPOSE_ACK (1554),
    // an opcode the 122 client has no parser for.
    public class PROTOCOL_CLAN_WAR_MATCHMAKING_ACK : GameServerPacket
    {
        private readonly uint Result;

        public PROTOCOL_CLAN_WAR_MATCHMAKING_ACK(uint result) => this.Result = result;

        public override void Write()
        {
            this.WriteH((short)6933);
            this.WriteD(this.Result);
        }
    }
}
