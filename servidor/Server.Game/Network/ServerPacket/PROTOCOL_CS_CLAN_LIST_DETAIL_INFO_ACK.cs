// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_CLAN_LIST_DETAIL_INFO_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_CLAN_LIST_DETAIL_INFO_ACK : GameServerPacket
    {
        private readonly ClanModel Field0;
        private readonly int Field1;
        private readonly Account Field2;
        private readonly int Field3;

        public PROTOCOL_CS_CLAN_LIST_DETAIL_INFO_ACK(int A_1, ClanModel A_2)
        {
            this.Field1 = A_1;
            this.Field0 = A_2;
            if (A_2 == null)
                return;
            this.Field2 = AccountManager.GetAccount(A_2.OwnerId, 31 /*0x1F*/);
            this.Field3 = DaoManagerSQL.GetClanPlayers(A_2.Id);
        }

        public override void Write()
        {
            this.WriteH((short)927);
            this.WriteH((short)0);
            this.WriteD(0);
            this.WriteD((int)this.Field0.Points);
            this.WriteC((byte)0);
            this.WriteD(0);
            this.WriteD(this.Field0.MatchLoses);
            this.WriteD(this.Field0.MatchWins);
            this.WriteD(this.Field0.Matches);
            this.WriteC((byte)this.Field0.MaxPlayers);
            this.WriteC((byte)this.Field3);
            this.WriteC((byte)this.Field0.GetClanUnit());
            this.WriteB(this.Method0(this.Field2));
            this.WriteD(this.Field0.Exp);
            this.WriteC((byte)this.Field0.Rank);
            this.WriteQ(0L);
            this.WriteC((byte)0);
        }

        
        private byte[] Method0(Account A_1)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                if (A_1 != null)
                {
                    syncServerPacket.WriteC((byte)(A_1.Nickname.Length + 1));
                    syncServerPacket.WriteN(A_1.Nickname, A_1.Nickname.Length + 2, "UTF-16LE");
                }
                else
                    syncServerPacket.WriteC((byte)0);
                return syncServerPacket.ToArray();
            }
        }
    }
}