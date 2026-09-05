
namespace Plugin.Core.Models
{
    public class PlayerBattlepass
    {
        public int BattlepassId { get; set; }
        public int BattlepassPremiumLevel { get; set; }
        public int BattlepassNormalLevel { get; set; }
        public bool HavePremium { get; set; }
        public int EarnedPoints { get; set; }

        // Season points earned on the current day (persisted in the
        // player_battlepass.points column). LastRecord is the yyyyMMdd day they were
        // last touched (persisted in last_record), so the server-owned daily cap
        // (BattlePassSeason.MaxDailyPoints) resets correctly across restarts.
        public int DailyPoints { get; set; }
        public uint LastRecord { get; set; }
    }
}