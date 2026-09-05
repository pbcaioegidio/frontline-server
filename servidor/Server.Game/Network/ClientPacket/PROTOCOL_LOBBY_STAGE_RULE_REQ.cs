using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    // LOBBY_STAGE_RULE_REQ (opcode 2565). Fired by the client during the room
    // create/enter phase (GameEvent send-1029 -> sub_D11F9E builder @0xD11F9E, 122 IDB).
    // Body carries no fields the server needs; server replies with the player's own
    // slot index + current room stage-rule via LOBBY_STAGE_RULE_ACK (2566).
    public class PROTOCOL_LOBBY_STAGE_RULE_REQ : GameClientPacket
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
                this.Client.SendPacket(new PROTOCOL_LOBBY_STAGE_RULE_ACK(room, room != null ? player.SlotId : -1));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_LOBBY_STAGE_RULE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
