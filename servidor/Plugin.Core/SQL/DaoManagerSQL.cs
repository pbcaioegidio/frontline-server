using Npgsql;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Models.Map;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Plugin.Core.SQL
{
    public static class DaoManagerSQL
    {
        public static List<ItemsModel> GetPlayerInventoryItems(long OwnerId)
        {
            try
            {
                List<ItemsModel> playerInventoryItems = new List<ItemsModel>();
                if (OwnerId != 0)
                {
                    using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                    {
                        NpgsqlCommand command = npgsqlConnection.CreateCommand();
                        npgsqlConnection.Open();
                        command.Parameters.AddWithValue("@owner", (object)OwnerId);
                        command.CommandText = "SELECT * FROM player_items WHERE owner_id=@owner ORDER BY object_id ASC;";
                        command.CommandType = CommandType.Text;
                        NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                        while (npgsqlDataReader.Read())
                        {
                            ItemsModel itemsModel = new ItemsModel(int.Parse(npgsqlDataReader["id"].ToString()))
                            {
                                ObjectId = long.Parse(npgsqlDataReader["object_id"].ToString()),
                                Name = npgsqlDataReader["name"].ToString(),
                                Count = uint.Parse(npgsqlDataReader["count"].ToString()),
                                Equip = (ItemEquipType)int.Parse(npgsqlDataReader["equip"].ToString())
                            };
                            playerInventoryItems.Add(itemsModel);
                        }
                        command.Dispose();
                        npgsqlDataReader.Close();
                        npgsqlConnection.Dispose();
                        npgsqlConnection.Close();
                    }
                }
                return playerInventoryItems;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static bool UpdatePlaytimeEventData(long OwnerId, uint lastPlaytimeDate, long lastPlaytimeValue, int lastPlaytimeFinish, int currentPlaytimeEventId, string playtimeCompletedLevels)
        {
            if (OwnerId == 0)
            {
                CLogger.Print("UpdatePlaytimeEventData: OwnerId inválido", LoggerType.Warning);
                return false;
            }

            try
            {
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.Parameters.AddWithValue("@owner_id", OwnerId);
                    Command.Parameters.AddWithValue("@last_playtime_date", (long)lastPlaytimeDate);
                    Command.Parameters.AddWithValue("@last_playtime_value", lastPlaytimeValue);
                    Command.Parameters.AddWithValue("@last_playtime_finish", lastPlaytimeFinish);
                    Command.Parameters.AddWithValue("@current_playtime_event_id", currentPlaytimeEventId);
                    Command.Parameters.AddWithValue("@playtime_completed_levels", playtimeCompletedLevels ?? "");

                    Command.CommandText = @"
                        UPDATE player_events SET
                            last_playtime_date = @last_playtime_date,
                            last_playtime_value = @last_playtime_value,
                            last_playtime_finish = @last_playtime_finish,
                            current_playtime_event_id = @current_playtime_event_id,
                            playtime_completed_levels = @playtime_completed_levels
                        WHERE owner_id = @owner_id";

                    Command.CommandType = CommandType.Text;
                    int rowsAffected = Command.ExecuteNonQuery();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                //CLogger.Print($"Error actualizando datos de evento de tiempo para {OwnerId}: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        public static bool CreatePlayerInventoryItem(ItemsModel Item, long OwnerId)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.Parameters.AddWithValue("@itmId", (object)Item.Id);
                    command.Parameters.AddWithValue("@ItmNm", (object)Item.Name);
                    command.Parameters.AddWithValue("@count", (object)(long)Item.Count);
                    command.Parameters.AddWithValue("@equip", (object)(int)Item.Equip);
                    command.CommandText = "INSERT INTO player_items(owner_id, id, name, count, equip) VALUES(@owner, @itmId, @ItmNm, @count, @equip) RETURNING object_id";
                    object obj = command.ExecuteScalar();
                    Item.ObjectId = (long)obj;
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool DeletePlayerInventoryItem(long ObjectId, long OwnerId)
        {
            return ObjectId != 0 && OwnerId != 0 && ComDiv.DeleteDB("player_items", "object_id", (object)ObjectId, "owner_id", (object)OwnerId);
        }

        public static BanHistory GetAccountBan(long ObjectId)
        {
            BanHistory accountBan = new BanHistory();
            if (ObjectId == 0)
            {
                return accountBan;
            }
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@obj", (object)ObjectId);
                    command.CommandText = "SELECT * FROM base_ban_history WHERE object_id=@obj";
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        accountBan.ObjectId = long.Parse(npgsqlDataReader["object_id"].ToString());
                        accountBan.PlayerId = long.Parse(npgsqlDataReader["owner_id"].ToString());
                        accountBan.Type = npgsqlDataReader["type"].ToString();
                        accountBan.Value = npgsqlDataReader["value"].ToString();
                        accountBan.Reason = npgsqlDataReader["reason"].ToString();
                        accountBan.StartDate = DateTime.Parse(npgsqlDataReader["start_date"].ToString());
                        accountBan.EndDate = DateTime.Parse(npgsqlDataReader["expire_date"].ToString());
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
            return accountBan;
        }

        public static List<string> GetHwIdList()
        {
            List<string> hwIdList = new List<string>();
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandText = "SELECT * FROM base_ban_hwid";
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        string str = npgsqlDataReader["hardware_id"].ToString();
                        if (str != null || str.Length != 0)
                            hwIdList.Add(str);
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return (List<string>)null;
            }
            return hwIdList;
        }

        public static void GetBanStatus(string MAC, string IP4, out bool ValidMac, out bool ValidIp4)
        {
            ValidMac = false;
            ValidIp4 = false;
            try
            {
                DateTime dateTime = DateTimeUtil.Now();
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@mac", (object)MAC);
                    command.Parameters.AddWithValue("@ip", (object)IP4);
                    command.CommandText = "SELECT * FROM base_ban_history WHERE value in (@mac, @ip)";
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        string str1 = npgsqlDataReader["type"].ToString();
                        string str2 = npgsqlDataReader["value"].ToString();
                        if (!(DateTime.Parse(npgsqlDataReader["expire_date"].ToString()) < dateTime))
                        {
                            if (str1 == nameof(MAC) && str2 == MAC)
                                ValidMac = true;
                            else if (str1 == nameof(IP4) && str2 == IP4)
                                ValidIp4 = true;
                        }
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public static void CheckLicenseBan(string licenseKey, out bool isBanned)
        {
            isBanned = false;
            try
            {
                using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
                {
                    connection.Open();
                    using (NpgsqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT 1 FROM ban_license WHERE license_key = @licenseKey LIMIT 1";
                        command.Parameters.AddWithValue("@licenseKey", licenseKey);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                isBanned = true; // License ditemukan di tabel ban, artinya diblokir
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public static BanHistory SaveBanHistory(long PlayerId, string Type, string Value, DateTime EndDate, string Reason = "")
        {
            BanHistory Ban = new BanHistory()
            {
                PlayerId = PlayerId,
                Type = Type,
                Value = Value,
                EndDate = EndDate,
                Reason = Reason
            };

            try
            {
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();

                    // ✅ PENTING: Tambahkan parameter owner_id
                    Command.Parameters.AddWithValue("@owner_id", Ban.PlayerId);
                    Command.Parameters.AddWithValue("@type", Ban.Type);
                    Command.Parameters.AddWithValue("@value", Ban.Value);
                    Command.Parameters.AddWithValue("@reason", Ban.Reason);
                    Command.Parameters.AddWithValue("@start", Ban.StartDate);
                    Command.Parameters.AddWithValue("@end", Ban.EndDate);

                    // ✅ PENTING: Insert owner_id dan player_id ke database untuk kompatibilitas 3.80
                    Command.CommandText = @"
                INSERT INTO base_ban_history(owner_id, player_id, type, value, reason, start_date, expire_date) 
                VALUES(@owner_id, @owner_id, @type, @value, @reason, @start, @end) 
                RETURNING object_id";

                    object data = Command.ExecuteScalar();
                    Ban.ObjectId = (long)data;

                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                    return Ban;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static BanHistory GetActiveBanForPlayer(long playerId)
        {
            if (playerId == 0)
                return null;

            try
            {
                using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = connection.CreateCommand();
                    connection.Open();

                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@now", DateTimeUtil.Now());

                    // ✅ Query berdasarkan kolom owner_id ATAU player_id (untuk kompatibilitas 3.80)
                    command.CommandText = @"
                SELECT * FROM base_ban_history 
                WHERE (owner_id = @playerId OR player_id = @playerId)
                  AND expire_date > @now 
                ORDER BY object_id DESC 
                LIMIT 1";

                    command.CommandType = CommandType.Text;

                    NpgsqlDataReader reader = command.ExecuteReader();
                    BanHistory ban = null;

                    if (reader.Read())
                    {
                        ban = new BanHistory
                        {
                            ObjectId = long.Parse(reader["object_id"].ToString()),
                            PlayerId = long.Parse(reader["owner_id"].ToString()),
                            Type = reader["type"].ToString(),
                            Value = reader["value"].ToString(),
                            Reason = reader["reason"].ToString(),
                            StartDate = DateTime.Parse(reader["start_date"].ToString()),
                            EndDate = DateTime.Parse(reader["expire_date"].ToString())
                        };
                    }

                    reader.Close();
                    command.Dispose();
                    connection.Close();

                    return ban;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error getting active ban for player {playerId}: {ex.Message}", LoggerType.Error, ex);
                return null;
            }
        }

        public static bool SaveAutoBan(long PlayerId, string Username, string Nickname, string Type, string Time, string Address, string HackType)
        {
            if (PlayerId == 0)
            {
                return false;
            }
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@player_id", (object)PlayerId);
                    command.Parameters.AddWithValue("@login", (object)Username);
                    command.Parameters.AddWithValue("@player_name", (object)Nickname);
                    command.Parameters.AddWithValue("@type", (object)Type);
                    command.Parameters.AddWithValue("@time", (object)Time);
                    command.Parameters.AddWithValue("@ip", (object)Address);
                    command.Parameters.AddWithValue("@hack_type", (object)HackType);
                    command.CommandText = "INSERT INTO base_auto_ban(owner_id, username, nickname, type, time, ip4_address, hack_type) VALUES(@player_id, @login, @player_name, @type, @time, @ip, @hack_type)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool SaveBanReason(long ObjectId, string Reason)
        {
            return ObjectId != 0 && ComDiv.UpdateDB("base_ban_history", "reason", (object)Reason, "object_id", (object)ObjectId);
        }

        public static bool CreateClan(out int ClanId, string Name, long OwnerId, string ClanInfo, uint CreateDate)
        {
            try
            {
                ClanId = -1;
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.Parameters.AddWithValue("@name", (object)Name);
                    command.Parameters.AddWithValue("@date", (object)(long)CreateDate);
                    command.Parameters.AddWithValue("@info", (object)ClanInfo);
                    command.Parameters.AddWithValue("@best", (object)"0-0");
                    command.CommandText = "INSERT INTO system_clan (name, owner_id, create_date, info, best_exp, best_participants, best_wins, best_kills, best_headshots) VALUES (@name, @owner, @date, @info, @best, @best, @best, @best, @best) RETURNING id";
                    object obj = command.ExecuteScalar();
                    ClanId = (int)obj;
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                ClanId = -1;
                return false;
            }
        }

        public static bool UpdateClanInfo(
          int ClanId,
          int Authority,
          int RankLimit,
          int MinAge,
          int MaxAge,
          int JoinType)
        {
            if (ClanId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@ClanId", (object)ClanId);
                    command.Parameters.AddWithValue("@Authority", (object)Authority);
                    command.Parameters.AddWithValue("@RankLimit", (object)RankLimit);
                    command.Parameters.AddWithValue("@MinAge", (object)MinAge);
                    command.Parameters.AddWithValue("@MaxAge", (object)MaxAge);
                    command.Parameters.AddWithValue("@JoinType", (object)JoinType);
                    command.CommandText = "UPDATE system_clan SET authority=@Authority, rank_limit=@RankLimit, min_age_limit=@MinAge, max_age_limit=@MaxAge, join_permission=@JoinType WHERE id=@ClanId";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static void UpdateClanBestPlayers(ClanModel Clan)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)Clan.Id);
                    command.Parameters.AddWithValue("@bp1", (object)Clan.BestPlayers.Exp.GetSplit());
                    command.Parameters.AddWithValue("@bp2", (object)Clan.BestPlayers.Participation.GetSplit());
                    command.Parameters.AddWithValue("@bp3", (object)Clan.BestPlayers.Wins.GetSplit());
                    command.Parameters.AddWithValue("@bp4", (object)Clan.BestPlayers.Kills.GetSplit());
                    command.Parameters.AddWithValue("@bp5", (object)Clan.BestPlayers.Headshots.GetSplit());
                    command.CommandType = CommandType.Text;
                    command.CommandText = "UPDATE system_clan SET best_exp=@bp1, best_participants=@bp2, best_wins=@bp3, best_kills=@bp4, best_headshots=@bp5 WHERE id=@id";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public static bool UpdateClanLogo(int ClanId, uint logo)
        {
            return ClanId != 0 && ComDiv.UpdateDB("system_clan", nameof(logo), (object)(long)logo, "id", (object)ClanId);
        }

        public static bool UpdateClanPoints(int ClanId, float Gold)
        {
            return ClanId != 0 && ComDiv.UpdateDB("system_clan", "gold", (object)Gold, "id", (object)ClanId);
        }

        public static bool UpdateClanExp(int ClanId, int Exp)
        {
            return ClanId != 0 && ComDiv.UpdateDB("system_clan", "exp", (object)Exp, "id", (object)ClanId);
        }

        public static bool UpdateClanRank(int ClanId, int Rank)
        {
            return ClanId != 0 && ComDiv.UpdateDB("system_clan", "rank", (object)Rank, "id", (object)ClanId);
        }

        public static bool UpdateClanBattles(int ClanId, int Matches, int Wins, int Loses)
        {
            if (ClanId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@clan", (object)ClanId);
                    command.Parameters.AddWithValue("@partidas", (object)Matches);
                    command.Parameters.AddWithValue("@vitorias", (object)Wins);
                    command.Parameters.AddWithValue("@derrotas", (object)Loses);
                    command.CommandText = "UPDATE system_clan SET matches=@partidas, match_wins=@vitorias, match_loses=@derrotas WHERE id=@clan";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static int GetClanPlayers(int ClanId)
        {
            int clanPlayers = 0;
            if (ClanId == 0)
                return clanPlayers;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@clan", (object)ClanId);
                    command.CommandText = "SELECT COUNT(*) FROM accounts WHERE clan_id=@clan";
                    clanPlayers = Convert.ToInt32(command.ExecuteScalar());
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return clanPlayers;
        }

        public static MessageModel GetMessage(long ObjectId, long PlayerId)
        {
            MessageModel message = (MessageModel)null;
            if (ObjectId != 0)
            {
                if (PlayerId != 0)
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            NpgsqlCommand command = npgsqlConnection.CreateCommand();
                            npgsqlConnection.Open();
                            command.Parameters.AddWithValue("@obj", (object)ObjectId);
                            command.Parameters.AddWithValue("@owner", (object)PlayerId);
                            command.CommandText = "SELECT * FROM player_messages WHERE object_id=@obj AND owner_id=@owner";
                            command.CommandType = CommandType.Text;
                            NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                            while (npgsqlDataReader.Read())
                                message = new MessageModel((long)uint.Parse(npgsqlDataReader["expire_date"].ToString()), DateTimeUtil.Now())
                                {
                                    ObjectId = ObjectId,
                                    SenderId = long.Parse(npgsqlDataReader["sender_id"].ToString()),
                                    SenderName = npgsqlDataReader["sender_name"].ToString(),
                                    ClanId = int.Parse(npgsqlDataReader["clan_id"].ToString()),
                                    ClanNote = (NoteMessageClan)int.Parse(npgsqlDataReader["clan_note"].ToString()),
                                    Text = npgsqlDataReader["text"].ToString(),
                                    Type = (NoteMessageType)int.Parse(npgsqlDataReader["type"].ToString()),
                                    State = (NoteMessageState)int.Parse(npgsqlDataReader["state"].ToString())
                                };
                            command.Dispose();
                            npgsqlDataReader.Close();
                            npgsqlConnection.Dispose();
                            npgsqlConnection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                        return (MessageModel)null;
                    }
                    return message;
                }
            }
            return message;
        }

        public static List<MessageModel> GetGiftMessages(long OwnerId)
        {
            List<MessageModel> giftMessages = new List<MessageModel>();
            if (OwnerId == 0)
                return giftMessages;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_messages WHERE owner_id=@owner";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        NoteMessageType noteMessageType = (NoteMessageType)int.Parse(npgsqlDataReader["type"].ToString());
                        if (noteMessageType == NoteMessageType.Gift)
                        {
                            MessageModel messageModel = new MessageModel((long)uint.Parse(npgsqlDataReader["expire_date"].ToString()), DateTimeUtil.Now())
                            {
                                ObjectId = long.Parse(npgsqlDataReader["object_id"].ToString()),
                                SenderId = long.Parse(npgsqlDataReader["sender_id"].ToString()),
                                SenderName = npgsqlDataReader["sender_name"].ToString(),
                                ClanId = int.Parse(npgsqlDataReader["clan_id"].ToString()),
                                ClanNote = (NoteMessageClan)int.Parse(npgsqlDataReader["clan_note"].ToString()),
                                Text = npgsqlDataReader["text"].ToString(),
                                Type = noteMessageType,
                                State = (NoteMessageState)int.Parse(npgsqlDataReader["state"].ToString())
                            };
                            giftMessages.Add(messageModel);
                        }
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return giftMessages;
        }

        public static List<MessageModel> GetMessages(long OwnerId)
        {
            List<MessageModel> messages = new List<MessageModel>();
            if (OwnerId == 0)
                return messages;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_messages WHERE owner_id=@owner";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        NoteMessageType noteMessageType = (NoteMessageType)int.Parse(npgsqlDataReader["type"].ToString());
                        if (noteMessageType != NoteMessageType.Gift)
                        {
                            MessageModel messageModel = new MessageModel((long)uint.Parse(npgsqlDataReader["expire_date"].ToString()), DateTimeUtil.Now())
                            {
                                ObjectId = long.Parse(npgsqlDataReader["object_id"].ToString()),
                                SenderId = long.Parse(npgsqlDataReader["sender_id"].ToString()),
                                SenderName = npgsqlDataReader["sender_name"].ToString(),
                                ClanId = int.Parse(npgsqlDataReader["clan_id"].ToString()),
                                ClanNote = (NoteMessageClan)int.Parse(npgsqlDataReader["clan_note"].ToString()),
                                Text = npgsqlDataReader["text"].ToString(),
                                Type = noteMessageType,
                                State = (NoteMessageState)int.Parse(npgsqlDataReader["state"].ToString())
                            };
                            messages.Add(messageModel);
                        }
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return messages;
        }

        public static bool MessageExists(long ObjectId, long OwnerId)
        {
            if (ObjectId != 0)
            {
                if (OwnerId != 0)
                {
                    try
                    {
                        int num = 0;
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            NpgsqlCommand command = npgsqlConnection.CreateCommand();
                            npgsqlConnection.Open();
                            command.Parameters.AddWithValue("@obj", (object)ObjectId);
                            command.Parameters.AddWithValue("@owner", (object)OwnerId);
                            command.CommandText = "SELECT COUNT(*) FROM player_messages WHERE object_id=@obj AND owner_id=@owner";
                            num = Convert.ToInt32(command.ExecuteScalar());
                            command.Dispose();
                            npgsqlConnection.Dispose();
                            npgsqlConnection.Close();
                        }
                        return num > 0;
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                    }
                    return false;
                }
            }
            return false;
        }

        public static int GetMessagesCount(long OwnerId)
        {
            int messagesCount = 0;
            if (OwnerId == 0)
                return messagesCount;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "SELECT COUNT(*) FROM player_messages WHERE owner_id=@owner";
                    messagesCount = Convert.ToInt32(command.ExecuteScalar());
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return messagesCount;
        }

        public static bool CreateMessage(long OwnerId, MessageModel Message)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.Parameters.AddWithValue("@sendid", (object)Message.SenderId);
                    command.Parameters.AddWithValue("@clan", (object)Message.ClanId);
                    command.Parameters.AddWithValue("@sendname", (object)Message.SenderName);
                    command.Parameters.AddWithValue("@text", (object)Message.Text);
                    command.Parameters.AddWithValue("@type", (object)(int)Message.Type);
                    command.Parameters.AddWithValue("@state", (object)(int)Message.State);
                    command.Parameters.AddWithValue("@expire", (object)Message.ExpireDate);
                    command.Parameters.AddWithValue("@cb", (object)(int)Message.ClanNote);
                    command.CommandType = CommandType.Text;
                    command.CommandText = "INSERT INTO player_messages(owner_id, sender_id, sender_name, clan_id, clan_note, text, type, state, expire_date) VALUES(@owner, @sendid, @sendname, @clan, @cb, @text, @type, @state, @expire) RETURNING object_id";
                    object obj = command.ExecuteScalar();
                    Message.ObjectId = (long)obj;
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static void UpdateState(long ObjectId, long OwnerId, int Value)
        {
            ComDiv.UpdateDB("player_messages", "state", (object)Value, "object_id", (object)ObjectId, "owner_id", (object)OwnerId);
        }

        public static void UpdateExpireDate(long ObjectId, long OwnerId, uint Date)
        {
            ComDiv.UpdateDB("player_messages", "expire_date", (object)(long)Date, "object_id", (object)ObjectId, "owner_id", (object)OwnerId);
        }

        public static bool DeleteMessage(long ObjectId, long OwnerId)
        {
            return ObjectId != 0 && OwnerId != 0 && ComDiv.DeleteDB("player_messages", "object_id", (object)ObjectId, "owner_id", (object)OwnerId);
        }

        public static bool DeleteMessages(List<object> ObjectIds, long OwnerId)
        {
            return ObjectIds.Count != 0 && OwnerId != 0 && ComDiv.DeleteDB("player_messages", "object_id", ObjectIds.ToArray(), "owner_id", (object)OwnerId);
        }

        public static void RecycleMessages(long OwnerId, List<MessageModel> Messages)
        {
            List<object> ObjectIds = new List<object>();
            for (int index = 0; index < Messages.Count; ++index)
            {
                MessageModel message = Messages[index];
                if (message.DaysRemaining == 0)
                {
                    ObjectIds.Add((object)message.ObjectId);
                    Messages.RemoveAt(index--);
                }
            }
            DaoManagerSQL.DeleteMessages(ObjectIds, OwnerId);
        }

        public static PlayerEquipment GetPlayerEquipmentsDB(long OwnerId)
        {
            PlayerEquipment playerEquipmentsDb = (PlayerEquipment)null;
            if (OwnerId == 0)
                return playerEquipmentsDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_equipments WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerEquipmentsDb = new PlayerEquipment()
                        {
                            OwnerId = OwnerId,
                            WeaponPrimary = int.Parse(npgsqlDataReader["weapon_primary"].ToString()),
                            WeaponSecondary = int.Parse(npgsqlDataReader["weapon_secondary"].ToString()),
                            WeaponMelee = int.Parse(npgsqlDataReader["weapon_melee"].ToString()),
                            WeaponExplosive = int.Parse(npgsqlDataReader["weapon_explosive"].ToString()),
                            WeaponSpecial = int.Parse(npgsqlDataReader["weapon_special"].ToString()),
                            CharaRedId = int.Parse(npgsqlDataReader["chara_red_side"].ToString()),
                            CharaBlueId = int.Parse(npgsqlDataReader["chara_blue_side"].ToString()),
                            DinoItem = int.Parse(npgsqlDataReader["dino_item_chara"].ToString()),
                            PartHead = int.Parse(npgsqlDataReader["part_head"].ToString()),
                            PartFace = int.Parse(npgsqlDataReader["part_face"].ToString()),
                            PartJacket = int.Parse(npgsqlDataReader["part_jacket"].ToString()),
                            PartPocket = int.Parse(npgsqlDataReader["part_pocket"].ToString()),
                            PartGlove = int.Parse(npgsqlDataReader["part_glove"].ToString()),
                            PartBelt = int.Parse(npgsqlDataReader["part_belt"].ToString()),
                            PartHolster = int.Parse(npgsqlDataReader["part_holster"].ToString()),
                            PartSkin = int.Parse(npgsqlDataReader["part_skin"].ToString()),
                            BeretItem = int.Parse(npgsqlDataReader["beret_item_part"].ToString()),
                            AccessoryId = int.Parse(npgsqlDataReader["accesory_id"].ToString()),
                            SprayId = int.Parse(npgsqlDataReader["spray_id"].ToString()),
                            NameCardId = int.Parse(npgsqlDataReader["namecard_id"].ToString()),
                            Emoticons = new int[6]
                            {
                                int.Parse(npgsqlDataReader["emoticon_0"].ToString()),
                                int.Parse(npgsqlDataReader["emoticon_1"].ToString()),
                                int.Parse(npgsqlDataReader["emoticon_2"].ToString()),
                                int.Parse(npgsqlDataReader["emoticon_3"].ToString()),
                                int.Parse(npgsqlDataReader["emoticon_4"].ToString()),
                                int.Parse(npgsqlDataReader["emoticon_5"].ToString())
                            }
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerEquipmentsDb;
        }

        public static bool CreatePlayerEquipmentsDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_equipments(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static List<CharacterModel> GetPlayerCharactersDB(long OwnerId)
        {
            List<CharacterModel> playerCharactersDb = new List<CharacterModel>();
            if (OwnerId == 0)
                return playerCharactersDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@OwnerId", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_characters WHERE owner_id=@OwnerId ORDER BY slot ASC;";
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        CharacterModel characterModel = new CharacterModel()
                        {
                            ObjectId = long.Parse(npgsqlDataReader["object_id"].ToString()),
                            Id = int.Parse(npgsqlDataReader["id"].ToString()),
                            Slot = int.Parse(npgsqlDataReader["slot"].ToString()),
                            Name = npgsqlDataReader["name"].ToString(),
                            CreateDate = uint.Parse(npgsqlDataReader["create_date"].ToString()),
                            PlayTime = uint.Parse(npgsqlDataReader["playtime"].ToString())
                        };
                        playerCharactersDb.Add(characterModel);
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerCharactersDb;
        }

        public static bool CreatePlayerCharacter(CharacterModel Chara, long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner_id", (object)OwnerId);
                    command.Parameters.AddWithValue("@id", (object)Chara.Id);
                    command.Parameters.AddWithValue("@slot", (object)Chara.Slot);
                    command.Parameters.AddWithValue("@name", (object)Chara.Name);
                    command.Parameters.AddWithValue("@createdate", (object)(long)Chara.CreateDate);
                    command.Parameters.AddWithValue("@playtime", (object)(long)Chara.PlayTime);
                    command.CommandType = CommandType.Text;
                    if (Chara.ObjectId > 0L && !ComDiv.IsStockId(Chara.ObjectId))
                    {
                        command.Parameters.AddWithValue("@object_id", (object)Chara.ObjectId);
                        command.CommandText = "INSERT INTO player_characters(object_id, owner_id, id, slot, name, create_date, playtime) VALUES(@object_id, @owner_id, @id, @slot, @name, @createdate, @playtime)";
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        command.CommandText = "INSERT INTO player_characters(owner_id, id, slot, name, create_date, playtime) VALUES(@owner_id, @id, @slot, @name, @createdate, @playtime) RETURNING object_id";
                        object obj = command.ExecuteScalar();
                        Chara.ObjectId = (long)obj;
                    }
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static StatisticTotal GetPlayerStatBasicDB(long OwnerId)
        {
            StatisticTotal playerStatBasicDb = (StatisticTotal)null;
            if (OwnerId == 0)
                return playerStatBasicDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_stat_basics WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerStatBasicDb = new StatisticTotal()
                        {
                            OwnerId = OwnerId,
                            Matches = int.Parse(npgsqlDataReader["matches"].ToString()),
                            MatchWins = int.Parse(npgsqlDataReader["match_wins"].ToString()),
                            MatchLoses = int.Parse(npgsqlDataReader["match_loses"].ToString()),
                            MatchDraws = int.Parse(npgsqlDataReader["match_draws"].ToString()),
                            KillsCount = int.Parse(npgsqlDataReader["kills_count"].ToString()),
                            DeathsCount = int.Parse(npgsqlDataReader["deaths_count"].ToString()),
                            HeadshotsCount = int.Parse(npgsqlDataReader["headshots_count"].ToString()),
                            AssistsCount = int.Parse(npgsqlDataReader["assists_count"].ToString()),
                            EscapesCount = int.Parse(npgsqlDataReader["escapes_count"].ToString()),
                            MvpCount = int.Parse(npgsqlDataReader["mvp_count"].ToString()),
                            TotalMatchesCount = int.Parse(npgsqlDataReader["total_matches"].ToString()),
                            TotalKillsCount = int.Parse(npgsqlDataReader["total_kills"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerStatBasicDb;
        }

        public static bool CreatePlayerStatBasicDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_stat_basics(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static StatisticSeason GetPlayerStatSeasonDB(long OwnerId)
        {
            StatisticSeason playerStatSeasonDb = (StatisticSeason)null;
            if (OwnerId == 0)
                return playerStatSeasonDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_stat_seasons WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerStatSeasonDb = new StatisticSeason()
                        {
                            OwnerId = OwnerId,
                            Matches = int.Parse(npgsqlDataReader["matches"].ToString()),
                            MatchWins = int.Parse(npgsqlDataReader["match_wins"].ToString()),
                            MatchLoses = int.Parse(npgsqlDataReader["match_loses"].ToString()),
                            MatchDraws = int.Parse(npgsqlDataReader["match_draws"].ToString()),
                            KillsCount = int.Parse(npgsqlDataReader["kills_count"].ToString()),
                            DeathsCount = int.Parse(npgsqlDataReader["deaths_count"].ToString()),
                            HeadshotsCount = int.Parse(npgsqlDataReader["headshots_count"].ToString()),
                            AssistsCount = int.Parse(npgsqlDataReader["assists_count"].ToString()),
                            EscapesCount = int.Parse(npgsqlDataReader["escapes_count"].ToString()),
                            MvpCount = int.Parse(npgsqlDataReader["mvp_count"].ToString()),
                            TotalMatchesCount = int.Parse(npgsqlDataReader["total_matches"].ToString()),
                            TotalKillsCount = int.Parse(npgsqlDataReader["total_kills"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerStatSeasonDb;
        }

        public static bool CreatePlayerStatSeasonDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_stat_seasons(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static StatisticClan GetPlayerStatClanDB(long OwnerId)
        {
            StatisticClan playerStatClanDb = (StatisticClan)null;
            if (OwnerId == 0)
                return playerStatClanDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_stat_clans WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerStatClanDb = new StatisticClan()
                        {
                            OwnerId = OwnerId,
                            Matches = int.Parse(npgsqlDataReader["clan_matches"].ToString()),
                            MatchWins = int.Parse(npgsqlDataReader["clan_match_wins"].ToString()),
                            MatchLoses = int.Parse(npgsqlDataReader["clan_match_loses"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerStatClanDb;
        }

        public static bool CreatePlayerStatClanDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_stat_clans(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static StatisticMercenary GetPlayerStatMercenaryDB(long OwnerId)
        {
            StatisticMercenary playerStatMercenaryDb = (StatisticMercenary)null;
            if (OwnerId == 0)
                return playerStatMercenaryDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_stat_mercenaries WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerStatMercenaryDb = new StatisticMercenary()
                        {
                            OwnerId = OwnerId,
                            Matches = int.Parse(npgsqlDataReader["matches"].ToString()),
                            MatchWins = int.Parse(npgsqlDataReader["match_wins"].ToString()),
                            MatchLoses = int.Parse(npgsqlDataReader["match_loses"].ToString()),
                            DropsCount = int.Parse(npgsqlDataReader["drops_count"].ToString()),
                            KillsCount = int.Parse(npgsqlDataReader["kills_count"].ToString()),
                            DeathsCount = int.Parse(npgsqlDataReader["deaths_count"].ToString()),
                            HeadshotsCount = int.Parse(npgsqlDataReader["headshots_count"].ToString()),
                            AssistsCount = int.Parse(npgsqlDataReader["assists_count"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerStatMercenaryDb;
        }

        public static bool CreatePlayerStatMercenaryDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_stat_mercenaries(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static StatisticDaily GetPlayerStatDailiesDB(long OwnerId)
        {
            StatisticDaily playerStatDailiesDb = (StatisticDaily)null;
            if (OwnerId == 0)
                return playerStatDailiesDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_stat_dailies WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerStatDailiesDb = new StatisticDaily()
                        {
                            OwnerId = OwnerId,
                            Matches = int.Parse(npgsqlDataReader["matches"].ToString()),
                            MatchWins = int.Parse(npgsqlDataReader["match_wins"].ToString()),
                            MatchLoses = int.Parse(npgsqlDataReader["match_loses"].ToString()),
                            MatchDraws = int.Parse(npgsqlDataReader["match_draws"].ToString()),
                            KillsCount = int.Parse(npgsqlDataReader["kills_count"].ToString()),
                            DeathsCount = int.Parse(npgsqlDataReader["deaths_count"].ToString()),
                            HeadshotsCount = int.Parse(npgsqlDataReader["headshots_count"].ToString()),
                            ExpGained = int.Parse(npgsqlDataReader["exp_gained"].ToString()),
                            PointGained = int.Parse(npgsqlDataReader["point_gained"].ToString()),
                            Playtime = uint.Parse($"{npgsqlDataReader["playtime"]}")
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerStatDailiesDb;
        }

        public static bool CreatePlayerStatDailiesDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_stat_dailies(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static StatisticWeapon GetPlayerStatWeaponsDB(long OwnerId)
        {
            StatisticWeapon playerStatWeaponsDb = (StatisticWeapon)null;
            if (OwnerId == 0)
                return playerStatWeaponsDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_stat_weapons WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerStatWeaponsDb = new StatisticWeapon()
                        {
                            OwnerId = OwnerId,
                            AssaultKills = int.Parse(npgsqlDataReader["assault_rifle_kills"].ToString()),
                            AssaultDeaths = int.Parse(npgsqlDataReader["assault_rifle_deaths"].ToString()),
                            SmgKills = int.Parse(npgsqlDataReader["sub_machine_gun_kills"].ToString()),
                            SmgDeaths = int.Parse(npgsqlDataReader["sub_machine_gun_deaths"].ToString()),
                            SniperKills = int.Parse(npgsqlDataReader["sniper_rifle_kills"].ToString()),
                            SniperDeaths = int.Parse(npgsqlDataReader["sniper_rifle_deaths"].ToString()),
                            MachinegunKills = int.Parse(npgsqlDataReader["machine_gun_kills"].ToString()),
                            MachinegunDeaths = int.Parse(npgsqlDataReader["machine_gun_deaths"].ToString()),
                            ShotgunKills = int.Parse(npgsqlDataReader["shot_gun_kills"].ToString()),
                            ShotgunDeaths = int.Parse(npgsqlDataReader["shot_gun_deaths"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerStatWeaponsDb;
        }

        public static bool CreatePlayerStatWeaponsDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_stat_weapons(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static StatisticAcemode GetPlayerStatAcemodesDB(long OwnerId)
        {
            StatisticAcemode playerStatAcemodesDb = (StatisticAcemode)null;
            if (OwnerId == 0)
                return playerStatAcemodesDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_stat_acemodes WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerStatAcemodesDb = new StatisticAcemode()
                        {
                            OwnerId = OwnerId,
                            Matches = int.Parse(npgsqlDataReader["matches"].ToString()),
                            MatchWins = int.Parse(npgsqlDataReader["match_wins"].ToString()),
                            MatchLoses = int.Parse(npgsqlDataReader["match_loses"].ToString()),
                            Kills = int.Parse(npgsqlDataReader["kills_count"].ToString()),
                            Deaths = int.Parse(npgsqlDataReader["deaths_count"].ToString()),
                            Headshots = int.Parse(npgsqlDataReader["headshots_count"].ToString()),
                            Assists = int.Parse(npgsqlDataReader["assists_count"].ToString()),
                            Escapes = int.Parse(npgsqlDataReader["escapes_count"].ToString()),
                            Winstreaks = int.Parse(npgsqlDataReader["winstreaks_count"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerStatAcemodesDb;
        }

        public static bool CreatePlayerStatAcemodesDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_stat_acemodes(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static StatisticBattlecup GetPlayerStatBattlecupDB(long OwnerId)
        {
            StatisticBattlecup playerStatBattlecupDb = (StatisticBattlecup)null;
            if (OwnerId == 0)
                return playerStatBattlecupDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_stat_battlecups WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerStatBattlecupDb = new StatisticBattlecup()
                        {
                            OwnerId = OwnerId,
                            Matches = int.Parse(npgsqlDataReader["matches"].ToString()),
                            MatchWins = int.Parse(npgsqlDataReader["match_wins"].ToString()),
                            MatchLoses = int.Parse(npgsqlDataReader["match_loses"].ToString()),
                            KillsCount = int.Parse(npgsqlDataReader["kills_count"].ToString()),
                            DeathsCount = int.Parse(npgsqlDataReader["deaths_count"].ToString()),
                            HeadshotsCount = int.Parse(npgsqlDataReader["headshots_count"].ToString()),
                            AssistsCount = int.Parse(npgsqlDataReader["assists_count"].ToString()),
                            EscapesCount = int.Parse(npgsqlDataReader["escapes_count"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerStatBattlecupDb;
        }

        public static bool CreatePlayerStatBattlecupsDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_stat_battlecups(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static PlayerTitles GetPlayerTitlesDB(long OwnerId)
        {
            PlayerTitles playerTitlesDb = (PlayerTitles)null;
            if (OwnerId == 0)
                return playerTitlesDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_titles WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerTitlesDb = new PlayerTitles()
                        {
                            OwnerId = OwnerId,
                            Equiped1 = int.Parse(npgsqlDataReader["equip_slot1"].ToString()),
                            Equiped2 = int.Parse(npgsqlDataReader["equip_slot2"].ToString()),
                            Equiped3 = int.Parse(npgsqlDataReader["equip_slot3"].ToString()),
                            Flags = long.Parse(npgsqlDataReader["flags"].ToString()),
                            Slots = int.Parse(npgsqlDataReader["slots"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerTitlesDb;
        }

        public static bool CreatePlayerTitlesDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_titles(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static PlayerBonus GetPlayerBonusDB(long OwnerId)
        {
            PlayerBonus playerBonusDb = (PlayerBonus)null;
            if (OwnerId == 0)
                return playerBonusDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_bonus WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerBonusDb = new PlayerBonus()
                        {
                            OwnerId = OwnerId,
                            Bonuses = int.Parse(npgsqlDataReader["bonuses"].ToString()),
                            CrosshairColor = int.Parse(npgsqlDataReader["crosshair_color"].ToString()),
                            FreePass = int.Parse(npgsqlDataReader["free_pass"].ToString()),
                            FakeRank = int.Parse(npgsqlDataReader["fake_rank"].ToString()),
                            FakeNick = npgsqlDataReader["fake_nick"].ToString(),
                            MuzzleColor = int.Parse(npgsqlDataReader["muzzle_color"].ToString()),
                            NickBorderColor = int.Parse(npgsqlDataReader["nick_border_color"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerBonusDb;
        }

        public static bool CreatePlayerBonusDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_bonus(owner_id) VALUES(@id)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static PlayerConfig GetPlayerConfigDB(long OwnerId)
        {
            PlayerConfig playerConfigDb = (PlayerConfig)null;
            if (OwnerId == 0)
                return playerConfigDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_configs WHERE owner_id=@owner";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        playerConfigDb = new PlayerConfig()
                        {
                            OwnerId = OwnerId,
                            Config = int.Parse(npgsqlDataReader["configs"].ToString()),
                            ShowBlood = int.Parse(npgsqlDataReader["show_blood"].ToString()),
                            Crosshair = int.Parse(npgsqlDataReader["crosshair"].ToString()),
                            HandPosition = int.Parse(npgsqlDataReader["hand_pos"].ToString()),
                            AudioSFX = int.Parse(npgsqlDataReader["audio_sfx"].ToString()),
                            AudioBGM = int.Parse(npgsqlDataReader["audio_bgm"].ToString()),
                            AudioEnable = int.Parse(npgsqlDataReader["audio_enable"].ToString()),
                            Sensitivity = int.Parse(npgsqlDataReader["sensitivity"].ToString()),
                            PointOfView = int.Parse(npgsqlDataReader["pov_size"].ToString()),
                            InvertMouse = int.Parse(npgsqlDataReader["invert_mouse"].ToString()),
                            EnableInviteMsg = int.Parse(npgsqlDataReader["enable_invite"].ToString()),
                            EnableWhisperMsg = int.Parse(npgsqlDataReader["enable_whisper"].ToString()),
                            Macro = int.Parse(npgsqlDataReader["macro_enable"].ToString()),
                            Macro1 = npgsqlDataReader["macro1"].ToString(),
                            Macro2 = npgsqlDataReader["macro2"].ToString(),
                            Macro3 = npgsqlDataReader["macro3"].ToString(),
                            Macro4 = npgsqlDataReader["macro4"].ToString(),
                            Macro5 = npgsqlDataReader["macro5"].ToString(),
                            Nations = int.Parse(npgsqlDataReader["nations"].ToString())
                        };
                        npgsqlDataReader.GetBytes(19, 0, playerConfigDb.KeyboardKeys, 0, 240);
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerConfigDb;
        }

        public static bool CreatePlayerConfigDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_configs(owner_id) VALUES(@owner)";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static PlayerEvent GetPlayerEventDB(long ownerId)
        {
            if (ownerId == 0)
            {
                return null;
            }

            try
            {
                using (var connection = ConnectionSQL.GetInstance().Conn())
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@id", ownerId);
                    command.CommandText = @"SELECT owner_id, last_visit_check_day, last_visit_seq_type, last_visit_date, last_xmas_date, last_playtime_date, last_playtime_value, last_playtime_finish, last_login_date, last_quest_date, last_quest_finish, current_playtime_event_id,  playtime_completed_levels, last_visit_reward_day FROM player_events WHERE owner_id = @id";

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new PlayerEvent
                            {
                                OwnerId = reader.GetInt64(0),
                                LastVisitCheckDay = reader.GetInt32(1),
                                LastVisitSeqType = reader.GetInt32(2),
                                LastVisitDate = (uint)reader.GetInt64(3), // int8 -> uint requiere cast
                                LastXmasDate = (uint)reader.GetInt64(4),
                                LastPlaytimeDate = (uint)reader.GetInt64(5),
                                LastPlaytimeValue = reader.GetInt32(6), // Es int4, no int8
                                LastPlaytimeFinish = reader.GetInt32(7),
                                LastLoginDate = (uint)reader.GetInt64(8),
                                LastQuestDate = (uint)reader.GetInt64(9),
                                LastQuestFinish = reader.GetInt32(10),
                                CurrentPlaytimeEventId = reader.GetInt32(11),
                                PlaytimeCompletedLevels = reader.GetString(12),
                                LastVisitRewardDay = reader.GetInt32(13)
                            };
                        }
                    }
                }

                return null; // No existe el registro
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error al obtener PlayerEvent para OwnerId {ownerId}: {ex.Message}",
                              LoggerType.Error, ex);
                return null;
            }
        }

        public static bool CreatePlayerEventDB(long OwnerId)
        {
            if (OwnerId == 0)
            {
                return false;
            }
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_events (owner_id) VALUES (@id)";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static List<FriendModel> GetPlayerFriendsDB(long OwnerId)
        {
            List<FriendModel> playerFriendsDb = new List<FriendModel>();
            if (OwnerId == 0)
            {
                return null;
            }
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_friends WHERE owner_id=@owner ORDER BY id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        FriendModel friendModel = new FriendModel(long.Parse(npgsqlDataReader["id"].ToString()))
                        {
                            OwnerId = OwnerId,
                            ObjectId = long.Parse(npgsqlDataReader["object_id"].ToString()),
                            State = int.Parse(npgsqlDataReader["state"].ToString()),
                            Removed = bool.Parse(npgsqlDataReader["removed"].ToString())
                        };
                        playerFriendsDb.Add(friendModel);
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return playerFriendsDb;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static void UpdatePlayerBonus(long PlayerId, int Bonuses, int FreePass)
        {
            if (PlayerId == 0)
            {
                return;
            }
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", (object)PlayerId);
                    command.Parameters.AddWithValue("@bonuses", (object)Bonuses);
                    command.Parameters.AddWithValue("@freepass", (object)FreePass);
                    command.CommandText = "UPDATE player_bonus SET bonuses=@bonuses, free_pass=@freepass WHERE owner_id=@id";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public static List<QuickstartModel> GetPlayerQuickstartsDB(long OwnerId)
        {
            List<QuickstartModel> playerQuickstartsDb = new List<QuickstartModel>();
            if (OwnerId == 0)
                return playerQuickstartsDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_quickstarts WHERE owner_id=@owner;";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        QuickstartModel quickstartModel1 = new QuickstartModel()
                        {
                            MapId = (int)byte.Parse(npgsqlDataReader["list0_map_id"].ToString()),
                            Rule = (int)byte.Parse(npgsqlDataReader["list0_map_rule"].ToString()),
                            StageOptions = (int)byte.Parse(npgsqlDataReader["list0_map_stage"].ToString()),
                            Type = (int)byte.Parse(npgsqlDataReader["list0_map_type"].ToString())
                        };
                        playerQuickstartsDb.Add(quickstartModel1);
                        QuickstartModel quickstartModel2 = new QuickstartModel()
                        {
                            MapId = (int)byte.Parse(npgsqlDataReader["list1_map_id"].ToString()),
                            Rule = (int)byte.Parse(npgsqlDataReader["list1_map_rule"].ToString()),
                            StageOptions = (int)byte.Parse(npgsqlDataReader["list1_map_stage"].ToString()),
                            Type = (int)byte.Parse(npgsqlDataReader["list1_map_type"].ToString())
                        };
                        playerQuickstartsDb.Add(quickstartModel2);
                        QuickstartModel quickstartModel3 = new QuickstartModel()
                        {
                            MapId = (int)byte.Parse(npgsqlDataReader["list2_map_id"].ToString()),
                            Rule = (int)byte.Parse(npgsqlDataReader["list2_map_rule"].ToString()),
                            StageOptions = (int)byte.Parse(npgsqlDataReader["list2_map_stage"].ToString()),
                            Type = (int)byte.Parse(npgsqlDataReader["list2_map_type"].ToString())
                        };
                        playerQuickstartsDb.Add(quickstartModel3);
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerQuickstartsDb;
        }

        public static bool CreatePlayerQuickstartsDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_quickstarts(owner_id) VALUES(@owner);";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool IsPlayerNameExist(string Nickname)
        {
            if (string.IsNullOrEmpty(Nickname))
                return true;
            try
            {
                int num = 0;
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@name", (object)Nickname);
                    command.CommandText = "SELECT COUNT(*) FROM accounts WHERE nickname=@name";
                    num = Convert.ToInt32(command.ExecuteScalar());
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return num > 0;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static List<NHistoryModel> GetPlayerNickHistory(object Value, int Type)
        {
            List<NHistoryModel> playerNickHistory = new List<NHistoryModel>();
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    string str = Type == 0 ? "WHERE new_nick=@valor" : "WHERE owner_id=@valor";
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@valor", Value);
                    command.CommandText = $"SELECT * FROM base_nick_history {str} ORDER BY change_date LIMIT 30";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        NHistoryModel nhistoryModel = new NHistoryModel()
                        {
                            ObjectId = long.Parse(npgsqlDataReader["object_id"].ToString()),
                            OwnerId = long.Parse(npgsqlDataReader["owner_id"].ToString()),
                            OldNick = npgsqlDataReader["old_nick"].ToString(),
                            NewNick = npgsqlDataReader["new_nick"].ToString(),
                            ChangeDate = uint.Parse(npgsqlDataReader["change_date"].ToString()),
                            Motive = npgsqlDataReader["motive"].ToString()
                        };
                        playerNickHistory.Add(nhistoryModel);
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerNickHistory;
        }

        public static bool CreatePlayerNickHistory(
          long OwnerId,
          string OldNick,
          string NewNick,
          string Motive)
        {
            NHistoryModel nhistoryModel = new NHistoryModel()
            {
                OwnerId = OwnerId,
                OldNick = OldNick,
                NewNick = NewNick,
                ChangeDate = uint.Parse(DateTimeUtil.Now("yyMMddHHmm")),
                Motive = Motive
            };
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)nhistoryModel.OwnerId);
                    command.Parameters.AddWithValue("@oldnick", (object)nhistoryModel.OldNick);
                    command.Parameters.AddWithValue("@newnick", (object)nhistoryModel.NewNick);
                    command.Parameters.AddWithValue("@date", (object)(long)nhistoryModel.ChangeDate);
                    command.Parameters.AddWithValue("@motive", (object)nhistoryModel.Motive);
                    command.CommandType = CommandType.Text;
                    command.CommandText = "INSERT INTO base_nick_history(owner_id, old_nick, new_nick, change_date, motive) VALUES(@owner, @oldnick, @newnick, @date, @motive)";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool UpdateAccountValuable(long PlayerId, int Gold, int Cash, int Tags)
        {
            if (PlayerId == 0 || Gold == -1 && Cash == -1 && Tags == -1)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@owner", (object)PlayerId);
                    string str = "";
                    if (Gold > -1)
                    {
                        command.Parameters.AddWithValue("@gold", (object)Gold);
                        str += "gold=@gold";
                    }
                    if (Cash > -1)
                    {
                        command.Parameters.AddWithValue("@cash", (object)Cash);
                        str = $"{str}{(str != "" ? ", " : "")}cash=@cash";
                    }
                    if (Tags > -1)
                    {
                        command.Parameters.AddWithValue("@tags", (object)Tags);
                        str = $"{str}{(str != "" ? ", " : "")}tags=@tags";
                    }
                    command.CommandText = $"UPDATE accounts SET {str} WHERE player_id=@owner";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool UpdatePlayerKD(
          long OwnerId,
          int Kills,
          int Deaths,
          int Headshots,
          int Totals)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.Parameters.AddWithValue("@deaths", (object)Deaths);
                    command.Parameters.AddWithValue("@kills", (object)Kills);
                    command.Parameters.AddWithValue("@hs", (object)Headshots);
                    command.Parameters.AddWithValue("@total", (object)Totals);
                    command.CommandText = "UPDATE player_stat_seasons SET kills_count=@kills, deaths_count=@deaths, headshots_count=@hs, total_kills=@total WHERE owner_id=@owner";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool UpdatePlayerMatches(
          int Matches,
          int MatchWins,
          int MatchLoses,
          int MatchDraws,
          int Totals,
          long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.Parameters.AddWithValue("@partidas", (object)Matches);
                    command.Parameters.AddWithValue("@ganhas", (object)MatchWins);
                    command.Parameters.AddWithValue("@perdidas", (object)MatchLoses);
                    command.Parameters.AddWithValue("@empates", (object)MatchDraws);
                    command.Parameters.AddWithValue("@todaspartidas", (object)Totals);
                    command.CommandText = "UPDATE player_stat_seasons SET matches=@partidas, match_wins=@ganhas, match_loses=@perdidas, match_draws=@empates, total_matches=@todaspartidas WHERE owner_id=@owner";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool UpdateAccountCash(long OwnerId, int Cash)
        {
            if (OwnerId != 0)
            {
                if (Cash != -1)
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            NpgsqlCommand command = npgsqlConnection.CreateCommand();
                            npgsqlConnection.Open();
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@owner", (object)OwnerId);
                            command.Parameters.AddWithValue("@cash", (object)Cash);
                            command.CommandText = "UPDATE accounts SET cash=@cash WHERE player_id=@owner";
                            command.ExecuteNonQuery();
                            command.Dispose();
                            npgsqlConnection.Dispose();
                            npgsqlConnection.Close();
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                        return false;
                    }
                }
            }
            return false;
        }

        public static bool UpdateAccountGold(long OwnerId, int Gold)
        {
            if (OwnerId != 0)
            {
                if (Gold != -1)
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            NpgsqlCommand command = npgsqlConnection.CreateCommand();
                            npgsqlConnection.Open();
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@owner", (object)OwnerId);
                            command.Parameters.AddWithValue("@gold", (object)Gold);
                            command.CommandText = "UPDATE accounts SET gold=@gold WHERE player_id=@owner";
                            command.ExecuteNonQuery();
                            command.Dispose();
                            npgsqlConnection.Dispose();
                            npgsqlConnection.Close();
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                        return false;
                    }
                }
            }
            return false;
        }

        public static bool UpdateAccountTags(long OwnerId, int Tags)
        {
            if (OwnerId != 0)
            {
                if (Tags != -1)
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            NpgsqlCommand command = npgsqlConnection.CreateCommand();
                            npgsqlConnection.Open();
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@owner", (object)OwnerId);
                            command.Parameters.AddWithValue("@tag", (object)Tags);
                            command.CommandText = "UPDATE accounts SET tags=@tag WHERE player_id=@owner";
                            command.ExecuteNonQuery();
                            command.Dispose();
                            npgsqlConnection.Dispose();
                            npgsqlConnection.Close();
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                        return false;
                    }
                }
            }
            return false;
        }

        public static void UpdateCouponEffect(long PlayerId, CouponEffects Effects)
        {
            if (PlayerId == 0)
                return;
            ComDiv.UpdateDB("accounts", "coupon_effect", (object)(long)Effects, "player_id", (object)PlayerId);
        }

        public static int GetRequestClanId(long OwnerId)
        {
            int requestClanId = 0;
            if (OwnerId == 0)
                return requestClanId;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "SELECT clan_id FROM system_clan_invites WHERE player_id=@owner";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    if (npgsqlDataReader.Read())
                        requestClanId = int.Parse(npgsqlDataReader["clan_id"].ToString());
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return requestClanId;
        }

        public static int GetRequestClanCount(int ClanId)
        {
            int requestClanCount = 0;
            if (ClanId == 0)
                return requestClanCount;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@clan", (object)ClanId);
                    command.CommandText = "SELECT COUNT(*) FROM system_clan_invites WHERE clan_id=@clan";
                    requestClanCount = Convert.ToInt32(command.ExecuteScalar());
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return requestClanCount;
        }

        public static List<ClanInvite> GetClanRequestList(int ClanId)
        {
            List<ClanInvite> clanRequestList = new List<ClanInvite>();
            if (ClanId == 0)
                return clanRequestList;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@clan", (object)ClanId);
                    command.CommandText = "SELECT * FROM system_clan_invites WHERE clan_id=@clan";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        ClanInvite clanInvite = new ClanInvite()
                        {
                            Id = ClanId,
                            PlayerId = long.Parse(npgsqlDataReader["player_id"].ToString()),
                            InviteDate = uint.Parse(npgsqlDataReader["invite_date"].ToString()),
                            Text = npgsqlDataReader["text"].ToString()
                        };
                        clanRequestList.Add(clanInvite);
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return clanRequestList;
        }

        public static int GetPlayerMessagesCount(long OwnerId)
        {
            int playerMessagesCount = 0;
            if (OwnerId == 0)
                return playerMessagesCount;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "SELECT COUNT(*) FROM player_messages WHERE owner_id=@owner";
                    playerMessagesCount = Convert.ToInt32(command.ExecuteScalar());
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerMessagesCount;
        }

        public static bool CreatePlayerMessage(long OwnerId, MessageModel Message)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.Parameters.AddWithValue("@sendid", (object)Message.SenderId);
                    command.Parameters.AddWithValue("@clan", (object)Message.ClanId);
                    command.Parameters.AddWithValue("@sendname", (object)Message.SenderName);
                    command.Parameters.AddWithValue("@text", (object)Message.Text);
                    command.Parameters.AddWithValue("@type", (object)Message.Type);
                    command.Parameters.AddWithValue("@state", (object)Message.State);
                    command.Parameters.AddWithValue("@expire", (object)Message.ExpireDate);
                    command.Parameters.AddWithValue("@cb", (object)(int)Message.ClanNote);
                    command.CommandType = CommandType.Text;
                    command.CommandText = "INSERT INTO player_messages(owner_id, sender_id, sender_name, clan_id, clan_note, text, type, state, expire)VALUES(@owner, @sendid, @sendname, @clan, @cb, @text, @type, @state, @expire) RETURNING object_id";
                    object obj = command.ExecuteScalar();
                    Message.ObjectId = (long)obj;
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool DeletePlayerFriend(long friendId, long pId)
        {
            return ComDiv.DeleteDB("player_friends", "id", (object)friendId, "owner_id", (object)pId);
        }

        public static void UpdatePlayerFriendState(long ownerId, FriendModel friend)
        {
            ComDiv.UpdateDB("player_friends", "state", (object)friend.State, "owner_id", (object)ownerId, "id", (object)friend.PlayerId);
        }

        public static void UpdatePlayerFriendBlock(long OwnerId, FriendModel Friend)
        {
            ComDiv.UpdateDB("player_friends", "removed", (object)Friend.Removed, "owner_id", (object)OwnerId, "id", (object)Friend.PlayerId);
        }

        public static bool DeleteClanInviteDB(int ClanId, long PlayerId)
        {
            return PlayerId != 0 && ClanId != 0 && ComDiv.DeleteDB("system_clan_invites", "clan_id", (object)ClanId, "player_id", (object)PlayerId);
        }

        public static bool DeleteClanInviteDB(long PlayerId)
        {
            return PlayerId != 0 && ComDiv.DeleteDB("system_clan_invites", "player_id", (object)PlayerId);
        }

        public static bool CreateClanInviteInDB(ClanInvite invite)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@clan", (object)invite.Id);
                    command.Parameters.AddWithValue("@player", (object)invite.PlayerId);
                    command.Parameters.AddWithValue("@date", (object)(long)invite.InviteDate);
                    command.Parameters.AddWithValue("@text", (object)invite.Text);
                    command.CommandText = "INSERT INTO system_clan_invites(clan_id, player_id, invite_date, text)VALUES(@clan,@player,@date,@text)";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static int GetRequestClanInviteCount(int clanId)
        {
            int requestClanInviteCount = 0;
            if (clanId == 0)
                return requestClanInviteCount;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@clan", (object)clanId);
                    command.CommandText = "SELECT COUNT(*) FROM system_clan_invites WHERE clan_id=@clan";
                    requestClanInviteCount = Convert.ToInt32(command.ExecuteScalar());
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return requestClanInviteCount;
        }

        public static string GetRequestClanInviteText(int ClanId, long PlayerId)
        {
            string requestClanInviteText = (string)null;
            if (ClanId != 0)
            {
                if (PlayerId != 0)
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            NpgsqlCommand command = npgsqlConnection.CreateCommand();
                            npgsqlConnection.Open();
                            command.Parameters.AddWithValue("@clan", (object)ClanId);
                            command.Parameters.AddWithValue("@player", (object)PlayerId);
                            command.CommandText = "SELECT text FROM system_clan_invites WHERE clan_id=@clan AND player_id=@player";
                            command.CommandType = CommandType.Text;
                            NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                            if (npgsqlDataReader.Read())
                                requestClanInviteText = npgsqlDataReader["text"].ToString();
                            command.Dispose();
                            npgsqlDataReader.Close();
                            npgsqlConnection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                    }
                    return requestClanInviteText;
                }
            }
            return requestClanInviteText;
        }

        public static string GetPlayerIP4Address(long PlayerId)
        {
            string playerIp4Address = "";
            if (PlayerId == 0)
                return playerIp4Address;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@player", (object)PlayerId);
                    command.CommandText = "SELECT ip4_address FROM accounts WHERE player_id=@player";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    if (npgsqlDataReader.Read())
                        playerIp4Address = npgsqlDataReader["ip4_address"].ToString();
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerIp4Address;
        }

        public static PlayerMissions GetPlayerMissionsDB(long OwnerId)
        {
            if (OwnerId == 0)
                return null;
            PlayerMissions missions = null;
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();

                    NpgsqlCommand head = conn.CreateCommand();
                    head.Parameters.AddWithValue("@owner", (object)OwnerId);
                    head.CommandText = "SELECT current_mission FROM player_missions WHERE owner_id=@owner";
                    head.CommandType = CommandType.Text;
                    object cur = head.ExecuteScalar();
                    head.Dispose();
                    if (cur == null)
                        return null;   // no player_missions row -> caller creates one

                    missions = new PlayerMissions
                    {
                        OwnerId = OwnerId,
                        ActualMission = Convert.ToInt32(cur)
                    };

                    NpgsqlCommand slotsCmd = conn.CreateCommand();
                    slotsCmd.Parameters.AddWithValue("@owner", (object)OwnerId);
                    slotsCmd.CommandText =
                        "SELECT slot, card_set_id, current_card, progress FROM player_mission_slots WHERE owner_id=@owner";
                    slotsCmd.CommandType = CommandType.Text;
                    using (NpgsqlDataReader reader = slotsCmd.ExecuteReader(CommandBehavior.Default))
                    {
                        while (reader.Read())
                        {
                            int slot = Convert.ToInt32(reader["slot"]);
                            if (slot < 0 || slot >= PlayerMissions.SlotCount)
                                continue;
                            MissionSlot target = missions[slot];
                            target.CardSetId = Convert.ToInt32(reader["card_set_id"]);
                            target.CurrentCard = Convert.ToInt32(reader["current_card"]);
                            target.Progress = MissionSlot.NormalizeProgress(reader["progress"] as byte[]);
                        }
                    }
                    slotsCmd.Dispose();
                    conn.Close();
                }

                missions.NormalizeActiveSlot();
                missions.UpdateSelectedCard();
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return missions;
        }

        public static bool CreatePlayerMissionsDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_missions(owner_id) VALUES(@owner)";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool UpsertPlayerMissionSlot(long ownerId, int slot, int cardSetId)
        {
            if (ownerId == 0 || slot < 0 || slot >= PlayerMissions.SlotCount)
                return false;
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand cmd = conn.CreateCommand();
                    conn.Open();
                    cmd.Parameters.AddWithValue("@owner", (object)ownerId);
                    cmd.Parameters.AddWithValue("@slot", (object)(short)slot);
                    cmd.Parameters.AddWithValue("@cs", (object)cardSetId);
                    cmd.Parameters.AddWithValue("@prog", (object)new byte[MissionSlot.ProgressSize]);
                    cmd.CommandText =
                        "INSERT INTO player_mission_slots(owner_id, slot, card_set_id, current_card, progress, acquired_at) " +
                        "VALUES(@owner, @slot, @cs, 0, @prog, now()) " +
                        "ON CONFLICT (owner_id, slot) DO UPDATE SET card_set_id=EXCLUDED.card_set_id, " +
                        "current_card=0, progress=EXCLUDED.progress, acquired_at=now()";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool DeletePlayerMissionSlot(long ownerId, int slot)
        {
            if (ownerId == 0 || slot < 0 || slot >= PlayerMissions.SlotCount)
                return false;
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand cmd = conn.CreateCommand();
                    conn.Open();
                    cmd.Parameters.AddWithValue("@owner", (object)ownerId);
                    cmd.Parameters.AddWithValue("@slot", (object)(short)slot);
                    cmd.CommandText = "DELETE FROM player_mission_slots WHERE owner_id=@owner AND slot=@slot";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        private static bool UpdateSlotColumn(long ownerId, int slot, string column, object value)
        {
            if (ownerId == 0 || slot < 0 || slot >= PlayerMissions.SlotCount)
                return false;
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand cmd = conn.CreateCommand();
                    conn.Open();
                    cmd.Parameters.AddWithValue("@owner", (object)ownerId);
                    cmd.Parameters.AddWithValue("@slot", (object)(short)slot);
                    cmd.Parameters.AddWithValue("@val", value);
                    cmd.CommandText = "UPDATE player_mission_slots SET " + column + "=@val WHERE owner_id=@owner AND slot=@slot";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool UpdatePlayerMissionSlotCard(long ownerId, int slot, int card)
        {
            return UpdateSlotColumn(ownerId, slot, "current_card", (object)card);
        }

        public static bool UpdatePlayerMissionSlotProgress(long ownerId, int slot, byte[] progress)
        {
            return UpdateSlotColumn(ownerId, slot, "progress", (object)MissionSlot.NormalizeProgress(progress));
        }

        public static bool UpdatePlayerActiveMissionSlot(long ownerId, int slot)
        {
            if (ownerId == 0)
                return false;
            return ComDiv.UpdateDB("player_missions", "current_mission", (object)slot, "owner_id", (object)ownerId);
        }

        public static bool DeletePlayerCharacter(long ObjectId, long OwnerId)
        {
            return ObjectId != 0 && OwnerId != 0 && ComDiv.DeleteDB("player_characters", "object_id", (object)ObjectId, "owner_id", (object)OwnerId);
        }

        public static bool UpdatePlayerCharacter(int Slot, long ObjectId, long OwnerId)
        {
            return ComDiv.UpdateDB("player_characters", "slot", (object)Slot, "object_id", (object)ObjectId, "owner_id", (object)OwnerId);
        }

        public static bool UpdateEquipedPlayerTitle(long player_id, int index, int titleId)
        {
            return ComDiv.UpdateDB("player_titles", $"equip_slot{index + 1}", (object)titleId, "owner_id", (object)player_id);
        }

        public static void UpdatePlayerTitlesFlags(long player_id, long flags)
        {
            ComDiv.UpdateDB("player_titles", nameof(flags), (object)flags, "owner_id", (object)player_id);
        }

        public static void UpdatePlayerTitleRequi(
          long player_id,
          int medalhas,
          int insignias,
          int ordens_azuis,
          int broche)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@pid", (object)player_id);
                    command.Parameters.AddWithValue("@broche", (object)broche);
                    command.Parameters.AddWithValue("@insignias", (object)insignias);
                    command.Parameters.AddWithValue("@medalhas", (object)medalhas);
                    command.Parameters.AddWithValue("@ordensazuis", (object)ordens_azuis);
                    command.CommandType = CommandType.Text;
                    command.CommandText = "UPDATE accounts SET ribbon=@broche, ensign=@insignias, medal=@medalhas, master_medal=@ordensazuis WHERE player_id=@pid";
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public static int GetUsedTicket(long OwnerId, string Token)
        {
            int usedTicket = 0;
            if (OwnerId != 0)
            {
                if (!string.IsNullOrEmpty(Token))
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            NpgsqlCommand command = npgsqlConnection.CreateCommand();
                            npgsqlConnection.Open();
                            command.Parameters.AddWithValue("@player", (object)OwnerId);
                            command.Parameters.AddWithValue("@token", (object)Token);
                            command.CommandText = "SELECT used_count FROM base_redeem_history WHERE used_token=@token AND owner_id=@player";
                            command.CommandType = CommandType.Text;
                            NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                            if (npgsqlDataReader.Read())
                                usedTicket = int.Parse(npgsqlDataReader["used_count"].ToString());
                            command.Dispose();
                            npgsqlDataReader.Close();
                            npgsqlConnection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                    }
                    return usedTicket;
                }
            }
            return usedTicket;
        }

        public static bool IsTicketUsedByPlayer(long OwnerId, string Token)
        {
            bool flag = false;
            if (OwnerId == 0)
                return flag;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@player", (object)OwnerId);
                    command.Parameters.AddWithValue("@token", (object)Token);
                    command.CommandText = "SELECT * FROM base_redeem_history WHERE used_token=@token AND owner_id=@player";
                    command.CommandType = CommandType.Text;
                    flag = Convert.ToBoolean(command.ExecuteScalar());
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return flag;
        }

        public static bool CreatePlayerRedeemHistory(long OwnerId, string Token, int Used)
        {
            if (OwnerId != 0 && !string.IsNullOrEmpty(Token))
            {
                if (Used != 0)
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            NpgsqlCommand command = npgsqlConnection.CreateCommand();
                            npgsqlConnection.Open();
                            command.Parameters.AddWithValue("@owner", (object)OwnerId);
                            command.Parameters.AddWithValue("@token", (object)Token);
                            command.Parameters.AddWithValue("@used", (object)Used);
                            command.CommandText = "INSERT INTO base_redeem_history(owner_id, used_token, used_count) VALUES(@owner, @token, @used)";
                            command.CommandType = CommandType.Text;
                            command.ExecuteNonQuery();
                            command.Dispose();
                            npgsqlConnection.Dispose();
                            npgsqlConnection.Close();
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                        return false;
                    }
                }
            }
            return false;
        }

        public static PlayerVip GetPlayerVIP(long OwnerId)
        {
            PlayerVip playerVip = (PlayerVip)null;
            if (OwnerId == 0)
                return playerVip;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@ownerId", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_vip WHERE owner_id=@ownerId";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    if (npgsqlDataReader.Read())
                        playerVip = new PlayerVip()
                        {
                            OwnerId = OwnerId,
                            Address = npgsqlDataReader["registered_ip"].ToString(),
                            Benefit = npgsqlDataReader["last_benefit"].ToString(),
                            Expirate = uint.Parse(npgsqlDataReader["expirate"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerVip;
        }

        public static PlayerReport GetPlayerReportDB(long OwnerId)
        {
            PlayerReport playerReportDb = (PlayerReport)null;
            if (OwnerId == 0)
                return playerReportDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        npgsqlConnection.Open();
                        command.Parameters.AddWithValue("@owner", (object)OwnerId);
                        command.CommandText = "SELECT * FROM player_reports WHERE owner_id=@owner";
                        command.CommandType = CommandType.Text;
                        using (NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default))
                        {
                            while (npgsqlDataReader.Read())
                                playerReportDb = new PlayerReport()
                                {
                                    OwnerId = OwnerId,
                                    TicketCount = int.Parse(npgsqlDataReader["ticket_count"].ToString()),
                                    ReportedCount = int.Parse(npgsqlDataReader["reported_count"].ToString())
                                };
                            npgsqlDataReader.Close();
                            npgsqlConnection.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerReportDb;
        }

        public static bool CreatePlayerReportDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        npgsqlConnection.Open();
                        command.Parameters.AddWithValue("@owner", (object)OwnerId);
                        command.CommandText = "INSERT INTO player_reports(owner_id) VALUES(@owner)";
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                        command.Dispose();
                        npgsqlConnection.Dispose();
                        npgsqlConnection.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool CreatePlayerReportHistory(
          long OwnerId,
          long SenderId,
          string OwnerNick,
          string SenderNick,
          ReportType Type,
          string Message)
        {
            RHistoryModel rhistoryModel = new RHistoryModel()
            {
                OwnerId = OwnerId,
                OwnerNick = OwnerNick,
                SenderId = SenderId,
                SenderNick = SenderNick,
                Date = uint.Parse(DateTimeUtil.Now("yyMMddHHmm")),
                Type = Type,
                Message = Message
            };
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        npgsqlConnection.Open();
                        command.Parameters.AddWithValue("@OwnerId", (object)rhistoryModel.OwnerId);
                        command.Parameters.AddWithValue("@OwnerNick", (object)rhistoryModel.OwnerNick);
                        command.Parameters.AddWithValue("@SenderId", (object)rhistoryModel.SenderId);
                        command.Parameters.AddWithValue("@SenderNick", (object)rhistoryModel.SenderNick);
                        command.Parameters.AddWithValue("@Date", (object)(long)rhistoryModel.Date);
                        command.Parameters.AddWithValue("@Type", (object)(int)rhistoryModel.Type);
                        command.Parameters.AddWithValue("@Message", (object)rhistoryModel.Message);
                        command.CommandText = "INSERT INTO base_report_history(date, owner_id, owner_nick, sender_id, sender_nick, type, message) VALUES(@Date, @OwnerId, @OwnerNick, @SenderId, @SenderNick, @Type, @Message)";
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                        command.Dispose();
                        npgsqlConnection.Dispose();
                        npgsqlConnection.Close();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static PlayerBattlepass GetPlayerBattlepassDB(long OwnerId)
        {
            PlayerBattlepass Battlepass = null;
            if (OwnerId == 0)
            {
                return Battlepass;
            }
            try
            {
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.Parameters.AddWithValue("@id", OwnerId);
                    Command.CommandText = "SELECT * FROM player_battlepass WHERE owner_id=@id";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Data = Command.ExecuteReader();
                    while (Data.Read())
                    {
                        Battlepass = new PlayerBattlepass()
                        {
                            BattlepassId = Data["battlepass_id"] != DBNull.Value ? int.Parse(Data["battlepass_id"].ToString()) : 0,
                            BattlepassPremiumLevel = Data["battlepass_premium_levels"] != DBNull.Value ? int.Parse(Data["battlepass_premium_levels"].ToString()) : 0,
                            BattlepassNormalLevel = Data["battlepass_normal_levels"] != DBNull.Value ? int.Parse(Data["battlepass_normal_levels"].ToString()) : 0,
                            HavePremium = Data["battlepass_premium"] != DBNull.Value ? bool.Parse(Data["battlepass_premium"].ToString()) : false,
                            EarnedPoints = Data["earned_points"] != DBNull.Value ? int.Parse(Data["earned_points"].ToString()) : 0,
                            DailyPoints = Data["points"] != DBNull.Value ? int.Parse(Data["points"].ToString()) : 0,
                            LastRecord = Data["last_record"] != DBNull.Value ? uint.Parse(Data["last_record"].ToString()) : 0
                        };
                    }
                    Command.Dispose();
                    Data.Close();
                    Connection.Dispose();
                    Connection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return Battlepass;
        }

        public static PlayerCompetitive GetPlayerCompetitiveDB(long OwnerId)
        {
            PlayerCompetitive playerCompetitiveDb = (PlayerCompetitive)null;
            if (OwnerId == 0)
                return playerCompetitiveDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@id", (object)OwnerId);
                    command.CommandText = "SELECT * FROM player_competitive WHERE owner_id=@id";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                        playerCompetitiveDb = new PlayerCompetitive()
                        {
                            OwnerId = OwnerId,
                            Level = int.Parse(npgsqlDataReader["level"].ToString()),
                            Points = int.Parse(npgsqlDataReader["points"].ToString())
                        };
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return playerCompetitiveDb;
        }

        public static bool CreatePlayerBattlepassDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_battlepass VALUES(@owner);";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool CreatePlayerCompetitiveDB(long OwnerId)
        {
            if (OwnerId == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@owner", (object)OwnerId);
                    command.CommandText = "INSERT INTO player_competitive VALUES(@owner);";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                    command.Dispose();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static List<SChannelModel> GetSystemServers()
        {
            try
            {
                List<SChannelModel> Servers = new List<SChannelModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, state, host, port, type, is_mobile, max_players, channel_players FROM system_servers ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        SChannelModel Server = new SChannelModel(Reader["host"].ToString(), ushort.Parse(Reader["port"].ToString()))
                        {
                            Id = int.Parse(Reader["id"].ToString()),
                            State = bool.Parse(Reader["state"].ToString()),
                            Type = ComDiv.ParseEnum<SChannelType>(Reader["type"].ToString()),
                            IsMobile = bool.Parse(Reader["is_mobile"].ToString()),
                            MaxPlayers = int.Parse(Reader["max_players"].ToString()),
                            ChannelPlayers = int.Parse(Reader["channel_players"].ToString())
                        };
                        Servers.Add(Server);
                    }
                    Command.Dispose();
                    Reader.Close();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Servers;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<ChannelRow> GetSystemChannels()
        {
            try
            {
                List<ChannelRow> Channels = new List<ChannelRow>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT server_id, id, type, max_rooms, exp_bonus, gold_bonus, cash_bonus, password FROM system_channels ORDER BY server_id ASC, id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        ChannelRow Channel = new ChannelRow
                        {
                            ServerId = int.Parse(Reader["server_id"].ToString()),
                            Id = int.Parse(Reader["id"].ToString()),
                            Type = Reader["type"].ToString(),
                            MaxRooms = int.Parse(Reader["max_rooms"].ToString()),
                            ExpBonus = int.Parse(Reader["exp_bonus"].ToString()),
                            GoldBonus = int.Parse(Reader["gold_bonus"].ToString()),
                            CashBonus = int.Parse(Reader["cash_bonus"].ToString()),
                            Password = Reader["password"] == DBNull.Value ? null : Reader["password"].ToString()
                        };
                        Channels.Add(Channel);
                    }
                    Command.Dispose();
                    Reader.Close();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Channels;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<ChannelTypeConditionRow> GetSystemChannelTypeConditions()
        {
            try
            {
                List<ChannelTypeConditionRow> Conditions = new List<ChannelTypeConditionRow>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT channel_type, min_value, max_value, enabled FROM system_channel_type_conditions ORDER BY channel_type ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        ChannelTypeConditionRow Condition = new ChannelTypeConditionRow
                        {
                            ChannelType = int.Parse(Reader["channel_type"].ToString()),
                            MinValue = int.Parse(Reader["min_value"].ToString()),
                            MaxValue = int.Parse(Reader["max_value"].ToString()),
                            Enabled = bool.Parse(Reader["enabled"].ToString())
                        };
                        Conditions.Add(Condition);
                    }
                    Command.Dispose();
                    Reader.Close();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Conditions;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<TicketModel> GetRedeemTickets()
        {
            try
            {
                Dictionary<string, TicketModel> ByKey = new Dictionary<string, TicketModel>();
                List<TicketModel> Tickets = new List<TicketModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT token, type, ticket_count, player_ration, gold_reward, cash_reward, tags_reward FROM system_redeem_codes ORDER BY token ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        TicketModel Ticket = new TicketModel
                        {
                            Token = Reader["token"].ToString(),
                            Type = ComDiv.ParseEnum<TicketType>(Reader["type"].ToString()),
                            TicketCount = uint.Parse(Reader["ticket_count"].ToString()),
                            PlayerRation = uint.Parse(Reader["player_ration"].ToString()),
                            GoldReward = int.Parse(Reader["gold_reward"].ToString()),
                            CashReward = int.Parse(Reader["cash_reward"].ToString()),
                            TagsReward = int.Parse(Reader["tags_reward"].ToString()),
                            Rewards = new List<int>()
                        };
                        ByKey[Ticket.Token + "|" + Ticket.Type] = Ticket;
                        Tickets.Add(Ticket);
                    }
                    Reader.Close();
                    Command.Dispose();

                    Command = Connection.CreateCommand();
                    Command.CommandText = "SELECT token, type, good_id FROM system_redeem_code_rewards ORDER BY token ASC, ordinal ASC;";
                    Command.CommandType = CommandType.Text;
                    Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        string Key = Reader["token"].ToString() + "|" + Reader["type"].ToString();
                        TicketModel Ticket;
                        if (ByKey.TryGetValue(Key, out Ticket))
                        {
                            Ticket.Rewards.Add(int.Parse(Reader["good_id"].ToString()));
                        }
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Tickets;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static SortedList<int, string> GetPermissionNames()
        {
            try
            {
                SortedList<int, string> permissoes = new SortedList<int, string>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT key, name FROM system_permissions ORDER BY key ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        int Key = int.Parse(Reader["key"].ToString());
                        if (!permissoes.ContainsKey(Key))
                        {
                            permissoes.Add(Key, Reader["name"].ToString());
                        }
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return permissoes;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static SortedList<int, int> GetAccessLevelFakeRanks()
        {
            try
            {
                SortedList<int, int> Levels = new SortedList<int, int>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT key, fake_rank FROM system_access_levels ORDER BY key ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        int Key = int.Parse(Reader["key"].ToString());
                        if (!Levels.ContainsKey(Key))
                        {
                            Levels.Add(Key, int.Parse(Reader["fake_rank"].ToString()));
                        }
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Levels;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<KeyValuePair<int, int>> GetAccessRights()
        {
            try
            {
                List<KeyValuePair<int, int>> Rights = new List<KeyValuePair<int, int>>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT level_key, permission_key FROM system_access_rights ORDER BY level_key ASC, permission_key ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Rights.Add(new KeyValuePair<int, int>(int.Parse(Reader["level_key"].ToString()), int.Parse(Reader["permission_key"].ToString())));
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Rights;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<Synchronize> GetSyncEndpoints()
        {
            try
            {
                List<Synchronize> Endpoints = new List<Synchronize>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT remote_port, host, port FROM system_sync_endpoints ORDER BY remote_port ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Synchronize Endpoint = new Synchronize(Reader["host"].ToString(), int.Parse(Reader["port"].ToString()))
                        {
                            RemotePort = int.Parse(Reader["remote_port"].ToString())
                        };
                        Endpoints.Add(Endpoint);
                    }
                    Command.Dispose();
                    Reader.Close();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Endpoints;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        /// <summary>
        /// Battle pass seasons with their level cards. Replaces Data/BattlepassInfo.json.
        /// The cards come back in the JSON-shaped CardJson list so ConvertCardsFromSeason and
        /// every consumer of BattlePassSeason keep working untouched.
        /// </summary>
        public static List<Managers.BattlePassSeason> GetBattlePassSeasons()
        {
            try
            {
                Dictionary<int, Managers.BattlePassSeason> ById = new Dictionary<int, Managers.BattlePassSeason>();
                List<Managers.BattlePassSeason> Seasons = new List<Managers.BattlePassSeason>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT season_id, name, description, enabled, enabled_for_free, enabled_for_premium, price, start_date, end_date, max_daily_points FROM system_battlepass_seasons ORDER BY season_id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        int SeasonId = int.Parse(Reader["season_id"].ToString());
                        Managers.BattlePassSeason Season = new Managers.BattlePassSeason
                        {
                            SeasonId = SeasonId.ToString(),
                            SeasonName = Reader["name"] == DBNull.Value ? string.Empty : Reader["name"].ToString(),
                            SeasonDescription = Reader["description"] == DBNull.Value ? string.Empty : Reader["description"].ToString(),
                            SeasonEnabled = int.Parse(Reader["enabled"].ToString()),
                            SeasonEnabledForFree = bool.Parse(Reader["enabled_for_free"].ToString()),
                            SeasonEnabledForPremium = bool.Parse(Reader["enabled_for_premium"].ToString()),
                            SeasonPrice = Reader["price"].ToString(),
                            SeasonStartDate = Reader["start_date"].ToString(),
                            SeasonEndDate = Reader["end_date"].ToString(),
                            MaxDailyPoints = int.Parse(Reader["max_daily_points"].ToString()),
                            SeasonExpValues = new Managers.SeasonExpValuesJson { Card = new List<Managers.CardJson>() }
                        };
                        ById[SeasonId] = Season;
                        Seasons.Add(Season);
                    }
                    Reader.Close();
                    Command.Dispose();

                    Command = Connection.CreateCommand();
                    Command.CommandText = "SELECT season_id, card_number, required_exp, normal_card, premium_card_a, premium_card_b FROM system_battlepass_cards ORDER BY season_id ASC, card_number ASC;";
                    Command.CommandType = CommandType.Text;
                    Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Managers.BattlePassSeason Season;
                        if (ById.TryGetValue(int.Parse(Reader["season_id"].ToString()), out Season))
                        {
                            Season.SeasonExpValues.Card.Add(new Managers.CardJson
                            {
                                _Number = Reader["card_number"].ToString(),
                                RequiredExp = Reader["required_exp"].ToString(),
                                NormalCard = Reader["normal_card"].ToString(),
                                PremiumCardA = Reader["premium_card_a"].ToString(),
                                PremiumCardB = Reader["premium_card_b"].ToString()
                            });
                        }
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Seasons;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<RankModel> GetPlayerRanks()
        {
            try
            {
                Dictionary<int, RankModel> ById = new Dictionary<int, RankModel>();
                List<RankModel> Ranks = new List<RankModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, title, on_next_level, on_gold_up, on_all_exp FROM system_player_ranks ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        RankModel Rank = new RankModel(int.Parse(Reader["id"].ToString()))
                        {
                            Title = Reader["title"] == DBNull.Value ? string.Empty : Reader["title"].ToString(),
                            OnNextLevel = int.Parse(Reader["on_next_level"].ToString()),
                            OnGoldUp = int.Parse(Reader["on_gold_up"].ToString()),
                            OnAllExp = int.Parse(Reader["on_all_exp"].ToString()),
                            Rewards = new List<int>()
                        };
                        ById[Rank.Id] = Rank;
                        Ranks.Add(Rank);
                    }
                    Reader.Close();
                    Command.Dispose();

                    Command = Connection.CreateCommand();
                    Command.CommandText = "SELECT rank_id, good_id FROM system_player_rank_rewards ORDER BY rank_id ASC, ordinal ASC;";
                    Command.CommandType = CommandType.Text;
                    Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        RankModel Rank;
                        if (ById.TryGetValue(int.Parse(Reader["rank_id"].ToString()), out Rank))
                        {
                            Rank.Rewards.Add(int.Parse(Reader["good_id"].ToString()));
                        }
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Ranks;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<RankModel> GetClanRanks()
        {
            try
            {
                List<RankModel> Ranks = new List<RankModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, title, on_next_level, on_all_exp FROM system_clan_ranks ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        RankModel Rank = new RankModel(int.Parse(Reader["id"].ToString()))
                        {
                            Title = Reader["title"] == DBNull.Value ? string.Empty : Reader["title"].ToString(),
                            OnNextLevel = int.Parse(Reader["on_next_level"].ToString()),
                            OnGoldUp = 0,
                            OnAllExp = int.Parse(Reader["on_all_exp"].ToString()),
                            Rewards = new List<int>()
                        };
                        Ranks.Add(Rank);
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Ranks;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<MissionStore> GetMissionStores()
        {
            try
            {
                List<MissionStore> Missions = new List<MissionStore>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, item_id, enabled FROM system_mission_stores ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Missions.Add(new MissionStore
                        {
                            Id = int.Parse(Reader["id"].ToString()),
                            ItemId = int.Parse(Reader["item_id"].ToString()),
                            Enable = bool.Parse(Reader["enabled"].ToString())
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Missions;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<MissionAwards> GetMissionAwards()
        {
            try
            {
                List<MissionAwards> Awards = new List<MissionAwards>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, master_medal, exp, gold FROM system_mission_awards ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Awards.Add(new MissionAwards(
                            int.Parse(Reader["id"].ToString()),
                            int.Parse(Reader["master_medal"].ToString()),
                            int.Parse(Reader["exp"].ToString()),
                            int.Parse(Reader["gold"].ToString())));
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Awards;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<ItemsStatistic> GetItemStatistics()
        {
            try
            {
                List<ItemsStatistic> Stats = new List<ItemsStatistic>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT item_id, name, bullet_loaded, bullet_total, damage, fire_delay, helmet_penetrate, \"range\" FROM system_item_statistics ORDER BY seq ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Stats.Add(new ItemsStatistic
                        {
                            Id = int.Parse(Reader["item_id"].ToString()),
                            Name = Reader["name"].ToString(),
                            BulletLoaded = int.Parse(Reader["bullet_loaded"].ToString()),
                            BulletTotal = int.Parse(Reader["bullet_total"].ToString()),
                            Damage = int.Parse(Reader["damage"].ToString()),
                            FireDelay = Convert.ToSingle(Reader["fire_delay"]),
                            HelmetPenetrate = int.Parse(Reader["helmet_penetrate"].ToString()),
                            Range = Convert.ToSingle(Reader["range"])
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Stats;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<string> GetDirectLibraryHashes()
        {
            try
            {
                List<string> Hashes = new List<string>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT md5 FROM system_direct_libraries ORDER BY seq ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                        Hashes.Add(Reader["md5"].ToString());
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Hashes;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<MapRule> GetMapRules()
        {
            try
            {
                List<MapRule> Rules = new List<MapRule>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT mode_id, rule, stage_options, conditions, name FROM system_map_rules ORDER BY seq ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Rules.Add(new MapRule
                        {
                            Id = int.Parse(Reader["mode_id"].ToString()),
                            Rule = int.Parse(Reader["rule"].ToString()),
                            StageOptions = int.Parse(Reader["stage_options"].ToString()),
                            Conditions = int.Parse(Reader["conditions"].ToString()),
                            Name = Reader["name"].ToString()
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Rules;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<MapMatch> GetMapMatches()
        {
            try
            {
                List<MapMatch> Matches = new List<MapMatch>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT mode_id, map_id, map_limit, tag, name FROM system_map_matches ORDER BY seq ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Matches.Add(new MapMatch(int.Parse(Reader["mode_id"].ToString()))
                        {
                            Id = int.Parse(Reader["map_id"].ToString()),
                            Limit = int.Parse(Reader["map_limit"].ToString()),
                            Tag = int.Parse(Reader["tag"].ToString()),
                            Name = Reader["name"].ToString()
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Matches;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<ItemsModel> GetTemplateItems(string Kind, ItemEquipType Equip, bool StockId)
        {
            try
            {
                List<ItemsModel> Items = new List<ItemsModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT item_id, name, item_count FROM system_template_items WHERE kind = @kind ORDER BY seq ASC;";
                    Command.CommandType = CommandType.Text;
                    Command.Parameters.AddWithValue("@kind", Kind);
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        int ItemId = int.Parse(Reader["item_id"].ToString());
                        Items.Add(new ItemsModel(ItemId)
                        {
                            ObjectId = StockId ? ComDiv.ValidateStockId(ItemId) : 0L,
                            Name = Reader["name"].ToString(),
                            Count = uint.Parse(Reader["item_count"].ToString()),
                            Equip = Equip
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Items;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<PCCafeModel> GetPCCafes()
        {
            try
            {
                Dictionary<string, PCCafeModel> ByType = new Dictionary<string, PCCafeModel>();
                List<PCCafeModel> Cafes = new List<PCCafeModel>();

                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT cafe_type, exp_up, point_up FROM system_pc_cafes ORDER BY cafe_type ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        string Type = Reader["cafe_type"].ToString();
                        if (ByType.ContainsKey(Type))
                            continue;

                        PCCafeModel Cafe = new PCCafeModel(ComDiv.ParseEnum<CafeEnum>(Type))
                        {
                            ExpUp = int.Parse(Reader["exp_up"].ToString()),
                            PointUp = int.Parse(Reader["point_up"].ToString()),
                            Rewards = new SortedList<CafeEnum, List<ItemsModel>>()
                        };
                        ByType.Add(Type, Cafe);
                        Cafes.Add(Cafe);
                    }
                    Reader.Close();
                    Command.Dispose();

                    if (ByType.Count > 0)
                    {
                        NpgsqlCommand RewardCommand = Connection.CreateCommand();
                        RewardCommand.CommandText = "SELECT cafe_type, item_id, name FROM system_pc_cafe_rewards ORDER BY cafe_type ASC, seq ASC;";
                        RewardCommand.CommandType = CommandType.Text;
                        NpgsqlDataReader RewardReader = RewardCommand.ExecuteReader(CommandBehavior.Default);
                        while (RewardReader.Read())
                        {
                            string Type = RewardReader["cafe_type"].ToString();
                            if (!ByType.ContainsKey(Type))
                                continue;

                            PCCafeModel Cafe = ByType[Type];
                            int ItemId = int.Parse(RewardReader["item_id"].ToString());
                            ItemsModel Item = new ItemsModel(ItemId)
                            {
                                ObjectId = ComDiv.ValidateStockId(ItemId),
                                Name = RewardReader["name"].ToString(),
                                Count = 1,
                                Equip = ItemEquipType.CafePC
                            };

                            if (Cafe.Rewards.ContainsKey(Cafe.Type))
                                Cafe.Rewards[Cafe.Type].Add(Item);
                            else
                                Cafe.Rewards.Add(Cafe.Type, new List<ItemsModel> { Item });
                        }
                        RewardReader.Close();
                        RewardCommand.Dispose();
                    }

                    Connection.Dispose();
                    Connection.Close();
                }

                return Cafes;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<MissionStreamEntry> GetMissionStreams()
        {
            try
            {
                Dictionary<int, MissionStreamEntry> Streams = new Dictionary<int, MissionStreamEntry>();
                List<MissionStreamEntry> Result = new List<MissionStreamEntry>();

                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT mission_id, name, description FROM system_mission_streams WHERE enabled = TRUE ORDER BY mission_id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        int MissionId = int.Parse(Reader["mission_id"].ToString());
                        if (Streams.ContainsKey(MissionId))
                            continue;

                        MissionStreamEntry Entry = new MissionStreamEntry
                        {
                            Id = MissionId,
                            Name = Reader["name"].ToString(),
                            Description = Reader["description"].ToString(),
                            ObjectivesData = MissionStreamXML.CreateObjectivesBuffer()
                        };
                        Streams.Add(MissionId, Entry);
                        Result.Add(Entry);
                    }
                    Reader.Close();
                    Command.Dispose();

                    if (Streams.Count > 0)
                    {
                        NpgsqlCommand TaskCommand = Connection.CreateCommand();
                        TaskCommand.CommandText = "SELECT mission_id, card_idx, task_idx, req_type, task_type, limit_count, weapon_series, flag6, map_id, extra1, extra2, extra3 FROM system_mission_stream_tasks ORDER BY mission_id ASC, card_idx ASC, task_idx ASC;";
                        TaskCommand.CommandType = CommandType.Text;
                        NpgsqlDataReader TaskReader = TaskCommand.ExecuteReader(CommandBehavior.Default);
                        while (TaskReader.Read())
                        {
                            int MissionId = int.Parse(TaskReader["mission_id"].ToString());
                            if (!Streams.ContainsKey(MissionId))
                                continue;

                            MissionStreamXML.SetTask(
                                Streams[MissionId].ObjectivesData,
                                int.Parse(TaskReader["card_idx"].ToString()),
                                int.Parse(TaskReader["task_idx"].ToString()),
                                int.Parse(TaskReader["req_type"].ToString()),
                                int.Parse(TaskReader["task_type"].ToString()),
                                int.Parse(TaskReader["limit_count"].ToString()),
                                int.Parse(TaskReader["weapon_series"].ToString()),
                                int.Parse(TaskReader["flag6"].ToString()),
                                uint.Parse(TaskReader["map_id"].ToString()),
                                uint.Parse(TaskReader["extra1"].ToString()),
                                uint.Parse(TaskReader["extra2"].ToString()),
                                uint.Parse(TaskReader["extra3"].ToString()));
                        }
                        TaskReader.Close();
                        TaskCommand.Dispose();

                        NpgsqlCommand RewardCommand = Connection.CreateCommand();
                        RewardCommand.CommandText = "SELECT mission_id, record_idx, price, point, medals, good_id_1, good_count_1, good_id_2, good_count_2, good_id_3, good_count_3, good_id_4, good_count_4, good_id_5, good_count_5 FROM system_mission_stream_rewards ORDER BY mission_id ASC, record_idx ASC;";
                        RewardCommand.CommandType = CommandType.Text;
                        NpgsqlDataReader RewardReader = RewardCommand.ExecuteReader(CommandBehavior.Default);
                        while (RewardReader.Read())
                        {
                            int MissionId = int.Parse(RewardReader["mission_id"].ToString());
                            if (!Streams.ContainsKey(MissionId))
                                continue;

                            int RecordIdx = int.Parse(RewardReader["record_idx"].ToString());
                            uint[] Values = new uint[3 + MissionStreamXML.RewardSlots * 2];
                            Values[0] = uint.Parse(RewardReader["price"].ToString());
                            Values[1] = uint.Parse(RewardReader["point"].ToString());
                            Values[2] = uint.Parse(RewardReader["medals"].ToString());
                            for (int Slot = 0; Slot < MissionStreamXML.RewardSlots; Slot++)
                            {
                                Values[3 + Slot * 2] = uint.Parse(RewardReader[$"good_id_{Slot + 1}"].ToString());
                                Values[4 + Slot * 2] = uint.Parse(RewardReader[$"good_count_{Slot + 1}"].ToString());
                            }

                            MissionStreamXML.SetReward(Streams[MissionId].ObjectivesData, RecordIdx, Values);

                            if (RecordIdx == MissionStreamXML.RewardRecords - 1)
                            {
                                Streams[MissionId].RewardId = (int)Values[3];
                                Streams[MissionId].RewardCount = (int)Values[4];
                            }
                        }
                        RewardReader.Close();
                        RewardCommand.Dispose();
                    }

                    Connection.Dispose();
                    Connection.Close();
                }

                return Result;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static SortedList<int, RandomBoxModel> GetRandomBoxes()
        {
            try
            {
                SortedList<int, RandomBoxModel> Boxes = new SortedList<int, RandomBoxModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT box_id, items_count FROM system_random_boxes ORDER BY box_id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        int BoxId = int.Parse(Reader["box_id"].ToString());
                        if (Boxes.ContainsKey(BoxId))
                            continue;
                        Boxes.Add(BoxId, new RandomBoxModel
                        {
                            ItemsCount = int.Parse(Reader["items_count"].ToString()),
                            Items = new List<RandomBoxItem>()
                        });
                    }
                    Reader.Close();
                    Command.Dispose();

                    if (Boxes.Count > 0)
                    {
                        NpgsqlCommand RewardCommand = Connection.CreateCommand();
                        RewardCommand.CommandText = "SELECT box_id, idx, good_id, percent, is_special FROM system_random_box_rewards ORDER BY box_id ASC, seq ASC;";
                        RewardCommand.CommandType = CommandType.Text;
                        NpgsqlDataReader RewardReader = RewardCommand.ExecuteReader(CommandBehavior.Default);
                        while (RewardReader.Read())
                        {
                            int BoxId = int.Parse(RewardReader["box_id"].ToString());
                            if (!Boxes.ContainsKey(BoxId))
                                continue;
                            Boxes[BoxId].Items.Add(new RandomBoxItem
                            {
                                Index = int.Parse(RewardReader["idx"].ToString()),
                                GoodsId = int.Parse(RewardReader["good_id"].ToString()),
                                Percent = int.Parse(RewardReader["percent"].ToString()),
                                Special = bool.Parse(RewardReader["is_special"].ToString())
                            });
                        }
                        RewardReader.Close();
                        RewardCommand.Dispose();
                    }

                    Connection.Dispose();
                    Connection.Close();
                }

                foreach (KeyValuePair<int, RandomBoxModel> Entry in Boxes)
                    Entry.Value.SetTopPercent();

                return Boxes;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<BattleBoxModel> GetBattleBoxes()
        {
            try
            {
                List<BattleBoxModel> Boxes = new List<BattleBoxModel>();
                Dictionary<int, BattleBoxModel> ByCoupon = new Dictionary<int, BattleBoxModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT coupon_id, require_tags FROM system_battle_boxes ORDER BY coupon_id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        int CouponId = int.Parse(Reader["coupon_id"].ToString());
                        if (ByCoupon.ContainsKey(CouponId))
                            continue;
                        BattleBoxModel Box = new BattleBoxModel
                        {
                            CouponId = CouponId,
                            RequireTags = int.Parse(Reader["require_tags"].ToString()),
                            Items = new List<BattleBoxItem>()
                        };
                        ByCoupon.Add(CouponId, Box);
                        Boxes.Add(Box);
                    }
                    Reader.Close();
                    Command.Dispose();

                    if (Boxes.Count > 0)
                    {
                        NpgsqlCommand RewardCommand = Connection.CreateCommand();
                        RewardCommand.CommandText = "SELECT coupon_id, good_id, percent FROM system_battle_box_rewards ORDER BY coupon_id ASC, seq ASC;";
                        RewardCommand.CommandType = CommandType.Text;
                        NpgsqlDataReader RewardReader = RewardCommand.ExecuteReader(CommandBehavior.Default);
                        while (RewardReader.Read())
                        {
                            int CouponId = int.Parse(RewardReader["coupon_id"].ToString());
                            if (!ByCoupon.ContainsKey(CouponId))
                                continue;
                            ByCoupon[CouponId].Items.Add(new BattleBoxItem
                            {
                                GoodsId = int.Parse(RewardReader["good_id"].ToString()),
                                Percent = int.Parse(RewardReader["percent"].ToString())
                            });
                        }
                        RewardReader.Close();
                        RewardCommand.Dispose();
                    }

                    Connection.Dispose();
                    Connection.Close();
                }

                foreach (BattleBoxModel Box in Boxes)
                    Box.InitItemPercentages();

                return Boxes;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<InternetCafe> GetInternetCafeBonuses()
        {
            try
            {
                List<InternetCafe> Cafes = new List<InternetCafe>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, basic_exp, basic_gold, premium_exp, premium_gold FROM system_internet_cafe_bonuses ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Cafes.Add(new InternetCafe(int.Parse(Reader["id"].ToString()))
                        {
                            BasicExp = int.Parse(Reader["basic_exp"].ToString()),
                            BasicGold = int.Parse(Reader["basic_gold"].ToString()),
                            PremiumExp = int.Parse(Reader["premium_exp"].ToString()),
                            PremiumGold = int.Parse(Reader["premium_gold"].ToString())
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Cafes;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<CompetitiveRank> GetCompetitiveRanks()
        {
            try
            {
                List<CompetitiveRank> Ranks = new List<CompetitiveRank>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, tourney_level, points, name FROM system_competitive_ranks ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Ranks.Add(new CompetitiveRank
                        {
                            Id = int.Parse(Reader["id"].ToString()),
                            TourneyLevel = int.Parse(Reader["tourney_level"].ToString()),
                            Points = int.Parse(Reader["points"].ToString()),
                            Name = Reader["name"].ToString()
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Ranks;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<BattleRewardModel> GetBattleRewards()
        {
            try
            {
                Dictionary<string, BattleRewardModel> ByType = new Dictionary<string, BattleRewardModel>();
                List<BattleRewardModel> Rewards = new List<BattleRewardModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT reward_type, slot_count, percentage FROM system_battle_rewards ORDER BY reward_type ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        string Key = Reader["reward_type"].ToString();
                        BattleRewardModel Reward = new BattleRewardModel
                        {
                            Type = ComDiv.ParseEnum<BattleRewardType>(Key),
                            Percentage = int.Parse(Reader["percentage"].ToString()),
                            Rewards = new int[int.Parse(Reader["slot_count"].ToString())]
                        };
                        ByType[Key] = Reward;
                        Rewards.Add(Reward);
                    }
                    Reader.Close();
                    Command.Dispose();

                    Command = Connection.CreateCommand();
                    Command.CommandText = "SELECT reward_type, slot_index, good_id FROM system_battle_reward_goods ORDER BY reward_type ASC, slot_index ASC;";
                    Command.CommandType = CommandType.Text;
                    Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        BattleRewardModel Reward;
                        int Slot = int.Parse(Reader["slot_index"].ToString());
                        if (ByType.TryGetValue(Reader["reward_type"].ToString(), out Reward) && Slot >= 0 && Slot < Reward.Rewards.Length)
                        {
                            Reward.Rewards[Slot] = int.Parse(Reader["good_id"].ToString());
                        }
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Rewards;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<KeyValuePair<string, List<string>>> GetGameRuleFilters()
        {
            try
            {
                List<int> Order = new List<int>();
                Dictionary<int, string> Names = new Dictionary<int, string>();
                Dictionary<int, List<string>> Filters = new Dictionary<int, List<string>>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, name FROM system_game_rules ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        int Id = int.Parse(Reader["id"].ToString());
                        Order.Add(Id);
                        Names[Id] = Reader["name"].ToString();
                        Filters[Id] = new List<string>();
                    }
                    Reader.Close();
                    Command.Dispose();

                    Command = Connection.CreateCommand();
                    Command.CommandText = "SELECT rule_id, filter FROM system_game_rule_bans ORDER BY rule_id ASC, ordinal ASC;";
                    Command.CommandType = CommandType.Text;
                    Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        List<string> RuleFilters;
                        if (Filters.TryGetValue(int.Parse(Reader["rule_id"].ToString()), out RuleFilters))
                        {
                            RuleFilters.Add(Reader["filter"].ToString());
                        }
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                List<KeyValuePair<string, List<string>>> Rules = new List<KeyValuePair<string, List<string>>>();
                foreach (int Id in Order)
                {
                    Rules.Add(new KeyValuePair<string, List<string>>(Names[Id], Filters[Id]));
                }
                return Rules;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        // El indice de fondo va al cliente como u8 (+0x153 del bloque comun). La columna es
        // smallint, asi que un valor fuera de rango escrito a mano reventaria byte.Parse y
        // tumbaria la carga entera del subsistema de eventos al fallback XML por una fila.
        private static byte ImageIndex(object Value)
        {
            int index;
            if (!int.TryParse(Value == null ? "" : Value.ToString(), out index))
                return 0;
            return (byte)(index < 0 ? 0 : index > 255 ? 255 : index);
        }

        public static List<EventLoginModel> GetLoginEvents()
        {
            try
            {
                Dictionary<int, EventLoginModel> ById = new Dictionary<int, EventLoginModel>();
                List<EventLoginModel> Events = new List<EventLoginModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, begin_date, ended_date, name, description, subtitle, image, period, priority FROM system_event_login ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        EventLoginModel Event = new EventLoginModel
                        {
                            Id = int.Parse(Reader["id"].ToString()),
                            BeginDate = uint.Parse(Reader["begin_date"].ToString()),
                            EndedDate = uint.Parse(Reader["ended_date"].ToString()),
                            Name = Reader["name"].ToString(),
                            Description = Reader["description"].ToString(),
                            Subtitle = Reader["subtitle"].ToString(),
                            Image = ImageIndex(Reader["image"]),
                            Period = bool.Parse(Reader["period"].ToString()),
                            Priority = bool.Parse(Reader["priority"].ToString()),
                            Goods = new List<int>()
                        };
                        ById[Event.Id] = Event;
                        Events.Add(Event);
                    }
                    Reader.Close();
                    Command.Dispose();

                    Command = Connection.CreateCommand();
                    Command.CommandText = "SELECT event_id, good_id FROM system_event_login_rewards ORDER BY event_id ASC, ordinal ASC;";
                    Command.CommandType = CommandType.Text;
                    Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        EventLoginModel Event;
                        if (ById.TryGetValue(int.Parse(Reader["event_id"].ToString()), out Event) && Event.Goods.Count < 5)
                        {
                            Event.Goods.Add(int.Parse(Reader["good_id"].ToString()));
                        }
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Events;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<EventBoostModel> GetBoostEvents()
        {
            try
            {
                List<EventBoostModel> Events = new List<EventBoostModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, begin_date, ended_date, boost_type, boost_value, bonus_exp, bonus_gold, percent, name, description, subtitle, image, period, priority FROM system_event_boost ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Events.Add(new EventBoostModel
                        {
                            Id = int.Parse(Reader["id"].ToString()),
                            BeginDate = uint.Parse(Reader["begin_date"].ToString()),
                            EndedDate = uint.Parse(Reader["ended_date"].ToString()),
                            BoostType = ComDiv.ParseEnum<PortalBoostEvent>(Reader["boost_type"].ToString()),
                            BoostValue = int.Parse(Reader["boost_value"].ToString()),
                            BonusExp = int.Parse(Reader["bonus_exp"].ToString()),
                            BonusGold = int.Parse(Reader["bonus_gold"].ToString()),
                            Percent = int.Parse(Reader["percent"].ToString()),
                            Name = Reader["name"].ToString(),
                            Description = Reader["description"].ToString(),
                            Subtitle = Reader["subtitle"].ToString(),
                            Image = ImageIndex(Reader["image"]),
                            Period = bool.Parse(Reader["period"].ToString()),
                            Priority = bool.Parse(Reader["priority"].ToString())
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Events;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<EventRankUpModel> GetRankUpEvents()
        {
            try
            {
                Dictionary<int, EventRankUpModel> ById = new Dictionary<int, EventRankUpModel>();
                List<EventRankUpModel> Events = new List<EventRankUpModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, begin_date, ended_date, name, description, subtitle, image, period, priority FROM system_event_rankup ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        EventRankUpModel Event = new EventRankUpModel
                        {
                            Id = int.Parse(Reader["id"].ToString()),
                            BeginDate = uint.Parse(Reader["begin_date"].ToString()),
                            EndedDate = uint.Parse(Reader["ended_date"].ToString()),
                            Name = Reader["name"].ToString(),
                            Description = Reader["description"].ToString(),
                            Subtitle = Reader["subtitle"].ToString(),
                            Image = ImageIndex(Reader["image"]),
                            Period = bool.Parse(Reader["period"].ToString()),
                            Priority = bool.Parse(Reader["priority"].ToString()),
                            Ranks = new List<int[]>()
                        };
                        ById[Event.Id] = Event;
                        Events.Add(Event);
                    }
                    Reader.Close();
                    Command.Dispose();

                    Command = Connection.CreateCommand();
                    Command.CommandText = "SELECT event_id, rank_id, bonus_exp, bonus_point, percent FROM system_event_rankup_bonuses ORDER BY event_id ASC, rank_id ASC;";
                    Command.CommandType = CommandType.Text;
                    Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        EventRankUpModel Event;
                        if (ById.TryGetValue(int.Parse(Reader["event_id"].ToString()), out Event))
                        {
                            Event.Ranks.Add(new int[4]
                            {
                                int.Parse(Reader["rank_id"].ToString()),
                                int.Parse(Reader["bonus_exp"].ToString()),
                                int.Parse(Reader["bonus_point"].ToString()),
                                int.Parse(Reader["percent"].ToString())
                            });
                        }
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Events;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<EventVisitModel> GetVisitEvents()
        {
            try
            {
                Dictionary<int, EventVisitModel> ById = new Dictionary<int, EventVisitModel>();
                List<EventVisitModel> Events = new List<EventVisitModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, begin_date, ended_date, title, check_days FROM system_event_visit ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        EventVisitModel Event = new EventVisitModel
                        {
                            Id = int.Parse(Reader["id"].ToString()),
                            BeginDate = uint.Parse(Reader["begin_date"].ToString()),
                            EndedDate = uint.Parse(Reader["ended_date"].ToString()),
                            Title = Reader["title"].ToString(),
                            Checks = int.Parse(Reader["check_days"].ToString()),
                            Boxes = new List<VisitBoxModel>()
                        };
                        for (int Index = 0; Index < 31; ++Index)
                            Event.Boxes.Add(new VisitBoxModel());
                        ById[Event.Id] = Event;
                        Events.Add(Event);
                    }
                    Reader.Close();
                    Command.Dispose();

                    Command = Connection.CreateCommand();
                    Command.CommandText = "SELECT event_id, day, good_id_1, good_id_2, is_both FROM system_event_visit_boxes ORDER BY event_id ASC, day ASC;";
                    Command.CommandType = CommandType.Text;
                    Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        EventVisitModel Event;
                        int Day = int.Parse(Reader["day"].ToString()) - 1;
                        if (ById.TryGetValue(int.Parse(Reader["event_id"].ToString()), out Event) && Day >= 0 && Day < Event.Boxes.Count)
                        {
                            Event.Boxes[Day].Reward1.SetGoodId(int.Parse(Reader["good_id_1"].ToString()));
                            Event.Boxes[Day].Reward2.SetGoodId(int.Parse(Reader["good_id_2"].ToString()));
                            Event.Boxes[Day].IsBothReward = bool.Parse(Reader["is_both"].ToString());
                        }
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                foreach (EventVisitModel Event in Events)
                    Event.SetBoxCounts();
                return Events;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<EventQuestModel> GetQuestEvents()
        {
            try
            {
                List<EventQuestModel> Events = new List<EventQuestModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT begin_date, ended_date FROM system_event_quest ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Events.Add(new EventQuestModel
                        {
                            BeginDate = uint.Parse(Reader["begin_date"].ToString()),
                            EndedDate = uint.Parse(Reader["ended_date"].ToString())
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Events;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<EventXmasModel> GetXmasEvents()
        {
            try
            {
                List<EventXmasModel> Events = new List<EventXmasModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT begin_date, ended_date, good_id FROM system_event_xmas ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        Events.Add(new EventXmasModel
                        {
                            BeginDate = uint.Parse(Reader["begin_date"].ToString()),
                            EndedDate = uint.Parse(Reader["ended_date"].ToString()),
                            GoodId = int.Parse(Reader["good_id"].ToString())
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Events;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<TitleModel> GetTitles()
        {
            try
            {
                List<TitleModel> Titles = new List<TitleModel>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT id, class_id, ribbon, ensign, medal, master_medal, rank, slot, req_title_1, req_title_2 FROM system_titles ORDER BY id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        TitleModel Title = new TitleModel(int.Parse(Reader["id"].ToString()))
                        {
                            ClassId = int.Parse(Reader["class_id"].ToString()),
                            Ribbon = int.Parse(Reader["ribbon"].ToString()),
                            Ensign = int.Parse(Reader["ensign"].ToString()),
                            Medal = int.Parse(Reader["medal"].ToString()),
                            MasterMedal = int.Parse(Reader["master_medal"].ToString()),
                            Rank = int.Parse(Reader["rank"].ToString()),
                            Slot = int.Parse(Reader["slot"].ToString()),
                            Req1 = int.Parse(Reader["req_title_1"].ToString()),
                            Req2 = int.Parse(Reader["req_title_2"].ToString())
                        };
                        Titles.Add(Title);
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Titles;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<TitleAward> GetTitleAwards()
        {
            try
            {
                List<TitleAward> Awards = new List<TitleAward>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT title_id, item_id, item_count, equip_type, item_name FROM system_title_awards ORDER BY title_id ASC, ordinal ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        int ItemId = int.Parse(Reader["item_id"].ToString());
                        ItemsModel Item = new ItemsModel(ItemId)
                        {
                            Name = Reader["item_name"] == DBNull.Value ? string.Empty : Reader["item_name"].ToString(),
                            Count = uint.Parse(Reader["item_count"].ToString()),
                            Equip = (ItemEquipType)int.Parse(Reader["equip_type"].ToString())
                        };
                        if (Item.Equip == ItemEquipType.Permanent)
                            Item.ObjectId = (long)ComDiv.ValidateStockId(ItemId);
                        Awards.Add(new TitleAward
                        {
                            Id = int.Parse(Reader["title_id"].ToString()),
                            Item = Item
                        });
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Awards;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        public static List<CouponFlag> GetCouponEffects()
        {
            try
            {
                List<CouponFlag> Flags = new List<CouponFlag>();
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT item_id, effect_flag FROM system_coupon_effects ORDER BY item_id ASC;";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Reader = Command.ExecuteReader(CommandBehavior.Default);
                    while (Reader.Read())
                    {
                        CouponFlag Flag = new CouponFlag
                        {
                            ItemId = int.Parse(Reader["item_id"].ToString()),
                            EffectFlag = ComDiv.ParseEnum<CouponEffects>(Reader["effect_flag"].ToString())
                        };
                        Flags.Add(Flag);
                    }
                    Reader.Close();
                    Command.Dispose();
                    Connection.Dispose();
                    Connection.Close();
                }
                return Flags;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }
    }
}