// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerStatistic
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;

namespace Plugin.Core.Models
{
    public class PlayerStatistic
    {
        public StatisticTotal Basic { get; set; }

        public StatisticSeason Season { get; set; }

        public StatisticDaily Daily { get; set; }

        public StatisticClan Clan { get; set; }

        public StatisticWeapon Weapon { get; set; }

        public StatisticAcemode Acemode { get; set; }

        public StatisticBattlecup Battlecup { get; set; }

        public StatisticMercenary Mercenary { get; set; }

        public int GetKDRatio()
        {
            return this.Basic.HeadshotsCount <= 0 && this.Basic.KillsCount <= 0 ? 0 : (int)Math.Floor(((double)(this.Basic.KillsCount * 100) + 0.5) / (double)(this.Basic.KillsCount + this.Basic.DeathsCount));
        }

        public int GetHSRatio()
        {
            return this.Basic.KillsCount <= 0 ? 0 : (int)Math.Floor((double)(this.Basic.HeadshotsCount * 100) / ((double)this.Basic.KillsCount + 0.5));
        }

        public int GetSeasonKDRatio()
        {
            return this.Season.HeadshotsCount <= 0 && this.Season.KillsCount <= 0 ? 0 : (int)Math.Floor(((double)(this.Season.KillsCount * 100) + 0.5) / (double)(this.Season.KillsCount + this.Season.DeathsCount));
        }

        public int GetSeasonHSRatio()
        {
            return this.Season.KillsCount <= 0 ? 0 : (int)Math.Floor((double)(this.Season.HeadshotsCount * 100) / ((double)this.Season.KillsCount + 0.5));
        }

        public int GetBCWinRatio()
        {
            return this.Battlecup.MatchWins <= 0 && this.Battlecup.Matches <= 0 ? 0 : (int)Math.Floor(((double)(this.Battlecup.MatchWins * 100) + 0.5) / (double)(this.Battlecup.MatchWins + this.Battlecup.MatchLoses));
        }

        public int GetBCKDRatio()
        {
            return this.Battlecup.HeadshotsCount <= 0 && this.Battlecup.KillsCount <= 0 ? 0 : (int)Math.Floor(((double)(this.Battlecup.KillsCount * 100) + 0.5) / (double)(this.Battlecup.KillsCount + this.Battlecup.DeathsCount));
        }
    }
}