// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_USER_GIFTLIST_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.Utility;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_USER_GIFTLIST_ACK : AuthServerPacket
    {
        private readonly int Field0;
        private readonly List<MessageModel> Field1;

        public PROTOCOL_BASE_USER_GIFTLIST_ACK(int A_1, List<MessageModel> A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        
        public override void Write()
        {
            this.WriteH((short)1042);
            this.WriteC((byte)0);
            this.WriteC((byte)this.Field1.Count);
            for (int index = 0; index < this.Field1.Count; ++index)
            {
                MessageModel messageModel = this.Field1[index];
            }
            this.WriteD(uint.Parse(DateTimeUtil.Now("yyMMddHHmm")));
        }
    }
}