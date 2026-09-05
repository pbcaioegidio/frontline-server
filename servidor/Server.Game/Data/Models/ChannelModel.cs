// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Models.ChannelModel
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Sync.Server;
using Server.Game.Network;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Models
{
    public class ChannelModel
    {
        public int Id { get; set; }

        public ChannelType Type { get; set; }

        public int ServerId { get; set; }

        public int MaxRooms { get; set; }

        public int ExpBonus { get; set; }

        public int GoldBonus { get; set; }

        public int CashBonus { get; set; }

        public string Password { get; set; }

        public List<PlayerSession> Players { get; set; }

        public List<Account> LobbyPlayers { get; set; } = new List<Account>();

        public List<RoomModel> Rooms { get; set; }

        public List<MatchModel> Matches { get; set; }

        // Clan-war matchmaking queue: teams that pressed MATCHMAKING_REQ (6932) and
        // are waiting to be paired against another clan. Always take `lock (MatchQueue)`.
        public readonly List<MatchModel> MatchQueue = new List<MatchModel>();

        // Clan-war mercenary board: players who pressed REGIST_MERCENARY_REQ (6938)
        // and are offering themselves to teams. Always take `lock (Mercenaries)`.
        public readonly List<Account> Mercenaries = new List<Account>();

        private DateTime Prop0 { get; set; }

        public ChannelModel(int A_1)
        {
            this.ServerId = A_1;
            this.Players = new List<PlayerSession>();
            this.Rooms = new List<RoomModel>();
            this.Matches = new List<MatchModel>();
            this.Prop0 = DateTimeUtil.Now();
        }

        public PlayerSession GetPlayer(int session)
        {
            lock (this.Players)
            {
                foreach (PlayerSession player in this.Players)
                {
                    if (player.SessionId == session)
                        return player;
                }
                return (PlayerSession)null;
            }
        }

        public PlayerSession GetPlayer(int SessionId, out int Index)
        {
            Index = -1;
            lock (this.Players)
            {
                for (int index = 0; index < this.Players.Count; ++index)
                {
                    PlayerSession player = this.Players[index];
                    if (player.SessionId == SessionId)
                    {
                        Index = index;
                        return player;
                    }
                }
                return (PlayerSession)null;
            }
        }

        public bool AddPlayer(PlayerSession pS)
        {
            lock (this.Players)
            {
                if (this.Players.Contains(pS))
                    return false;
                this.Players.Add(pS);
                
                Account account = AccountManager.GetAccount(pS.PlayerId, true);
                if (account != null)
                {
                    lock (this.LobbyPlayers)
                    {
                        if (!this.LobbyPlayers.Contains(account))
                            this.LobbyPlayers.Add(account);
                    }
                }

                UpdateServer.RefreshSChannel(this.ServerId);
                UpdateChannel.RefreshChannel(this.ServerId, this.Id, this.Players.Count);
                return true;
            }
        }

        public void RemoveMatch(int matchId)
        {
            lock (this.Matches)
            {
                for (int index = 0; index < this.Matches.Count; ++index)
                {
                    if (matchId == this.Matches[index].MatchId)
                    {
                        this.Matches.RemoveAt(index);
                        break;
                    }
                }
            }
        }

        public void AddMatch(MatchModel match)
        {
            lock (this.Matches)
            {
                if (this.Matches.Contains(match))
                    return;
                this.Matches.Add(match);
            }
        }

        public void AddRoom(RoomModel room)
        {
            lock (this.Rooms)
                this.Rooms.Add(room);
        }

        public void RemoveEmptyRooms()
        {
            lock (this.Rooms)
            {
                if (ComDiv.GetDuration(this.Prop0) < (double)ConfigLoader.EmptyRoomRemovalInterval)
                    return;
                this.Prop0 = DateTimeUtil.Now();
                for (int index = 0; index < this.Rooms.Count; ++index)
                {
                    if (this.Rooms[index].GetCountPlayers() < 1)
                        this.Rooms.RemoveAt(index--);
                }
            }
        }

        public MatchModel GetMatch(int id)
        {
            lock (this.Matches)
            {
                foreach (MatchModel match in this.Matches)
                {
                    if (match.MatchId == id)
                        return match;
                }
                return (MatchModel)null;
            }
        }

        public MatchModel GetMatch(int id, int clan)
        {
            lock (this.Matches)
            {
                foreach (MatchModel match in this.Matches)
                {
                    if (match.FriendId == id && match.Clan.Id == clan)
                        return match;
                }
                return (MatchModel)null;
            }
        }

        public RoomModel GetRoom(int id)
        {
            lock (this.Rooms)
            {
                foreach (RoomModel room in this.Rooms)
                {
                    if (room.RoomId == id)
                        return room;
                }
                return (RoomModel)null;
            }
        }

        public List<Account> GetWaitPlayers()
        {
            lock (this.LobbyPlayers)
            {
                return new List<Account>(this.LobbyPlayers);
            }
        }

        
        public void SendPacketToWaitPlayers(GameServerPacket Packet)
        {
            byte[] completeBytes = null;
            lock (this.LobbyPlayers)
            {
                if (this.LobbyPlayers.Count == 0)
                    return;

                completeBytes = Packet.GetCompleteBytes("Channel.SendPacketToWaitPlayers");
                foreach (Account account in this.LobbyPlayers)
                {
                    account.SendCompletePacket(completeBytes, Packet.GetType().Name);
                }
            }
        }

        public bool RemovePlayer(Account Player)
        {
            bool flag = false;
            Player.ChannelId = -1;
            Player.ServerId = -1;
            // Leaving the channel always drops the clan-war mercenary offer: the
            // board is per channel, and a stale entry would let other clans invite
            // a player who is no longer here.
            lock (this.Mercenaries)
                this.Mercenaries.Remove(Player);
            Player.PendingMercenaryInvite = null;
            if (Player.Session != null)
            {
                lock (this.Players)
                    flag = this.Players.Remove(Player.Session);
                
                lock (this.LobbyPlayers)
                    this.LobbyPlayers.Remove(Player);

                UpdateChannel.RefreshChannel(this.ServerId, this.Id, this.Players.Count);
                if (flag)
                    UpdateServer.RefreshSChannel(this.ServerId);
            }
            return flag;
        }
    }
}