// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.WeaponRecoil
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
    public class WeaponRecoil
    {
        
        public static WeaponRecoilInfo ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            WeaponRecoilInfo weaponRecoilInfo = new WeaponRecoilInfo()
            {
                RecoilHorzAngle = C.ReadT(),
                RecoilHorzMax = C.ReadT(),
                RecoilVertAngle = C.ReadT(),
                RecoilVertMax = C.ReadT(),
                Deviation = C.ReadT(),
                RecoilHorzCount = C.ReadC(),
                WeaponId = C.ReadD(),
                Accessory = C.ReadC(),
                Extensions = C.ReadC()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; WeaponId: {weaponRecoilInfo.WeaponId}", LoggerType.Warning);
            return weaponRecoilInfo;
        }

        public static void WriteInfo(SyncServerPacket S, WeaponRecoilInfo Info)
        {
            S.WriteT(Info.RecoilHorzAngle);
            S.WriteT(Info.RecoilHorzMax);
            S.WriteT(Info.RecoilVertAngle);
            S.WriteT(Info.RecoilVertMax);
            S.WriteT(Info.Deviation);
            S.WriteC(Info.RecoilHorzCount);
            S.WriteD(Info.WeaponId);
            S.WriteC(Info.Accessory);
            S.WriteC(Info.Extensions);
        }
    }
}