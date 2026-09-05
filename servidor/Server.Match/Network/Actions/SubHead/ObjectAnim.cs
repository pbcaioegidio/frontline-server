// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.SubHead.ObjectAnim
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
    public class ObjectAnim
    {
        
        public static ObjectAnimInfo ReadInfo(SyncClientPacket C, bool GenLog)
        {
            ObjectAnimInfo objectAnimInfo = new ObjectAnimInfo()
            {
                Life = C.ReadUH(),
                Anim1 = C.ReadC(),
                Anim2 = C.ReadC(),
                SyncDate = C.ReadT()
            };
            if (GenLog)
                CLogger.Print($"Sub Head: ObjectAnim; Life: {objectAnimInfo.Life}; Animation[1]: {objectAnimInfo.Anim1}; Animation[2]: {objectAnimInfo.Anim2}; Sync: {objectAnimInfo.SyncDate}", LoggerType.Warning);
            return objectAnimInfo;
        }

        public static void WriteInfo(SyncServerPacket S, ObjectAnimInfo Info)
        {
            S.WriteH(Info.Life);
            S.WriteC(Info.Anim1);
            S.WriteC(Info.Anim2);
            S.WriteT(Info.SyncDate);
        }
    }
}