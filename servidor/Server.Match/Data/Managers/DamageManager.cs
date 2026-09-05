// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Managers.DamageManager
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.SharpDX;
using Plugin.Core.Utility;
using Server.Match.Data.Enums;
using Server.Match.Data.Models;
using Server.Match.Data.Sync.Server;
using Server.Match.Data.Utils;
using System;
using System.Collections.Generic;


namespace Server.Match.Data.Managers
{
    public static class DamageManager
    {
        public static void SabotageDestroy(
          RoomModel Room,
          PlayerModel Player,
          ObjectModel ObjM,
          ObjectInfo ObjI,
          int Damage)
        {
            if (ObjM.UltraSync <= 0 || Room.RoomType != RoomCondition.Destroy && Room.RoomType != RoomCondition.Defense)
                return;
            if (ObjM.UltraSync == 1 || ObjM.UltraSync == 3)
                Room.Bar1 = ObjI.Life;
            else if (ObjM.UltraSync == 2 || ObjM.UltraSync == 4)
                Room.Bar2 = ObjI.Life;
            SendMatchInfo.SendSabotageSync(Room, Player, Damage, ObjM.UltraSync == 4 ? 2 : 1);
        }

        private static void StaticMethod0(
          RoomModel A_0,
          List<DeathServerData> A_1,
          PlayerModel A_2,
          PlayerModel A_3,
          CharaDeath A_4)
        {
            try
            {
                A_2.Life = 0;
                A_2.Dead = true;
                A_2.LastDie = DateTimeUtil.Now();
                DeathServerData deathServerData = new DeathServerData()
                {
                    Player = A_2,
                    DeathType = A_4
                };
                try
                {
                    AssistServerData assist = AssistManager.GetAssist(A_2.Slot, A_0.RoomId);
                    deathServerData.AssistSlot = assist != null ? (assist.IsAssist ? assist.Killer : A_3.Slot) : A_3.Slot;
                    AssistManager.RemoveAssist(assist);
                }
                catch
                {
                    deathServerData.AssistSlot = A_3.Slot;
                }
                A_1.Add(deathServerData);
            }
            catch (Exception ex)
            {
                CLogger.Print("DamageManager.StaticMethod0: " + ex.Message, LoggerType.Error);
            }
        }

        private static void StaticMethod1(
          List<ObjectHitInfo> A_0,
          PlayerModel A_1,
          PlayerModel A_2,
          CharaDeath A_3,
          CharaHitPart A_4)
        {
            ObjectHitInfo objectHitInfo = new ObjectHitInfo(5)
            {
                ObjId = A_1.Slot,
                KillerSlot = A_2.Slot,
                DeathType = A_3,
                ObjLife = A_1.Life,
                HitPart = A_4
            };
            A_0.Add(objectHitInfo);
        }

        public static void BoomDeath(
          RoomModel Room,
          PlayerModel Killer,
          int Damage,
          int WeaponId,
          List<DeathServerData> Deaths,
          List<ObjectHitInfo> Objs,
          List<int> BoomPlayers,
          CharaHitPart HitPart,
          CharaDeath DeathType)
        {
            if (BoomPlayers == null || BoomPlayers.Count == 0)
                return;
            foreach (int boomPlayer in BoomPlayers)
            {
                PlayerModel Player;
                if (Room.GetPlayer(boomPlayer, out Player) && !Player.Dead)
                {
                    DamageManager.StaticMethod0(Room, Deaths, Player, Killer, DeathType);
                    if (Damage > 0)
                    {
                        if (ConfigLoader.UseHitMarker)
                            SendMatchInfo.SendHitMarkerSync(Room, Killer, DeathType, HitType.Normal, Damage);
                        ObjectHitInfo objectHitInfo = new ObjectHitInfo(2)
                        {
                            ObjId = Player.Slot,
                            ObjLife = Player.Life,
                            HitPart = HitPart,
                            KillerSlot = Killer.Slot,
                            Position = (Half3)((Vector3)Player.Position - (Vector3)Killer.Position),
                            DeathType = DeathType,
                            WeaponId = WeaponId
                        };
                        Objs.Add(objectHitInfo);
                    }
                }
            }
        }

        public static void SimpleDeath(
          RoomModel Room,
          List<DeathServerData> Deaths,
          List<ObjectHitInfo> Objs,
          PlayerModel Killer,
          PlayerModel Victim,
          int Damage,
          int WeaponId,
          CharaHitPart HitPart,
          CharaDeath DeathType)
        {
            try
            {
                Victim.Life -= Damage;
                if (Victim.Life <= 0)
                {
                    Victim.Life = 0;
                    Victim.Dead = true;
                    Victim.LastDie = DateTimeUtil.Now();
                    if (Killer != null && Killer != Victim && !Killer.Dead)
                    {
                        bool hs = DeathType == CharaDeath.HEADSHOT;
                        AllUtils.TrackKillBurst(Killer, hs);
                    }
                }
                try
                {
                    DamageManager.StaticMethod2(Room, Victim, Killer, Victim.Life);
                }
                catch
                {
                }
                if (Victim.Life > 0)
                {
                    Victim.Dead = false;
                    DamageManager.StaticMethod1(Objs, Victim, Killer, DeathType, HitPart);
                }
                else
                    DamageManager.StaticMethod0(Room, Deaths, Victim, Killer, DeathType);
            }
            catch (Exception ex)
            {
                if (Victim.Life <= 0)
                    Victim.Dead = true;
                CLogger.Print("DamageManager.SimpleDeath: " + ex.Message, LoggerType.Error);
            }
        }

        private static void StaticMethod2(RoomModel A_0, PlayerModel A_1, PlayerModel A_2, int A_3)
        {
            AssistServerData Assist = new AssistServerData()
            {
                RoomId = A_0.RoomId,
                Killer = A_2.Slot,
                Victim = A_1.Slot,
                IsKiller = A_3 <= 0,
                VictimDead = A_3 <= 0
            };
            Assist.IsAssist = !Assist.IsKiller;
            if (Assist.Killer == Assist.Victim)
                return;
            AssistManager.AddAssist(Assist);
        }
    }
}