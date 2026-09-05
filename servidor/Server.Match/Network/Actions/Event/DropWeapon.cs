// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.DropWeapon
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Match.Data.Models;
using Server.Match.Data.Models.Event;
using System.Runtime.CompilerServices;


namespace Server.Match.Network.Actions.Event
{
    public class DropWeapon
    {
        
        public static DropWeaponInfo ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            DropWeaponInfo dropWeaponInfo = new DropWeaponInfo()
            {
                AmmoPrin = C.ReadUH(),
                AmmoDual = C.ReadUH(),
                AmmoTotal = C.ReadUH(),
                Unk1 = C.ReadUH(),
                Unk2 = C.ReadUH(),
                Unk3 = C.ReadUH(),
                Flags = C.ReadC(),
                WeaponId = C.ReadD(),
                Accessory = C.ReadC(),
                Extensions = C.ReadC()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot : {Action.Slot}; Weapon Id: {dropWeaponInfo.WeaponId}; Ext: {dropWeaponInfo.Extensions}; Acc: {dropWeaponInfo.Accessory}; Ammo Prin: {dropWeaponInfo.AmmoPrin}; Ammo Dual: {dropWeaponInfo.AmmoDual}; Ammo Total: {dropWeaponInfo.AmmoTotal}", LoggerType.Warning);
            return dropWeaponInfo;
        }

        public static void WriteInfo(SyncServerPacket S, DropWeaponInfo Info)
        {
            if (ConfigLoader.UseMaxAmmoInDrop)
            {
                S.WriteH(ushort.MaxValue);
                S.WriteH(Info.AmmoDual);
                S.WriteH((short)10000);
            }
            else
            {
                S.WriteH(Info.AmmoPrin);
                S.WriteH(Info.AmmoDual);
                S.WriteH(Info.AmmoTotal);
            }
            S.WriteH(Info.Unk1);
            S.WriteH(Info.Unk2);
            S.WriteH(Info.Unk3);
            S.WriteC((byte)((uint)Info.Flags + (uint)Info.Counter));
            S.WriteD(Info.WeaponId);
            S.WriteC(Info.Accessory);
            S.WriteC(Info.Extensions);
        }
    }
}