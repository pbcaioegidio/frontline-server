// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.NHistoryModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class NHistoryModel
    {
        public string OldNick { get; set; }

        public string NewNick { get; set; }

        public string Motive { get; set; }

        public long ObjectId { get; set; }

        public long OwnerId { get; set; }

        public uint ChangeDate { get; set; }
    }
}