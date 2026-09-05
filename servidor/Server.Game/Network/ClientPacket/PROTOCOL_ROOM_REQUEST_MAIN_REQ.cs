using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_REQUEST_MAIN_REQ : GameClientPacket
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
                if (room != null)
                {
                    bool isGM = player.IsGM();
                    if ((room.State != RoomState.READY && !isGM) || room.Leader == player.SlotId)
                        return;
                    List<Account> allPlayers = room.GetAllPlayers();
                    if (allPlayers.Count == 0)
                        return;
                    if (player.IsGM())
                    {
                        this.Method0(room, allPlayers, player.SlotId);
                    }
                    else
                    {
                        if (!room.RequestRoomMaster.Contains(player.PlayerId))
                        {
                            room.RequestRoomMaster.Add(player.PlayerId);
                            if (room.RequestRoomMaster.Count() < allPlayers.Count / 2 + 1)
                            {
                                using (PROTOCOL_ROOM_REQUEST_MAIN_ACK A_1 = new PROTOCOL_ROOM_REQUEST_MAIN_ACK(player.SlotId))
                                    this.Method1(A_1, allPlayers);
                            }
                        }
                        if (room.RequestRoomMaster.Count() < allPlayers.Count / 2 + 1)
                            return;
                        this.Method0(room, allPlayers, player.SlotId);
                    }
                }
                else
                    Client.SendPacket(new PROTOCOL_ROOM_REQUEST_MAIN_ACK(2147483648U /*0x80000000*/));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_ROOM_REQUEST_MAIN_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        private void Method0(RoomModel A_1, List<Account> A_2, int A_3)
        {
            A_1.SetNewLeader(A_3, SlotState.EMPTY, -1, false);
            using (PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_WHO_ACK A_1_1 = new PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_WHO_ACK(A_3))
                this.Method1(A_1_1, A_2);
            A_1.UpdateSlotsInfo();
            A_1.RequestRoomMaster.Clear();
        }

        private void Method1(GameServerPacket A_1, List<Account> A_2)
        {
            byte[] completeBytes = A_1.GetCompleteBytes(nameof(PROTOCOL_ROOM_REQUEST_MAIN_REQ));
            foreach (Account account in A_2)
                account.SendCompletePacket(completeBytes, A_1.GetType().Name);
        }
    }
}