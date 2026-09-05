// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_UNKNOWN_PACKET_1803_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_UNKNOWN_PACKET_1803_ACK : GameServerPacket
    {
        private readonly string Field0;
        private readonly string Field1;

        public PROTOCOL_BASE_UNKNOWN_PACKET_1803_ACK(string A_1, string A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        
        public override void Write()
        {
            this.WriteH((short)1803);
            this.WriteD(94767);
            this.WriteD(100950);
            this.WriteD(0);
            this.WriteD(983299);
            this.WriteC((byte)0);
            this.WriteC((byte)0);
            this.WriteC((byte)0);
            this.WriteC((byte)3);
            this.WriteC((byte)8);
            this.WriteB(Bitwise.HexStringToByteArray("47 00 4D 00 00 00 45 00 56 00 45 00 4E 00 54 00 5F 00 38 00 00 00"));
            this.WriteD(56);
            this.WriteC((byte)1);
            this.WriteD(180214952);
            this.WriteB(Bitwise.HexStringToByteArray("81 E0 D0 03 09 04 15 00 80 22"));
        }
    }
}