using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    // Client 122 (OBSERVED, factory 0xD14CF4 uiMsg 0x82BE): PACKET_CLAN_WAR_MATCHMAKING_REQ
    // (6932) is allocated as a bare 0x14-byte S2MOPacketBaseT<0x1B14> with a null
    // field-list head -> zero payload. The reply is MATCHMAKING_ACK (6933), a bare
    // u32 result (handler 0xEF1230).
    //
    // This replaces the 068 propose/accept/create-room handshake, whose four ACK
    // opcodes (1554/1559/1564/1574) the 122 client has no parser for. Instead the
    // team leader enqueues, and when two teams from different clans with the same
    // size are queued the server pairs them and drops everyone into a normal room.
    public class PROTOCOL_CLAN_WAR_MATCHMAKING_REQ : GameClientPacket
    {
        // Clan-war stage rotation. Every entry MUST exist in Data/Maps/Matches.xml
        // under the Deathmatch mode (Rules.xml Mode Id=1, Rule=0), otherwise
        // RoomModel.SetSlotCount finds no MapMatch, leaves every slot closed and
        // nobody can be seated.
        // ponytail: fixed list, move to Settings.ini when rotation needs config.
        private static readonly MapIdEnum[] ClanWarMaps = { MapIdEnum.PortAkaba };

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
                if (Server.Game.Data.Utils.AllUtils.BlockIfProbation(player, "clan"))
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MATCHMAKING_ACK(2147483648U));
                    return;
                }
                ChannelModel channel = player.GetChannel();
                MatchModel match = player.Match;
                if (channel == null || channel.Type != ChannelType.Clan || match == null || player.Room != null)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MATCHMAKING_ACK(2147483648U /*0x80000000*/));
                    return;
                }
                if (player.MatchSlot != match.Leader)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MATCHMAKING_ACK(2147487892U));
                    return;
                }
                if (match.State != MatchState.Ready)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MATCHMAKING_ACK(2147487888U /*0x80001090*/));
                    return;
                }
                // Same fullness rule the 068 accept path used: every seat taken.
                if (match.GetCountPlayers() != match.Training)
                {
                    this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MATCHMAKING_ACK(2147487889U));
                    return;
                }

                lock (channel.MatchQueue)
                {
                    if (match.InQueue)
                    {
                        this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MATCHMAKING_ACK(2147483648U /*0x80000000*/));
                        return;
                    }
                    match.InQueue = true;
                    channel.MatchQueue.Add(match);
                }
                this.Client.SendPacket(new PROTOCOL_CLAN_WAR_MATCHMAKING_ACK(0U));
                TryPairMatches(channel);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CLAN_WAR_MATCHMAKING_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        // Pair the first two queued teams that have the same size and belong to
        // different clans, then build the battle room for them.
        public static void TryPairMatches(ChannelModel channel)
        {
            MatchModel red = null;
            MatchModel blue = null;
            lock (channel.MatchQueue)
            {
                for (int i = 0; i < channel.MatchQueue.Count && blue == null; i++)
                {
                    MatchModel a = channel.MatchQueue[i];
                    for (int j = i + 1; j < channel.MatchQueue.Count; j++)
                    {
                        MatchModel b = channel.MatchQueue[j];
                        if (a.Training != b.Training || a.Clan.Id == b.Clan.Id)
                            continue;
                        red = a;
                        blue = b;
                        break;
                    }
                }
                if (blue == null)
                    return;
                channel.MatchQueue.Remove(red);
                channel.MatchQueue.Remove(blue);
                red.InQueue = false;
                blue.InQueue = false;
            }

            if (!BuildBattleRoom(channel, red, blue))
            {
                // No free room slot: put both teams back so the next request retries.
                lock (channel.MatchQueue)
                {
                    red.InQueue = true;
                    blue.InQueue = true;
                    channel.MatchQueue.Add(red);
                    channel.MatchQueue.Add(blue);
                }
            }
        }

        // Room construction transplanted from the deleted PROTOCOL_CLAN_WAR_CREATE_ROOM_REQ
        // and reconciled with the normal lobby path (PROTOCOL_ROOM_CREATE_REQ:107-147).
        //
        // CLAN_MATCH_MAKING_RESULT (6952) carries no map id and no room index — it is the
        // post-match clan-chat bulletin (consumer 0x9C71BA builds
        // STBL_IDX_CLAN_CHATMESSAGE_MATCH with WIN/LOSE/DRAW and two scores). So the
        // pairing has to be announced through the ordinary room packets.
        //
        // Nobody here "created" the room and nobody sent ROOM_JOIN_REQ, so every one of
        // the four players gets ROOM_CREATE_ACK (3593): its body is the full RoomInfo
        // block, the only server push that is self-sufficient to render a room the
        // client did not ask to enter. ROOM_JOIN_ACK carries a single slot and assumes
        // the client already has the room. The old 068 flow pushed 1574+1566 here, and
        // the 122 client parses neither.
        // UNVERIFIED against the live client: this needs the two-account smoke test to
        // confirm all four clients land in the room with the right host/guest state.
        private static bool BuildBattleRoom(ChannelModel channel, MatchModel red, MatchModel blue)
        {
            RoomModel room = null;
            lock (channel.Rooms)
            {
                for (int index = 0; index < channel.MaxRooms; ++index)
                {
                    if (channel.GetRoom(index) != null)
                        continue;
                    room = new RoomModel(index, channel)
                    {
                        Name = red.Clan.Name + " vs " + blue.Clan.Name,
                        MapId = ClanWarMaps[0],
                        Rule = MapRules.None,
                        Stage = StageOptions.Default,
                        RoomType = RoomCondition.DeathMatch
                    };
                    room.GenerateSeed();
                    room.State = RoomState.READY;
                    room.LeaderName = red.Clan.Name;
                    room.Password = "";
                    room.KillTime = 3;
                    room.Limit = 1;
                    room.BalanceType = TeamBalance.None;
                    // WriteRoomInfoBlock emits both of these raw; RoomModel leaves
                    // RandomMaps null (the lobby path fills it from the create REQ's
                    // 24-byte field), and a null there throws inside ROOM_CREATE_ACK.
                    room.RandomMaps = new byte[24];
                    room.LeaderAddr = new byte[4];
                    room.SetSlotCount(red.Training * 2, true, false);
                    break;
                }
                if (room == null)
                    return false;

                List<Account> redPlayers = red.GetAllPlayers();
                List<Account> bluePlayers = blue.GetAllPlayers();
                foreach (Account account in redPlayers)
                    room.AddPlayer(account, TeamEnum.FR_TEAM);
                foreach (Account account in bluePlayers)
                    room.AddPlayer(account, TeamEnum.CT_TEAM);
                channel.AddRoom(room);

                red.State = MatchState.Play;
                blue.State = MatchState.Play;
                // 6930 is the matchmaking screen: the two facing teams plus the
                // per-slot record rows. It is a pure push (no result dword) and the
                // pairing is the only moment both teams are known.
                using (PROTOCOL_CLAN_WAR_MATCH_TEAM_INFO_ACK Packet = new PROTOCOL_CLAN_WAR_MATCH_TEAM_INFO_ACK(red, blue))
                {
                    byte[] bytes = Packet.GetCompleteBytes("ClanWar.TryPairMatches");
                    foreach (Account account in redPlayers)
                        account.SendCompletePacket(bytes, Packet.GetType().Name);
                    foreach (Account account in bluePlayers)
                        account.SendCompletePacket(bytes, Packet.GetType().Name);
                }
                foreach (Account account in redPlayers)
                    SendIntoRoom(account, red, room);
                foreach (Account account in bluePlayers)
                    SendIntoRoom(account, blue, room);
            }
            return true;
        }

        private static void SendIntoRoom(Account account, MatchModel match, RoomModel room)
        {
            if (account.Room != room)
                return;
            account.ResetPages();
            if (account.MatchSlot >= 0)
                match.Slots[account.MatchSlot].State = SlotMatchState.Ready;
            account.SendPacket(new PROTOCOL_ROOM_CREATE_ACK(0U, room));
            // ROOM_CREATE_ACK only carries the room header; the occupants come from
            // the slot table, which normally arrives because each client joined one at
            // a time. Nobody joined here, so push the whole table once.
            account.SendPacket(new PROTOCOL_ROOM_GET_SLOTINFO_ACK(room));
        }
    }
}
