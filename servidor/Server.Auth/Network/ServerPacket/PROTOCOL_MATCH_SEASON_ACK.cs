namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_MATCH_SEASON_ACK : AuthServerPacket
    {
        private const int RecordSize = 121;
        private const int RecordsPerMatchVersion = 2;
        private const int StateOffset = 14;

        private const byte StateNone = 255;
        private const byte StateOngoing = 1;
        private const byte StateEnded = 2;

        private const byte SoloSeasonState = StateOngoing;
        private const byte ClanSeasonState = StateEnded;

        public override void Write()
        {
            WriteH((short)7698);
            WriteH((short)0);

            WriteMatchVersion(ClanSeasonState);
            WriteMatchVersion(SoloSeasonState);
        }

        private void WriteMatchVersion(byte typeZeroState)
        {
            WriteC((byte)RecordsPerMatchVersion);
            WriteB(Record(typeZeroState));
            WriteB(Record(StateNone));
        }

        private static byte[] Record(byte state)
        {
            byte[] record = new byte[RecordSize];
            record[StateOffset] = state;
            return record;
        }
    }
}
