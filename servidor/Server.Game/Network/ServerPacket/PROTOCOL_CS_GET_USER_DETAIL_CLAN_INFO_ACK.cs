using Plugin.Core.Models;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_GET_USER_DETAIL_CLAN_INFO_ACK : GameServerPacket
    {
        private const int UntrackedClanStat = 0;
        private const int UntrackedClanMedals = 0;

        private readonly Account Field0;

        public PROTOCOL_CS_GET_USER_DETAIL_CLAN_INFO_ACK(Account A_1)
        {
            this.Field0 = A_1;
        }

        public override void Write()
        {
            StatisticClan clan = this.Field0.Statistic.Clan ?? new StatisticClan();
            StatisticTotal basic = this.Field0.Statistic.Basic ?? new StatisticTotal();
            StatisticMercenary merc = this.Field0.Statistic.Mercenary ?? new StatisticMercenary();

            this.WriteH((short)979);
            this.WriteD(0);
            this.WriteQ((ulong)this.Field0.PlayerId);

            WriteMercenaryRecord(merc);
            WriteClanRecord(clan);
            WriteTotalRecord(basic);

            this.WriteD(UntrackedClanMedals);
        }

        private void WriteMercenaryRecord(StatisticMercenary merc)
        {
            this.WriteD(merc.Matches);
            this.WriteD(merc.MatchWins);
            this.WriteD(merc.MatchLoses);
            this.WriteD(merc.DropsCount);
            this.WriteD(merc.KillsCount);
            this.WriteD(merc.DeathsCount);
            this.WriteD(merc.HeadshotsCount);
            this.WriteD(merc.AssistsCount);
        }

        private void WriteClanRecord(StatisticClan clan)
        {
            this.WriteD(clan.Matches);
            this.WriteD(clan.MatchWins);
            this.WriteD(clan.MatchLoses);
            this.WriteD(UntrackedClanStat);
            this.WriteD(UntrackedClanStat);
            this.WriteD(UntrackedClanStat);
            this.WriteD(UntrackedClanStat);
            this.WriteD(UntrackedClanStat);
            this.WriteD(UntrackedClanStat);
            this.WriteD(UntrackedClanStat);
            this.WriteD(UntrackedClanStat);
            this.WriteD(UntrackedClanStat);
        }

        private void WriteTotalRecord(StatisticTotal basic)
        {
            this.WriteD(basic.Matches);
            this.WriteD(basic.MatchWins);
            this.WriteD(basic.MatchLoses);
            this.WriteD(basic.MatchDraws);
            this.WriteD(basic.KillsCount);
            this.WriteD(basic.HeadshotsCount);
            this.WriteD(basic.DeathsCount);
            this.WriteD(basic.TotalMatchesCount);
            this.WriteD(basic.TotalKillsCount);
            this.WriteD(basic.EscapesCount);
            this.WriteD(basic.AssistsCount);
            this.WriteD(basic.MvpCount);
        }
    }
}
