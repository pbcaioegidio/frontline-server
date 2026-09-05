// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.RankModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Collections.Generic;


namespace Plugin.Core.Models
{
    public class RankModel
    {
        public int Id;
        public string Title;
        public int OnNextLevel;
        public int OnGoldUp;
        public int OnAllExp;
        public List<int> Rewards = new List<int>();

        public RankModel(int A_1) => this.Id = A_1;
    }
}