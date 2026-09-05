using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Client;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_MISSION_GENERATOR_INFO_REQ : GameClientPacket
    {
        private ushort Field0;
        private ushort Field1;
        private List<ushort> Field2 = new List<ushort>();

        public override void Read()
        {
            this.Field0 = this.ReadUH();
            this.Field1 = this.ReadUH();
            for (int index = 0; index < 18; ++index)
                this.Field2.Add(this.ReadUH());
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null || room.RoundTime.IsTimer() || room.State != RoomState.BATTLE)
                    return;
                SlotModel slot1 = room.GetSlot(player.SlotId);
                if (slot1 == null || slot1.State != SlotState.BATTLE)
                    return;
                room.Bar1 = (int)this.Field0;
                room.Bar2 = (int)this.Field1;
                for (int index = 0; index < 18; ++index)
                {
                    SlotModel slot2 = room.Slots[index];
                    if (slot2.PlayerId > 0L && slot2.State == SlotState.BATTLE)
                    {
                        slot2.DamageBar1 = this.Field2[index];
                        slot2.EarnedEXP = (int)this.Field2[index] / 600;
                    }
                }
                using (PROTOCOL_BATTLE_MISSION_GENERATOR_INFO_ACK Packet = new PROTOCOL_BATTLE_MISSION_GENERATOR_INFO_ACK(room))
                    room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
                if (this.Field0 == (ushort)0)
                {
                    RoomSabotageSync.EndRound(room, TeamEnum.CT_TEAM);
                }
                else
                {
                    if (this.Field1 != (ushort)0)
                        return;
                    RoomSabotageSync.EndRound(room, TeamEnum.FR_TEAM);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}