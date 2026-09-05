// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.SubHead.StageInfoObjControl
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Match.Data.Models.SubHead;
using System.Runtime.CompilerServices;


namespace Server.Match.Network.Actions.SubHead
{
    public class StageInfoObjControl
    {
        
        public static StageControlInfo ReadInfo(SyncClientPacket C, bool GenLog)
        {
            StageControlInfo stageControlInfo = new StageControlInfo()
            {
                Unk = C.ReadB(9)
            };
            if (GenLog)
                CLogger.Print("Sub Head: StageInfoObjControl; " + Bitwise.ToHexData("Controled Object Hex Data", stageControlInfo.Unk), LoggerType.Opcode);
            return stageControlInfo;
        }

        public static void WriteInfo(SyncServerPacket S, StageControlInfo Info) => S.WriteB(Info.Unk);
    }
}