// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Enums.ResultIcon
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;

namespace Plugin.Core.Enums
{
    [Flags]
    public enum ResultIcon
    {
        None = 0,
        Pc = 1,
        PcPlus = 2,
        Item = 4,
        Event = 8,
        Unk = 16, // 0x00000010
    }
}