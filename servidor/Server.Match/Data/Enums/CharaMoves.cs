// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Enums.CharaMoves
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using System;


namespace Server.Match.Data.Enums
{
    public enum CharaMoves
    {
        Stop = 0,
        Left = 1,
        Back = 2,
        Right = 4,
        Front = 8,
        HeliInMove = 16, // 0x00000010
        HeliUnknown = 32, // 0x00000020
        HeliLeave = 64, // 0x00000040
        HeliStopped = 128, // 0x00000080
    }
}