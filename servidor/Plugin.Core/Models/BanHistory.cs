// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.BanHistory
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System;

namespace Plugin.Core.Models
{
    public class BanHistory
    {
        public long ObjectId { get; set; }

        public long PlayerId { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public string Reason { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public BanHistory()
        {
            this.StartDate = DateTimeUtil.Now();
            this.Type = "";
            this.Value = "";
            this.Reason = "";
        }
    }
}
