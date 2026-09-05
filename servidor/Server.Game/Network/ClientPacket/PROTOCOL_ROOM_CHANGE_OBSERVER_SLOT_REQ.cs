using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_CHANGE_OBSERVER_SLOT_REQ : GameClientPacket
    {
        private ViewerType ViewerType;

        public override void Read() => ViewerType = (ViewerType)ReadD();

        public override void Run()
        {
            try
            {
                Account player = Client.GetAccount();
                if (player == null)
                    return;
                if (ViewerType == ViewerType.SpecGM && !player.IsGM())
                {
                    CLogger.Print($"[FL GUARD] Observer slot negado para não-GM {player.Nickname}", LoggerType.Warning);
                    Plugin.Core.Security.SecurityDao.LogEvent(Plugin.Core.Security.SecurityDao.SourceGame,
                        player.PlayerId, player.Username, player.Nickname, "flag", "OBSERVER",
                        "Tentativa de slot observer sem ser GM", "{}", 2, "");
                    Client.SendPacket(new PROTOCOL_ROOM_CHANGE_OBSERVER_SLOT_ACK(player.SlotId));
                    return;
                }
                RoomModel room = player.GetRoom();
                if (room == null || room.ChangingSlots)
                    return;
                SlotModel slot = room.GetSlot(player.SlotId);
                if (slot == null || slot.State != SlotState.NORMAL)
                    return;
                lock (room.Slots)
                {
                    room.ChangingSlots = true;
                    try
                    {
                        List<SlotModel> slotChanges = new List<SlotModel>();
                        if (ViewerType == ViewerType.SpecGM && slot.Id != 16 && slot.Id != 17)
                        {
                            foreach (int slotIdx in new int[2] { 16, 17 })
                            {
                                room.SwitchNewSlot(slotChanges, player, slot, room.CheckTeam(slotIdx), slotIdx);
                                if (slotChanges.Count > 0)
                                    break;
                            }
                        }
                        else if (ViewerType == ViewerType.Normal && (slot.Id == 16 || slot.Id == 17))
                        {
                            for (int slotIdx = 0; slotIdx < 16; ++slotIdx)
                            {
                                room.SwitchNewSlot(slotChanges, player, slot, room.CheckTeam(slotIdx), slotIdx);
                                if (slotChanges.Count > 0)
                                    break;
                            }
                        }
                        if (slotChanges.Count > 0)
                        {
                            using (PROTOCOL_ROOM_TEAM_BALANCE_ACK Packet = new PROTOCOL_ROOM_TEAM_BALANCE_ACK(slotChanges, room.Leader, 0))
                                room.SendPacketToPlayers(Packet);
                        }
                    }
                    finally
                    {
                        room.ChangingSlots = false;
                    }
                }
                Client.SendPacket(new PROTOCOL_ROOM_CHANGE_OBSERVER_SLOT_ACK(player.SlotId));
            }
            catch (Exception ex)
            {
                CLogger.Print($"{(this).GetType().Name}; {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}
