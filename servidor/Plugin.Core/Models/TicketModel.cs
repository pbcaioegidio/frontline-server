// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.TicketModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using System.Collections.Generic;

namespace Plugin.Core.Models
{
    public class TicketModel
    {
        public string Token { get; set; }

        public TicketType Type { get; set; }

        public int GoldReward { get; set; }

        public int CashReward { get; set; }

        public int TagsReward { get; set; }

        public uint TicketCount { get; set; }

        public uint PlayerRation { get; set; }

        public List<int> Rewards { get; set; }
    }
}