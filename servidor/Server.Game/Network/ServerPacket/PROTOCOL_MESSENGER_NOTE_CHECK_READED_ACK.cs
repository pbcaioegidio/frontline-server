// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_MESSENGER_NOTE_CHECK_READED_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_MESSENGER_NOTE_CHECK_READED_ACK : GameServerPacket
    {
        private readonly List<int> Field0;

        public PROTOCOL_MESSENGER_NOTE_CHECK_READED_ACK(List<int> A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)1927);
            this.WriteC((byte)this.Field0.Count);
            for (int index = 0; index < this.Field0.Count; ++index)
                this.WriteD(this.Field0[index]);
        }
    }
}