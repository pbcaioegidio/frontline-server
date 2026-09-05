namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, ctor at the 0x1B25 site, handler 0xEF0BC9): opcode 6949
    // derives S2MOPacketBaseResultT<0x1B25> and registers one S2MOValue<ushort,1>
    // -> body is `u32 result` then a u16. The u16 is the mercenary_list_id of the
    // player who refused, so the inviting leader can drop that board row.
    public class PROTOCOL_CLAN_WAR_INVITE_DENIAL_ACK : GameServerPacket
    {
        private readonly uint Result;
        private readonly ushort MercenaryListId;

        public PROTOCOL_CLAN_WAR_INVITE_DENIAL_ACK(uint result, int mercenaryListId = 0)
        {
            this.Result = result;
            this.MercenaryListId = (ushort)mercenaryListId;
        }

        public override void Write()
        {
            this.WriteH((short)6949);
            this.WriteH((short) 0);
            this.WriteD(this.Result);
            this.WriteH(this.MercenaryListId);
        }
    }
}
