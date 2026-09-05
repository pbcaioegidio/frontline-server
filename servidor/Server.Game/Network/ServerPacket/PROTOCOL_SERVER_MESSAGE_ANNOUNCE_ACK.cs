// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK : GameServerPacket
    {
        private readonly string Field0;

        public PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(string A_1) => this.Field0 = A_1;

        
        public override void Write()
        {
            this.WriteH((short)3079);
            this.WriteH((short)0);
            this.WriteD(0);
            this.WriteH((ushort)this.Field0.Length);
            this.WriteN(this.Field0, this.Field0.Length, "UTF-16LE");
            this.WriteD(2);
        }
    }
}