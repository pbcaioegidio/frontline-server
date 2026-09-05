// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PassBoxModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll


namespace Plugin.Core.Models
{
    public class PassBoxModel
    {
        public PassItemModel Normal { get; set; }

        public PassItemModel PremiumA { get; set; }

        public PassItemModel PremiumB { get; set; }

        public int RequiredPoints { get; set; }

        public int RewardCount { get; set; }

        public int Card { get; set; }

        public PassBoxModel()
        {
            this.Normal = new PassItemModel();
            this.PremiumA = new PassItemModel();
            this.PremiumB = new PassItemModel();
        }

        public void SetCount()
        {
            if (this.Normal != null && this.Normal.GoodId > 0)
                ++this.RewardCount;
            if (this.PremiumA != null && this.PremiumA.GoodId > 0)
                ++this.RewardCount;
            if (this.PremiumB == null || this.PremiumB.GoodId <= 0)
                return;
            ++this.RewardCount;
        }
    }
}