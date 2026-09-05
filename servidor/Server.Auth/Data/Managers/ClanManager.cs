using Npgsql;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Auth.Data.Managers;
using Plugin.Core.SQL;
using Server.Auth.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Server.Auth.Data.Managers
{
    public class ClanManager
    {
        public static ClanModel GetClanDB(object Value, int Type)
        {
            ClanModel clanDb = new ClanModel();
            if (Type == 1 && (int)Value <= 0 || Type == 0 && string.IsNullOrEmpty(Value.ToString()))
                return clanDb;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    string str = Type == 0 ? "name" : "id";
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@valor", Value);
                    command.CommandText = $"SELECT * FROM system_clan WHERE {str}=@valor";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        clanDb.Id = int.Parse(npgsqlDataReader["id"].ToString());
                        clanDb.Rank = (int)byte.Parse(npgsqlDataReader["rank"].ToString());
                        clanDb.Name = npgsqlDataReader["name"].ToString();
                        clanDb.OwnerId = long.Parse(npgsqlDataReader["owner_id"].ToString());
                        clanDb.Logo = uint.Parse(npgsqlDataReader["logo"].ToString());
                        clanDb.NameColor = (int)byte.Parse(npgsqlDataReader["name_color"].ToString());
                        clanDb.Effect = (int)byte.Parse(npgsqlDataReader["effects"].ToString());
                    }
                    command.Dispose();
                    npgsqlDataReader.Close();
                    npgsqlConnection.Dispose();
                    npgsqlConnection.Close();
                }
                return clanDb.Id == 0 ? new ClanModel() : clanDb;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return new ClanModel();
            }
        }

        public static List<Account> GetClanPlayers(int ClanId, long Exception)
        {
            List<Account> clanPlayers = new List<Account>();
            if (ClanId <= 0)
                return clanPlayers;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@clan", (object)ClanId);
                    command.CommandText = @"
                        SELECT 
                            a.nickname, a.rank, a.online, a.player_id, a.status,
                            b.bonuses, b.crosshair_color, b.free_pass, b.fake_rank, b.fake_nick, b.muzzle_color, b.nick_border_color,
                            e.weapon_primary, e.weapon_secondary, e.weapon_melee, e.weapon_explosive, e.weapon_special,
                            e.chara_red_side, e.chara_blue_side, e.dino_item_chara, e.part_head, e.part_face, e.part_jacket,
                            e.part_pocket, e.part_glove, e.part_belt, e.part_holster, e.part_skin, e.beret_item_part,
                            e.accesory_id, e.spray_id, e.namecard_id,
                            s.clan_matches, s.clan_match_wins, s.clan_match_loses
                        FROM accounts as a
                        LEFT JOIN player_bonus as b ON a.player_id = b.owner_id
                        LEFT JOIN player_equipments as e ON a.player_id = e.owner_id
                        LEFT JOIN player_stat_clans as s ON a.player_id = s.owner_id
                        WHERE a.clan_id = @clan";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);

                    if (!npgsqlDataReader.HasRows)
                    {
                        CLogger.Print($"No se encontraron jugadores para el clan {ClanId}", LoggerType.Warning);
                        npgsqlDataReader.Close();
                        command.Dispose();
                        npgsqlConnection.Close();
                        return clanPlayers;
                    }

                    while (npgsqlDataReader.Read())
                    {
                        long num = long.Parse(npgsqlDataReader["player_id"].ToString());
                        if (num != Exception)
                        {
                            Account account = new Account()
                            {
                                PlayerId = num,
                                Nickname = npgsqlDataReader["nickname"].ToString(),
                                Rank = (int)byte.Parse(npgsqlDataReader["rank"].ToString()),
                                IsOnline = bool.Parse(npgsqlDataReader["online"].ToString())
                            };
                            
                            // Populate Bonus
                            if (npgsqlDataReader["bonuses"] != DBNull.Value)
                            {
                                account.Bonus = new PlayerBonus()
                                {
                                    OwnerId = num,
                                    Bonuses = int.Parse(npgsqlDataReader["bonuses"].ToString()),
                                    CrosshairColor = int.Parse(npgsqlDataReader["crosshair_color"].ToString()),
                                    FreePass = int.Parse(npgsqlDataReader["free_pass"].ToString()),
                                    FakeRank = int.Parse(npgsqlDataReader["fake_rank"].ToString()),
                                    FakeNick = npgsqlDataReader["fake_nick"].ToString(),
                                    MuzzleColor = int.Parse(npgsqlDataReader["muzzle_color"].ToString()),
                                    NickBorderColor = int.Parse(npgsqlDataReader["nick_border_color"].ToString())
                                };
                            }

                            // Populate Equipment
                            if (npgsqlDataReader["weapon_primary"] != DBNull.Value)
                            {
                                account.Equipment = new PlayerEquipment()
                                {
                                    OwnerId = num,
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
                                    NameCardId = int.Parse(npgsqlDataReader["namecard_id"].ToString())
                                };
                            }

                            // Populate Statistic Clan
                            if (npgsqlDataReader["clan_matches"] != DBNull.Value)
                            {
                                account.Statistic.Clan = new StatisticClan()
                                {
                                    OwnerId = num,
                                    Matches = int.Parse(npgsqlDataReader["clan_matches"].ToString()),
                                    MatchWins = int.Parse(npgsqlDataReader["clan_match_wins"].ToString()),
                                    MatchLoses = int.Parse(npgsqlDataReader["clan_match_loses"].ToString())
                                };
                            }

                            account.Status.SetData(uint.Parse(npgsqlDataReader["status"].ToString()), num);
                            if (account.IsOnline && !AccountManager.Accounts.ContainsKey(num))
                            {
                                account.SetOnlineStatus(false);
                                account.Status.ResetData(account.PlayerId);
                            }
                            clanPlayers.Add(account);
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
            return clanPlayers;
        }

        public static List<Account> GetClanPlayers(int ClanId, long Exception, bool IsOnline)
        {
            List<Account> clanPlayers = new List<Account>();
            if (ClanId <= 0)
                return clanPlayers;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    npgsqlConnection.Open();
                    command.Parameters.AddWithValue("@clan", (object)ClanId);
                    command.Parameters.AddWithValue("@on", (object)IsOnline);
                    command.CommandText = "SELECT player_id, nickname, rank, online, status FROM accounts WHERE clan_id=@clan AND online=@on";
                    command.CommandType = CommandType.Text;
                    NpgsqlDataReader npgsqlDataReader = command.ExecuteReader(CommandBehavior.Default);
                    while (npgsqlDataReader.Read())
                    {
                        long num = long.Parse(npgsqlDataReader["player_id"].ToString());
                        if (num != Exception)
                        {
                            Account account = new Account()
                            {
                                PlayerId = num,
                                Nickname = npgsqlDataReader["nickname"].ToString(),
                                Rank = (int)byte.Parse(npgsqlDataReader["rank"].ToString()),
                                IsOnline = bool.Parse(npgsqlDataReader["online"].ToString())
                            };
                            account.Bonus = DaoManagerSQL.GetPlayerBonusDB(account.PlayerId);
                            account.Equipment = DaoManagerSQL.GetPlayerEquipmentsDB(account.PlayerId);
                            account.Statistic.Clan = DaoManagerSQL.GetPlayerStatClanDB(account.PlayerId);
                            account.Status.SetData(uint.Parse(npgsqlDataReader["status"].ToString()), num);
                            if (account.IsOnline && !AccountManager.Accounts.ContainsKey(num))
                            {
                                account.SetOnlineStatus(false);
                                account.Status.ResetData(account.PlayerId);
                            }
                            clanPlayers.Add(account);
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
            return clanPlayers;
        }
    }
}