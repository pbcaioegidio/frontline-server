// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_SHOP_DELETE_ITEM_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SHOP_DELETE_ITEM_ACK : GameServerPacket
    {
        private readonly long Field0;
        private readonly uint Field1;

        public PROTOCOL_AUTH_SHOP_DELETE_ITEM_ACK(uint A_1, long A_2 = 0)
        {
            this.Field1 = A_1;
            if (A_1 != 1U)
                return;
            this.Field0 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)1059);
            this.WriteD(this.Field1);
            this.WriteD((int)this.Field0);
        }
    }
}
