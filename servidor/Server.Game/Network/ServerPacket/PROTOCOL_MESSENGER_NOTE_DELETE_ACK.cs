// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_MESSENGER_NOTE_DELETE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_MESSENGER_NOTE_DELETE_ACK : GameServerPacket
    {
        private readonly uint Field0;
        private readonly List<object> Field1;

        public PROTOCOL_MESSENGER_NOTE_DELETE_ACK(uint A_1, List<object> A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)1929);
            this.WriteD(this.Field0);
            this.WriteC((byte)this.Field1.Count);
            foreach (long num in this.Field1)
                this.WriteD((uint)num);
        }
    }
}