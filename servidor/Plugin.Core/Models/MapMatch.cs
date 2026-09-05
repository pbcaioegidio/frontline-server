// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.MapMatch
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class MapMatch
    {
        public int Mode { get; set; }

        public int Id { get; set; }

        public int Limit { get; set; }

        public int Tag { get; set; }

        public string Name { get; set; }

        public MapMatch(int A_1) => this.Mode = A_1;
    }
}
