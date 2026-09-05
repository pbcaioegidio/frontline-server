// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.RHistoryModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;

namespace Plugin.Core.Models
{
    public class RHistoryModel
    {
        public long ObjectId { get; set; }

        public long OwnerId { get; set; }

        public long SenderId { get; set; }

        public uint Date { get; set; }

        public string OwnerNick { get; set; }

        public string SenderNick { get; set; }

        public string Message { get; set; }

        public ReportType Type { get; set; }
    }
}