// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_SHOP_GOODSLIST_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SHOP_GOODSLIST_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly ShopData Field1;

        public PROTOCOL_AUTH_SHOP_GOODSLIST_ACK(ShopData A_1, int A_2)
        {
            this.Field1 = A_1;
            this.Field0 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)1036);
            this.WriteD(this.Field0);
            this.WriteD(this.Field1.ItemsCount);
            this.WriteD(this.Field1.Offset);
            this.WriteB(this.Field1.Buffer);
            this.WriteD(50);
        }
    }
}