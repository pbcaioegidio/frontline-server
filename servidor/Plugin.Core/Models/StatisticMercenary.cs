namespace Plugin.Core.Models
{
    public class StatisticMercenary
    {
        public long OwnerId { get; set; }

        public int Matches { get; set; }

        public int MatchWins { get; set; }

        public int MatchLoses { get; set; }

        public int DropsCount { get; set; }

        public int KillsCount { get; set; }

        public int DeathsCount { get; set; }

        public int HeadshotsCount { get; set; }

        public int AssistsCount { get; set; }
    }
}
