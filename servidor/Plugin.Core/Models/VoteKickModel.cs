// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.VoteKickModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Collections.Generic;

namespace Plugin.Core.Models
{
    public class VoteKickModel
    {
        public int CreatorIdx { get; set; }

        public int VictimIdx { get; set; }

        public int Motive { get; set; }

        public int Accept { get; set; }

        public int Denie { get; set; }

        public int Allies { get; set; }

        public int Enemies { get; set; }

        public List<int> Votes { get; set; }

        public bool[] TotalArray { get; set; }

        public VoteKickModel(int A_1, int A_2)
        {
            this.Accept = 1;
            this.Denie = 1;
            this.CreatorIdx = A_1;
            this.VictimIdx = A_2;
            this.Votes = new List<int>();
            this.Votes.Add(A_1);
            this.Votes.Add(A_2);
            this.TotalArray = new bool[18];
        }

        public int GetInGamePlayers()
        {
            int inGamePlayers = 0;
            for (int index = 0; index < 18; ++index)
            {
                if (this.TotalArray[index])
                    ++inGamePlayers;
            }
            return inGamePlayers;
        }
    }
}