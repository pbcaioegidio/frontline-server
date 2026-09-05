namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_MISSION_CARD_INFO_STREAM_ACK : AuthServerPacket
    {
        private readonly byte[] Data;
        private readonly byte MissionsCount;
        private readonly short TotalMissions;

        public PROTOCOL_BASE_MISSION_CARD_INFO_STREAM_ACK(byte[] data, short totalMissions, byte missionsCount)
        {
            Data = data;
            TotalMissions = totalMissions;
            MissionsCount = missionsCount;
        }

        public override void Write()
        {
            WriteH(2521);
            WriteH(TotalMissions);
            WriteC(MissionsCount);
            WriteB(Data);
            // Depois do loop de missoes o cliente le dois dwords (MissionCardProc_SetTimerXY).
            WriteD(0);
            WriteD(0);
        }
    }
}
