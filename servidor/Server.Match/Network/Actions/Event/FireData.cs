// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.FireData
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
    public class FireData
    {
        
        public static FireDataInfo ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            FireDataInfo fireDataInfo = new FireDataInfo()
            {
                Effect = C.ReadC(),
                Part = C.ReadC(),
                Index = C.ReadH(),
                WeaponId = C.ReadD(),
                Accessory = C.ReadC(),
                Extensions = C.ReadC(),
                X = C.ReadUH(),
                Y = C.ReadUH(),
                Z = C.ReadUH()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; Weapon Id: {fireDataInfo.WeaponId}; Extensions: {fireDataInfo.Extensions}; Fire: {fireDataInfo.Effect}; Part: {fireDataInfo.Part}; X: {fireDataInfo.X}; Y: {fireDataInfo.Y}; Z: {fireDataInfo.Z}", LoggerType.Warning);
            return fireDataInfo;
        }

        public static void WriteInfo(SyncServerPacket S, FireDataInfo Info)
        {
            S.WriteC(Info.Effect);
            S.WriteC(Info.Part);
            S.WriteH(Info.Index);
            S.WriteD(Info.WeaponId);
            S.WriteC(Info.Accessory);
            S.WriteC(Info.Extensions);
            S.WriteH(Info.X);
            S.WriteH(Info.Y);
            S.WriteH(Info.Z);
        }
    }
}