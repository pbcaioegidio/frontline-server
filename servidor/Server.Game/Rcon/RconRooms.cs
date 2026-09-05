using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Data.XML;

namespace Server.Game.Rcon
{
    /// <summary>
    /// Room lookup and battle bookkeeping shared by the panel's room-control commands.
    /// Rooms live in each channel's list, so an id has to be resolved by scanning them.
    /// </summary>
    public static class RconRooms
    {
        public static RoomModel Find(int roomId)
        {
            foreach (ChannelModel channel in ChannelsXML.Channels)
            {
                if (channel == null)
                    continue;
                RoomModel room = channel.GetRoom(roomId);
                if (room != null)
                    return room;
            }
            return null;
        }

        /// <summary>
        /// Players still fighting on each side — the same counts the give-up handler uses
        /// to decide whether a match can survive the host leaving.
        /// </summary>
        public static void CountBattleTeams(RoomModel room, out int teamFR, out int teamCT)
        {
            teamFR = 0;
            teamCT = 0;
            lock (room.Slots)
            {
                foreach (SlotModel slot in room.Slots)
                {
                    if (slot == null || slot.PlayerId <= 0L || slot.State != SlotState.BATTLE)
                        continue;
                    if (slot.Team != TeamEnum.FR_TEAM)
                        ++teamCT;
                    else
                        ++teamFR;
                }
            }
        }

        public static TeamEnum ParseTeam(string value, TeamEnum fallback)
        {
            if (string.IsNullOrEmpty(value))
                return fallback;
            switch (value.Trim().ToUpperInvariant())
            {
                case "FR":
                case "0":
                    return TeamEnum.FR_TEAM;
                case "CT":
                case "1":
                    return TeamEnum.CT_TEAM;
                case "DRAW":
                case "2":
                    return TeamEnum.TEAM_DRAW;
                default:
                    return fallback;
            }
        }
    }
}
