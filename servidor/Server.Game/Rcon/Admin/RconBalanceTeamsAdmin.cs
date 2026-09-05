using Server.Game.Data.Models;
using Server.Game.Data.Utils;

namespace Server.Game.Rcon.Admin
{
    public class RconBalanceTeamsAdmin : RconReceive
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

            AllUtils.TryBalanceTeams(room);
            RconLogger.LogsPanel($"[!] Teams rebalanced in room {RoomId}", 0);
        }
    }
}
