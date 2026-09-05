// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly ulong Field1;

        public PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(Account A_1)
        {
            this.Field0 = A_1;
            if (A_1 == null)
                return;
            this.Field1 = ComDiv.GetClanStatus(A_1.Status, A_1.IsOnline);
        }

        public PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(Account A_1, FriendState A_2)
        {
            this.Field0 = A_1;
            if (A_1 == null)
                return;
            this.Field1 = A_2 == FriendState.None ? ComDiv.GetClanStatus(A_1.Status, A_1.IsOnline) : ComDiv.GetClanStatus(A_2);
        }

        public override void Write()
        {
            this.WriteH((short)851);
            this.WriteQ(this.Field0.PlayerId);
            this.WriteQ(this.Field1);
        }
    }
}