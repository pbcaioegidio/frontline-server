// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Enums.RoomWeaponsFlag
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;

namespace Plugin.Core.Enums
{
    [Flags]
    public enum RoomWeaponsFlag
    {
        None = 0,
        Grenade = 1,
        Melee = 2,
        Secondary = 4,
        Accessory = 8,
        Assault = 16, // 0x00000010
        SMG = 32, // 0x00000020
        Sniper = 64, // 0x00000040
        Shotgun = 128, // 0x00000080
        Machinegun = 256, // 0x00000100
        Baist = 512, // 0x00000200
        RPG7 = 1024, // 0x00000400
        Shield = 2048, // 0x00000800
    }
}