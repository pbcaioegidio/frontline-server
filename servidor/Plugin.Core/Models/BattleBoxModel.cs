// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.BattleBoxModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    public class BattleBoxModel
    {
        public int CouponId { get; set; }

        public int RequireTags { get; set; }

        public List<BattleBoxItem> Items { get; set; }

        public List<int> Goods { get; set; }

        public List<double> Probabilities { get; set; }


        public T GetItemWithProbabilities<T>(List<T> Items, List<double> Probabilities)
        {
            if (Items == null || Items.Count == 0 || Probabilities == null || Probabilities.Count == 0 || Items.Count != Probabilities.Count)
                CLogger.Print("Battle Box Item List Is Not Valid!", LoggerType.Warning);
            double num1 = new Random().NextDouble();
            double num2 = 0.0;
            for (int index = 0; index < Items.Count; ++index)
            {
                num2 += Probabilities[index] / 100.0;
                if (num1 < num2)
                    return Items[index];
            }
            return Items[Items.Count - 1];
        }

        public void InitItemPercentages()
        {
            this.Goods = new List<int>();
            this.Probabilities = new List<double>();
            foreach (BattleBoxItem battleBoxItem in this.Items)
            {
                this.Goods.Add(battleBoxItem.GoodsId);
                this.Probabilities.Add((double)battleBoxItem.Percent);
            }
        }
    }
}