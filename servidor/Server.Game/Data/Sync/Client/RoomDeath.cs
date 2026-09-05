// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.RoomDeath
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Update;
using Server.Game.Data.Utils;
using Server.Game.Data.XML;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Data.Sync.Client
{
    public class RoomDeath
    {
        
        public static void Load(SyncClientPacket clientPacket)
        {
            int roomId = (int)clientPacket.ReadH();
            int channelId = (int)clientPacket.ReadH();
            int serverId = (int)clientPacket.ReadH();
            byte killCount = clientPacket.ReadC();
            byte killerSlotIdx = clientPacket.ReadC();
            int weaponId = clientPacket.ReadD();
            float killerX = clientPacket.ReadT();
            float killerY = clientPacket.ReadT();
            float killerZ = clientPacket.ReadT();
            byte killFlag = clientPacket.ReadC();
            byte unknownByte = clientPacket.ReadC();
            int expectedDataLength = (int)killCount * 25;

            if (clientPacket.ToArray().Length > 28 + expectedDataLength)
                CLogger.Print($"Invalid Death (Length > 53): {clientPacket.ToArray().Length}", LoggerType.Warning);

            ChannelModel channel = ChannelsXML.GetChannel(serverId, channelId);
            if (channel == null)
                return;

            RoomModel room = channel.GetRoom(roomId);
            if (room == null || room.RoundTime.IsTimer() || room.State != RoomState.BATTLE)
                return;

            SlotModel killerSlot = room.GetSlot((int)killerSlotIdx);
            if (killerSlot == null || killerSlot.State != SlotState.BATTLE)
                return;

            // Limites básicos: pacote de morte inválido / flood
            if (killCount == 0 || killCount > 16)
            {
                CLogger.Print($"[AntiCheat] Death sync inválido KillsCount={killCount} Room={roomId}", LoggerType.Warning);
                return;
            }

            DateTime now = DateTimeUtil.Now();
            if (killerSlot.LastDeathSync != default(DateTime) &&
                (now - killerSlot.LastDeathSync).TotalMilliseconds < 40 &&
                killCount > 1)
            {
                CLogger.Print($"[AntiCheat] Death sync flood Slot={killerSlotIdx} Room={roomId}", LoggerType.Warning);
                return;
            }
            killerSlot.LastDeathSync = now;

            FragInfos killInfo = new FragInfos()
            {
                KillsCount = killCount,
                KillerSlot = killerSlotIdx,
                WeaponId = weaponId,
                X = killerX,
                Y = killerY,
                Z = killerZ,
                Flag = killFlag,
                Unk = unknownByte
            };

            bool isSuicide = false;
            for (int index = 0; index < (int)killCount; ++index)
            {
                byte victimSlotIdx = clientPacket.ReadC();
                byte weaponClass = clientPacket.ReadC();
                byte hitspotInfo = clientPacket.ReadC();
                float victimX = clientPacket.ReadT();
                float victimY = clientPacket.ReadT();
                float victimZ = clientPacket.ReadT();
                byte assistSlotIdx = clientPacket.ReadC();
                byte unknownByte2 = clientPacket.ReadC();
                byte[] unknownBytes = clientPacket.ReadB(8);

                SlotModel victimSlot = room.GetSlot((int)victimSlotIdx);
                if (victimSlot != null && victimSlot.State == SlotState.BATTLE && !victimSlot.DeathState.HasFlag(DeadEnum.Dead))
                {
                    FragModel fragInfo = new FragModel()
                    {
                        VictimSlot = victimSlotIdx,
                        WeaponClass = weaponClass,
                        HitspotInfo = hitspotInfo,
                        X = victimX,
                        Y = victimY,
                        Z = victimZ,
                        AssistSlot = assistSlotIdx,
                        Unk = unknownByte2,
                        Unks = unknownBytes
                    };

                    if ((int)killInfo.KillerSlot == (int)victimSlotIdx)
                        isSuicide = true;

                    killInfo.Frags.Add(fragInfo);
                }
            }

            killInfo.KillsCount = (byte)killInfo.Frags.Count;
            KillFragInfo.GenDeath(room, killerSlot, killInfo, isSuicide);
        }

        
        public static void RegistryFragInfos(
          RoomModel room,
          SlotModel killerSlot,
          out int score,
          bool isBotMode,
          bool isSuicide,
          FragInfos killsInfo)
        {
            score = 0;
            ItemClass weaponItemClass = (ItemClass)ComDiv.GetIdStatics(killsInfo.WeaponId, 1);
            ClassType weaponClassType = (ClassType)ComDiv.GetIdStatics(killsInfo.WeaponId, 2);

            foreach (FragModel frag in killsInfo.Frags)
            {
                CharaDeath deathType = (CharaDeath)((int)frag.HitspotInfo >> 4);

                if ((int)killsInfo.KillsCount - (isSuicide ? 1 : 0) <= 1)
                {
                    int killType = 0;
                    switch (deathType)
                    {
                        case CharaDeath.DEFAULT:
                            if (weaponItemClass == ItemClass.Melee)
                            {
                                killType = 6;
                                break;
                            }
                            break;
                        case CharaDeath.HEADSHOT:
                            killType = 4;
                            break;
                    }

                    if (killType > 0)
                    {
                        int lastKillType = killerSlot.LastKillState >> 12;
                        switch (killType)
                        {
                            case 4: // Headshot
                                if (lastKillType != 4)
                                    killerSlot.RepeatLastState = false;
                                killerSlot.LastKillState = killType << 12 | killerSlot.KillsOnLife + 1;
                                if (killerSlot.RepeatLastState)
                                {
                                    frag.KillFlag |= (killerSlot.LastKillState & 16383 /*0x3FFF*/) <= 1 ? KillingMessage.Headshot : KillingMessage.ChainHeadshot;
                                    break;
                                }
                                frag.KillFlag |= KillingMessage.Headshot;
                                killerSlot.RepeatLastState = true;
                                break;
                            case 6: // Melee
                                if (lastKillType != 6)
                                    killerSlot.RepeatLastState = false;
                                killerSlot.LastKillState = killType << 12 | killerSlot.KillsOnLife + 1;
                                if (killerSlot.RepeatLastState && (killerSlot.LastKillState & 16383 /*0x3FFF*/) > 1)
                                {
                                    frag.KillFlag |= KillingMessage.ChainSlugger;
                                    break;
                                }
                                killerSlot.RepeatLastState = true;
                                break;
                        }
                    }
                    else
                    {
                        killerSlot.LastKillState = 0;
                        killerSlot.RepeatLastState = false;
                    }
                }
                else
                    frag.KillFlag |= deathType == CharaDeath.BOOM || deathType == CharaDeath.OBJECT_EXPLOSION || deathType == CharaDeath.POISON || deathType == CharaDeath.HOWL || deathType == CharaDeath.TRAMPLED || weaponClassType == ClassType.Shotgun ? KillingMessage.MassKill : KillingMessage.PiercingShot;

                byte victimSlotIdx = frag.VictimSlot;
                byte assistSlotIdx = frag.AssistSlot;
                SlotModel victimSlot = room.Slots[(int)victimSlotIdx];
                SlotModel assistSlot = room.Slots[(int)assistSlotIdx];

                if (victimSlot.DeathState.HasFlag(DeadEnum.Dead))
                    continue;

                if (victimSlot.KillsOnLife > 3)
                    frag.KillFlag |= KillingMessage.ChainStopper;

                if (killsInfo.WeaponId != 19016 && killsInfo.WeaponId != 19022 || (int)killsInfo.KillerSlot != (int)victimSlotIdx || !victimSlot.SpecGM)
                    ++victimSlot.AllDeaths;

                if ((int)killsInfo.KillerSlot != (int)assistSlotIdx)
                    ++assistSlot.AllAssists;

                if (room.RoomType != RoomCondition.FreeForAll)
                {
                    if (killerSlot.Team != victimSlot.Team)
                    {
                        score += AllUtils.GetKillScore(frag.KillFlag);
                        ++killerSlot.AllKills;
                        if (killerSlot.DeathState == DeadEnum.Alive)
                            ++killerSlot.KillsOnLife;

                        if (victimSlot.Team == TeamEnum.FR_TEAM)
                        {
                            ++room.FRDeaths;
                            ++room.CTKills;
                        }
                        else
                        {
                            ++room.CTDeaths;
                            ++room.FRKills;
                        }

                        if (room.IsDinoMode("DE"))
                        {
                            if (killerSlot.Team != TeamEnum.FR_TEAM)
                                room.CTDino += 4;
                            else
                                room.FRDino += 4;
                        }
                    }
                }
                else
                {
                    ++killerSlot.AllKills;
                    if (killerSlot.DeathState == DeadEnum.Alive)
                        ++killerSlot.KillsOnLife;
                }

                victimSlot.LastKillState = 0;
                victimSlot.KillsOnLife = 0;
                victimSlot.RepeatLastState = false;
                victimSlot.PassSequence = 0;
                victimSlot.DeathState = DeadEnum.Dead;

                if (!isBotMode)
                {
                    switch (weaponClassType)
                    {
                        case ClassType.Assault:
                            ++killerSlot.AR[0];
                            ++victimSlot.AR[1];
                            break;
                        case ClassType.SMG:
                            ++killerSlot.SMG[0];
                            ++victimSlot.SMG[1];
                            break;
                        case ClassType.Sniper:
                            ++killerSlot.SR[0];
                            ++victimSlot.SR[1];
                            break;
                        case ClassType.Shotgun:
                            ++killerSlot.SG[0];
                            ++victimSlot.SG[1];
                            break;
                        case ClassType.Machinegun:
                            ++killerSlot.MG[0];
                            ++victimSlot.MG[1];
                            break;
                        case ClassType.Shield:
                            ++killerSlot.SHD[0];
                            ++victimSlot.SHD[1];
                            break;
                    }
                    AllUtils.CompleteMission(room, victimSlot, MissionType.DEATH, 0);
                }

                if (deathType == CharaDeath.HEADSHOT)
                    ++killerSlot.AllHeadshots;
            }
        }

        public static void EndBattleByDeath(
          RoomModel room,
          SlotModel killerSlot,
          bool isBotMode,
          bool isSuicide,
          FragInfos killsInfo)
        {
            if (room.RoomType == RoomCondition.DeathMatch && !isBotMode)
                AllUtils.CheckBattleEndByKills(room, isBotMode);
            else if (room.RoomType != RoomCondition.FreeForAll)
            {
                if (killerSlot.SpecGM || room.RoomType != RoomCondition.Bomb && room.RoomType != RoomCondition.Annihilation && room.RoomType != RoomCondition.Convoy && room.RoomType != RoomCondition.Ace)
                    return;

                if (room.RoomType != RoomCondition.Bomb && room.RoomType != RoomCondition.Annihilation && room.RoomType != RoomCondition.Convoy)
                {
                    if (room.RoomType != RoomCondition.Ace)
                        return;

                    SlotModel[] aceSlots = new SlotModel[2]
                    {
                        room.GetSlot(0),
                        room.GetSlot(1)
                    };

                    if (aceSlots[0].DeathState == DeadEnum.Dead)
                    {
                        ++room.CTRounds;
                        AllUtils.BattleEndRound(room, TeamEnum.CT_TEAM, true, killsInfo, killerSlot);
                    }
                    else
                    {
                        if (aceSlots[1].DeathState != DeadEnum.Dead)
                            return;
                        ++room.FRRounds;
                        AllUtils.BattleEndRound(room, TeamEnum.FR_TEAM, true, killsInfo, killerSlot);
                    }
                }
                else
                {
                    TeamEnum winnerTeam = TeamEnum.TEAM_DRAW;
                    int frPlayersAlive;
                    int ctPlayersAlive;
                    int frPlayersDead;
                    int ctPlayersDead;
                    room.GetPlayingPlayers(true, out frPlayersAlive, out ctPlayersAlive, out frPlayersDead, out ctPlayersDead);

                    TeamEnum killerTeamEffective;
                    AdjustTeamCountsForSwap(room, killerSlot, ref frPlayersAlive, ref ctPlayersAlive, ref frPlayersDead, ref ctPlayersDead, out killerTeamEffective);

                    if (((frPlayersDead != frPlayersAlive ? 0 : (killerTeamEffective == TeamEnum.FR_TEAM ? 1 : 0)) & (isSuicide ? 1 : 0)) != 0 && !room.ActiveC4)
                    {
                        DetermineWinnerTeam(room, ref winnerTeam, 1);
                        AllUtils.BattleEndRound(room, winnerTeam, true, killsInfo, killerSlot);
                    }
                    else if (ctPlayersDead == ctPlayersAlive && killerTeamEffective == TeamEnum.CT_TEAM)
                    {
                        DetermineWinnerTeam(room, ref winnerTeam, 2);
                        AllUtils.BattleEndRound(room, winnerTeam, true, killsInfo, killerSlot);
                    }
                    else if (frPlayersDead == frPlayersAlive && killerTeamEffective == TeamEnum.CT_TEAM)
                    {
                        if (room.ActiveC4)
                        {
                            if (isSuicide)
                                DetermineWinnerTeam(room, ref winnerTeam, 2);
                        }
                        else
                            DetermineWinnerTeam(room, ref winnerTeam, 1);
                        AllUtils.BattleEndRound(room, winnerTeam, false, killsInfo, killerSlot);
                    }
                    else
                    {
                        if (ctPlayersDead != ctPlayersAlive || killerTeamEffective != TeamEnum.FR_TEAM)
                            return;

                        if (isSuicide && room.ActiveC4)
                            DetermineWinnerTeam(room, ref winnerTeam, 1);
                        else
                            DetermineWinnerTeam(room, ref winnerTeam, 2);
                        AllUtils.BattleEndRound(room, winnerTeam, true, killsInfo, killerSlot);
                    }
                }
            }
            else
                AllUtils.BattleEndKills(room);
        }

        private static void AdjustTeamCountsForSwap(
          RoomModel room,
          SlotModel killerSlot,
          ref int frPlayersAlive,
          ref int ctPlayersAlive,
          ref int frPlayersDead,
          ref int ctPlayersDead,
          out TeamEnum killerTeamEffective)
        {
            killerTeamEffective = killerSlot.Team;

            if (!room.IsTeamSwap() || !room.SwapRound)
                return;

            killerTeamEffective = killerTeamEffective == TeamEnum.FR_TEAM ? TeamEnum.CT_TEAM : TeamEnum.FR_TEAM;

            // Swap FR and CT counts
            int tempFRAlive = frPlayersAlive;
            int tempCTAlive = ctPlayersAlive;
            ctPlayersAlive = tempFRAlive;
            frPlayersAlive = tempCTAlive;

            int tempFRDead = frPlayersDead;
            int tempCTDead = ctPlayersDead;
            ctPlayersDead = tempFRDead;
            frPlayersDead = tempCTDead;
        }

        private static void DetermineWinnerTeam(RoomModel room, ref TeamEnum winnerTeam, int winnerType)
        {
            switch (winnerType)
            {
                case 1: // CT wins
                    if (room.IsTeamSwap() && room.SwapRound)
                    {
                        winnerTeam = TeamEnum.FR_TEAM;
                        ++room.FRRounds;
                        break;
                    }
                    winnerTeam = TeamEnum.CT_TEAM;
                    ++room.CTRounds;
                    break;
                case 2: // FR wins
                    if (room.IsTeamSwap() && room.SwapRound)
                    {
                        winnerTeam = TeamEnum.CT_TEAM;
                        ++room.CTRounds;
                        break;
                    }
                    winnerTeam = TeamEnum.FR_TEAM;
                    ++room.FRRounds;
                    break;
            }
        }
    }
}