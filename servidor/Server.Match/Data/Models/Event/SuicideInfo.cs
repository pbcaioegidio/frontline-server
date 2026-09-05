// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.Event.SuicideInfo
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.SharpDX;


namespace Server.Match.Data.Models.Event
{
    public class SuicideInfo
    {
        public uint HitInfo;
        public Half3 PlayerPos;
        public ClassType WeaponClass;
        public byte Extensions;
        public byte Accessory;
        public int WeaponId;
    }
}
