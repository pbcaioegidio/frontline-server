namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_COMMUNITY_USER_REPORT_CONDITION_CHECK_ACK : GameServerPacket
    {
        // Client 122 PACKET_USER_REPORT_CONDITION_CHECK_ACK (ctor 0xEB5780) registers
        // S2MOValue<int> then S2MOValue<unsigned char>; head insertion reverses that, so
        // the wire carries the byte first. Consumer 0xC5472F: a non-zero int shows
        // MSGBOX__ShowReportError, otherwise a non-zero byte opens the report popup and a
        // zero byte shows error 0x80001408.
        private readonly uint _error;
        private readonly bool _canReport;

        public PROTOCOL_COMMUNITY_USER_REPORT_CONDITION_CHECK_ACK(uint error, bool canReport)
        {
            _error = error;
            _canReport = canReport;
        }

        public override void Write()
        {
            this.WriteH((short) 3853);
            this.WriteH((short) 0);
            this.WriteC((byte) (_canReport ? 1 : 0));
            this.WriteD(_error);
        }
    }
}
