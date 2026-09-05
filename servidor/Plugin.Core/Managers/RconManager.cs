using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using Plugin.Core.Enums;
using Plugin.Core.SQL;

namespace Plugin.Core.Managers
{
    public static class RconManager
    {
        public static bool CheckLauncherKey(long Id, string LauncherKey)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                {
                    npgsqlConnection.Open();
                    command.CommandText = "SELECT token FROM accounts WHERE player_id = @Id LIMIT 1";
                    command.Parameters.AddWithValue("@Id", Id);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string dbLauncherKey = reader.GetString(0);
                            if (dbLauncherKey == LauncherKey)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.ToString(), LoggerType.Error, ex);
            }

            return false;
        }

        public static List<string> GetTokenRconList()
        {
            List<string> TokenList = new List<string>();
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandText = "SELECT * FROM web_rcon_token";
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader();
                    while (npgsqlDataReader.Read())
                    {
                        string str = npgsqlDataReader.GetString(0); //
                        if (str != null || str.Length != 0)
                            TokenList.Add(str);
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.ToString(), LoggerType.Error, ex);
                return (List<string>)null;
            }
            return TokenList;
        }

        public static bool FindItemInPlayerTable(long ownerId, int itemId)
        {
            bool isItemFound = false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    npgsqlConnection.Open();
                    string query = "SELECT COUNT(1) FROM player_items WHERE owner_id = @OwnerId AND item_id = @ItemId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, npgsqlConnection))
                    {
                        command.Parameters.AddWithValue("@OwnerId", ownerId);
                        command.Parameters.AddWithValue("@ItemId", itemId);

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        isItemFound = count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error while searching item: {ex.Message}", LoggerType.Error);
            }
            return isItemFound;
        }

        public static bool FindCharaInPlayerTable(long PlayerId, int Id)
        {
            bool isItemFound = false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    npgsqlConnection.Open();
                    string query = "SELECT COUNT(1) FROM player_characters WHERE player_id = @PlayerId AND id = @Id";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, npgsqlConnection))
                    {
                        command.Parameters.AddWithValue("@PlayerId", PlayerId);
                        command.Parameters.AddWithValue("@Id", Id);

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        isItemFound = count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error while searching item: {ex.Message}", LoggerType.Error);
            }
            return isItemFound;
        }

        public static long GetPlayerId(string username)
        {
            long playerId = 0;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    npgsqlConnection.Open();
                    string query = "SELECT player_id FROM accounts WHERE username = @Username";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, npgsqlConnection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            playerId = Convert.ToInt64(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error while retrieving player_id: {ex.Message}", LoggerType.Error);
            }
            return playerId;
        }

        public static int GetVipBalance(long accountId)
        {
            int vipBalance = 0;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    npgsqlConnection.Open();
                    string query = "SELECT vip_balance FROM accounts WHERE player_id = @AccountId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, npgsqlConnection))
                    {
                        command.Parameters.AddWithValue("@AccountId", accountId);

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            vipBalance = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error while retrieving vip_balance: {ex.Message}", LoggerType.Error);
            }
            return vipBalance;
        }

        public static int GetCashBalance(long accountId)
        {
            int cashBalance = 0;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    npgsqlConnection.Open();
                    string query = "SELECT cash_balance FROM accounts WHERE player_id = @AccountId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, npgsqlConnection))
                    {
                        command.Parameters.AddWithValue("@AccountId", accountId);

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            cashBalance = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error while retrieving cash_balance: {ex.Message}", LoggerType.Error);
            }
            return cashBalance;
        }

        public static int GetWebcoin(long accountId)
        {
            int cashBalance = 0;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    npgsqlConnection.Open();
                    string query = "SELECT progressive_coin FROM accounts WHERE player_id = @AccountId";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, npgsqlConnection))
                    {
                        command.Parameters.AddWithValue("@AccountId", accountId);

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            cashBalance = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error while retrieving progressive_coin: {ex.Message}", LoggerType.Error, ex);
            }
            return cashBalance;
        }
    }
}