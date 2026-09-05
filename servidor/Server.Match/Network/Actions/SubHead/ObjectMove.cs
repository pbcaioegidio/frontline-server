// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.SubHead.ObjectMove
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
    public class ObjectMove
    {
        
        public static ObjectMoveInfo ReadInfo(SyncClientPacket C, bool GenLog)
        {
            ObjectMoveInfo objectMoveInfo = new ObjectMoveInfo()
            {
                Unk = C.ReadB(16 /*0x10*/)
            };
            if (GenLog)
                CLogger.Print(Bitwise.ToHexData("UDP_SUB_HEAD: OBJECT_MOVE", objectMoveInfo.Unk), LoggerType.Opcode);
            return objectMoveInfo;
        }

        public static void WriteInfo(SyncServerPacket S, ObjectMoveInfo Info) => S.WriteB(Info.Unk);
    }
}