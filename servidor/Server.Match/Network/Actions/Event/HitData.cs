// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.HitData
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Match.Data.Enums;
using Server.Match.Data.Models;
using Server.Match.Data.Models.Event;
using Server.Match.Data.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Match.Network.Actions.Event
{
    public class HitData
    {
        
        public static List<HitDataInfo> ReadInfo(
          ActionModel Action,
          SyncClientPacket C,
          bool genLog,
          bool OnlyBytes = false)
        {
            List<HitDataInfo> hitDataInfoList = new List<HitDataInfo>();
            int num1 = (int)C.ReadC();
            for (int index1 = 0; index1 < num1; ++index1)
            {
                HitDataInfo hitDataInfo = new HitDataInfo()
                {
                    StartBullet = C.ReadTV(),
                    EndBullet = C.ReadTV(),
                    BulletPos = C.ReadTV(),
                    BoomInfo = C.ReadUH(),
                    ObjectId = C.ReadUH(),
                    HitIndex = C.ReadUD(),
                    WeaponId = C.ReadD(),
                    Accessory = C.ReadC(),
                    Extensions = C.ReadC()
                };
                if (!OnlyBytes)
                {
                    hitDataInfo.HitEnum = (HitType)AllUtils.GetHitHelmet(hitDataInfo.HitIndex);
                    if (hitDataInfo.BoomInfo > (ushort)0)
                    {
                        hitDataInfo.BoomPlayers = new List<int>();
                        for (int index2 = 0; index2 < 18; ++index2)
                        {
                            int num2 = 1 << index2;
                            if (((int)hitDataInfo.BoomInfo & num2) == num2)
                                hitDataInfo.BoomPlayers.Add(index2);
                        }
                    }
                    hitDataInfo.WeaponClass = (ClassType)ComDiv.GetIdStatics(hitDataInfo.WeaponId, 2);
                }
                if (genLog)
                {
                    CLogger.Print($"PVP Slot: {Action.Slot}; Weapon Id: {hitDataInfo.WeaponId}; Ext: {hitDataInfo.Extensions}; Acc: {hitDataInfo.Accessory}", LoggerType.Warning);
                    CLogger.Print($"PVP Slot: {Action.Slot}; Hit Data: {hitDataInfo.HitIndex} [Start]: X: {hitDataInfo.StartBullet.X}; Y: {hitDataInfo.StartBullet.Y}; Z: {hitDataInfo.StartBullet.Z}", LoggerType.Warning);
                    CLogger.Print($"PVP Slot: {Action.Slot}; Hit Data: {hitDataInfo.HitIndex} [Ended]: X: {hitDataInfo.EndBullet.X}; Y: {hitDataInfo.EndBullet.Y}; Z: {hitDataInfo.EndBullet.Z}", LoggerType.Warning);
                }
                hitDataInfoList.Add(hitDataInfo);
            }
            return hitDataInfoList;
        }

        public static void WriteInfo(SyncServerPacket S, List<HitDataInfo> Hits)
        {
            S.WriteC((byte)Hits.Count);
            foreach (HitDataInfo hit in Hits)
            {
                S.WriteTV(hit.StartBullet);
                S.WriteTV(hit.EndBullet);
                S.WriteTV(hit.BulletPos);
                S.WriteH(hit.BoomInfo);
                S.WriteH(hit.ObjectId);
                S.WriteD(hit.HitIndex);
                S.WriteD(hit.WeaponId);
                S.WriteC(hit.Accessory);
                S.WriteC(hit.Extensions);
            }
        }
    }
}