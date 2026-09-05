// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.FragModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;

namespace Plugin.Core.Models
{
    public class FragModel
    {
        public byte WeaponClass { get; set; }

        public byte HitspotInfo { get; set; }

        public byte Unk { get; set; }

        public KillingMessage KillFlag { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public byte VictimSlot { get; set; }

        public byte AssistSlot { get; set; }

        public byte[] Unks { get; set; }
    }
}