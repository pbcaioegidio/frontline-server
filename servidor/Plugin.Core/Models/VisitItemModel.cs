// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.VisitItemModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class VisitItemModel
    {
        public int GoodId { get; set; }

        public bool IsReward { get; set; }

        public void SetGoodId(int GoodId)
        {
            this.GoodId = GoodId;
            if (GoodId <= 0)
                return;
            this.IsReward = true;
        }
    }
}