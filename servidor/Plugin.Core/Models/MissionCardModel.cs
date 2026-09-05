// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.MissionCardModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;

namespace Plugin.Core.Models
{
    public class MissionCardModel
    {
        public ClassType WeaponReq { get; set; }

        public MissionType MissionType { get; set; }

        public int MissionId { get; set; }

        public int MapId { get; set; }

        public int WeaponReqId { get; set; }

        public int MissionLimit { get; set; }

        public int MissionBasicId { get; set; }

        public int CardBasicId { get; set; }

        public int ArrayIdx { get; set; }

        public int Flag { get; set; }

        public MissionCardModel(int A_1, int A_2)
        {
            this.CardBasicId = A_1;
            this.MissionBasicId = A_2;
            this.ArrayIdx = A_1 * 4 + A_2;
            this.Flag = 15 << 4 * A_2;
        }
    }
}