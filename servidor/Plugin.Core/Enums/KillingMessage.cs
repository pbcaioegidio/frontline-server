// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Enums.KillingMessage
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;

namespace Plugin.Core.Enums
{
    [Flags]
    public enum KillingMessage
    {
        None = 0,
        PiercingShot = 1,
        MassKill = 2,
        ChainStopper = 4,
        Headshot = 8,
        ChainHeadshot = 16, // 0x00000010
        ChainSlugger = 32, // 0x00000020
        Suicide = 64, // 0x00000040
        ObjectDefense = 128, // 0x00000080
    }
}