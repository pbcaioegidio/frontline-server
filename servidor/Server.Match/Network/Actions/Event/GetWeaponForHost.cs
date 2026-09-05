// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.GetWeaponForHost
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
    public class GetWeaponForHost
    {
        
        public static WeaponHost ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            WeaponHost weaponHost = new WeaponHost()
            {
                DeathType = C.ReadC(),
                X = C.ReadUH(),
                Y = C.ReadUH(),
                Z = C.ReadUH(),
                Unks = C.ReadC(),
                UNK = C.ReadD(),
                HitPart = C.ReadC()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; UNK: {weaponHost.UNK}; Death Type: {weaponHost.DeathType}; Hit: {weaponHost.HitPart}; X: {weaponHost.X}; Y: {weaponHost.Y}; Z: {weaponHost.Z}", LoggerType.Warning);
            return weaponHost;
        }

        public static void WriteInfo(SyncServerPacket S, WeaponHost Info)
        {
            S.WriteC(Info.DeathType);
            S.WriteH(Info.X);
            S.WriteH(Info.Y);
            S.WriteH(Info.Z);
            S.WriteC(Info.Unks);
            S.WriteD(Info.UNK);
            S.WriteC(Info.HitPart);
        }
    }
}