// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PCCafeModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using System.Collections.Generic;

namespace Plugin.Core.Models
{
    public class PCCafeModel
    {
        public CafeEnum Type { get; set; }

        public int PointUp { get; set; }

        public int ExpUp { get; set; }

        public SortedList<CafeEnum, List<ItemsModel>> Rewards { get; set; }

        public PCCafeModel(CafeEnum A_1)
        {
            this.Type = A_1;
            this.PointUp = 0;
            this.ExpUp = 0;
        }
    }
}