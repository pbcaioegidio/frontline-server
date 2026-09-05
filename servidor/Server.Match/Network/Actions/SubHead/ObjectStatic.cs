// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.SubHead.ObjectStatic
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
    public class ObjectStatic
    {
        
        public static ObjectStaticInfo ReadInfo(SyncClientPacket C, bool GenLog)
        {
            ObjectStaticInfo objectStaticInfo = new ObjectStaticInfo()
            {
                Type = C.ReadUH(),
                Life = C.ReadUH(),
                DestroyedBySlot = C.ReadC()
            };
            if (GenLog)
                CLogger.Print($"Sub Head: ObjectStatic; Type: {objectStaticInfo.Type}; Life: {objectStaticInfo.Life}; Destroyed: {objectStaticInfo.DestroyedBySlot}", LoggerType.Warning);
            return objectStaticInfo;
        }

        public static void WriteInfo(SyncServerPacket S, ObjectStaticInfo Info)
        {
            S.WriteH(Info.Type);
            S.WriteH(Info.Life);
            S.WriteC(Info.DestroyedBySlot);
        }
    }
}
