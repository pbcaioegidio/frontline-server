// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.FragInfos
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using System;
using System.Collections.Generic;


namespace Plugin.Core.Models
{
    public class FragInfos
    {
        public byte KillerSlot { get; set; }
        public byte KillsCount { get; set; }
        public byte Flag { get; set; }
        public byte Unk { get; set; }
        public CharaKillType KillingType { get; set; }
        public int WeaponId { get; set; }
        public int Score { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public List<FragModel> Frags { get; set; }
        public FragInfos() => this.Frags = new List<FragModel>();
        public KillingMessage GetAllKillFlags()
        {
            KillingMessage allKillFlags = KillingMessage.None;
            foreach (FragModel frag in this.Frags)
            {
                if (!allKillFlags.HasFlag((Enum)frag.KillFlag))
                    allKillFlags |= frag.KillFlag;
            }
            return allKillFlags;
        }
    }
}