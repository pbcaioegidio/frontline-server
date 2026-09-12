// Decompiled with JetBrains decompiler
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Auth.Data.Managers;
using Server.Auth.Data.Utils;
using Server.Auth.Network;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace Server.Auth.Data.Models
{
    public class Account
    {
        public long PlayerId;
        public long BanObjectId;
        public string Nickname;
        public string Password;
        public string Username;
        public string Token;
        public string HardwareId;
        public string Email;
        public uint LastRankUpDate;
        public int InventoryPlus;
        public int Exp;
        public int Gold;
        public int ClanId;
        public int ClanAccess;
        public int Cash;
        public int Rank;
        public int Ribbon;
        public int Ensign;
        public int Medal;
        public int MasterMedal;
        public int NickColor;
        public int Age;
        public int Tags;
        public bool MyConfigsLoaded;
        public bool IsOnline;
        public int CountryFlags;
        public AccessLevel Access;
        public CouponEffects Effects;
        public CafeEnum CafePC;
        public PhysicalAddress MacAddress;
        public AuthClient Connection;
        public PlayerBonus Bonus = new PlayerBonus();
        public PlayerConfig Config = new PlayerConfig();
        public PlayerEvent Event = new PlayerEvent();
        public PlayerTitles Title = new PlayerTitles();
        public PlayerInventory Inventory = new PlayerInventory();
        public AccountStatus Status = new AccountStatus();
        public PlayerFriends Friend = new PlayerFriends();
        public PlayerStatistic Statistic = new PlayerStatistic();
        public PlayerQuickstart Quickstart = new PlayerQuickstart();
        public PlayerCharacters Character = new PlayerCharacters();
        public PlayerEquipment Equipment = new PlayerEquipment();
        public PlayerMissions Mission = new PlayerMissions();
        public PlayerReport Report = new PlayerReport();
        public PlayerBattlepass Battlepass = new PlayerBattlepass();
        public PlayerCompetitive Competitive = new PlayerCompetitive();
        public List<Account> ClanPlayers = new List<Account>();
        public List<PlayerTopup> TopUps = new List<PlayerTopup>();
        public DateTime LastLoginDate;
        public DateTime LastChannelList;
        public DateTime LastSaveConfigs;

        public Account()
        {
            DateTime dateTime = DateTimeUtil.Now();
            this.LastLoginDate = dateTime;
            this.LastChannelList = dateTime;
            this.LastSaveConfigs = dateTime;
            this.Nickname = "";
            this.Password = "";
            this.Username = "";
            this.HardwareId = "";
            this.Email = "";
        }

        public void SimpleClear()
        {
            this.Connection = (AuthClient)null;
            this.Config = new PlayerConfig();
            this.Bonus = new PlayerBonus();
            this.Event = new PlayerEvent();
            this.Title = new PlayerTitles();
            this.Inventory = new PlayerInventory();
            this.Statistic = new PlayerStatistic();
            this.Character = new PlayerCharacters();
            this.Equipment = new PlayerEquipment();
            this.Friend = new PlayerFriends();
            this.Status = new AccountStatus();
            this.Mission = new PlayerMissions();
            this.Quickstart = new PlayerQuickstart();
            this.Report = new PlayerReport();
            this.Battlepass = new PlayerBattlepass();
            this.Competitive = new PlayerCompetitive();
            this.ClanPlayers = new List<Account>();
            this.TopUps = new List<PlayerTopup>();
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

        public void Close(int time)
        {
            if (this.Connection == null)
                return;
            this.Connection.Close(time, true);
        }

        public void SendPacket(AuthServerPacket Packet)
        {
            if (this.Connection == null)
                return;
            this.Connection.SendPacket(Packet);
        }

        public void SendPacket(byte[] Data, string PacketName)
        {
            if (this.Connection == null)
                return;
            this.Connection.SendPacket(Data, PacketName);
        }

        public void SendCompletePacket(byte[] Data, string PacketName)
        {
            if (this.Connection == null)
                return;
            this.Connection.SendCompletePacket(Data, PacketName);
        }

        public long StatusId() => string.IsNullOrEmpty(this.Nickname) ? 0L : 1L;

        public bool ComparePassword(string Password)
        {
            return ConfigLoader.IsTestMode || this.Password == Password;
        }

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
                case 58:
                    return AccessLevel.GAMEMASTER;

                case 59:
                    return AccessLevel.MODERATOR;

                default:
                    return AccessLevel.NORMAL;
            }
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

        public bool IsBanned()
        {
            if (this.Access == AccessLevel.BANNED)
            {
                return true;
            }
            int num1 = ComDiv.CountDB($"SELECT * FROM base_auto_ban WHERE owner_id = '{this.PlayerId}'");
            // só bans ativos: ban temporário vencido não pode virar permanente
            int num2 = ComDiv.CountDB($"SELECT * FROM base_ban_history WHERE owner_id = '{this.PlayerId}' AND type IN ('ACCOUNT','PERMANENT','DURATION') AND expire_date > now()");
            return num1 > 0 || num2 > 0;
        }
    }
}