namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor 0xEED785): opcode 6923 derives
    // S2MOPacketBaseResultT<0x1B0B> and registers three nodes; wire order
    // (= field-list order = reverse member order) is bool, signed char, signed char.
    // The 068 body stopped at the result dword.
    //
    // Field meaning is INFERENCE: the leave path only has to tell the client whether
    // the team survived and which slot/leader changed, so the server reports
    // teamDisbanded + the leaving slot + the (possibly new) leader slot. The client
    // tolerates the shape either way; only the widths are contract.
    public class PROTOCOL_CLAN_WAR_LEAVE_TEAM_ACK : GameServerPacket
    {
        private readonly uint Result;
        private readonly bool TeamDisbanded;
        private readonly sbyte LeftSlot;
        private readonly sbyte LeaderSlot;

        public PROTOCOL_CLAN_WAR_LEAVE_TEAM_ACK(uint result, bool teamDisbanded = false, int leftSlot = -1, int leaderSlot = -1)
        {
            this.Result = result;
            this.TeamDisbanded = teamDisbanded;
            this.LeftSlot = (sbyte)leftSlot;
            this.LeaderSlot = (sbyte)leaderSlot;
        }

        public override void Write()
        {
            this.WriteH((short)6923);
            this.WriteH((short) 0);
            this.WriteD(this.Result);
            this.WriteC(this.TeamDisbanded ? (byte)1 : (byte)0);
            this.WriteC((byte)this.LeftSlot);
            this.WriteC((byte)this.LeaderSlot);
        }
    }
}
