// Decompiled with JetBrains decompiler
// Type: Server.Match.Network.Actions.Event.GrenadeHit
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
    public class GrenadeHit
    {
        
        public static List<GrenadeHitInfo> ReadInfo(
          ActionModel Action,
          SyncClientPacket C,
          bool GenLog,
          bool OnlyBytes = false)
        {
            List<GrenadeHitInfo> grenadeHitInfoList = new List<GrenadeHitInfo>();
            int num1 = (int)C.ReadC();
            for (int index1 = 0; index1 < num1; ++index1)
            {
                GrenadeHitInfo grenadeHitInfo = new GrenadeHitInfo()
                {
                    WeaponId = C.ReadD(),
                    Accessory = C.ReadC(),
                    Extensions = C.ReadC(),
                    BoomInfo = C.ReadUH(),
                    ObjectId = C.ReadUH(),
                    HitInfo = C.ReadUD(),
                    PlayerPos = C.ReadUHV(),
                    FirePos = C.ReadUHV(),
                    HitPos = C.ReadUHV(),
                    GrenadesCount = C.ReadUH(),
                    DeathType = (CharaDeath)C.ReadC()
                };
                if (!OnlyBytes)
                {
                    grenadeHitInfo.HitEnum = (HitType)AllUtils.GetHitHelmet(grenadeHitInfo.HitInfo);
                    if (grenadeHitInfo.BoomInfo > (ushort)0)
                    {
                        grenadeHitInfo.BoomPlayers = new List<int>();
                        for (int index2 = 0; index2 < 18; ++index2)
                        {
                            int num2 = 1 << index2;
                            if (((int)grenadeHitInfo.BoomInfo & num2) == num2)
                                grenadeHitInfo.BoomPlayers.Add(index2);
                        }
                    }
                    grenadeHitInfo.WeaponClass = (ClassType)ComDiv.GetIdStatics(grenadeHitInfo.WeaponId, 2);
                }
                if (GenLog)
                {
                    CLogger.Print($"PVP Slot: {Action.Slot}; Weapon Id: {grenadeHitInfo.WeaponId}; Ext: {grenadeHitInfo.Extensions}; Acc: {grenadeHitInfo.Accessory}", LoggerType.Warning);
                    CLogger.Print($"PVP Slot: {Action.Slot}; Grenade Hit: {grenadeHitInfo.HitInfo}; [Object Postion] X: {grenadeHitInfo.HitPos.X}; Y: {grenadeHitInfo.HitPos.Y}; Z: {grenadeHitInfo.HitPos.Z}", LoggerType.Warning);
                    CLogger.Print($"PVP Slot: {Action.Slot}; Grenade Hit: {grenadeHitInfo.HitInfo}; [Player Postion] X: {grenadeHitInfo.FirePos.X}; Y: {grenadeHitInfo.FirePos.Y}; Z: {grenadeHitInfo.FirePos.Z}", LoggerType.Warning);
                }
                grenadeHitInfoList.Add(grenadeHitInfo);
            }
            return grenadeHitInfoList;
        }

        public static void WriteInfo(SyncServerPacket S, List<GrenadeHitInfo> Hits)
        {
            S.WriteC((byte)Hits.Count);
            foreach (GrenadeHitInfo hit in Hits)
            {
                S.WriteD(hit.WeaponId);
                S.WriteC(hit.Accessory);
                S.WriteC(hit.Extensions);
                S.WriteH(hit.BoomInfo);
                S.WriteH(hit.ObjectId);
                S.WriteD(hit.HitInfo);
                S.WriteHV(hit.PlayerPos);
                S.WriteHV(hit.FirePos);
                S.WriteHV(hit.HitPos);
                S.WriteH(hit.GrenadesCount);
                S.WriteC((byte)hit.DeathType);
            }
        }
    }
}