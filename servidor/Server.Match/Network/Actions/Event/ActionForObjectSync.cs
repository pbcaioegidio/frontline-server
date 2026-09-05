// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.ActionForObjectSync
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Match.Data.Models;
using Server.Match.Data.Models.Event.Event;
using System.Runtime.CompilerServices;


namespace Server.Match.Network.Actions.Event
{
    public class ActionForObjectSync
    {
        
        public static ActionObjectInfo ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            ActionObjectInfo actionObjectInfo = new ActionObjectInfo()
            {
                Unk1 = C.ReadC(),
                Unk2 = C.ReadC()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot} Unk1: {actionObjectInfo.Unk1}; Unk2: {actionObjectInfo.Unk2}", LoggerType.Warning);
            return actionObjectInfo;
        }

        public static void WriteInfo(SyncServerPacket S, ActionObjectInfo Info)
        {
            S.WriteC(Info.Unk1);
            S.WriteC(Info.Unk2);
        }
    }
}