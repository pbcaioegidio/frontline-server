using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Security;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_READYBATTLE_REQ : GameClientPacket
    {
        private byte Field0;
        private int Field1;

        public override void Read()
        {
            this.Field0 = this.ReadC();
            this.Field1 = this.ReadD();
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || ComDiv.GetDuration(player.LastReadyBattle) < 0.7)
                    return;

                RoomModel room = player.Room;
                ChannelModel Channel;
                SlotModel Slot;

                if (room == null || room.GetLeader() == null || !room.GetChannel(out Channel) || !room.GetSlot(player.SlotId, out Slot))
                    return;

                // Probation: ranked / clan room
                if (room.Competitive && AllUtils.BlockIfProbation(player, "ranked"))
                {
                    this.Client.SendPacket(new PROTOCOL_BATTLE_READYBATTLE_ACK(2147487753U));
                    return;
                }
                if (Channel != null && Channel.Type == ChannelType.Clan && AllUtils.BlockIfProbation(player, "clan"))
                {
                    this.Client.SendPacket(new PROTOCOL_BATTLE_READYBATTLE_ACK(2147487753U));
                    return;
                }
                if (AllUtils.BlockIfProbation(player, "match"))
                {
                    this.Client.SendPacket(new PROTOCOL_BATTLE_READYBATTLE_ACK(2147487753U));
                    return;
                }
                SecurityDao.LogEvent(SecurityDao.SourceGame, player.PlayerId, player.Username, player.Nickname, "info", "MATCH_START",
                    "Ready battle", "{\"room\":" + room.RoomId + ",\"map\":" + (int)room.MapId + "}", 1, "");

                if (Slot.Equipment != null)
                {
                    MapMatch mapLimit = SystemMapXML.GetMapLimit((int)room.MapId, (int)room.Rule);
                    if (mapLimit == null)
                        return;

                    bool flag = room.IsBotMode();
                    Slot.SpecGM = this.Field0 == (byte)2 && player.IsGM() || room.RoomType == RoomCondition.Ace && (Slot.Id < 0 || Slot.Id > 1);

                    // Logging untuk debugging
                    CLogger.Print($"[DEBUG] Player: {player.Nickname}, RoomType: {room.RoomType}, IsLeader: {room.Leader == player.SlotId}", LoggerType.Debug);

                    if (!flag && ConfigLoader.TournamentRule && AllUtils.ClassicModeCheck(player, room, player.Effects))
                        return;

                    int TotalEnemys = 0;
                    int num1 = 0;
                    int num2 = 0;
                    AllUtils.GetReadyPlayers(room, ref num1, ref num2, ref TotalEnemys);

                    // Logging tambahan untuk debugging
                    CLogger.Print($"[DEBUG] Ready Players - FR: {num1}, CT: {num2}, TotalEnemies: {TotalEnemys}", LoggerType.Debug);
                    CLogger.Print($"[DEBUG] Room State: {room.State}, BotMode: {flag}", LoggerType.Debug);

                    if (room.Leader != player.SlotId)
                    {
                        // Logika untuk non-leader players
                        if (room.Slots[room.Leader].State >= SlotState.LOAD && room.IsPreparing())
                        {
                            if (Slot.State == SlotState.NORMAL)
                            {
                                if (mapLimit.Limit == 8 && AllUtils.Check4vs4(room, false, num1, num1, TotalEnemys))
                                {
                                    this.Client.SendPacket(new PROTOCOL_ROOM_UNREADY_4VS4_ACK());
                                    return;
                                }

                                if (room.BalanceType != TeamBalance.None && !flag)
                                    AllUtils.TryBalancePlayer(room, player, true, Slot);

                                room.ChangeSlotState(Slot, SlotState.LOAD, true);
                                Slot.SetMissionsClone(player.Mission);
                                this.Client.SendPacket(new PROTOCOL_BATTLE_READYBATTLE_ACK((uint)Slot.State));
                                this.Client.SendPacket(new PROTOCOL_BATTLE_START_GAME_ACK(room));

                                using (PROTOCOL_BATTLE_START_GAME_TRANS_ACK Packet = new PROTOCOL_BATTLE_START_GAME_TRANS_ACK(room, Slot, player.Title))
                                    room.SendPacketToPlayers(Packet, SlotState.READY, 1, Slot.Id);
                            }
                        }
                        else if (Slot.State == SlotState.NORMAL)
                            room.ChangeSlotState(Slot, SlotState.READY, true);
                        else if (Slot.State == SlotState.READY)
                        {
                            room.ChangeSlotState(Slot, SlotState.NORMAL, false);
                            if (room.State == RoomState.COUNTDOWN &&
                                room.GetPlayingPlayers(room.Leader % 2 == 0 ? TeamEnum.CT_TEAM : TeamEnum.FR_TEAM, SlotState.READY, 0) == 0)
                            {
                                room.ChangeSlotState(room.Leader, SlotState.NORMAL, false);
                                room.StopCountDown(CountDownEnum.StopByPlayer);
                            }
                            room.UpdateSlotsInfo();
                        }
                    }
                    else
                    {
                        // Logika untuk leader (host)
                        if (room.State != RoomState.READY && room.State != RoomState.COUNTDOWN)
                            return;

                        if (mapLimit.Limit == 8 && AllUtils.Check4vs4(room, true, num1, num2, TotalEnemys))
                        {
                            this.Client.SendPacket(new PROTOCOL_ROOM_UNREADY_4VS4_ACK());
                            return;
                        }

                        uint Error1;
                        if (!AllUtils.ClanMatchCheck(room, Channel.Type, TotalEnemys, out Error1))
                        {
                            uint Error2;
                            if (AllUtils.CompetitiveMatchCheck(player, room, out Error2))
                            {
                                this.Client.SendPacket(new PROTOCOL_BATTLE_READYBATTLE_ACK(Error2));
                                return;
                            }

                            // Perhitungan total pemain siap
                            int totalReadyPlayers = room.GetAllPlayers(SlotState.READY, 1).Count;
                            CLogger.Print($"[DEBUG] Total ready players: {totalReadyPlayers}", LoggerType.Debug);

                            // Perbaikan logika untuk 2 pemain
                            if (TotalEnemys == 0 && (flag || room.RoomType == RoomCondition.Tutorial))
                            {
                                // Mode bot atau tutorial
                                room.ChangeSlotState(Slot, SlotState.READY, false);
                                room.StartBattle(false);
                                room.UpdateSlotsInfo();
                            }
                            else if (!flag && (TotalEnemys > 0 || totalReadyPlayers == 2))
                            {
                                // Mode normal dengan pemain lain atau 2 pemain
                                room.ChangeSlotState(Slot, SlotState.READY, false);

                                if (room.BalanceType != TeamBalance.None)
                                    AllUtils.TryBalanceTeams(room);

                                // Perbaikan untuk 2 pemain
                                if (totalReadyPlayers == 2 && room.RoomType != RoomCondition.Ace)
                                {
                                    CLogger.Print("[DEBUG] Starting battle for 2 players (non-Ace mode)", LoggerType.Debug);
                                    room.StartBattle(false);
                                }
                                else if (!room.ThisModeHaveCD())
                                {
                                    room.StartBattle(false);
                                }
                                else if (room.State == RoomState.READY)
                                {
                                    SlotModel[] slotModelArray = new SlotModel[2]
                                    {
                                        room.GetSlot(0),
                                        room.GetSlot(1)
                                    };

                                    if (room.RoomType == RoomCondition.Ace &&
                                        (slotModelArray[0].State != SlotState.READY || slotModelArray[1].State != SlotState.READY))
                                    {
                                        this.Client.SendPacket(new PROTOCOL_BATTLE_READYBATTLE_ACK(2147487753U /*0x80001009*/));
                                        room.ChangeSlotState(room.Leader, SlotState.NORMAL, false);
                                        room.StopCountDown(CountDownEnum.StopByHost);
                                    }
                                    else
                                    {
                                        CLogger.Print("[DEBUG] Starting countdown for battle", LoggerType.Debug);
                                        room.State = RoomState.COUNTDOWN;
                                        room.UpdateRoomInfo();
                                        room.StartCountDown();
                                    }
                                }
                                else if (room.State == RoomState.COUNTDOWN)
                                {
                                    room.ChangeSlotState(room.Leader, SlotState.NORMAL, false);
                                    room.StopCountDown(CountDownEnum.StopByHost);
                                }
                                room.UpdateSlotsInfo();
                            }
                            else if (TotalEnemys == 0 && !flag)
                            {
                                CLogger.Print("[DEBUG] Cannot start battle: no enemies and not bot mode", LoggerType.Debug);
                                this.Client.SendPacket(new PROTOCOL_BATTLE_READYBATTLE_ACK(2147487753U /*0x80001009*/));
                            }
                        }
                        else
                        {
                            CLogger.Print($"[DEBUG] Clan match check failed with error: {Error1}", LoggerType.Debug);
                            this.Client.SendPacket(new PROTOCOL_BATTLE_READYBATTLE_ACK(Error1));
                            return;
                        }
                    }
                    player.LastReadyBattle = DateTimeUtil.Now();
                }
                else
                    this.Client.SendPacket(new PROTOCOL_BATTLE_READYBATTLE_ACK(2147487915U));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_READYBATTLE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}