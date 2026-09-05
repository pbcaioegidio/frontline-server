// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.Event.HitDataInfo
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.SharpDX;
using Server.Match.Data.Enums;
using System.Collections.Generic;


namespace Server.Match.Data.Models.Event
{
    public class HitDataInfo
    {
        public byte Extensions;
        public byte Accessory;
        public ushort BoomInfo;
        public ushort ObjectId;
        public uint HitIndex;
        public int WeaponId;
        public Half3 StartBullet;
        public Half3 EndBullet;
        public Half3 BulletPos;
        public List<int> BoomPlayers;
        public HitType HitEnum;
        public ClassType WeaponClass;
    }
}