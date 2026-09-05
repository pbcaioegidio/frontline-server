using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_MISSION_TUTORIAL_ROUND_END_REQ : GameClientPacket
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
                if (room == null)
                    return;
                room.State = RoomState.BATTLE_END;
                this.Client.SendPacket(new PROTOCOL_BATTLE_MISSION_TUTORIAL_ROUND_END_ACK(room));
                this.Client.SendPacket(new PROTOCOL_BATTLE_MISSION_ROUND_END_ACK(room, 0, RoundEndType.Tutorial));
                this.Client.SendPacket(new PROTOCOL_ROOM_CHANGE_ROOMINFO_ACK(room));
                if (room.State == RoomState.BATTLE_END)
                {
                    room.State = RoomState.READY;
                    this.Client.SendPacket(new PROTOCOL_BATTLE_ENDBATTLE_ACK(player));
                    this.Client.SendPacket(new PROTOCOL_ROOM_CHANGE_ROOMINFO_ACK(room));
                }
                AllUtils.ResetBattleInfo(room);
                this.Client.SendPacket(new PROTOCOL_ROOM_GET_SLOTINFO_ACK(room));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_MISSION_TUTORIAL_ROUND_END_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}