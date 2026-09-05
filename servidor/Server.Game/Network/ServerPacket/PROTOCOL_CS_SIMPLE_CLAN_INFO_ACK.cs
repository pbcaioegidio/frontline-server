// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_SIMPLE_CLAN_INFO_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null

using Plugin.Core.Models;
using Plugin.Core.SQL;
using System.Text;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_SIMPLE_CLAN_INFO_ACK : GameServerPacket
    {
        private readonly int Field0;
        private readonly ClanModel Field1;
        private readonly int Field2;

        public PROTOCOL_CS_SIMPLE_CLAN_INFO_ACK(int A_1, ClanModel A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            if (A_2 != null && A_2.Id > 0)
                this.Field2 = DaoManagerSQL.GetClanPlayers(A_2.Id);
        }

        public override void Write()
        {
            this.WriteH((short)1002);
            this.WriteD(0);
            this.WriteH((short)0);
            this.WriteD(this.Field0);
            if (this.Field0 < 0)
                return;
            if (this.Field1 == null)
                return;

            // Client 121 unpacks the S2MO member chain in reverse construction order.
            this.WriteD((int)this.Field1.Points);
            this.WriteC((byte)this.Field1.NameColor);
            this.WriteD(this.Field1.MatchLoses);
            this.WriteD(this.Field1.MatchWins);
            this.WriteD(this.Field1.Matches);
            this.WriteD(this.Field1.Logo);
            this.WriteC((byte)this.Field1.GetClanUnit(this.Field2));
            this.WriteC((byte)this.Field1.MaxPlayers);
            this.WriteC((byte)this.Field2);
            this.WriteS2MOStringW33(this.Field1.Name);
            this.WriteQ(this.Field1.OwnerId);
            this.WriteC((byte)this.Field1.Rank);
        }

        private void WriteS2MOStringW33(string Value)
        {
            if (Value == null)
                Value = string.Empty;
            if (Value.Length > 33)
                Value = Value.Substring(0, 33);
            this.WriteC((byte)Value.Length);
            if (Value.Length > 0)
                this.WriteB(Encoding.Unicode.GetBytes(Value));
        }
    }
}
