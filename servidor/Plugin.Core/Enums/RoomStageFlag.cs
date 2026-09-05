// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Enums.RoomStageFlag
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;

namespace Plugin.Core.Enums
{
    [Flags]
    public enum RoomStageFlag
    {
        NONE = 0,
        TEAM_SWAP = 1,
        RANDOM_MAP = 2,
        PASSWORD = 4,
        OBSERVER_MODE = 8,
        REAL_IP = 16, // 0x00000010
        TEAM_BALANCE = 32, // 0x00000020
        OBSERVER = 64, // 0x00000040
        INTER_ENTER = 128, // 0x00000080
    }
}