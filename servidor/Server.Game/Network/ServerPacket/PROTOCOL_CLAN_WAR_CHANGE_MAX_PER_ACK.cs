namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor at the 0x1B0F site): opcode 6927 derives
    // S2MOPacketBaseResultT<0x1B0F> and registers a single S2MOValue<uchar,1>
    // -> body is `u32 result` then one byte (the team's new max players).
    // The 068 body wrote a whole team+leader blob here and was used as the
    // "match propose" push; the 122 client parses exactly 5 bytes.
    public class PROTOCOL_CLAN_WAR_CHANGE_MAX_PER_ACK : GameServerPacket
    {
        private readonly uint Result;
        private readonly byte MaxPlayers;

        public PROTOCOL_CLAN_WAR_CHANGE_MAX_PER_ACK(uint result, byte maxPlayers = 0)
        {
            this.Result = result;
            this.MaxPlayers = maxPlayers;
        }

        public override void Write()
        {
            this.WriteH((short)6927);
            this.WriteH((short) 0);
            this.WriteD(this.Result);
            this.WriteC(this.MaxPlayers);
        }
    }
}
