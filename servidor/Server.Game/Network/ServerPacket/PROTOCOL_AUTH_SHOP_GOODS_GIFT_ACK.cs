// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_SHOP_GOODS_GIFT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SHOP_GOODS_GIFT_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly List<GoodsItem> Field1;
        private readonly uint Field2;

        public PROTOCOL_AUTH_SHOP_GOODS_GIFT_ACK(uint A_1, List<GoodsItem> A_2, Account A_3)
        {
            this.Field2 = A_1;
            this.Field1 = A_2;
            this.Field0 = A_3;
        }

        public PROTOCOL_AUTH_SHOP_GOODS_GIFT_ACK(uint A_1) => this.Field2 = A_1;

        
        public override void Write()
        {
            this.WriteH((short)1049);
            this.WriteH((short)0);
            if (this.Field2 == 1U)
            {
                this.WriteC((byte)this.Field1.Count);
                foreach (GoodsItem goodsItem in this.Field1)
                {
                    this.WriteD(0);
                    this.WriteD(goodsItem.Id);
                    this.WriteC((byte)0);
                }
                this.WriteD(this.Field0.Cash);
                this.WriteD(this.Field0.Gold);
                this.WriteD(uint.Parse(DateTimeUtil.Now("yyMMddHHmm")));
            }
            else
                this.WriteD(this.Field2);
        }
    }
}
