// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.TitleModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class TitleModel
    {
        public int Id { get; set; }

        public int ClassId { get; set; }

        public int Medal { get; set; }

        public int Ribbon { get; set; }

        public int MasterMedal { get; set; }

        public int Ensign { get; set; }

        public int Rank { get; set; }

        public int Slot { get; set; }

        public int Req1 { get; set; }

        public int Req2 { get; set; }

        public long Flag { get; set; }

        public TitleModel()
        {
        }

        public TitleModel(int A_1)
        {
            this.Id = A_1;
            this.Flag = 1L << A_1;
        }
    }
}