using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Data.XML;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Server.Game.Rcon
{
    /// <summary>
    /// Read-only view of live rooms for the admin panel. Rooms exist only in memory, so
    /// this is the only way out; it takes no token and does not rotate one, which keeps
    /// the panel free to poll it.
    /// </summary>
    public static class RconSnapshot
    {
        public static string Build()
        {
            List<object> rooms = new List<object>();
            int lobbyPlayers = 0;

            foreach (ChannelModel channel in ChannelsXML.Channels)
            {
                if (channel == null)
                    continue;

                lock (channel.LobbyPlayers)
                    lobbyPlayers += channel.LobbyPlayers.Count;

                lock (channel.Rooms)
                {
                    foreach (RoomModel room in channel.Rooms)
                    {
                        if (room != null)
                            rooms.Add(Describe(channel, room));
                    }
                }
            }

            return JsonSerializer.Serialize(new
            {
                rooms,
                lobbyPlayers,
                roomCount = rooms.Count,
                sampledAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            });
        }

        private static object Describe(ChannelModel channel, RoomModel room)
        {
            return new
            {
                roomId = room.RoomId,
                channelId = channel.Id,
                name = room.Name ?? string.Empty,
                map = room.MapId.ToString(),
                mapName = room.MapName ?? string.Empty,
                rule = room.Rule.ToString(),
                state = room.State.ToString(),
                competitive = room.Competitive,
                locked = !string.IsNullOrEmpty(room.Password),
                botMode = room.IsBotMode(),
                leader = room.LeaderName ?? string.Empty,
                players = room.GetCountPlayers(),
                elapsed = room.IsStartingMatch() ? room.GetInBattleTime() : 0,
                maxSlots = room.CountMaxSlots,
                scoreFR = room.FRRounds,
                scoreCT = room.CTRounds,
                slots = DescribeSlots(room),
            };
        }

        private static List<object> DescribeSlots(RoomModel room)
        {
            List<object> slots = new List<object>();
            lock (room.Slots)
            {
                foreach (SlotModel slot in room.Slots)
                {
                    if (slot == null || slot.PlayerId <= 0L)
                        continue;

                    Account player = room.GetPlayerBySlot(slot);
                    slots.Add(new
                    {
                        slotId = slot.Id,
                        playerId = slot.PlayerId,
                        nickname = player != null ? player.Nickname : string.Empty,
                        team = slot.Team == TeamEnum.CT_TEAM ? "CT" : "FR",
                        state = slot.State.ToString(),
                        spectator = slot.Spectator,
                        kills = slot.AllKills,
                        deaths = slot.AllDeaths,
                        assists = slot.AllAssists,
                        score = slot.Score,
                        ping = slot.Ping,
                        isLeader = slot.Id == room.Leader,
                    });
                }
            }
            return slots;
        }
    }
}
