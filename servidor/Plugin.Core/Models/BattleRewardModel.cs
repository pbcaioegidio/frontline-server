// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.BattleRewardModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;

namespace Plugin.Core.Models
{
    public class BattleRewardModel
    {
        public BattleRewardType Type { get; set; }

        public int Percentage { get; set; }

        public int[] Rewards { get; set; }
    }
}
