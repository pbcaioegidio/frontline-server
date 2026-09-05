// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_AUTH_SHOP_CAPSULE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using System;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    // 121 reward-result popup ACK (opcode 1067). Client struct (PACKET_AUTH_SHOP_CAPSULE_ACK,
    // walker order head->tail = C,B,A), proven from the S2MO field deserializers:
    //   field C  S2MOValue<u8,20>  = [len:u8][len bytes]  -> won-reward indices (position in
    //                                                        the box reward list / 2501 order)
    //   field B  S2MOValue<u8,1>   = [u8]                 -> count (loop bound, == len)
    //   field A  S2MOValue<u32,1>  = [u32]                -> couponId / boxId
    // Scalars <T,1> are fixed (no length prefix); <u8,N> arrays carry a 1-byte length prefix.
    public class PROTOCOL_AUTH_SHOP_CAPSULE_ACK : GameServerPacket
    {
        private const int MaxIndices = 20;

        private readonly List<int> Indices;
        private readonly int CouponId;

        public PROTOCOL_AUTH_SHOP_CAPSULE_ACK(List<int> indices, int couponId)
        {
            this.Indices = indices ?? new List<int>();
            this.CouponId = couponId;
        }

        public override void Write()
        {
            this.WriteH((short)1067);
            this.WriteH((short)0);
            int count = Math.Min(this.Indices.Count, MaxIndices);
            // field C: S2MOValue<u8,20> -> [len][index bytes]
            this.WriteC((byte)count);
            for (int i = 0; i < count; i++)
                this.WriteC((byte)this.Indices[i]);
            // field B: S2MOValue<u8,1> -> count
            this.WriteC((byte)count);
            // field A: S2MOValue<u32,1> -> couponId / boxId
            this.WriteD(this.CouponId);
        }
    }
}
