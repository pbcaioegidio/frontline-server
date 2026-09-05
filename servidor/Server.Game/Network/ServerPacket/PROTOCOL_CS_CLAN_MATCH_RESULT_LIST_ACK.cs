// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_CS_CLAN_MATCH_RESULT_LIST_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Server.Game.Data.Models;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_CLAN_MATCH_RESULT_LIST_ACK : GameServerPacket
    {
        private readonly List<MatchModel> Field0;
        private readonly int Field1;

        public PROTOCOL_CS_CLAN_MATCH_RESULT_LIST_ACK(int A_1, List<MatchModel> A_2)
        {
            this.Field1 = A_1;
            this.Field0 = A_2;
        }

        public override void Write()
        {
            this.WriteH((short)1957);
            this.WriteC(this.Field1 == 0 ? (byte)this.Field0.Count : (byte)this.Field1);
            if (this.Field1 > 0 || this.Field0.Count == 0)
                return;
            this.WriteC((byte)1);
            this.WriteC((byte)0);
            this.WriteC((byte)this.Field0.Count);
            for (int index = 0; index < this.Field0.Count; ++index)
            {
                MatchModel matchModel = this.Field0[index];
                this.WriteH((short)matchModel.MatchId);
                this.WriteH((ushort)matchModel.GetServerInfo());
                this.WriteH((ushort)matchModel.GetServerInfo());
                this.WriteC((byte)matchModel.State);
                this.WriteC((byte)matchModel.FriendId);
                this.WriteC((byte)matchModel.Training);
                this.WriteC((byte)matchModel.GetCountPlayers());
                this.WriteC((byte)0);
                this.WriteD(matchModel.Leader);
                Account leader = matchModel.GetLeader();
                if (leader == null)
                {
                    this.WriteB(new byte[76]);
                }
                else
                {
                    this.WriteC((byte)leader.Rank);
                    this.WriteU(leader.Nickname, 66);
                    this.WriteQ(leader.PlayerId);
                    this.WriteC((byte)matchModel.Slots[matchModel.Leader].State);
                }
            }
        }
    }
}