using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;

namespace Server.Game.Rcon.Admin
{
    /// <summary>
    /// Force a live match to finish. EndBattle emits the same ENDBATTLE_ACK a natural
    /// finish does, so clients show a normal result screen.
    /// </summary>
    public class RconEndBattleAdmin : RconReceive
    {
        private string Token, Mode, Winner;
        private int RoomId;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                RoomId = PopInt("room_id");
                Mode = PopStringOr("mode", "end");
                Winner = PopStringOr("winner", string.Empty);
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
            if (!room.IsStartingMatch())
            {
                RconLogger.LogsPanel($"Room {RoomId} is not in a match", 1);
                return;
            }

            switch ((Mode ?? "end").ToLowerInvariant())
            {
                case "draw":
                    AllUtils.EndBattleNoPoints(room);
                    RconLogger.LogsPanel($"[!] Room {RoomId} ended as a draw, no points awarded", 0);
                    return;
                case "round":
                    AllUtils.BattleEndRound(room, RconRooms.ParseTeam(Winner, TeamEnum.TEAM_DRAW), RoundEndType.Normal);
                    RconLogger.LogsPanel($"[!] Round force-ended in room {RoomId}", 0);
                    return;
                default:
                    if (string.IsNullOrEmpty(Winner))
                        AllUtils.EndBattle(room);
                    else
                        AllUtils.EndBattle(room, room.IsBotMode(), RconRooms.ParseTeam(Winner, TeamEnum.TEAM_DRAW));
                    RconLogger.LogsPanel($"[!] Match force-ended in room {RoomId}", 0);
                    return;
            }
        }
    }
}
