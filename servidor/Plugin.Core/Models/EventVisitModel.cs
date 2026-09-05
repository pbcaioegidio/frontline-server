// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.EventVisitModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    public class EventVisitModel
    {
        public int Id { get; set; }

        public uint BeginDate { get; set; }

        public uint EndedDate { get; set; }

        public int Checks { get; set; }

        public string Title { get; set; }

        public List<VisitBoxModel> Boxes { get; set; }

        public EventVisitModel()
        {
            this.Checks = 31 /*0x1F*/;
            this.Title = "";
        }


        public bool EventIsEnabled()
        {
            uint num = uint.Parse(DateTimeUtil.Now("yyMMddHHmm"));
            return this.BeginDate <= num && num < this.EndedDate;
        }

        public List<VisitItemModel> GetReward(int Day, int Type)
        {
            List<VisitItemModel> reward = new List<VisitItemModel>();
            switch (Type)
            {
                case 0:
                    reward.Add(this.Boxes[Day].Reward1);
                    break;
                case 1:
                    reward.Add(this.Boxes[Day].Reward2);
                    break;
                default:
                    reward.Add(this.Boxes[Day].Reward1);
                    reward.Add(this.Boxes[Day].Reward2);
                    break;
            }
            return reward;
        }

        public void SetBoxCounts()
        {
            for (int index = 0; index < 31 /*0x1F*/; ++index)
                this.Boxes[index].SetCount();
        }
    }
}