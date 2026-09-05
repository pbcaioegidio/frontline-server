// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.ObjectHitInfo
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.SharpDX;
using Server.Match.Data.Enums;


namespace Server.Match.Data.Models
{
    public class ObjectHitInfo
    {
        public int Type { get; set; }

        public int ObjSyncId { get; set; }

        public int ObjId { get; set; }

        public int ObjLife { get; set; }

        public int KillerSlot { get; set; }

        public int AnimId1 { get; set; }

        public int AnimId2 { get; set; }

        public int DestroyState { get; set; }

        public int WeaponId { get; set; }

        public byte Accessory { get; set; }

        public byte Extensions { get; set; }

        public float SpecialUse { get; set; }

        public Half3 Position { get; set; }

        public ClassType WeaponClass { get; set; }

        public CharaHitPart HitPart { get; set; }

        public CharaDeath DeathType { get; set; }

        public ObjectHitInfo(int A_1)
        {
            this.Type = A_1;
            this.DeathType = CharaDeath.DEFAULT;
        }
    }
}