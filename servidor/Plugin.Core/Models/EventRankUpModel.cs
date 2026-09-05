// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.EventRankUpModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    public class EventRankUpModel
    {
        public int Id { get; set; }

        public uint BeginDate { get; set; }

        public uint EndedDate { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        // Segundo texto del bloque comun (+0x4F, 60 bytes), separado del nombre en el
        // cliente (BoostEvent__BuildDescriptionList @0xBF8BD4).
        public string Subtitle { get; set; }

        // Indice de Gui/EventPortal/img_*_<N>.i3i, u8 en +0x153. Clave ausente -> fallback 1.
        public byte Image { get; set; }

        public bool Period { get; set; }

        public bool Priority { get; set; }

        public List<int[]> Ranks { get; set; }

        public EventRankUpModel()
        {
            this.Name = "";
            this.Description = "";
            this.Subtitle = "";
            this.Image = 1;
        }


        public bool EventIsEnabled()
        {
            uint num = uint.Parse(DateTimeUtil.Now("yyMMddHHmm"));
            return this.BeginDate <= num && num < this.EndedDate;
        }

        public int[] GetBonuses(int RankId)
        {
            lock (this.Ranks)
            {
                foreach (int[] rank in this.Ranks)
                {
                    if (rank[0] == RankId)
                        return new int[3] { rank[1], rank[2], rank[3] };
                }
                return new int[3];
            }
        }
    }

    namespace Plugin.Core.Models
    {
        public class EventRankUpModel
        {
            public int Id { get; set; }

            public uint BeginDate { get; set; }

            public uint EndedDate { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public bool Period { get; set; }

            public bool Priority { get; set; }

            public List<int[]> Ranks { get; set; }

            public EventRankUpModel()
            {
                this.Name = "";
                this.Description = "";
            }


            public bool EventIsEnabled()
            {
                uint num = uint.Parse(DateTimeUtil.Now("yyMMddHHmm"));
                return this.BeginDate <= num && num < this.EndedDate;
            }

            public int[] GetBonuses(int RankId)
            {
                lock (this.Ranks)
                {
                    foreach (int[] rank in this.Ranks)
                    {
                        if (rank[0] == RankId)
                            return new int[3] { rank[1], rank[2], rank[3] };
                    }
                    return new int[3];
                }
            }
        }
    }
}