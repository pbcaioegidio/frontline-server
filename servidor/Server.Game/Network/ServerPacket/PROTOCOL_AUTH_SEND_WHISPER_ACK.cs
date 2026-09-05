// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_SEND_WHISPER_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using System.Runtime.CompilerServices;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SEND_WHISPER_ACK : GameServerPacket
    {
        private readonly string Field0;
        private readonly string Field1;
        private readonly uint Error;

        public PROTOCOL_AUTH_SEND_WHISPER_ACK(string A_1, string A_2, uint Error)
        {
            Field0 = A_1;
            Field1 = A_2;
            this.Error = Error;
        }

        public override void Write()
        {
            WriteH((short)1827);
            WriteD(Error);
            WriteC((byte)Error);
            WriteU(Field0, 66);
            if (Error == 0)
            {
                WriteH((ushort)(Field1.Length + 1));
                WriteN(Field1, Field1.Length + 2, "UTF-16LE");
            }
        }
    }
}