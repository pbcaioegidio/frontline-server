using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_PERSONAL_TEAM_CHANGE_REQ : GameClientPacket
    {
        private TeamEnum Team;
        private int Slot;

        public override void Read()
        {
            Team = (TeamEnum)ReadD();
            Slot = (int)ReadC();
        }

        public override void Run()
        {
            try
            {
                Account player = Client.GetAccount();
                if (player == null)
                {
                    CLogger.Print("PROTOCOL_ROOM_PERSONAL_TEAM_CHANGE_REQ: Player is null", LoggerType.Warning);
                    return;
                }
                RoomModel room = player.Room;
                if (room == null || Team == TeamEnum.ALL_TEAM || room.ChangingSlots)
                {
                    CLogger.Print("PROTOCOL_ROOM_PERSONAL_TEAM_CHANGE_REQ: Room is null or invalid team or changing slots", LoggerType.Warning);
                    return;
                }
                SlotModel slot = room.GetSlot(player.SlotId);
                if (slot == null || slot.State != SlotState.NORMAL)
                {
                    CLogger.Print("PROTOCOL_ROOM_PERSONAL_TEAM_CHANGE_REQ: Slot is null or not in NORMAL state", LoggerType.Warning);
                    return;
                }
                lock (room.Slots)
                {
                    room.ChangingSlots = true;
                    List<SlotModel> slotModelList = new List<SlotModel>();

                    room.SwitchNewSlot(slotModelList, player, slot, Team, Slot);

                    if (slotModelList.Count > 0)
                    {
                        using (PROTOCOL_ROOM_TEAM_BALANCE_ACK Packet = new PROTOCOL_ROOM_TEAM_BALANCE_ACK(slotModelList, room.Leader, 0))
                        {
                            room.SendPacketToPlayers(Packet);
                        }
                    }
                    room.ChangingSlots = false;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_ROOM_PERSONAL_TEAM_CHANGE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}