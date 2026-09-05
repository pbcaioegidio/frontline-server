// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_DAILY_RECORD_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Models;

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_DAILY_RECORD_ACK : AuthServerPacket
    {
        private readonly StatisticDaily Field0;
        private readonly byte Field1;
        private readonly uint Field2;

        public PROTOCOL_BASE_DAILY_RECORD_ACK(StatisticDaily A_1, byte A_2, uint A_3)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field2 = A_3;
        }

        public override void Write()
        {
            this.WriteH((short)2415);
            this.WriteH((ushort)this.Field0.Matches);
            this.WriteH((ushort)this.Field0.MatchWins);
            this.WriteH((ushort)this.Field0.MatchLoses);
            this.WriteH((ushort)this.Field0.MatchDraws);
            this.WriteH((ushort)this.Field0.KillsCount);
            this.WriteH((ushort)this.Field0.HeadshotsCount);
            this.WriteH((ushort)this.Field0.DeathsCount);
            this.WriteD(this.Field0.ExpGained);
            this.WriteD(this.Field0.PointGained);
            this.WriteD(this.Field0.Playtime);
            this.WriteC(this.Field1);
            this.WriteD(this.Field2);
            // Client 122 contract (sub_EBE93A / Battle__ParseGiveup): the stats struct is
            // 33 bytes (7H + 3D + C + D + H tail), then a 4-byte flag, then a 9-byte
            // block (D id + C flag + D value). The writer stopped at 33 total, starving
            // the client's 46-byte post-opcode read by 15 bytes. Zero values keep the
            // client-side indicator paths inert.
            this.WriteH(0);
            this.WriteD(0);
            this.WriteD(0);
            this.WriteC(0);
            this.WriteD(0);
        }
    }
}