// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.ClanBestPlayers
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    public class ClanBestPlayers
    {
        public RecordInfo Exp { get; set; }

        public RecordInfo Participation { get; set; }

        public RecordInfo Wins { get; set; }

        public RecordInfo Kills { get; set; }

        public RecordInfo Headshots { get; set; }

        public void SetPlayers(
          string Exp,
          string Participation,
          string Wins,
          string Kills,
          string Headshots)
        {
            this.Exp = new RecordInfo(Exp.Split('-'));
            this.Participation = new RecordInfo(Participation.Split('-'));
            this.Wins = new RecordInfo(Wins.Split('-'));
            this.Kills = new RecordInfo(Kills.Split('-'));
            this.Headshots = new RecordInfo(Headshots.Split('-'));
        }

        
        public void SetDefault()
        {
            string[] A_1 = new string[2] { "0", "0" };
            this.Exp = new RecordInfo(A_1);
            this.Participation = new RecordInfo(A_1);
            this.Wins = new RecordInfo(A_1);
            this.Kills = new RecordInfo(A_1);
            this.Headshots = new RecordInfo(A_1);
        }

        public long GetPlayerId(string[] Split)
        {
            try
            {
                return long.Parse(Split[0]);
            }
            catch
            {
                return 0;
            }
        }

        public int GetPlayerValue(string[] Split)
        {
            try
            {
                return int.Parse(Split[1]);
            }
            catch
            {
                return 0;
            }
        }

        public void SetBestExp(SlotModel Slot)
        {
            if (Slot.Exp <= this.Exp.RecordValue)
                return;
            this.Exp.PlayerId = Slot.PlayerId;
            this.Exp.RecordValue = Slot.Exp;
        }

        public void SetBestHeadshot(SlotModel Slot)
        {
            if (Slot.AllHeadshots <= this.Headshots.RecordValue)
                return;
            this.Headshots.PlayerId = Slot.PlayerId;
            this.Headshots.RecordValue = Slot.AllHeadshots;
        }

        public void SetBestKills(SlotModel Slot)
        {
            if (Slot.AllKills <= this.Kills.RecordValue)
                return;
            this.Kills.PlayerId = Slot.PlayerId;
            this.Kills.RecordValue = Slot.AllKills;
        }

        
        public void SetBestWins(PlayerStatistic Stat, SlotModel Slot, bool WonTheMatch)
        {
            if (!WonTheMatch)
                return;
            ComDiv.UpdateDB("player_stat_clans", "clan_match_wins", (object)++Stat.Clan.MatchWins, "owner_id", (object)Slot.PlayerId);
            if (Stat.Clan.MatchWins <= this.Wins.RecordValue)
                return;
            this.Wins.PlayerId = Slot.PlayerId;
            this.Wins.RecordValue = Stat.Clan.MatchWins;
        }

        
        public void SetBestParticipation(PlayerStatistic Stat, SlotModel Slot)
        {
            ComDiv.UpdateDB("player_stat_clans", "clan_matches", (object)++Stat.Clan.Matches, "owner_id", (object)Slot.PlayerId);
            if (Stat.Clan.Matches <= this.Participation.RecordValue)
                return;
            this.Participation.PlayerId = Slot.PlayerId;
            this.Participation.RecordValue = Stat.Clan.Matches;
        }
    }
}