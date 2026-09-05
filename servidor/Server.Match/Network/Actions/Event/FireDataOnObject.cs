// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.FireDataOnObject
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
    public class FireDataOnObject
    {
        
        public static FireDataObjectInfo ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            FireDataObjectInfo fireDataObjectInfo = new FireDataObjectInfo()
            {
                DeathType = (CharaDeath)C.ReadC(),
                HitPart = C.ReadC(),
                Unk = C.ReadC()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; Death Type: {fireDataObjectInfo.DeathType}; Hit Part: {fireDataObjectInfo.HitPart}", LoggerType.Warning);
            return fireDataObjectInfo;
        }

        public static void WriteInfo(SyncServerPacket S, FireDataObjectInfo Info)
        {
            S.WriteC((byte)Info.DeathType);
            S.WriteC(Info.HitPart);
            S.WriteC(Info.Unk);
        }
    }
}
