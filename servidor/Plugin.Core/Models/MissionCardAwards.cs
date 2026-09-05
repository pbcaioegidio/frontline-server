// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.MissionCardAwards
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class MissionCardAwards
    {
        public int Id { get; set; }

        public int Card { get; set; }

        public int Ensign { get; set; }

        public int Medal { get; set; }

        public int Ribbon { get; set; }

        public int Exp { get; set; }

        public int Gold { get; set; }

        public bool Unusable()
        {
            return this.Ensign == 0 && this.Medal == 0 && this.Ribbon == 0 && this.Exp == 0 && this.Gold == 0;
        }
    }
}