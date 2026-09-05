using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_PRESTARTBATTLE_REQ : GameClientPacket
    {
        private StageOptions Field0;
        private MapRules Field1;
        private MapIdEnum Field2;
        private RoomCondition Field3;

        public override void Read()
        {
            this.Field2 = (MapIdEnum)this.ReadC();
            this.Field1 = (MapRules)this.ReadC();
            this.Field0 = (StageOptions)this.ReadC();
            this.Field3 = (RoomCondition)this.ReadC();
        }


        public override void Run()
        {
            try
            {
                // Tambahkan logging untuk debugging
                CLogger.Print("PROTOCOL_BATTLE_PRESTARTBATTLE_REQ: Start processing", LoggerType.Debug);

                Account player = this.Client.GetAccount();
                if (player == null)
                {
                    CLogger.Print("PROTOCOL_BATTLE_PRESTARTBATTLE_REQ: Player is null", LoggerType.Debug);
                    return;
                }

                RoomModel room = player.Room;
                if (room == null)
                {
                    CLogger.Print("PROTOCOL_BATTLE_PRESTARTBATTLE_REQ: Room is null", LoggerType.Debug);
                    return;
                }

                if (room.Stage == this.Field0 && room.RoomType == this.Field3 && room.MapId == this.Field2 && room.Rule == this.Field1)
                {
                    SlotModel slot = room.GetSlot(player.SlotId);
                    if (slot != null && room.IsPreparing() && room.UdpServer != null && slot.State >= SlotState.LOAD)
                    {
                        Account leader = room.GetLeader();
                        if (leader == null)
                        {
                            this.Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_KICK_BATTLE_PLAYER_ACK(EventErrorEnum.BATTLE_FIRST_HOLE));
                            this.Client.SendPacket(new PROTOCOL_BATTLE_GIVEUPBATTLE_ACK(player, 0));
                            room.ChangeSlotState(slot, SlotState.NORMAL, true);
                            AllUtils.BattleEndPlayersCount(room, room.IsBotMode());
                            slot.StopTiming();
                        }
                        else if (string.IsNullOrEmpty(player.PublicIP.ToString()))
                        {
                            this.Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_KICK_BATTLE_PLAYER_ACK(EventErrorEnum.BATTLE_NO_REAL_IP));
                            this.Client.SendPacket(new PROTOCOL_BATTLE_GIVEUPBATTLE_ACK(player, 0));
                            room.ChangeSlotState(slot, SlotState.NORMAL, true);
                            AllUtils.BattleEndPlayersCount(room, room.IsBotMode());
                            slot.StopTiming();
                        }
                        else
                        {
                            // Update timestamp untuk debugging
                            try
                            {
                                CLogger.UpdateLastMatchSocket();
                                CLogger.Print("PROTOCOL_BATTLE_PRESTARTBATTLE_REQ: Updated LastMatchSocket", LoggerType.Debug);
                            }
                            catch (Exception ex)
                            {
                                CLogger.Print($"PROTOCOL_BATTLE_PRESTARTBATTLE_REQ: Error updating LastMatchSocket: {ex.Message}", LoggerType.Error, ex);
                            }

                            slot.PreStartDate = DateTimeUtil.Now();
                            if (slot.Id == room.Leader)
                            {
                                room.State = RoomState.PRE_BATTLE;
                                room.UpdateRoomInfo();
                            }
                            room.ChangeSlotState(slot, SlotState.PRESTART, true);
                            this.Client.SendPacket(new PROTOCOL_BATTLE_PRESTARTBATTLE_ACK(player, true));
                            if (slot.Id != room.Leader)
                                leader.SendPacket(new PROTOCOL_BATTLE_PRESTARTBATTLE_ACK(player, false));
                            room.StartCounter(1, player, slot);
                        }
                    }
                    else
                    {
                        this.Client.SendPacket(new PROTOCOL_BATTLE_STARTBATTLE_ACK());
                        room.ChangeSlotState(slot, SlotState.NORMAL, true);
                        AllUtils.BattleEndPlayersCount(room, room.IsBotMode());
                        slot.StopTiming();
                    }
                }
                else
                {
                    this.Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_KICK_BATTLE_PLAYER_ACK(EventErrorEnum.BATTLE_FIRST_MAINLOAD));
                    this.Client.SendPacket(new PROTOCOL_BATTLE_PRESTARTBATTLE_ACK());
                    room.ChangeSlotState(player.SlotId, SlotState.NORMAL, true);
                    AllUtils.BattleEndPlayersCount(room, room.IsBotMode());
                }

                CLogger.Print("PROTOCOL_BATTLE_PRESTARTBATTLE_REQ: Processing completed", LoggerType.Debug);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_PRESTARTBATTLE_REQ: " + ex.Message, LoggerType.Error, ex);

                // Tambahkan informasi stack trace yang lebih detail
                CLogger.Print("Stack Trace: " + ex.StackTrace, LoggerType.Error);

                // Jika ada inner exception, log juga
                if (ex.InnerException != null)
                {
                    CLogger.Print("Inner Exception: " + ex.InnerException.Message, LoggerType.Error);
                    CLogger.Print("Inner Stack Trace: " + ex.InnerException.StackTrace, LoggerType.Error);
                }
            }
        }
    }
}