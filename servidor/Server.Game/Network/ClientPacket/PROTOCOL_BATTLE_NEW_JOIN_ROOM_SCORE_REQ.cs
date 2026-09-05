using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_NEW_JOIN_ROOM_SCORE_REQ : GameClientPacket
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
                if (room == null || room.State < RoomState.LOADING || room.Slots[player.SlotId].State != SlotState.NORMAL)
                    return;
                this.Client.SendPacket(new PROTOCOL_BATTLE_NEW_JOIN_ROOM_SCORE_ACK(room));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_NEW_JOIN_ROOM_SCORE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}