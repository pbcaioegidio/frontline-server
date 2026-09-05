using Npgsql;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Server.Game.Data.Managers
{
    public static class AccountManager
    {
        public static SortedList<long, Account> Accounts = new SortedList<long, Account>();

        #region Helper Methods for Safe Parsing

        private static int SafeParseInt(object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            string str = value.ToString();
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            return int.TryParse(str, out int result) ? result : defaultValue;
        }

        private static long SafeParseLong(object value, long defaultValue = 0L)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            string str = value.ToString();
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            return long.TryParse(str, out long result) ? result : defaultValue;
        }

        private static uint SafeParseUInt(object value, uint defaultValue = 0U)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            string str = value.ToString();
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            return uint.TryParse(str, out uint result) ? result : defaultValue;
        }

        private static bool SafeParseBool(object value, bool defaultValue = false)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            string str = value.ToString();
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            return bool.TryParse(str, out bool result) ? result : defaultValue;
        }

        private static string SafeParseString(object value, string defaultValue = "")
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            return value.ToString() ?? defaultValue;
        }

        #endregion

        public static void AddAccount(Account acc)
        {
            lock (Accounts)
            {
                if (Accounts.ContainsKey(acc.PlayerId))
                    return;
                Accounts.Add(acc.PlayerId, acc);
            }
        }

        public static Account GetAccountDB(object valor, int type, int searchDBFlag)
        {
            if (type == 2 && (long)valor == 0 || (type == 0 || type == 1) && (string)valor == "")
                return (Account)null;
            Account acc = (Account)null;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@value", valor);
                    NpgsqlCommand npgsqlCommand = command;
                    string str1;
                    switch (type)
                    {
                        case 0:
                            str1 = "username";
                            break;

                        case 1:
                            str1 = "nickname";
                            break;

                        default:
                            str1 = "player_id";
                            break;
                    }
                    string str2 = $"SELECT * FROM accounts WHERE {str1}=@value";
                    npgsqlCommand.CommandText = str2;
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.Default);
                    while (reader.Read())
                    {
                        acc = new Account()
                        {
                            Username = SafeParseString(reader["username"]),
                            Password = SafeParseString(reader["password"])
                        };
                        acc.SetPlayerId(SafeParseLong(reader["player_id"]), searchDBFlag);
                        acc.Email = SafeParseString(reader["email"]);
                        acc.Age = SafeParseInt(reader["age"]);

                        // Handle ip4_address safely (index 5 di schema Anda)
                        try
                        {
                            string ipAddress = SafeParseString(reader["ip4_address"], "0.0.0.0");
                            acc.SetPublicIP(ipAddress);
                        }
                        catch (Exception ex)
                        {
                            CLogger.Print($"Error reading ip4_address: {ex.Message}", LoggerType.Warning);
                            acc.SetPublicIP("0.0.0.0");
                        }

                        acc.Nickname = SafeParseString(reader["nickname"]);
                        acc.NickColor = SafeParseInt(reader["nick_color"]);
                        acc.Rank = SafeParseInt(reader["rank"]);
                        acc.Exp = SafeParseInt(reader["experience"]);
                        acc.Gold = SafeParseInt(reader["gold"]);
                        acc.Cash = SafeParseInt(reader["cash"]);
                        acc.CafePC = (CafeEnum)SafeParseInt(reader["pc_cafe"]);
                        acc.Access = (AccessLevel)SafeParseInt(reader["access_level"]);
                        acc.IsOnline = SafeParseBool(reader["online"]);
                        acc.ClanId = SafeParseInt(reader["clan_id"]);
                        acc.ClanAccess = SafeParseInt(reader["clan_access"]);
                        acc.Effects = (CouponEffects)SafeParseLong(reader["coupon_effect"]);
                        acc.Status.SetData(SafeParseUInt(reader["status"]), acc.PlayerId);
                        acc.LastRankUpDate = SafeParseUInt(reader["last_rank_update"]);
                        acc.BanObjectId = SafeParseLong(reader["ban_object_id"]);
                        acc.Ribbon = SafeParseInt(reader["ribbon"]);
                        acc.Ensign = SafeParseInt(reader["ensign"]);
                        acc.Medal = SafeParseInt(reader["medal"]);
                        acc.MasterMedal = SafeParseInt(reader["master_medal"]);
                        acc.Tags = SafeParseInt(reader["tags"]);
                        acc.InventoryPlus = SafeParseInt(reader["inventory_plus"]);
                        acc.CountryFlags = SafeParseInt(reader["country_flags"]);
                        AccountManager.AddAccount(acc);
                    }
                    command.Dispose();
                    reader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("was a problem loading accounts! " + ex.Message, LoggerType.Error, ex);
            }
            return acc;
        }

        public static void GetFriendlyAccounts(PlayerFriends System)
        {
            if (System == null)
                return;
            if (System.Friends.Count == 0)
                return;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    List<string> stringList = new List<string>();
                    for (int index = 0; index < System.Friends.Count; ++index)
                    {
                        FriendModel friend = System.Friends[index];
                        string str = "@valor" + index.ToString();
                        command.Parameters.AddWithValue(str, (object)friend.PlayerId);
                        stringList.Add(str);
                    }
                    string str1 = string.Join(",", stringList.ToArray());
                    command.CommandText = $"SELECT nickname, player_id, rank, online, status FROM accounts WHERE player_id in ({str1}) ORDER BY player_id";
                    NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.Default);
                    while (reader.Read())
                    {
                        FriendModel friend = System.GetFriend(SafeParseLong(reader["player_id"]));
                        if (friend != null)
                        {
                            friend.Info.Nickname = SafeParseString(reader["nickname"]);
                            friend.Info.Rank = SafeParseInt(reader["rank"]);
                            friend.Info.IsOnline = SafeParseBool(reader["online"]);
                            friend.Info.Status.SetData(SafeParseUInt(reader["status"]), friend.PlayerId);
                        }
                    }
                    command.Dispose();
                    reader.Dispose();
                    reader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("was a problem loading (FriendlyAccounts); " + ex.Message, LoggerType.Error, ex);
            }
        }

        public static Account GetAccount(long id, int searchFlag)
        {
            if (id == 0L)
                return (Account)null;
            try
            {
                Account account;
                return AccountManager.Accounts.TryGetValue(id, out account) ? account : AccountManager.GetAccountDB((object)id, 2, searchFlag);
            }
            catch
            {
                return (Account)null;
            }
        }

        public static Account GetAccount(long id, bool noUseDB)
        {
            if (id == 0L)
                return (Account)null;
            try
            {
                Account account;
                return AccountManager.Accounts.TryGetValue(id, out account) ? account : (noUseDB ? (Account)null : AccountManager.GetAccountDB((object)id, 2, 7455));
            }
            catch
            {
                return (Account)null;
            }
        }

        public static Account GetAccount(string text, int type, int searchFlag)
        {
            if (string.IsNullOrEmpty(text))
                return (Account)null;
            foreach (Account account in (IEnumerable<Account>)AccountManager.Accounts.Values)
            {
                if (account != null && (type == 1 && account.Nickname == text && account.Nickname.Length > 0 || type == 0 && string.Compare(account.Username, text) == 0))
                    return account;
            }
            return AccountManager.GetAccountDB((object)text, type, searchFlag);
        }

        public static bool UpdatePlayerName(string name, long playerId)
        {
            return ComDiv.UpdateDB("accounts", "nickname", (object)name, "player_id", (object)playerId);
        }
    }
}