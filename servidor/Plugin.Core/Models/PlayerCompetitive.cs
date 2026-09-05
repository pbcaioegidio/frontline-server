// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerCompetitive
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.XML;

namespace Plugin.Core.Models
{
    public class PlayerCompetitive
    {
        public long OwnerId { get; set; }

        public int Level { get; set; }

        public int Points { get; set; }

        public CompetitiveRank Rank() => CompetitiveXML.GetRank(this.Level) ?? new CompetitiveRank();
    }
}