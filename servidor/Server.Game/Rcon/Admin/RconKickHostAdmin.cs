using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;

namespace Server.Game.Rcon.Admin
{
    /// <summary>
    /// Remove the host from a room exactly as a give-up would: the match migrates to a new
    /// host when it can survive, and ends when it cannot.
    /// </summary>
    public class RconKickHostAdmin : RconReceive
    {
        private string Token;
        private int RoomId;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                RoomId = PopInt("room_id");
            }

            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("An error occurred in the process, please try again later. ", 1);
                return;
            }

            RoomModel room = RconRooms.Find(RoomId);
            if (room == null)
            {
                RconLogger.LogsPanel($"Room {RoomId} does not exist", 1);
                return;
            }

            Account host = room.GetLeader();
            if (host == null)
            {
                RconLogger.LogsPanel($"Room {RoomId} has no host", 1);
                return;
            }

            if (room.IsBotMode())
            {
                if (room.GetAllPlayers(SlotState.READY, 1, room.Leader).Count == 0)
                    AllUtils.LeaveHostEndBattlePVE(room, host);
                else
                    AllUtils.LeaveHostGiveBattlePVE(room, host);
            }
            else
            {
                RconRooms.CountBattleTeams(room, out int teamFR, out int teamCT);
                if (room.State != RoomState.BATTLE || (teamFR != 0 && teamCT != 0))
                    AllUtils.LeaveHostGiveBattlePVP(room, host);
                else
                    AllUtils.LeaveHostEndBattlePVP(room, host, teamFR, teamCT, out bool _);
            }

            RconLogger.LogsPanel($"[!] Host {host.Nickname} removed from room {RoomId}", 0);
        }
    }
}
