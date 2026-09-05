// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BATTLE_GIVEUPBATTLE_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_GIVEUPBATTLE_REQ : GameClientPacket
    {
        private bool Field0;
        private long Field1;

        public override void Read() => this.Field1 = (long)this.ReadD();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                SlotModel Slot;
                if (room == null || room.State < RoomState.LOADING || !room.GetSlot(player.SlotId, out Slot) || Slot.State < SlotState.LOAD)
                    return;
                bool IsBotMode = room.IsBotMode();
                bool wasInBattle = room.State == RoomState.BATTLE;
                int quitSlotId = Slot.Id;
                AllUtils.FreepassEffect(player, Slot, room, IsBotMode);
                if (room.VoteTime.IsTimer() && room.VoteKick != null && room.VoteKick.VictimIdx == Slot.Id)
                {
                    room.VoteTime.StopJob();
                    room.VoteKick = (VoteKickModel)null;
                    using (PROTOCOL_BATTLE_NOTIFY_KICKVOTE_CANCEL_ACK Packet = new PROTOCOL_BATTLE_NOTIFY_KICKVOTE_CANCEL_ACK())
                        room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0, Slot.Id);
                }
                int TeamFR = 0;
                int TeamCT = 0;
                int num1 = 0;
                int num2 = 0;
                foreach (SlotModel slot in room.Slots)
                {
                    if (slot.State >= SlotState.LOAD)
                    {
                        if (slot.Team != TeamEnum.FR_TEAM)
                            ++num2;
                        else
                            ++num1;
                        if (slot.State == SlotState.BATTLE)
                        {
                            if (slot.Team != TeamEnum.FR_TEAM)
                                ++TeamCT;
                            else
                                ++TeamFR;
                        }
                    }
                }
                if (Slot.Id != room.Leader)
                {
                    if (IsBotMode)
                        AllUtils.LeavePlayerQuitBattle(room, player);
                    else if ((room.State != RoomState.BATTLE || TeamFR != 0 && TeamCT != 0) && (room.State > RoomState.PRE_BATTLE || num1 != 0 && num2 != 0))
                        AllUtils.LeavePlayerQuitBattle(room, player);
                    else
                        AllUtils.LeavePlayerEndBattlePVP(room, player, TeamFR, TeamCT, out this.Field0);
                }
                else if (!IsBotMode)
                {
                    if ((room.State != RoomState.BATTLE || TeamFR != 0 && TeamCT != 0) && (room.State > RoomState.PRE_BATTLE || num1 != 0 && num2 != 0))
                        AllUtils.LeaveHostGiveBattlePVP(room, player);
                    else
                        AllUtils.LeaveHostEndBattlePVP(room, player, TeamFR, TeamCT, out this.Field0);
                }
                else if (room.GetAllPlayers(SlotState.READY, 1, quitSlotId).Count == 0)
                    AllUtils.LeaveHostEndBattlePVE(room, player);
                else
                    AllUtils.LeaveHostGiveBattlePVE(room, player);
                if (wasInBattle)
                    BattleLeaveSync.SendUDPPlayerLeave(room, quitSlotId);
                AllUtils.ResetSlotInfo(room, Slot, true);
                this.Client.SendPacket(new PROTOCOL_BATTLE_GIVEUPBATTLE_ACK(player, 0));
                if (this.Field0 || room.State != RoomState.BATTLE)
                    return;
                AllUtils.BattleEndRoundPlayersCount(room);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_GIVEUPBATTLE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}