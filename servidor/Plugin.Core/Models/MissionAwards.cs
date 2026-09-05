// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.MissionAwards
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class MissionAwards
    {
        public int Id { get; set; }

        public int MasterMedal { get; set; }

        public int Exp { get; set; }

        public int Gold { get; set; }

        public MissionAwards(int id, int masterMedal, int exp, int gold)
        {
            this.Id = id;
            this.MasterMedal = masterMedal;
            this.Exp = exp;
            this.Gold = gold;
        }
    }
}
