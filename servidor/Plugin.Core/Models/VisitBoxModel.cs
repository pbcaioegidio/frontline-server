using System;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    // Token: 0x0200009D RID: 157
    public class VisitBoxModel
    {
        // Token: 0x170001C4 RID: 452
        // (get) Token: 0x06000744 RID: 1860 RVA: 0x0001FBC8 File Offset: 0x0001DDC8
        // (set) Token: 0x06000745 RID: 1861 RVA: 0x0001FBDC File Offset: 0x0001DDDC
        public VisitItemModel Reward1 { get; set; }

        // Token: 0x170001C5 RID: 453
        // (get) Token: 0x06000746 RID: 1862 RVA: 0x0001FBF0 File Offset: 0x0001DDF0
        // (set) Token: 0x06000747 RID: 1863 RVA: 0x0001FC04 File Offset: 0x0001DE04
        public VisitItemModel Reward2 { get; set; }

        // Token: 0x170001C6 RID: 454
        // (get) Token: 0x06000748 RID: 1864 RVA: 0x0001FC18 File Offset: 0x0001DE18
        // (set) Token: 0x06000749 RID: 1865 RVA: 0x0001FC2C File Offset: 0x0001DE2C
        public int RewardCount { get; set; }

        // Token: 0x170001C7 RID: 455
        // (get) Token: 0x0600074A RID: 1866 RVA: 0x0001FC40 File Offset: 0x0001DE40
        // (set) Token: 0x0600074B RID: 1867 RVA: 0x0001FC54 File Offset: 0x0001DE54
        public bool IsBothReward { get; set; }

        // Token: 0x0600074C RID: 1868 RVA: 0x0001FC68 File Offset: 0x0001DE68
        public VisitBoxModel()
        {
            this.Reward1 = new VisitItemModel();
            this.Reward2 = new VisitItemModel();
        }

        // Token: 0x0600074D RID: 1869 RVA: 0x0001FC94 File Offset: 0x0001DE94
        public void SetCount()
        {
            if (this.Reward1 != null && this.Reward1.GoodId > 0)
            {
                int rewardCount = this.RewardCount;
                this.RewardCount = rewardCount + 1;
            }
            if (this.Reward2 != null && this.Reward2.GoodId > 0)
            {
                int rewardCount = this.RewardCount;
                this.RewardCount = rewardCount + 1;
            }
        }
    }
}