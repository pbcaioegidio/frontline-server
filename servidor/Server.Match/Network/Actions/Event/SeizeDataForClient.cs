// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.SeizeDataForClient
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
    public class SeizeDataForClient
    {
        
        public static SeizeDataForClientInfo ReadInfo(
          ActionModel Action,
          SyncClientPacket C,
          bool GenLog)
        {
            SeizeDataForClientInfo dataForClientInfo = new SeizeDataForClientInfo()
            {
                UseTime = C.ReadT(),
                Flags = C.ReadC()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; Use Time: {dataForClientInfo.UseTime}; Flags: {dataForClientInfo.Flags}", LoggerType.Warning);
            return dataForClientInfo;
        }

        public static void WriteInfo(SyncServerPacket S, SeizeDataForClientInfo Info)
        {
            S.WriteT(Info.UseTime);
            S.WriteC(Info.Flags);
        }
    }
}