// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.HpSync
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
    public class HpSync
    {
        
        public static HPSyncInfo ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            HPSyncInfo hpSyncInfo = new HPSyncInfo()
            {
                CharaLife = C.ReadUH()
            };
            if (GenLog)
                CLogger.Print($"PVP Slot: {Action.Slot}; is using Chara with HP ({hpSyncInfo.CharaLife})", LoggerType.Warning);
            return hpSyncInfo;
        }

        public static void WriteInfo(SyncServerPacket S, HPSyncInfo Info) => S.WriteH(Info.CharaLife);
    }
}