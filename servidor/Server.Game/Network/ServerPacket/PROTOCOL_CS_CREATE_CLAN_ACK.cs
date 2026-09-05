// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_CREATE_CLAN_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_CREATE_CLAN_ACK : GameServerPacket
    {
        private readonly Account Field0;
        private readonly ClanModel Field1;
        private readonly uint Field2;

        public PROTOCOL_CS_CREATE_CLAN_ACK(uint A_1, ClanModel A_2, Account A_3)
        {
            this.Field2 = A_1;
            this.Field1 = A_2;
            this.Field0 = A_3;
        }

        
        public override void Write()
        {
            this.WriteH((short)807);
            this.WriteD(this.Field2);
            if (this.Field2 != 0U)
                return;
            this.WriteD(this.Field1.Id);
            this.WriteU(this.Field1.Name, 34);
            this.WriteC((byte)this.Field1.Rank);
            this.WriteC((byte)DaoManagerSQL.GetClanPlayers(this.Field1.Id));
            this.WriteC((byte)this.Field1.MaxPlayers);
            this.WriteD(this.Field1.CreationDate);
            this.WriteD(this.Field1.Logo);
            this.WriteB(new byte[11]);
            this.WriteQ(this.Field1.OwnerId);
            this.WriteS(this.Field0.Nickname, 66);
            this.WriteC((byte)this.Field0.NickColor);
            this.WriteC((byte)this.Field0.Rank);
            this.WriteU(this.Field1.Info, 510);
            this.WriteU("Temp", 42);
            this.WriteC((byte)this.Field1.RankLimit);
            this.WriteC((byte)this.Field1.MinAgeLimit);
            this.WriteC((byte)this.Field1.MaxAgeLimit);
            this.WriteC((byte)this.Field1.Authority);
            this.WriteU("", 510);
            this.WriteB(new byte[44]);
            this.WriteF((double)this.Field1.Points);
            this.WriteF(60.0);
            this.WriteB(new byte[16 /*0x10*/]);
            this.WriteF((double)this.Field1.Points);
            this.WriteF(60.0);
            this.WriteB(new byte[80 /*0x50*/]);
            this.WriteB(new byte[66]);
            this.WriteD(this.Field0.Gold);
        }
    }
}