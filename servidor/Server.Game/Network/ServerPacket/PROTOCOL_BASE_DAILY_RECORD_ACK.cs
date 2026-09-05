// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_DAILY_RECORD_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_DAILY_RECORD_ACK : GameServerPacket
    {
        private readonly StatisticDaily statisticDaily;
        private readonly byte Field1;
        private readonly uint Field2;

        public PROTOCOL_BASE_DAILY_RECORD_ACK(StatisticDaily statistic, byte A_2, uint A_3)
        {
            statisticDaily = statistic;
            Field1 = A_2;
            Field2 = A_3;
        }

        public override void Write()
        {
            WriteH(2415);
            WriteH((ushort)(statisticDaily?.Matches ?? 0));
            WriteH((ushort)(statisticDaily?.MatchWins ?? 0));
            WriteH((ushort)(statisticDaily?.MatchLoses ?? 0));
            WriteH((ushort)(statisticDaily?.MatchDraws ?? 0));
            WriteH((ushort)(statisticDaily?.KillsCount ?? 0));
            WriteH((ushort)(statisticDaily?.HeadshotsCount ?? 0));
            WriteH((ushort)statisticDaily.DeathsCount);
            WriteD(statisticDaily?.ExpGained ?? 0);
            WriteD(statisticDaily?.PointGained ?? 0);
            WriteD(statisticDaily?.Playtime ?? 0);
            WriteC(Field1);
            WriteD(Field2);
            // Client 122 contract (sub_EBE93A / Battle__ParseGiveup): the stats struct is
            // 33 bytes (7H + 3D + C + D + H tail), then a 4-byte flag, then a 9-byte
            // block (D id + C flag + D value). The writer stopped at 33 total, starving
            // the client's 46-byte post-opcode read by 15 bytes. Zero values keep the
            // client-side indicator paths inert.
            WriteH(0);
            WriteD(0);
            WriteD(0);
            WriteC(0);
            WriteD(0);
        }
    }
}