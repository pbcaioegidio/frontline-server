// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_SHOP_RECV_GIFT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_AUTH_SHOP_RECV_GIFT_ACK : GameServerPacket
    {
        private readonly MessageModel Field0;

        public PROTOCOL_AUTH_SHOP_RECV_GIFT_ACK(MessageModel A_1) => this.Field0 = A_1;

        
        public override void Write()
        {
            this.WriteH((short)1086);
            this.WriteD((uint)this.Field0.ObjectId);
            this.WriteD((uint)this.Field0.SenderId);
            this.WriteD((int)this.Field0.State);
            this.WriteD((uint)this.Field0.ExpireDate);
            this.WriteC((byte)(this.Field0.SenderName.Length + 1));
            this.WriteS(this.Field0.SenderName, this.Field0.SenderName.Length + 1);
            this.WriteC((byte)6);
            this.WriteS("EVENT", 6);
        }
    }
}
