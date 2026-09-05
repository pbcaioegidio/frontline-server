// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_RECV_WHISPER_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using System.Runtime.CompilerServices;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_RECV_WHISPER_ACK : GameServerPacket
    {
        private readonly string Field0;
        private readonly string Field1;
        private readonly bool Field2;

        public PROTOCOL_AUTH_RECV_WHISPER_ACK(string A_1, string A_2, bool A_3)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field2 = A_3;
        }

        public override void Write()
        {
            WriteH((short)1830);
            WriteU(Field0, 66);
            WriteC(Field2 ? (byte)1 : (byte)0);
            WriteC((byte)0);
            WriteH((ushort)(Field1.Length + 1));
            WriteN(Field1, Field1.Length + 2, "UTF-16LE");
            WriteC(0);
        }
    }
}