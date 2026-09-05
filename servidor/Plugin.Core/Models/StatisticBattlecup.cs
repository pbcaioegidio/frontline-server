// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.StatisticBattlecup
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll


namespace Plugin.Core.Models
{
    public class StatisticBattlecup
    {
        public long OwnerId { get; set; }

        public int Matches { get; set; }

        public int MatchWins { get; set; }

        public int MatchLoses { get; set; }

        public int MatchDraws { get; set; }

        public int KillsCount { get; set; }

        public int DeathsCount { get; set; }

        public int HeadshotsCount { get; set; }

        public int AssistsCount { get; set; }

        public int EscapesCount { get; set; }

        public int AverageDamage { get; set; }

        public int PlayTime { get; set; }
    }
}