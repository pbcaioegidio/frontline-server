// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_REQUEST_INFO_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_REQUEST_INFO_ACK : GameServerPacket
    {
        private readonly string Field0;
        private readonly uint Field1;
        private readonly Account Field2;

        public PROTOCOL_CS_REQUEST_INFO_ACK(long A_1, string A_2)
        {
            this.Field0 = A_2;
            this.Field2 = AccountManager.GetAccount(A_1, 31 /*0x1F*/);
            if (this.Field2 != null && A_2 != null)
                return;
            this.Field1 = 2147483648U /*0x80000000*/;
        }

        
        public override void Write()
        {
            this.WriteH((short)821);
            this.WriteD(this.Field1);
            if (this.Field1 != 0U)
                return;
            this.WriteQ(this.Field2.PlayerId);
            this.WriteU(this.Field2.Nickname, 66);
            this.WriteC((byte)this.Field2.Rank);
            this.WriteD(this.Field2.Statistic.Basic.KillsCount);
            this.WriteD(this.Field2.Statistic.Basic.DeathsCount);
            this.WriteD(this.Field2.Statistic.Basic.Matches);
            this.WriteD(this.Field2.Statistic.Basic.MatchWins);
            this.WriteD(this.Field2.Statistic.Basic.MatchLoses);
            this.WriteN(this.Field0, this.Field0.Length + 2, "UTF-16LE");
        }
    }
}