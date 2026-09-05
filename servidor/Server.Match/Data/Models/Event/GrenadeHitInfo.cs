// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.Event.GrenadeHitInfo
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.SharpDX;
using Server.Match.Data.Enums;
using System.Collections.Generic;


namespace Server.Match.Data.Models.Event
{
    public class GrenadeHitInfo
    {
        public byte Extensions;
        public byte Accessory;
        public ushort BoomInfo;
        public ushort GrenadesCount;
        public ushort ObjectId;
        public uint HitInfo;
        public int WeaponId;
        public List<int> BoomPlayers;
        public CharaDeath DeathType;
        public Half3 FirePos;
        public Half3 HitPos;
        public Half3 PlayerPos;
        public HitType HitEnum;
        public ClassType WeaponClass;
    }
}
