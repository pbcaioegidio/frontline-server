// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.EventLoginModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    public class EventLoginModel
    {
        public int Id { get; set; }

        public uint BeginDate { get; set; }

        public uint EndedDate { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        // Segundo texto del bloque comun (+0x4F, 60 bytes). El cliente lo lee aparte del
        // nombre en BoostEvent__BuildDescriptionList @0xBF8BD4.
        public string Subtitle { get; set; }

        // Indice de Gui/EventPortal/img_*_<N>.i3i, u8 en +0x153. Clave ausente -> fallback 1.
        public byte Image { get; set; }

        public bool Period { get; set; }

        public bool Priority { get; set; }

        public List<int> Goods { get; set; }

        public EventLoginModel()
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
    }
}