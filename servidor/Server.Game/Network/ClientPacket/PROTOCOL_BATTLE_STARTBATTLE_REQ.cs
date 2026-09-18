using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_STARTBATTLE_REQ : GameClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                ChannelModel Channel;
                if (room == null || room.GetLeader() == null || !room.GetChannel(out Channel))
                    return;
                if (room.IsPreparing())
                {
                    bool flag1 = room.IsBotMode();
                    SlotModel slot1 = room.GetSlot(player.SlotId);
                    if (slot1 != null && slot1.State == SlotState.PRESTART)
                    {
                        room.ChangeSlotState(slot1, SlotState.BATTLE_READY, true);
                        slot1.StopTiming();
                        // Emotes: re-push mv=0+1 ao entrar BATTLE_READY (treino/desafio).
                        PROTOCOL_SERVER_MESSAGE_CHANGE_INVENTORY_ACK.SendBattleEmoteLoadout(player);
                        if (flag1)
                        {
                            room.SetBotLevel();
                            this.Client.SendPacket(new PROTOCOL_BATTLE_CHANGE_DIFFICULTY_LEVEL_ACK(room));
                        }
                        this.Client.SendPacket(new PROTOCOL_ROOM_CHANGE_ROOMINFO_ACK(room, flag1));
                        int num1 = 0;
                        int num2 = 0;
                        int num3 = 0;
                        int num4 = 0;
                        int num5 = 0;
                        int num6 = 0;
                        foreach (SlotModel slot2 in room.Slots)
                        {
                            if (slot2.State >= SlotState.LOAD)
                            {
                                ++num4;
                                if (slot2.Team != TeamEnum.FR_TEAM)
                                    ++num6;
                                else
                                    ++num5;
                                if (slot2.State >= SlotState.BATTLE_READY)
                                {
                                    ++num1;
                                    if (slot2.Team != TeamEnum.FR_TEAM)
                                        ++num2;
                                    else
                                        ++num3;
                                }
                            }
                        }
                        int num7 = room.State == RoomState.BATTLE ? 1 : 0;
                        SlotModel leaderSlot = room.GetSlot(room.Leader);
                        bool leaderIsOnFrTeam = leaderSlot != null && leaderSlot.Team == TeamEnum.FR_TEAM;
                        bool flag2 = leaderSlot != null && leaderSlot.State >= SlotState.BATTLE_READY & flag1 && (num4 <= 1 || leaderIsOnFrTeam && num3 > num5 / 2 || !leaderIsOnFrTeam && num2 > num6 / 2);
                        bool flag3 = room.GetSlot(room.Leader).State >= SlotState.BATTLE_READY && num2 > num6 / 2 && num3 > num5 / 2;
                        bool flag4 = room.GetSlot(room.Leader).State >= SlotState.BATTLE_READY && room.RoomType == RoomCondition.FreeForAll && num1 >= 2 && num4 >= 2;
                        bool flag5 = Channel.Type == ChannelType.Clan && num1 == num3 + num2;
                        bool flag6 = room.Competitive && num1 == num3 + num2;
                        int num8 = flag2 ? 1 : 0;
                        if ((num7 | num8 | (flag3 ? 1 : 0) | (flag4 ? 1 : 0) | (flag5 ? 1 : 0) | (flag6 ? 1 : 0)) == 0)
                            return;
                        if (flag5)
                            CLogger.Print($"Starting Clan War Match with '{num1}' players. FR: {num3} CT: {num2}", LoggerType.Warning);
                        if (flag6)
                            CLogger.Print($"Starting Competitive Match with '{num1}' players. FR: {num3} CT: {num2}", LoggerType.Warning);
                        room.SpawnReadyPlayers();
                    }
                    else
                    {
                        this.Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_KICK_BATTLE_PLAYER_ACK(EventErrorEnum.BATTLE_FIRST_HOLE));
                        this.Client.SendPacket(new PROTOCOL_BATTLE_GIVEUPBATTLE_ACK(player, 0));
                        room.ChangeSlotState(slot1, SlotState.NORMAL, true);
                        AllUtils.BattleEndPlayersCount(room, flag1);
                    }
                }
                else
                {
                    this.Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_KICK_BATTLE_PLAYER_ACK(EventErrorEnum.BATTLE_FIRST_HOLE));
                    this.Client.SendPacket(new PROTOCOL_BATTLE_STARTBATTLE_ACK());
                    room.ChangeSlotState(player.SlotId, SlotState.NORMAL, true);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_STARTBATTLE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}