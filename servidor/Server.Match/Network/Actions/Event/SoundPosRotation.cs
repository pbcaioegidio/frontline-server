// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.SoundPosRotation
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
    public class SoundPosRotation
    {
        
        public static SoundPosRotationInfo ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            SoundPosRotationInfo soundPosRotationInfo = new SoundPosRotationInfo()
            {
                Time = C.ReadT()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; Time: {soundPosRotationInfo.Time}", LoggerType.Warning);
            return soundPosRotationInfo;
        }

        
        public static SoundPosRotationInfo ReadInfo(
          ActionModel Action,
          SyncClientPacket C,
          float Time,
          bool GenLog)
        {
            SoundPosRotationInfo soundPosRotationInfo = new SoundPosRotationInfo()
            {
                Time = Time
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; Time: {soundPosRotationInfo.Time}", LoggerType.Warning);
            return soundPosRotationInfo;
        }

        public static void WriteInfo(SyncServerPacket S, SoundPosRotationInfo Info)
        {
            S.WriteT(Info.Time);
        }
    }
}