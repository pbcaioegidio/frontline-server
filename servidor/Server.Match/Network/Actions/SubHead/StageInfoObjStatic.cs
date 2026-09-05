// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.SubHead.StageInfoObjStatic
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Match.Data.Models.SubHead;
using System.Runtime.CompilerServices;


namespace Server.Match.Network.Actions.SubHead
{
    public class StageInfoObjStatic
    {
        
        public static StageStaticInfo ReadInfo(SyncClientPacket C, bool GenLog)
        {
            StageStaticInfo stageStaticInfo = new StageStaticInfo()
            {
                IsDestroyed = C.ReadC()
            };
            if (GenLog)
                CLogger.Print($"Sub Head: StageInfoObjStatic; Destroyed: {stageStaticInfo.IsDestroyed}", LoggerType.Warning);
            return stageStaticInfo;
        }

        public static void WriteInfo(SyncServerPacket S, StageStaticInfo Info)
        {
            S.WriteC(Info.IsDestroyed);
        }
    }
}