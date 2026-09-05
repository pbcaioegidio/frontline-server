// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.CharaFireNHitData
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
    public class CharaFireNHitData
    {
        
        public static List<CharaFireNHitDataInfo> ReadInfo(
          ActionModel Action,
          SyncClientPacket C,
          bool GenLog,
          bool OnlyBytes = false)
        {
            List<CharaFireNHitDataInfo> fireNhitDataInfoList = new List<CharaFireNHitDataInfo>();
            int num = (int)C.ReadC();
            for (int index = 0; index < num; ++index)
            {
                CharaFireNHitDataInfo fireNhitDataInfo = new CharaFireNHitDataInfo()
                {
                    WeaponId = C.ReadD(),
                    Accessory = C.ReadC(),
                    Extensions = C.ReadC(),
                    HitInfo = C.ReadUD(),
                    Unk = C.ReadH(),
                    X = C.ReadUH(),
                    Y = C.ReadUH(),
                    Z = C.ReadUH()
                };
                if (!OnlyBytes)
                    fireNhitDataInfo.WeaponClass = (ClassType)ComDiv.GetIdStatics(fireNhitDataInfo.WeaponId, 2);
                if (GenLog)
                    CLogger.Print($"PVP Slot: {Action.Slot}; Weapon Id: {fireNhitDataInfo.WeaponId}; X: {fireNhitDataInfo.X} Y: {fireNhitDataInfo.Y} Z: {fireNhitDataInfo.Z}", LoggerType.Warning);
                fireNhitDataInfoList.Add(fireNhitDataInfo);
            }
            return fireNhitDataInfoList;
        }

        public static void WriteInfo(SyncServerPacket S, List<CharaFireNHitDataInfo> Hits)
        {
            S.WriteC((byte)Hits.Count);
            foreach (CharaFireNHitDataInfo hit in Hits)
            {
                S.WriteD(hit.WeaponId);
                S.WriteC(hit.Accessory);
                S.WriteC(hit.Extensions);
                S.WriteD(hit.HitInfo);
                S.WriteH(hit.Unk);
                S.WriteH(hit.X);
                S.WriteH(hit.Y);
                S.WriteH(hit.Z);
            }
        }
    }
}