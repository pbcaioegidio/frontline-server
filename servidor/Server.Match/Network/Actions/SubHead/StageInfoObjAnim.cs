// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.SubHead.StageInfoObjAnim
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
    public class StageInfoObjAnim
    {
        
        public static StageAnimInfo ReadInfo(SyncClientPacket C, bool GenLog)
        {
            StageAnimInfo stageAnimInfo = new StageAnimInfo()
            {
                Unk = C.ReadC(),
                Life = C.ReadUH(),
                SyncDate = C.ReadT(),
                Anim1 = C.ReadC(),
                Anim2 = C.ReadC()
            };
            if (GenLog)
                CLogger.Print($"Sub Head: StageObjAnim; Unk: {stageAnimInfo.Unk}; Life: {stageAnimInfo.Life}; Sync: {stageAnimInfo.SyncDate}; Animation[1]: {stageAnimInfo.Anim1}; Animation[2]: {stageAnimInfo.Anim2}", LoggerType.Warning);
            return stageAnimInfo;
        }

        public static void WriteInfo(SyncServerPacket S, StageAnimInfo Info)
        {
            S.WriteC(Info.Unk);
            S.WriteH(Info.Life);
            S.WriteT(Info.SyncDate);
            S.WriteC(Info.Anim1);
            S.WriteC(Info.Anim2);
        }
    }
}