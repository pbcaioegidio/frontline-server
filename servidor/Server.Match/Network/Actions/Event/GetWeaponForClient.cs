// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.GetWeaponForClient
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
    public class GetWeaponForClient
    {
        
        public static WeaponClient ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            WeaponClient weaponClient = new WeaponClient()
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
                CLogger.Print($"PVP Slot: {Action.Slot}; Weapon Id: {weaponClient.WeaponId}; Ext: {weaponClient.Extensions}; Acc: {weaponClient.Accessory}; Ammo Prin: {weaponClient.AmmoPrin}; Ammo Dual: {weaponClient.AmmoDual}; Ammo Total: {weaponClient.AmmoTotal}", LoggerType.Warning);
            return weaponClient;
        }

        public static void WriteInfo(SyncServerPacket S, WeaponClient Info)
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
            S.WriteC(Info.Flags);
            S.WriteD(Info.WeaponId);
            S.WriteC(Info.Accessory);
            S.WriteC(Info.Extensions);
        }
    }
}