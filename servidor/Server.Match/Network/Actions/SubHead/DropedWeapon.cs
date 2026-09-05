// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.SubHead.DropedWeapon
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Match.Data.Models.SubHead;
using System.Runtime.CompilerServices;


namespace Server.Match.Network.Actions.SubHead
{
    public class DropedWeapon
    {
        
        public static DropedWeaponInfo ReadInfo(SyncClientPacket C, bool GenLog)
        {
            DropedWeaponInfo dropedWeaponInfo = new DropedWeaponInfo()
            {
                WeaponPos = C.ReadUHV(),
                Unk1 = C.ReadUH(),
                Unk2 = C.ReadUH(),
                Unk3 = C.ReadUH(),
                Unk4 = C.ReadUH(),
                WeaponFlag = C.ReadC(),
                Unks = C.ReadB(16 /*0x10*/)
            };
            if (GenLog)
                CLogger.Print($"Sub Head: DroppedWeapon; Weapon Flag: {dropedWeaponInfo.WeaponFlag}; X: {dropedWeaponInfo.WeaponPos.X}; Y: {dropedWeaponInfo.WeaponPos.Y}; Z: {dropedWeaponInfo.WeaponPos.Z}", LoggerType.Warning);
            return dropedWeaponInfo;
        }

        public static void WriteInfo(SyncServerPacket S, DropedWeaponInfo Info)
        {
            S.WriteHV(Info.WeaponPos);
            S.WriteH(Info.Unk1);
            S.WriteH(Info.Unk2);
            S.WriteH(Info.Unk3);
            S.WriteH(Info.Unk4);
            S.WriteC(Info.WeaponFlag);
            S.WriteB(Info.Unks);
        }
    }
}