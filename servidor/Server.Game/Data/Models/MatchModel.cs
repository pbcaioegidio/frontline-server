// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Models.MatchModel
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Managers;
using Server.Game.Data.Utils;
using Server.Game.Data.XML;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Models
{
    public class MatchModel
    {
        public ClanModel Clan { get; set; }

        public int Training { get; set; }

        public int ServerId { get; set; }

        public int ChannelId { get; set; }

        public int MatchId { get; set; }

        public int Leader { get; set; }

        public int FriendId { get; set; }

        public SlotMatch[] Slots { get; set; }

        public MatchState State { get; set; }

        // True while this team sits in ChannelModel.MatchQueue waiting for an
        // opponent (clan-war matchmaking, opcode 6932). Mutated only under
        // lock (channel.MatchQueue).
        public bool InQueue { get; set; }

        // Free-text team notice set by the leader via CHANGE_OPERATION_REQ (6924),
        // echoed back to the team as CHANGE_OPERATION_ACK (6925). Max 64 wchars.
        public string Operation { get; set; } = string.Empty;

        public MatchModel(ClanModel A_1)
        {
            this.Clan = A_1;
            this.MatchId = -1;
            this.Slots = new SlotMatch[9];
            this.State = MatchState.Ready;
            for (int index = 0; index < 9; ++index)
                this.Slots[index] = new SlotMatch(index);
        }

        public bool GetSlot(int SlotId, out SlotMatch Slot)
        {
            lock (this.Slots)
            {
                Slot = (SlotMatch)null;
                if (SlotId >= 0 && SlotId <= 17)
                    Slot = this.Slots[SlotId];
                return Slot != null;
            }
        }

        public SlotMatch GetSlot(int SlotId)
        {
            lock (this.Slots)
                return SlotId >= 0 && SlotId <= 17 ? this.Slots[SlotId] : (SlotMatch)null;
        }

        public void SetNewLeader(int Leader, int OldLeader)
        {
            lock (this.Slots)
            {
                if (Leader == -1)
                {
                    for (int index = 0; index < this.Training; ++index)
                    {
                        if (index != OldLeader && this.Slots[index].PlayerId > 0L)
                        {
                            this.Leader = index;
                            break;
                        }
                    }
                }
                else
                    this.Leader = Leader;
            }
        }

        public bool AddPlayer(Account Player)
        {
            lock (this.Slots)
            {
                for (int index = 0; index < this.Training; ++index)
                {
                    SlotMatch slot = this.Slots[index];
                    if (slot.PlayerId == 0L && slot.State == SlotMatchState.Empty)
                    {
                        slot.PlayerId = Player.PlayerId;
                        slot.State = SlotMatchState.Normal;
                        Player.Match = this;
                        Player.MatchSlot = index;
                        Player.Status.UpdateClanMatch((byte)this.FriendId);
                        AllUtils.SyncPlayerToClanMembers(Player);
                        return true;
                    }
                }
            }
            return false;
        }

        public Account GetPlayerBySlot(SlotMatch Slot)
        {
            try
            {
                long playerId = Slot.PlayerId;
                return playerId > 0L ? AccountManager.GetAccount(playerId, true) : (Account)null;
            }
            catch
            {
                return (Account)null;
            }
        }

        public Account GetPlayerBySlot(int SlotId)
        {
            try
            {
                long playerId = this.Slots[SlotId].PlayerId;
                return playerId > 0L ? AccountManager.GetAccount(playerId, true) : (Account)null;
            }
            catch
            {
                return (Account)null;
            }
        }

        public List<Account> GetAllPlayers(int Exception)
        {
            List<Account> allPlayers = new List<Account>();
            lock (this.Slots)
            {
                for (int index = 0; index < 9; ++index)
                {
                    long playerId = this.Slots[index].PlayerId;
                    if (playerId > 0L && index != Exception)
                    {
                        Account account = AccountManager.GetAccount(playerId, true);
                        if (account != null)
                            allPlayers.Add(account);
                    }
                }
            }
            return allPlayers;
        }

        public List<Account> GetAllPlayers()
        {
            List<Account> allPlayers = new List<Account>();
            lock (this.Slots)
            {
                for (int index = 0; index < 9; ++index)
                {
                    long playerId = this.Slots[index].PlayerId;
                    if (playerId > 0L)
                    {
                        Account account = AccountManager.GetAccount(playerId, true);
                        if (account != null)
                            allPlayers.Add(account);
                    }
                }
            }
            return allPlayers;
        }

        
        public void SendPacketToPlayers(GameServerPacket Packet)
        {
            List<Account> allPlayers = this.GetAllPlayers();
            if (allPlayers.Count == 0)
                return;
            byte[] completeBytes = Packet.GetCompleteBytes("Match.SendPacketToPlayers(SendPacket)");
            foreach (Account account in allPlayers)
                account.SendCompletePacket(completeBytes, Packet.GetType().Name);
        }

        
        public void SendPacketToPlayers(GameServerPacket Packet, int Exception)
        {
            List<Account> allPlayers = this.GetAllPlayers(Exception);
            if (allPlayers.Count == 0)
                return;
            byte[] completeBytes = Packet.GetCompleteBytes("Match.SendPacketToPlayers(SendPacket,int)");
            foreach (Account account in allPlayers)
                account.SendCompletePacket(completeBytes, Packet.GetType().Name);
        }

        public Account GetLeader()
        {
            try
            {
                return AccountManager.GetAccount(this.Slots[this.Leader].PlayerId, true);
            }
            catch
            {
                return (Account)null;
            }
        }

        public int GetServerInfo() => this.ChannelId + this.ServerId * 10;

        public int GetCountPlayers()
        {
            lock (this.Slots)
            {
                int countPlayers = 0;
                foreach (SlotMatch slot in this.Slots)
                {
                    if (slot.PlayerId > 0L)
                        ++countPlayers;
                }
                return countPlayers;
            }
        }

        private void Method0(Account A_1)
        {
            lock (this.Slots)
            {
                SlotMatch Slot;
                if (!this.GetSlot(A_1.MatchSlot, out Slot) || Slot.PlayerId != A_1.PlayerId)
                    return;
                Slot.PlayerId = 0L;
                Slot.State = SlotMatchState.Empty;
            }
        }

        public bool RemovePlayer(Account Player)
        {
            ChannelModel channel = ChannelsXML.GetChannel(this.ServerId, this.ChannelId);
            if (channel == null)
                return false;
            this.Method0(Player);
            // Losing a member always drops the team out of matchmaking: a partial
            // team must never be paired, and a disbanded one must not linger in the
            // queue (that would stall every later pairing attempt).
            this.LeaveQueue(channel);
            if (this.GetCountPlayers() == 0)
            {
                channel.RemoveMatch(this.MatchId);
            }
            else
            {
                if (Player.MatchSlot == this.Leader)
                    this.SetNewLeader(-1, -1);
                // 6939 carries no roster in 122 (bare result); the roster push is 6921.
                this.BroadcastRoster();
            }
            Player.MatchSlot = -1;
            Player.Match = (MatchModel)null;
            return true;
        }

        // Push the current team roster to every member. JOIN_TEAM_ACK (6921) is the
        // only 122 clan-war packet that carries MATCH_MEMBER_INFO + MATCH_TEAM_DETAIL_INFO,
        // so every "team changed" event re-sends it. Sent per player because the
        // packet's trailing localSlot/hasLocalSlot pair is viewer-specific.
        public void BroadcastRoster()
        {
            foreach (Account member in this.GetAllPlayers())
            {
                using (PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK Packet = new PROTOCOL_CLAN_WAR_JOIN_TEAM_ACK(0U, this, member))
                    member.SendPacket(Packet);
            }
        }

        // Drop this team from its channel's matchmaking queue. Idempotent.
        public void LeaveQueue(ChannelModel channel)
        {
            if (channel == null)
                return;
            lock (channel.MatchQueue)
            {
                channel.MatchQueue.Remove(this);
                this.InQueue = false;
            }
        }
    }
}