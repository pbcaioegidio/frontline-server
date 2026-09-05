// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_REPLACE_MEMBER_BASIC_RESULT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_REPLACE_MEMBER_BASIC_RESULT_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly ulong Field1;

        public PROTOCOL_CS_REPLACE_MEMBER_BASIC_RESULT_ACK(Account A_1)
        {
            this.Field0 = A_1;
            this.Field1 = ComDiv.GetClanStatus(A_1.Status, A_1.IsOnline);
        }

        public override void Write()
        {
            this.WriteH((short)876);
            this.WriteQ(this.Field0.PlayerId);
            this.WriteU(this.Field0.Nickname, 66);
            this.WriteC((byte)this.Field0.Rank);
            this.WriteC((byte)this.Field0.ClanAccess);
            this.WriteQ(this.Field1);
            this.WriteD(this.Field0.ClanDate);
            this.WriteC((byte)this.Field0.NickColor);
            this.WriteD(0);
            this.WriteD(0);
        }
    }
}