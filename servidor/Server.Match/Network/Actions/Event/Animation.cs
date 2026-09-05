// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.Animation
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
    public class Animation
    {
        
        public static AnimationInfo ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            AnimationInfo animationInfo = new AnimationInfo()
            {
                Animation = C.ReadUH(),
                PositionX = C.ReadUH(),
                PositionY = C.ReadUH(),
                PositionZ = C.ReadUH()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; POV: {animationInfo.Animation}", LoggerType.Warning);
            return animationInfo;
        }

        public static void WriteInfo(SyncServerPacket S, AnimationInfo Info)
        {
            S.WriteH(Info.Animation);
            S.WriteH(Info.PositionX);
            S.WriteH(Info.PositionY);
            S.WriteH(Info.PositionZ);
        }
    }
}