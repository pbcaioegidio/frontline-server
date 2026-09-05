// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ServerPacket.PROTOCOL_BASE_GET_RECORD_INFO_DB_ACK
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Server.Game.Data.Models;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_GET_RECORD_INFO_DB_ACK : GameServerPacket
    {
        private readonly PlayerStatistic Field0;

        public PROTOCOL_BASE_GET_RECORD_INFO_DB_ACK(Account A_1) => this.Field0 = A_1.Statistic;

        public override void Write()
        {
            this.WriteH((short)2351);
            this.WriteD(this.Field0.Season.Matches);
            this.WriteD(this.Field0.Season.MatchWins);
            this.WriteD(this.Field0.Season.MatchLoses);
            this.WriteD(this.Field0.Season.MatchDraws);
            this.WriteD(this.Field0.Season.KillsCount);
            this.WriteD(this.Field0.Season.HeadshotsCount);
            this.WriteD(this.Field0.Season.DeathsCount);
            this.WriteD(this.Field0.Season.TotalMatchesCount);
            this.WriteD(this.Field0.Season.TotalKillsCount);
            this.WriteD(this.Field0.Season.EscapesCount);
            this.WriteD(this.Field0.Season.AssistsCount);
            this.WriteD(this.Field0.Season.MvpCount);
            this.WriteD(this.Field0.Basic.Matches);
            this.WriteD(this.Field0.Basic.MatchWins);
            this.WriteD(this.Field0.Basic.MatchLoses);
            this.WriteD(this.Field0.Basic.MatchDraws);
            this.WriteD(this.Field0.Basic.KillsCount);
            this.WriteD(this.Field0.Basic.HeadshotsCount);
            this.WriteD(this.Field0.Basic.DeathsCount);
            this.WriteD(this.Field0.Basic.TotalMatchesCount);
            this.WriteD(this.Field0.Basic.TotalKillsCount);
            this.WriteD(this.Field0.Basic.EscapesCount);
            this.WriteD(this.Field0.Basic.AssistsCount);
            this.WriteD(this.Field0.Basic.MvpCount);
        }
    }
}