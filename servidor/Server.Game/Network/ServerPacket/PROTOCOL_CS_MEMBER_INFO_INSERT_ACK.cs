// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_MEMBER_INFO_INSERT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_MEMBER_INFO_INSERT_ACK : GameServerPacket
    {
        private readonly Account Field0;

        public PROTOCOL_CS_MEMBER_INFO_INSERT_ACK(Account A_1) => this.Field0 = A_1;

        
        public override void Write()
        {
            this.WriteH((short)847);
            this.WriteC((byte)(this.Field0.Nickname.Length + 1));
            this.WriteN(this.Field0.Nickname, this.Field0.Nickname.Length + 2, "UTF-16LE");
            this.WriteQ(this.Field0.PlayerId);
            this.WriteQ(ComDiv.GetClanStatus(this.Field0.Status, this.Field0.IsOnline));
            this.WriteC((byte)this.Field0.Rank);
            this.WriteC((byte)this.Field0.NickColor);
            this.WriteD(this.Field0.Statistic.Clan.MatchWins);
            this.WriteD(this.Field0.Statistic.Clan.MatchLoses);
            this.WriteD(this.Field0.Equipment.NameCardId);
            this.WriteC((byte)this.Field0.Bonus.NickBorderColor);
            this.WriteD(10);
            this.WriteD(20);
            this.WriteD(30);
        }
    }
}