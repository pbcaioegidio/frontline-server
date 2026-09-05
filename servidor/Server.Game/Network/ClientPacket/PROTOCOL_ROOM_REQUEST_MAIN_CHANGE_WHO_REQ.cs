using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_WHO_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read() => this.Field0 = this.ReadD();

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room != null && room.Leader != this.Field0 && room.Slots[this.Field0].PlayerId != 0L)
                {
                    if (room.State != RoomState.READY || room.Leader != player.SlotId)
                        return;
                    room.SetNewLeader(this.Field0, SlotState.EMPTY, room.Leader, false);
                    using (PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_WHO_ACK Packet = new PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_WHO_ACK(this.Field0))
                        room.SendPacketToPlayers(Packet);
                    room.UpdateSlotsInfo();
                }
                else
                    this.Client.SendPacket(new PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_WHO_ACK(2147483648U /*0x80000000*/));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_WHO_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}