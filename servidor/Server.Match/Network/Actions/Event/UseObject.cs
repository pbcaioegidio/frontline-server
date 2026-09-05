// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.UseObject
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Match.Data.Enums;
using Server.Match.Data.Models;
using Server.Match.Data.Models.Event;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Match.Network.Actions.Event
{
    public class UseObject
    {
        
        public static List<UseObjectInfo> ReadInfo(ActionModel Action, SyncClientPacket C, bool GenLog)
        {
            List<UseObjectInfo> useObjectInfoList = new List<UseObjectInfo>();
            int num = (int)C.ReadC();
            for (int index = 0; index < num; ++index)
            {
                UseObjectInfo useObjectInfo = new UseObjectInfo()
                {
                    ObjectId = C.ReadUH(),
                    Use = C.ReadC(),
                    SpaceFlags = (CharaMoves)C.ReadC()
                };
                if (GenLog)
                    CLogger.Print($"PVP Slot: {Action.Slot}; Use Object: {useObjectInfo.Use}; Flag: {useObjectInfo.SpaceFlags}; ObjectId: {useObjectInfo.ObjectId}", LoggerType.Warning);
                useObjectInfoList.Add(useObjectInfo);
            }
            return useObjectInfoList;
        }

        public static void WriteInfo(SyncServerPacket S, List<UseObjectInfo> Infos)
        {
            S.WriteC((byte)Infos.Count);
            foreach (UseObjectInfo info in Infos)
            {
                S.WriteH(info.ObjectId);
                S.WriteC(info.Use);
                S.WriteC((byte)info.SpaceFlags);
            }
        }
    }
}