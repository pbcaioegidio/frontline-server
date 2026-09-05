// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_USER_LEAVE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_USER_LEAVE_ACK : GameServerPacket
    {
        private readonly int Field0;

        public PROTOCOL_BASE_USER_LEAVE_ACK(int A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)2329);
            this.WriteD(this.Field0);
            this.WriteH((short)0);
        }
    }
}
