using Npgsql;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Auth.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Server.Auth.Data.Managers
{
    public class AccountManager
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

        private static bool ColumnExists(NpgsqlDataReader reader, string columnName)
        {
            try
            {
                reader.GetOrdinal(columnName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        public static bool AddAccount(Account acc)
        {
            lock (AccountManager.Accounts)
            {
                if (!AccountManager.Accounts.ContainsKey(acc.PlayerId))
                {
                    AccountManager.Accounts.Add(acc.PlayerId, acc);
                    return true;
                }
            }
            return false;
        }

        public static Account GetAccountDB(object valor, object valor2, int type, int searchFlag)
        {
            if (type == 0 && (string)valor == "" || type == 1 && (long)valor == 0L || type == 2 && (string.IsNullOrEmpty((string)valor) || string.IsNullOrEmpty((string)valor2)))
                return (Account)null;
            Account acc = (Account)null;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@valor", valor);
                    switch (type)
                    {
                        case 0:
                            command.CommandText = "SELECT * FROM accounts WHERE token=@valor LIMIT 1"; //username or token. RU = username. SEA/BR/ID = Token
                            break;

                        case 1:
                            command.CommandText = "SELECT * FROM accounts WHERE player_id=@valor LIMIT 1";
                            break;

                        case 2:
                            command.Parameters.AddWithValue("@valor2", valor2);
                            command.CommandText = "SELECT * FROM accounts WHERE username=@valor AND password=@valor2 LIMIT 1";
                            break;
                    }
                    NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.Default);
                    while (reader.Read())
                    {
                        acc = new Account()
                        {
                            Username = SafeParseString(reader["username"]),
                            Password = SafeParseString(reader["password"])
                        };
                        acc.SetPlayerId(SafeParseLong(reader["player_id"]), searchFlag);
                        acc.Email = SafeParseString(reader["email"]);
                        acc.Age = SafeParseInt(reader["age"]);

                        // Handle MAC address safely
                        try
                        {
                            acc.MacAddress = (PhysicalAddress)reader["mac_address"];
                        }
                        catch
                        {
                            acc.MacAddress = new PhysicalAddress(new byte[] { 0, 0, 0, 0, 0, 0 });
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

                        if (AddAccount(acc) && acc.IsOnline)
                            acc.SetOnlineStatus(false);
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
                CLogger.Print("was a problem loading accounts! " + ex.Message, LoggerType.Error, ex);
            }
            return acc;
        }

        public static void GetFriendlyAccounts(PlayerFriends system)
        {
            if (system == null)
                return;
            if (system.Friends.Count == 0)
                return;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    List<string> stringList = new List<string>();
                    for (int index = 0; index < system.Friends.Count; ++index)
                    {
                        FriendModel friend = system.Friends[index];
                        string str = "@valor" + index.ToString();
                        command.Parameters.AddWithValue(str, (object)friend.PlayerId);
                        stringList.Add(str);
                    }
                    string str1 = string.Join(",", stringList.ToArray());
                    command.CommandText = $"SELECT a.nickname, a.player_id, a.rank, a.online, a.status, COALESCE(c.logo, {uint.MaxValue}) AS clan_logo FROM accounts a LEFT JOIN system_clan c ON a.clan_id = c.id WHERE a.player_id in ({str1}) ORDER BY a.player_id";
                    NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.Default);
                    while (reader.Read())
                    {
                        FriendModel friend = system.GetFriend(SafeParseLong(reader["player_id"]));
                        if (friend != null)
                        {
                            friend.Info.Nickname = SafeParseString(reader["nickname"]);
                            friend.Info.Rank = SafeParseInt(reader["rank"]);
                            friend.Info.IsOnline = SafeParseBool(reader["online"]);
                            friend.Info.Status.SetData(SafeParseUInt(reader["status"]), friend.PlayerId);
                            friend.Info.ClanLogo = SafeParseUInt(reader["clan_logo"]);
                            if (friend.Info.IsOnline && !AccountManager.Accounts.ContainsKey(friend.PlayerId))
                            {
                                friend.Info.SetOnlineStatus(false);
                                friend.Info.Status.ResetData(friend.PlayerId);
                            }
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
                CLogger.Print("was a problem loading (FriendAccounts)! " + ex.Message, LoggerType.Error, ex);
            }
        }

        public static void GetFriendlyAccounts(PlayerFriends System, bool isOnline)
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
                    List<string> stringList = new List<string>();
                    for (int index = 0; index < System.Friends.Count; ++index)
                    {
                        FriendModel friend = System.Friends[index];
                        if (friend.State > 0)
                            return;
                        string str = "@valor" + index.ToString();
                        command.Parameters.AddWithValue(str, (object)friend.PlayerId);
                        stringList.Add(str);
                    }
                    string str1 = string.Join(",", stringList.ToArray());
                    if (str1 == "")
                        return;
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@on", (object)isOnline);
                    command.CommandText = $"SELECT a.nickname, a.player_id, a.rank, a.status, COALESCE(c.logo, {uint.MaxValue}) AS clan_logo FROM accounts a LEFT JOIN system_clan c ON a.clan_id = c.id WHERE a.player_id in ({str1}) AND a.online=@on ORDER BY a.player_id";
                    NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.Default);
                    while (reader.Read())
                    {
                        FriendModel friend = System.GetFriend(SafeParseLong(reader["player_id"]));
                        if (friend != null)
                        {
                            friend.Info.Nickname = SafeParseString(reader["nickname"]);
                            friend.Info.Rank = SafeParseInt(reader["rank"]);
                            friend.Info.IsOnline = isOnline;
                            friend.Info.Status.SetData(SafeParseUInt(reader["status"]), friend.PlayerId);
                            friend.Info.ClanLogo = SafeParseUInt(reader["clan_logo"]);
                            if (isOnline && !AccountManager.Accounts.ContainsKey(friend.PlayerId))
                            {
                                friend.Info.SetOnlineStatus(false);
                                friend.Info.Status.ResetData(friend.PlayerId);
                            }
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
                CLogger.Print("was a problem loading (FriendAccounts2)! " + ex.Message, LoggerType.Error, ex);
            }
        }

        public static Account GetAccount(long id, int searchFlag)
        {
            if (id == 0L)
                return (Account)null;
            try
            {
                Account account;
                return AccountManager.Accounts.TryGetValue(id, out account) ? account : AccountManager.GetAccountDB((object)id, (object)null, 1, searchFlag);
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
                return AccountManager.Accounts.TryGetValue(id, out account) ? account : (noUseDB ? (Account)null : AccountManager.GetAccountDB((object)id, (object)null, 1, 6175));
            }
            catch
            {
                return (Account)null;
            }
        }

        public static bool CreateAccount(out Account Player, string Username, string Password)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@login", (object)Username);
                    command.Parameters.AddWithValue("@pass", (object)Password);
                    command.CommandText = "INSERT INTO accounts (username, password) VALUES (@login, @pass)";
                    command.ExecuteNonQuery();
                    command.CommandText = "SELECT * FROM accounts WHERE username=@login";
                    NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.Default);
                    Account acc = new Account();
                    while (reader.Read())
                    {
                        acc.Username = SafeParseString(reader["username"]);
                        acc.Password = SafeParseString(reader["password"]);
                        acc.SetPlayerId(SafeParseLong(reader["player_id"]), 95);
                        acc.Email = SafeParseString(reader["email"]);
                        acc.Age = SafeParseInt(reader["age"]);

                        // Handle MAC address safely
                        try
                        {
                            acc.MacAddress = (PhysicalAddress)reader["mac_address"];
                        }
                        catch
                        {
                            acc.MacAddress = new PhysicalAddress(new byte[] { 0, 0, 0, 0, 0, 0 });
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

                    }
                    Player = acc;
                    AccountManager.AddAccount(acc);
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[AccountManager.CreateAccount] " + ex.Message, LoggerType.Error, ex);
                Player = (Account)null;
                return false;
            }
        }
    }
}
