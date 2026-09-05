// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.Suicide
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Match.Data.Models;
using Server.Match.Data.Models.Event;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Match.Network.Actions.Event
{
    public class Suicide
    {
        
        public static List<SuicideInfo> ReadInfo(
          ActionModel Action,
          SyncClientPacket C,
          bool GenLog,
          bool OnlyBytes = false)
        {
            List<SuicideInfo> suicideInfoList = new List<SuicideInfo>();
            int num = (int)C.ReadC();
            for (int index = 0; index < num; ++index)
            {
                SuicideInfo suicideInfo = new SuicideInfo()
                {
                    PlayerPos = C.ReadUHV(),
                    WeaponId = C.ReadD(),
                    Accessory = C.ReadC(),
                    Extensions = C.ReadC(),
                    HitInfo = C.ReadUD()
                };
                if (!OnlyBytes)
                    suicideInfo.WeaponClass = (ClassType)ComDiv.GetIdStatics(suicideInfo.WeaponId, 2);
                if (GenLog)
                    CLogger.Print($"PVP Slot: {Action.Slot}; Weapon Id: {suicideInfo.WeaponId}; Ext: {suicideInfo.Extensions}; Acc: {suicideInfo.Accessory}; Suicide Hit: {suicideInfo.HitInfo}; X: {suicideInfo.PlayerPos.X}; Y: {suicideInfo.PlayerPos.Y}; Z: {suicideInfo.PlayerPos.Z}", LoggerType.Warning);
                suicideInfoList.Add(suicideInfo);
            }
            return suicideInfoList;
        }

        public static void WriteInfo(SyncServerPacket S, List<SuicideInfo> Hits)
        {
            S.WriteC((byte)Hits.Count);
            foreach (SuicideInfo hit in Hits)
            {
                S.WriteHV(hit.PlayerPos);
                S.WriteD(hit.WeaponId);
                S.WriteC(hit.Accessory);
                S.WriteC(hit.Extensions);
                S.WriteD(hit.HitInfo);
            }
        }
    }
}