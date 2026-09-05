// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_COMMISSION_REGULAR_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_COMMISSION_REGULAR_ACK : GameServerPacket
    {
        private readonly uint Field0;

        public PROTOCOL_CS_COMMISSION_REGULAR_ACK(uint A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)840);
            this.WriteD(this.Field0);
        }
    }
}