// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.WeaponSync
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Match.Data.Models;
using Server.Match.Data.Models.Event;
using System.Runtime.CompilerServices;

namespace Server.Match.Network.Actions.Event
{
    public class WeaponSync
    {
        
        public static WeaponSyncInfo ReadInfo(
          ActionModel Action,
          SyncClientPacket C,
          bool GenLog,
          bool OnlyBytes = false)
        {
            WeaponSyncInfo weaponSyncInfo = new WeaponSyncInfo()
            {
                WeaponId = C.ReadD(),
                Accessory = C.ReadC(),
                Extensions = C.ReadC()
            };
            if (!OnlyBytes)
                weaponSyncInfo.WeaponClass = (ClassType)ComDiv.GetIdStatics(weaponSyncInfo.WeaponId, 2);
            if (GenLog)
                CLogger.Print($"PVP Slot {Action.Slot}; Weapon Id: {weaponSyncInfo.WeaponId}; Extensions: {weaponSyncInfo.Extensions}; Unknowns: {weaponSyncInfo.Accessory}", LoggerType.Warning);
            return weaponSyncInfo;
        }

        public static void WriteInfo(SyncServerPacket S, WeaponSyncInfo Info)
        {
            S.WriteD(Info.WeaponId);
            S.WriteC(Info.Accessory);
            S.WriteC(Info.Extensions);
        }
    }
}