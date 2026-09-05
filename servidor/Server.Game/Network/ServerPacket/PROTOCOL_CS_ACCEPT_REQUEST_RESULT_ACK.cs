// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_ACCEPT_REQUEST_RESULT_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_ACCEPT_REQUEST_RESULT_ACK : GameServerPacket
    {
        private readonly ClanModel Field0;
        private readonly Account Field1;
        private readonly int Field2;

        public PROTOCOL_CS_ACCEPT_REQUEST_RESULT_ACK(ClanModel A_1, Account A_2, int A_3)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
            this.Field2 = A_3;
        }

        public PROTOCOL_CS_ACCEPT_REQUEST_RESULT_ACK(ClanModel A_1, int A_2)
        {
            this.Field0 = A_1;
            this.Field1 = AccountManager.GetAccount(A_1.OwnerId, 31 /*0x1F*/);
            this.Field2 = A_2;
        }

        
        public override void Write()
        {
            this.WriteH((short)824);
            this.WriteD(this.Field0.Id);
            this.WriteU(this.Field0.Name, 34);
            this.WriteC((byte)this.Field0.Rank);
            this.WriteC((byte)this.Field2);
            this.WriteC((byte)this.Field0.MaxPlayers);
            this.WriteD(this.Field0.CreationDate);
            this.WriteD(this.Field0.Logo);
            this.WriteC((byte)this.Field0.NameColor);
            this.WriteC((byte)this.Field0.Effect);
            this.WriteC((byte)this.Field0.GetClanUnit());
            this.WriteD(this.Field0.Exp);
            this.WriteD(10);
            this.WriteQ(this.Field0.OwnerId);
            this.WriteU(this.Field1 != null ? this.Field1.Nickname : "", 66);
            this.WriteC(this.Field1 != null ? (byte)this.Field1.NickColor : (byte)0);
            this.WriteC(this.Field1 != null ? (byte)this.Field1.Rank : (byte)0);
            this.WriteU(this.Field0.Info, 510);
            this.WriteU("Temp", 42);
            this.WriteC((byte)this.Field0.RankLimit);
            this.WriteC((byte)this.Field0.MinAgeLimit);
            this.WriteC((byte)this.Field0.MaxAgeLimit);
            this.WriteC((byte)this.Field0.Authority);
            this.WriteU(this.Field0.News, 510);
            this.WriteD(this.Field0.Matches);
            this.WriteD(this.Field0.MatchWins);
            this.WriteD(this.Field0.MatchLoses);
            this.WriteD(this.Field0.Matches);
            this.WriteD(this.Field0.MatchWins);
            this.WriteD(this.Field0.MatchLoses);
        }
    }
}