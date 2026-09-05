// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.StatisticAcemode
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class StatisticAcemode
    {
        public long OwnerId { get; set; }

        public int Matches { get; set; }

        public int MatchWins { get; set; }

        public int MatchLoses { get; set; }

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int Headshots { get; set; }

        public int Assists { get; set; }

        public int Escapes { get; set; }

        public int Winstreaks { get; set; }
    }
}
