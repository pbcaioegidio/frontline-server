// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Auth.Data.Models;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK : AuthServerPacket
    {
        private readonly ulong Field0;
        private readonly Account Field1;

        public PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(Account A_1)
        {
            this.Field1 = A_1;
            this.Field0 = ComDiv.GetClanStatus(A_1.Status, A_1.IsOnline);
        }

        public PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(Account A_1, FriendState A_2)
        {
            this.Field1 = A_1;
            this.Field0 = A_2 == FriendState.None ? ComDiv.GetClanStatus(A_1.Status, A_1.IsOnline) : ComDiv.GetClanStatus(A_2);
        }

        public override void Write()
        {
            this.WriteH((short)851);
            this.WriteQ(this.Field1.PlayerId);
            this.WriteQ(this.Field0);
        }
    }
}