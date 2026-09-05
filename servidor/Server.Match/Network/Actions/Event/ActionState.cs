// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.ActionState
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Match.Data.Enums;
using Server.Match.Data.Models;
using Server.Match.Data.Models.Event;
using System.Runtime.CompilerServices;


namespace Server.Match.Network.Actions.Event
{
    public class ActionState
    {
        
        public static ActionStateInfo ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            ActionStateInfo actionStateInfo = new ActionStateInfo()
            {
                Action = (ActionFlag)C.ReadUH(),
                Value = C.ReadC(),
                Flag = (WeaponSyncType)C.ReadC()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; Action {actionStateInfo.Action}; Value: {actionStateInfo.Value}; Flag: {actionStateInfo.Flag}", LoggerType.Warning);
            return actionStateInfo;
        }

        public static void WriteInfo(SyncServerPacket S, ActionStateInfo Info)
        {
            S.WriteH((ushort)Info.Action);
            S.WriteC(Info.Value);
            S.WriteC((byte)Info.Flag);
        }
    }
}