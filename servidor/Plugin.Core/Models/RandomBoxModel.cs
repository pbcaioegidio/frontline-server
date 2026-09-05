// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.RandomBoxModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Collections.Generic;

namespace Plugin.Core.Models
{
    public class RandomBoxModel
    {
        public int ItemsCount { get; set; }

        public int MinPercent { get; set; }

        public int MaxPercent { get; set; }

        public List<RandomBoxItem> Items { get; set; }

        public List<RandomBoxItem> GetRewardList(List<RandomBoxItem> SortedLists, int RandomId)
        {
            List<RandomBoxItem> rewardList = new List<RandomBoxItem>();
            if (SortedLists.Count > 0)
            {
                int index = SortedLists[RandomId].Index;
                foreach (RandomBoxItem sortedList in SortedLists)
                {
                    if (sortedList.Index == index)
                        rewardList.Add(sortedList);
                }
            }
            return rewardList;
        }

        public List<RandomBoxItem> GetSortedList(int Percent)
        {
            if (Percent < this.MinPercent)
                Percent = this.MinPercent;
            List<RandomBoxItem> sortedList = new List<RandomBoxItem>();
            foreach (RandomBoxItem randomBoxItem in this.Items)
            {
                if (Percent <= randomBoxItem.Percent)
                    sortedList.Add(randomBoxItem);
            }
            return sortedList;
        }

        public void SetTopPercent()
        {
            int num1 = 100;
            int num2 = 0;
            foreach (RandomBoxItem randomBoxItem in this.Items)
            {
                if (randomBoxItem.Percent < num1)
                    num1 = randomBoxItem.Percent;
                if (randomBoxItem.Percent > num2)
                    num2 = randomBoxItem.Percent;
            }
            this.MinPercent = num1;
            this.MaxPercent = num2;
        }
    }
}