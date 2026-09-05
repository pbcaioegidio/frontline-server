// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Enums.UdpGameEvent
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using System;


namespace Server.Match.Data.Enums
{
    public enum UdpGameEvent : uint
    {
        ActionState = 256, // 0x00000100
        Animation = 2,
        PosRotation = 134217728, // 0x08000000
        SoundPosRotation = 8388608, // 0x00800000
        UseObject = 4,
        ActionForObjectSync = 16, // 0x00000010
        RadioChat = 32, // 0x00000020
        WeaponSync = 67108864, // 0x04000000
        WeaponRecoil = 128, // 0x00000080
        HpSync = 8,
        Suicide = 1048576, // 0x00100000
        MissionData = 2048, // 0x00000800
        RetriveDataForClient = 4096, // 0x00001000
        SeizeDataForClient = 32768, // 0x00008000
        DropWeapon = 4194304, // 0x00400000
        GetWeaponForClient = 16777216, // 0x01000000
        FireData = 33554432, // 0x02000000
        CharaFireNHitData = 1024, // 0x00000400
        HitData = 131072, // 0x00020000
        GrenadeHit = 268435456, // 0x10000000
        GetWeaponForHost = 512, // 0x00000200
        FireDataOnObject = 1073741824, // 0x40000000
        FireNHitDataOnObject = 8192, // 0x00002000
        BattleRoyalItem = 64, // 0x00000040
        DirectPickUp = 16384, // 0x00004000
        DeathDataForClient = CharaFireNHitData, // 0x00000400
    }
}