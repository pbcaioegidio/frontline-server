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
    public class PROTOCOL_BATTLE_MISSION_TOUCHDOWN_COUNT_REQ : GameClientPacket
    {
        private int Field0;
        private int Field1;

        public override void Read()
        {
            this.Field1 = (int)this.ReadC();
            this.Field0 = this.ReadD();
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null || room.RoundTime.IsTimer() || room.State != RoomState.BATTLE || room.TRex != this.Field1)
                    return;
                SlotModel slot = room.GetSlot(player.SlotId);
                if (slot == null || slot.State != SlotState.BATTLE)
                    return;
                if (slot.Team == TeamEnum.FR_TEAM)
                    room.FRDino += 5;
                else
                    room.CTDino += 5;
                using (PROTOCOL_BATTLE_MISSION_TOUCHDOWN_COUNT_ACK Packet = new PROTOCOL_BATTLE_MISSION_TOUCHDOWN_COUNT_ACK(room))
                    room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
                AllUtils.CompleteMission(room, slot, MissionType.DEATHBLOW, this.Field0);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_MISSION_TOUCHDOWN_COUNT_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}