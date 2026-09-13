using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Managers;
using Server.Game.Data.Utils;
using Server.Game.Data.XML;
using Server.Game.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Server.Game.Data.Models
{
    public class Account
    {
        public int CountryFlags;
        public string NameCard;
        public long PlayerId;
        public long BanObjectId;
        public string Nickname;
        public string Password;
        public string Username;
        public string HardwareId;
        public string Email;
        public string FindPlayer;
        public uint LastRankUpDate;
        public uint ClanDate;
        public int InventoryPlus;
        public int Sight;
        public int LastRoomPage;
        public int LastPlayerPage;
        public int ServerId;
        public int ChannelId;
        public int ClanAccess;
        public int Exp;
        public int Gold;
        public int Cash;
        public int ClanId;
        public int SlotId;
        public int NickColor;
        public int Rank;
        public int Ribbon;
        public int Ensign;
        public int Medal;
        public int MasterMedal;
        public int MatchSlot;
        public int Age;
        public int Tags;
        public int FindClanId;
        public bool IsOnline;
        public bool HideGMcolor;
        public bool AntiKickGM;
        public bool LoadedShop;
        public bool LoadedPackedGoods;
        /// <summary>ACK sintético Throw2 já enviado nesta sessão (após packed shop).</summary>
        public bool Throw2UnlockAckSent;
        public bool UpdateSeasonpass = true;
        public bool RefreshLobbyInfo;
        public CouponEffects Effects;
        public AccessLevel Access;
        public CafeEnum CafePC;
        public IPAddress PublicIP;
        public GameClient Connection;
        public RoomModel Room;
        public PlayerSession Session;
        public MatchModel Match;
        // Team that invited this player as a clan-war mercenary (opcode 6942).
        // Consumed by INVITE_ACCEPT_REQ (6946) / INVITE_DENIAL_REQ (6948); cleared
        // when the player leaves the mercenary board or disconnects.
        public MatchModel PendingMercenaryInvite;
        public PlayerConfig Config = new PlayerConfig();
        public PlayerBonus Bonus = new PlayerBonus();
        public PlayerEvent Event = new PlayerEvent();
        public PlayerTitles Title = new PlayerTitles();
        public PlayerInventory Inventory = new PlayerInventory();
        public PlayerStatistic Statistic = new PlayerStatistic();
        public PlayerCharacters Character = new PlayerCharacters();
        public PlayerEquipment Equipment = new PlayerEquipment();
        public PlayerFriends Friend = new PlayerFriends();
        public PlayerQuickstart Quickstart = new PlayerQuickstart();
        public PlayerMissions Mission = new PlayerMissions();
        public PlayerReport Report = new PlayerReport();
        public PlayerBattlepass Battlepass = new PlayerBattlepass();
        public PlayerCompetitive Competitive = new PlayerCompetitive();
        public AccountStatus Status = new AccountStatus();
        public List<PlayerTopup> TopUps = new List<PlayerTopup>();
        public DateTime LastChannelList;
        public DateTime LastLobbyEnter;
        public DateTime LastTimerSync;
        public DateTime LastChatting;
        public DateTime LastFriendInvite;
        public DateTime LastFriendDelete;
        public DateTime LastFriendInviteRoom;
        public DateTime LastClanInvite;
        public DateTime LastCreateRoom;
        public DateTime LastRoomInvitePlayers;
        public DateTime LastRoomGetLobbyPlayers;
        public DateTime LastRoomList;
        public int MissionCardBootstrap;
        public DateTime LastSaveConfigs;
        public DateTime LastReadyBattle;
        public DateTime LastFindUser;
        public DateTime LastProfileEnter;
        public DateTime LastProfileLeave;
        public DateTime LastShopEnter;
        public DateTime LastShopLeave;
        public DateTime LastInventoryEnter;
        public DateTime LastInventoryLeave;
        public DateTime LastPingDebug;

        public Account()
        {
            DateTime dateTime = DateTimeUtil.Now();
            this.LastChannelList = dateTime;
            this.LastLobbyEnter = dateTime;
            this.LastTimerSync = dateTime;
            this.LastChatting = dateTime;
            this.LastFriendInvite = dateTime;
            this.LastFriendDelete = dateTime;
            this.LastFriendInviteRoom = dateTime;
            this.LastClanInvite = dateTime;
            this.LastCreateRoom = dateTime;
            this.LastRoomInvitePlayers = dateTime;
            this.LastRoomGetLobbyPlayers = dateTime;
            this.LastRoomList = dateTime;
            this.LastSaveConfigs = dateTime;
            this.LastReadyBattle = dateTime;
            this.LastFindUser = dateTime;
            this.LastProfileLeave = dateTime;
            this.LastShopEnter = dateTime;
            this.LastShopLeave = dateTime;
            this.LastInventoryEnter = dateTime;
            this.LastInventoryLeave = dateTime;
            this.Nickname = "";
            this.Password = "";
            this.Username = "";
            this.HardwareId = "";
            this.Email = "";
            this.FindPlayer = "";
            this.ServerId = -1;
            this.ChannelId = -1;
            this.SlotId = -1;
            this.MatchSlot = -1;
        }

        public void SimpleClear()
        {
            this.Title = new PlayerTitles();
            this.Equipment = new PlayerEquipment();
            this.Inventory = new PlayerInventory();
            this.Status = new AccountStatus();
            this.Character = new PlayerCharacters();
            this.Statistic = new PlayerStatistic();
            this.Quickstart = new PlayerQuickstart();
            this.Battlepass = new PlayerBattlepass();
            this.Competitive = new PlayerCompetitive();
            this.Report = new PlayerReport();
            this.Mission = new PlayerMissions();
            this.Bonus = new PlayerBonus();
            this.Event = new PlayerEvent();
            this.Config = new PlayerConfig();
            this.TopUps.Clear();
            this.Friend.CleanList();
            this.Session = (PlayerSession)null;
            this.Match = (MatchModel)null;
            this.Room = (RoomModel)null;
            this.Connection = (GameClient)null;
        }

        public void SetPublicIP(IPAddress address)
        {
            if (address == null)
                this.PublicIP = new IPAddress(new byte[4]);
            this.PublicIP = address;
        }

        public RoomModel GetRoom()
        {
            RoomModel roomModel = Room;

            try
            {
                if (roomModel != null)
                {
                    roomModel = Room;
                }
                else
                {
                    CLogger.Print($"Account.GetRoom: RoomModel is null for Username: {this.Username}", LoggerType.Warning);
                    roomModel = null;
                }
                return roomModel;
            }
            catch (Exception Ex)
            {
                CLogger.Print($"Account.GetRoom Exception: {Ex.Message}", LoggerType.Error, Ex);
                return null;
            }
        }

        public void SetPublicIP(string address) => this.PublicIP = IPAddress.Parse(address);

        public ChannelModel GetChannel() => ChannelsXML.GetChannel(this.ServerId, this.ChannelId);

        public void ResetPages()
        {
            this.LastRoomPage = 0;
            this.LastPlayerPage = 0;
        }

        public bool GetChannel(out ChannelModel Channel)
        {
            Channel = ChannelsXML.GetChannel(this.ServerId, this.ChannelId);
            return Channel != null;
        }

        public void SetOnlineStatus(bool Online)
        {
            if (this.IsOnline == Online || !ComDiv.UpdateDB("accounts", "online", (object)Online, "player_id", (object)this.PlayerId))
                return;
            this.IsOnline = Online;
            CLogger.Print($"Account User: {this.Username}, Player UID: {this.PlayerId}, Is {(this.IsOnline ? (object)"Connected" : (object)"Disconnected")}", LoggerType.Info);
            try
            {
                int count = Plugin.Core.StatusFeed.StatusFeedHub.SafeOnlineCount();
                Plugin.Core.StatusFeed.StatusFeedHub.PublishPlayer(this.PlayerId, this.Nickname, Online, count);
            }
            catch { /* StatusFeed opcional */ }
            if (Online || !ConfigLoader.RandomPassword)
                return;
            ComDiv.UpdateDB("accounts", "password", (object)Bitwise.GenerateRandomPassword(ConfigLoader.RandomPasswordChars, 16 /*0x10*/, ConfigLoader.CryptedPasswordSalt), "player_id", (object)this.PlayerId);
        }

        public void UpdateCacheInfo()
        {
            if (this.PlayerId == 0L)
                return;
            lock (AccountManager.Accounts)
                AccountManager.Accounts[this.PlayerId] = this;
        }

        public void Close(int time, bool kicked = false)
        {
            if (this.Connection == null)
                return;
            this.Connection.Close(time, true, kicked);
        }

        public void SendPacket(GameServerPacket Packet)
        {
            if (this.Connection == null)
                return;
            this.Connection.SendPacket(Packet);
        }

        public void SendPacket(GameServerPacket Packet, bool OnlyInServer)
        {
            if (this.Connection != null)
            {
                this.Connection.SendPacket(Packet);
            }
            else
            {
                if (OnlyInServer || this.Status.ServerId == byte.MaxValue || (int)this.Status.ServerId == this.ServerId)
                    return;
                GameXender.SyncOf(this.ServerId)?.SendBytes(this.PlayerId, Packet, (int)this.Status.ServerId);
            }
        }

        public void SendPacket(byte[] Data, string PacketName)
        {
            if (this.Connection == null)
                return;
            this.Connection.SendPacket(Data, PacketName);
        }

        public void SendPacket(byte[] Data, string PacketName, bool OnlyInServer)
        {
            if (this.Connection != null)
            {
                this.Connection.SendPacket(Data, PacketName);
            }
            else
            {
                if (OnlyInServer || this.Status.ServerId == byte.MaxValue || (int)this.Status.ServerId == this.ServerId)
                    return;
                GameXender.SyncOf(this.ServerId)?.SendBytes(this.PlayerId, PacketName, Data, (int)this.Status.ServerId);
            }
        }

        public void SendCompletePacket(byte[] Data, string PacketName)
        {
            if (this.Connection == null)
                return;
            this.Connection.SendCompletePacket(Data, PacketName);
        }

        public void SendCompletePacket(byte[] Data, string PacketName, bool OnlyInServer)
        {
            if (this.Connection != null)
            {
                this.Connection.SendCompletePacket(Data, PacketName);
            }
            else
            {
                if (OnlyInServer || this.Status.ServerId == byte.MaxValue || (int)this.Status.ServerId == this.ServerId)
                    return;
                GameXender.SyncOf(this.ServerId)?.SendCompleteBytes(this.PlayerId, PacketName, Data, (int)this.Status.ServerId);
            }
        }

        public long StatusId() => string.IsNullOrEmpty(this.Nickname) ? 0L : 1L;

        public int GetSessionId() => this.Session == null ? 0 : this.Session.SessionId;

        public void SetPlayerId(long PlayerId, int LoadType)
        {
            this.PlayerId = PlayerId;
            this.GetAccountInfos(LoadType);
        }

        public void GetAccountInfos(int LoadType)
        {
            if (LoadType > 0 || PlayerId > 0)
            {
                if ((LoadType & 1) == 1)
                {
                    AllUtils.LoadPlayerEquipments(this);
                }
                if ((LoadType & 2) == 2)
                {
                    AllUtils.LoadPlayerCharacters(this);
                }
                if ((LoadType & 4) == 4)
                {
                    AllUtils.LoadPlayerStatistic(this);
                }
                if ((LoadType & 8) == 8)
                {
                    AllUtils.LoadPlayerTitles(this);
                }
                if ((LoadType & 16) == 16)
                {
                    AllUtils.LoadPlayerBonus(this);
                }
                if ((LoadType & 32) == 32)
                {
                    AllUtils.LoadPlayerFriend(this, true);
                }
                if ((LoadType & 64) == 64)
                {
                    AllUtils.LoadPlayerEvent(this);
                }
                if ((LoadType & 128) == 128)
                {
                    AllUtils.LoadPlayerConfig(this);
                }
                if ((LoadType & 256) == 256)
                {
                    AllUtils.LoadPlayerFriend(this, false);
                }
                if ((LoadType & 512) == 512)
                {
                    AllUtils.LoadPlayerQuickstarts(this);
                }
                if ((LoadType & 1024) == 1024)
                {
                    AllUtils.LoadPlayerReport(this);
                }
                if ((LoadType & 2048) == 2048)
                {
                    AllUtils.LoadPlayerBattlepass(this);
                }
                if ((LoadType & 4096) != 4096)
                {
                    AllUtils.LoadPlayerCompetitive(this);
                }
            }
        }

        public int GetRank()
        {
            int fakeRank = this.Bonus != null ? this.Bonus.FakeRank : RankDomain.NoFakeRank;
            return RankDomain.Resolve(this.Rank, fakeRank);
        }

        public int GetDisplayRank()
        {
            return this.GetRank();
        }

        public bool UseChatGM() => !HideGMcolor && (Rank == 53 || Rank == 54 || Rank == 58 || Rank == 59 || RankDomain.IsStaff(Rank));

        public AccessLevel AuthLevel()
        {
            if (this.Access == AccessLevel.BANNED)
            {
                return AccessLevel.BANNED;
            }
            if (this.Access >= AccessLevel.GAMEMASTER)
            {
                return AccessLevel.GAMEMASTER;
            }
            if (this.Access >= AccessLevel.MODERATOR)
            {
                return AccessLevel.MODERATOR;
            }

            switch (this.Rank)
            {
                case 53:
                case 58:
                    return AccessLevel.GAMEMASTER;

                case 54:
                case 59:
                    return AccessLevel.MODERATOR;

                default:
                    return AccessLevel.NORMAL;
            }
        }

        public bool IsBanned()
        {
            if (this.Access == AccessLevel.BANNED)
            {
                return true;
            }
            BanHistory ban = DaoManagerSQL.GetActiveBanForPlayer(this.PlayerId);
            return ban != null && ban.Type != "MUTE";
        }

        public bool IsMuted()
        {
            BanHistory ban = DaoManagerSQL.GetActiveBanForPlayer(this.PlayerId);
            return ban != null && ban.Type == "MUTE";
        }

        public bool IsGM() => this.AuthLevel() > AccessLevel.NORMAL;

        public bool HavePermission(string Permission)
        {
            return PermissionXML.HavePermission(Permission, this.Access);
        }

        public float PointUp()
        {
            PCCafeModel pcCafe = TemplatePackXML.GetPCCafe(this.CafePC);
            return pcCafe != null ? (float)(pcCafe.PointUp / 100) : 0.0f;
        }

        public float ExpUp()
        {
            PCCafeModel pcCafe = TemplatePackXML.GetPCCafe(this.CafePC);
            return pcCafe != null ? (float)(pcCafe.ExpUp / 100) : 0.0f;
        }

        public int TourneyLevel()
        {
            CompetitiveRank rank = CompetitiveXML.GetRank(this.Competitive.Level);
            return rank != null ? rank.TourneyLevel : 0;
        }

        public string GetNickname()
        {
            string nickname = $"({this.PlayerId}) {this.Nickname}";
            if (nickname.Length > 33)
                nickname = nickname.Substring(30) + "..";
            return nickname;
        }
    }
}