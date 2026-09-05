// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.MissionData
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
    public class MissionData
    {
        
        public static MissionDataInfo ReadInfo(
          ActionModel Action,
          SyncClientPacket C,
          float Time,
          bool GenLog,
          bool OnlyBytes = false)
        {
            MissionDataInfo missionDataInfo = new MissionDataInfo()
            {
                PlantTime = C.ReadT(),
                Bomb = (int)C.ReadC()
            };
            if (!OnlyBytes)
            {
                missionDataInfo.BombEnum = (BombFlag)(missionDataInfo.Bomb & 15);
                missionDataInfo.BombId = missionDataInfo.Bomb >> 4;
            }
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; Bomb: {missionDataInfo.BombEnum}; Id: {missionDataInfo.BombId}; PlantTime: {missionDataInfo.PlantTime}; Time: {Time}", LoggerType.Warning);
            return missionDataInfo;
        }

        public static void WriteInfo(SyncServerPacket S, MissionDataInfo Info)
        {
            S.WriteT(Info.PlantTime);
            S.WriteC((byte)Info.Bomb);
        }
    }
}