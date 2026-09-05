namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, handler 0xEF0A50): opcode 6925 derives
    // S2MOPacketBaseResultT<0x1B0D> and registers one S2MOStringW<64>
    // -> body is `u32 result` then `[u8 count][wchar_t x count]`, count <= 64.
    public class PROTOCOL_CLAN_WAR_CHANGE_OPERATION_ACK : GameServerPacket
    {
        public const int MaxChars = 64;

        private readonly uint Result;
        private readonly string Operation;

        public PROTOCOL_CLAN_WAR_CHANGE_OPERATION_ACK(uint result, string operation = null)
        {
            this.Result = result;
            this.Operation = operation ?? string.Empty;
        }

        public override void Write()
        {
            this.WriteH((short)6925);
            this.WriteD(this.Result);
            if (this.Result != 0U)
                return;
            this.WriteStrW(this.Operation, MaxChars);
        }
    }
}
