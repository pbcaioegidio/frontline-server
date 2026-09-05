using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_SELECT_SLOT_CHANGE_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read() => this.Field0 = (int)this.ReadC();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null || room.ChangingSlots)
                    return;
                SlotModel slot = room.GetSlot(player.SlotId);
                if (slot == null || slot.State != SlotState.NORMAL)
                    return;
                lock (room.Slots)
                {
                    room.ChangingSlots = true;
                    List<SlotModel> slotModelList = new List<SlotModel>();
                    room.SwitchNewSlot(slotModelList, player, slot, room.CheckTeam(this.Field0), this.Field0);
                    if (slotModelList.Count > 0)
                    {
                        using (PROTOCOL_ROOM_TEAM_BALANCE_ACK Packet = new PROTOCOL_ROOM_TEAM_BALANCE_ACK(slotModelList, room.Leader, 0))
                            room.SendPacketToPlayers(Packet);
                    }
                    room.ChangingSlots = false;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_ROOM_SELECT_SLOT_CHANGE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}