using Npgsql;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Network;
using Server.Game.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Plugin.Core.Managers;

namespace Server.Game.Data.Managers
{
    public static class ClanManager
    {
        public static List<ClanModel> Clans = new List<ClanModel>();

        // PASO 5: Reemplazar el método Load() en ClanManager.cs con este código completo

        public static void Load()
        {
            try
            {
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.CommandText = "SELECT * FROM system_clan";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Data = Command.ExecuteReader();
                    while (Data.Read())
                    {
                        long owner = long.Parse(Data["owner_id"].ToString());
                        if (owner == 0)
                        {
                            continue;
                        }
                        string BestEXP, BestParticipant, BestWins, BestKills, BestHeadshots;
                        ClanModel Clan = new ClanModel()
                        {
                            Id = int.Parse(Data["id"].ToString()),
                            Rank = int.Parse(Data["rank"].ToString()),
                            Name = Data["name"].ToString(),
                            OwnerId = owner,
                            Logo = uint.Parse(Data["logo"].ToString()),
                            NameColor = int.Parse(Data["name_color"].ToString()),
                            Info = Data["info"].ToString(),
                            News = Data["news"].ToString(),
                            CreationDate = uint.Parse(Data["create_date"].ToString()),
                            Authority = int.Parse(Data["authority"].ToString()),
                            RankLimit = int.Parse(Data["rank_limit"].ToString()),
                            MinAgeLimit = int.Parse(Data["min_age_limit"].ToString()),
                            MaxAgeLimit = int.Parse(Data["max_age_limit"].ToString()),
                            JoinType = (JoinClanType)int.Parse(Data["join_permission"].ToString()),
                            Matches = int.Parse(Data["matches"].ToString()),
                            MatchWins = int.Parse(Data["match_wins"].ToString()),
                            MatchLoses = int.Parse(Data["match_loses"].ToString()),
                            Points = int.Parse(Data["point"].ToString()),
                            MaxPlayers = int.Parse(Data["max_players"].ToString()),
                            Exp = int.Parse(Data["exp"].ToString()),
                            Effect = int.Parse(Data["effects"].ToString()),

                            // NUEVO: Cargar estadísticas históricas
                            TotalKills = Data["total_kills"] != DBNull.Value ? int.Parse(Data["total_kills"].ToString()) : 0,
                            TotalDeaths = Data["total_deaths"] != DBNull.Value ? int.Parse(Data["total_deaths"].ToString()) : 0,
                            TotalAssists = Data["total_assists"] != DBNull.Value ? int.Parse(Data["total_assists"].ToString()) : 0,
                            TotalHeadshots = Data["total_headshots"] != DBNull.Value ? int.Parse(Data["total_headshots"].ToString()) : 0,
                            TotalEscapes = Data["total_escapes"] != DBNull.Value ? int.Parse(Data["total_escapes"].ToString()) : 0,
                            SeasonRank = Data["season_rank"] != DBNull.Value ? int.Parse(Data["season_rank"].ToString()) : 0,
                            PreviousSeasonRank = Data["previous_season_rank"] != DBNull.Value ? int.Parse(Data["previous_season_rank"].ToString()) : 0
                        };
                        BestEXP = Data["best_exp"].ToString();
                        BestParticipant = Data["best_participants"].ToString();
                        BestWins = Data["best_wins"].ToString();
                        BestKills = Data["best_kills"].ToString();
                        BestHeadshots = Data["best_headshots"].ToString();
                        Clan.BestPlayers.SetPlayers(BestEXP, BestParticipant, BestWins, BestKills, BestHeadshots);
                        Clans.Add(Clan);
                    }
                    Command.Dispose();
                    Data.Close();
                    Connection.Dispose();
                    Connection.Close();
                }

                CLogger.Print($"Clans carregados: {Clans.Count}.", LoggerType.Info);
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        public static List<ClanModel> GetClanListPerPage(int Page)
        {
            List<ClanModel> PageClans = new List<ClanModel>(); // *** CAMBIO: Renombrar variable para evitar conflicto
            if (Page == 0)
            {
                return PageClans;
            }
            try
            {
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.Parameters.AddWithValue("@page", (170 * Page));
                    Command.CommandText = "SELECT * FROM system_clan ORDER BY id DESC OFFSET @page LIMIT 170";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Data = Command.ExecuteReader();
                    while (Data.Read())
                    {
                        long OwnerId = long.Parse(Data["owner_id"].ToString());
                        if (OwnerId == 0)
                        {
                            continue;
                        }
                        string BestEXP, BestParticipant, BestWins, BestKills, BestHeadshots;
                        ClanModel Clan = new ClanModel()
                        {
                            Id = int.Parse(Data["id"].ToString()),
                            Rank = byte.Parse(Data["rank"].ToString()),
                            Name = Data["name"].ToString(),
                            OwnerId = OwnerId,
                            Logo = uint.Parse(Data["logo"].ToString()),
                            NameColor = byte.Parse(Data["name_color"].ToString()),
                            Info = Data["info"].ToString(),
                            News = Data["news"].ToString(),
                            CreationDate = uint.Parse(Data["create_date"].ToString()),
                            Authority = int.Parse(Data["authority"].ToString()),
                            RankLimit = int.Parse(Data["rank_limit"].ToString()),
                            MinAgeLimit = int.Parse(Data["age_limit_start"].ToString()),
                            MaxAgeLimit = int.Parse(Data["age_limit_end"].ToString()),
                            JoinType = (JoinClanType)int.Parse(Data["join_permission"].ToString()),
                            Matches = int.Parse(Data["matches"].ToString()),
                            MatchWins = int.Parse(Data["match_wins"].ToString()),
                            MatchLoses = int.Parse(Data["match_loses"].ToString()),
                            Points = int.Parse(Data["point"].ToString()),
                            MaxPlayers = int.Parse(Data["max_players"].ToString()),
                            Exp = int.Parse(Data["exp"].ToString()),
                            Effect = byte.Parse(Data["effects"].ToString())
                        };
                        BestEXP = Data["best_exp"].ToString();
                        BestParticipant = Data["best_participants"].ToString();
                        BestWins = Data["best_wins"].ToString();
                        BestKills = Data["best_kills"].ToString();
                        BestHeadshots = Data["best_headshots"].ToString();
                        Clan.BestPlayers.SetPlayers(BestEXP, BestParticipant, BestWins, BestKills, BestHeadshots);
                        PageClans.Add(Clan); // *** CAMBIO: Usar nueva variable
                    }
                    Command.Dispose();
                    Data.Close();
                    Connection.Dispose();
                    Connection.Close();
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
            return PageClans; // *** CAMBIO: Retornar nueva variable
        }

        public static ClanModel GetClan(int ClanId)
        {
            if (ClanId == 0)
            {
                return new ClanModel();
            }
            lock (Clans)
            {
                foreach (ClanModel Clan in Clans)
                {
                    if (Clan.Id == ClanId)
                    {
                        return Clan;
                    }
                }
            }
            return new ClanModel();
        }

        public static List<Account> GetClanPlayers(int ClanId, long Exception, bool UseCache)
        {
            List<Account> Players = new List<Account>();
            if (ClanId == 0)
            {
                return Players;
            }
            try
            {
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.Parameters.AddWithValue("@clan", ClanId);
                    Command.CommandText = "SELECT player_id, nickname, nick_color, rank, online, clan_access, clan_date, status FROM accounts WHERE clan_id=@clan";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Data = Command.ExecuteReader();
                    while (Data.Read())
                    {
                        long PlayerId = long.Parse(Data["player_id"].ToString());
                        if (PlayerId == Exception)
                        {
                            continue;
                        }
                        Account Player = new Account()
                        {
                            PlayerId = PlayerId,
                            Nickname = Data["nickname"].ToString(),
                            NickColor = int.Parse(Data["nick_color"].ToString()),
                            Rank = int.Parse(Data["rank"].ToString()),
                            IsOnline = bool.Parse(Data["online"].ToString()),
                            ClanId = ClanId,
                            ClanAccess = int.Parse(Data["clan_access"].ToString()),
                            ClanDate = uint.Parse(Data["clan_date"].ToString())
                        };
                        Player.Bonus = DaoManagerSQL.GetPlayerBonusDB(Player.PlayerId);
                        Player.Equipment = DaoManagerSQL.GetPlayerEquipmentsDB(Player.PlayerId);
                        Player.Statistic.Clan = DaoManagerSQL.GetPlayerStatClanDB(Player.PlayerId);
                        Player.Status.SetData(uint.Parse(Data["status"].ToString()), Player.PlayerId);
                        if (UseCache)
                        {
                            Account Member = AccountManager.GetAccount(Player.PlayerId, true);
                            if (Member != null)
                            {
                                Player.Connection = Member.Connection;
                            }
                        }
                        Players.Add(Player);
                    }
                    Command.Dispose();
                    Data.Close();
                    Connection.Dispose();
                    Connection.Close();
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
            return Players;
        }

        public static List<Account> GetClanPlayers(int ClanId, long Exception, bool UseCache, bool IsOnline)
        {
            List<Account> Players = new List<Account>();
            if (ClanId == 0)
            {
                return Players;
            }
            try
            {
                using (NpgsqlConnection Connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand Command = Connection.CreateCommand();
                    Connection.Open();
                    Command.Parameters.AddWithValue("@clan", ClanId);
                    Command.Parameters.AddWithValue("@on", IsOnline);
                    Command.CommandText = "SELECT player_id, nickname, nick_color, rank, clan_access, clan_date, status FROM accounts WHERE clan_id=@clan AND online=@on";
                    Command.CommandType = CommandType.Text;
                    NpgsqlDataReader Data = Command.ExecuteReader();
                    while (Data.Read())
                    {
                        long PlayerId = Data.GetInt64(0);
                        if (PlayerId == Exception)
                        {
                            continue;
                        }
                        Account Player = new Account()
                        {
                            PlayerId = PlayerId,
                            Nickname = Data["nickname"].ToString(),
                            NickColor = int.Parse(Data["nick_color"].ToString()),
                            Rank = int.Parse(Data["rank"].ToString()),
                            IsOnline = IsOnline,
                            ClanId = ClanId,
                            ClanAccess = int.Parse(Data["clan_access"].ToString()),
                            ClanDate = uint.Parse(Data["clan_date"].ToString())
                        };
                        Player.Bonus = DaoManagerSQL.GetPlayerBonusDB(Player.PlayerId);
                        Player.Equipment = DaoManagerSQL.GetPlayerEquipmentsDB(Player.PlayerId);
                        Player.Statistic.Clan = DaoManagerSQL.GetPlayerStatClanDB(Player.PlayerId);
                        Player.Status.SetData(uint.Parse(Data["status"].ToString()), Player.PlayerId);
                        if (UseCache)
                        {
                            Account Member = AccountManager.GetAccount(Player.PlayerId, true);
                            if (Member != null)
                            {
                                Player.Connection = Member.Connection;
                            }
                        }
                        Players.Add(Player);
                    }
                    Command.Dispose();
                    Data.Close();
                    Connection.Dispose();
                    Connection.Close();
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
            return Players;
        }

        public static void SendPacket(GameServerPacket Packet, List<Account> Players)
        {
            if (Players.Count == 0)
            {
                return;
            }

            byte[] Data = Packet.GetCompleteBytes("ClanManager.SendPacket");
            foreach (Account Player in Players)
            {
                Player.SendCompletePacket(Data, Packet.GetType().Name, false);
            }
        }

        public static void SendPacket(GameServerPacket Packet, List<Account> Players, long Exception)
        {
            if (Players.Count == 0)
            {
                return;
            }

            byte[] Data = Packet.GetCompleteBytes("ClanManager.SendPacket");
            foreach (Account Player in Players)
            {
                if (Player.PlayerId != Exception)
                {
                    Player.SendCompletePacket(Data, Packet.GetType().Name, false);
                }
            }
        }

        public static void SendPacket(GameServerPacket Packet, int ClanId, long Exception, bool UseCache, bool IsOnline)
        {
            SendPacket(Packet, GetClanPlayers(ClanId, Exception, UseCache, IsOnline));
        }

        public static bool RemoveClan(ClanModel clan)
        {
            lock (Clans)
            {
                return Clans.Remove(clan);
            }
        }

        public static void AddClan(ClanModel clan)
        {
            lock (Clans)
            {
                Clans.Add(clan);
            }
        }

        public static bool IsClanNameExist(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }
            try
            {
                int value = 0;
                using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = connection.CreateCommand();
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@name", name);
                    command.CommandText = "SELECT COUNT(*) FROM system_clan WHERE name=@name";
                    value = Convert.ToInt32(command.ExecuteScalar());
                    command.Dispose();
                    connection.Dispose();
                    connection.Close();
                }
                return value > 0;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return true;
            }
        }

        public static bool IsClanLogoExist(uint logo)
        {
            try
            {
                int value = 0;
                using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = connection.CreateCommand();
                    connection.Open();
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@logo", (long)logo);
                    command.CommandText = "SELECT COUNT(*) FROM system_clan WHERE logo=@logo";
                    value = Convert.ToInt32(command.ExecuteScalar());
                    command.Dispose();
                    connection.Dispose();
                    connection.Close();
                }
                return value > 0;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return true;
            }
        }

        // SOLUCIÓN: Reemplazar los métodos en ClanManager.cs con esta versión corregida

        /// <summary>
        /// Calcula automáticamente las estadísticas estacionales del clan desde los datos existentes
        /// </summary>
        public static ClanSeasonalStats CalculateClanSeasonalStats(int clanId)
        {
            var stats = new ClanSeasonalStats();

            if (clanId == 0)
                return stats;

            try
            {
                DateTime now = DateTime.Now;

                // Fechas para temporada actual (mes actual)
                DateTime currentSeasonStart = new DateTime(now.Year, now.Month, 1);
                DateTime currentSeasonEnd = currentSeasonStart.AddMonths(1);

                // Fechas para temporada anterior (mes anterior)
                DateTime previousSeasonStart = currentSeasonStart.AddMonths(-1);
                DateTime previousSeasonEnd = currentSeasonStart;

                // USAR CONEXIONES SEPARADAS PARA EVITAR CONFLICTOS
                // Estadísticas de la temporada actual
                stats.CurrentSeason = CalculateSeasonStats(clanId, currentSeasonStart, currentSeasonEnd);

                // Estadísticas de la temporada anterior  
                stats.PreviousSeason = CalculateSeasonStats(clanId, previousSeasonStart, previousSeasonEnd);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error calculating clan seasonal stats: {ex.Message}", LoggerType.Error, ex);
            }

            return stats;
        }

        /// <summary>
        /// Calcula las estadísticas de una temporada específica - VERSIÓN CORREGIDA
        /// </summary>
        /// <summary>
        /// Calcula las estadísticas de una temporada específica - VERSIÓN CORREGIDA PARA DBNull
        /// </summary>
        private static SeasonStats CalculateSeasonStats(int clanId, DateTime startDate, DateTime endDate)
        {
            var seasonStats = new SeasonStats();

            try
            {
                // Convertir fechas a timestamps unix para la consulta
                long startTimestamp = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
                long endTimestamp = ((DateTimeOffset)endDate).ToUnixTimeSeconds();

                using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
            WITH clan_stats AS (
                SELECT 
                    CASE 
                        WHEN clan_1 = @clanId THEN COALESCE(clan_1_kills, 0)
                        WHEN clan_2 = @clanId THEN COALESCE(clan_2_kills, 0)
                        ELSE 0 
                    END as kills,
                    CASE 
                        WHEN clan_1 = @clanId THEN COALESCE(clan_1_deaths, 0)
                        WHEN clan_2 = @clanId THEN COALESCE(clan_2_deaths, 0)
                        ELSE 0 
                    END as deaths,
                    CASE 
                        WHEN clan_1 = @clanId THEN COALESCE(clan_1_assists, 0)
                        WHEN clan_2 = @clanId THEN COALESCE(clan_2_assists, 0)
                        ELSE 0 
                    END as assists,
                    CASE 
                        WHEN clan_1 = @clanId THEN COALESCE(clan_1_headshots, 0)
                        WHEN clan_2 = @clanId THEN COALESCE(clan_2_headshots, 0)
                        ELSE 0 
                    END as headshots,
                    CASE 
                        WHEN clan_1 = @clanId THEN COALESCE(clan_1_escapes, 0)
                        WHEN clan_2 = @clanId THEN COALESCE(clan_2_escapes, 0)
                        ELSE 0 
                    END as escapes,
                    CASE 
                        WHEN clan_winner = @clanId THEN 1 
                        ELSE 0 
                    END as won,
                    CASE 
                        WHEN clan_winner != @clanId AND clan_winner != 0 THEN 1 
                        ELSE 0 
                    END as lost
                FROM clan_matches 
                WHERE (clan_1 = @clanId OR clan_2 = @clanId)
                AND match_date >= @startDate 
                AND match_date < @endDate
            )
            SELECT 
                COALESCE(COUNT(*), 0) as total_matches,
                COALESCE(SUM(won), 0) as total_wins,
                COALESCE(SUM(lost), 0) as total_loses,
                COALESCE(SUM(kills), 0) as total_kills,
                COALESCE(SUM(deaths), 0) as total_deaths,
                COALESCE(SUM(assists), 0) as total_assists,
                COALESCE(SUM(headshots), 0) as total_headshots,
                COALESCE(SUM(escapes), 0) as total_escapes
            FROM clan_stats";

                        command.Parameters.AddWithValue("@clanId", clanId);
                        command.Parameters.AddWithValue("@startDate", startTimestamp);
                        command.Parameters.AddWithValue("@endDate", endTimestamp);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Usar método seguro para convertir DBNull a int
                                seasonStats.Matches = reader["total_matches"] == DBNull.Value ? 0 : Convert.ToInt32(reader["total_matches"]);
                                seasonStats.MatchWins = reader["total_wins"] == DBNull.Value ? 0 : Convert.ToInt32(reader["total_wins"]);
                                seasonStats.MatchLoses = reader["total_loses"] == DBNull.Value ? 0 : Convert.ToInt32(reader["total_loses"]);
                                seasonStats.TotalKills = reader["total_kills"] == DBNull.Value ? 0 : Convert.ToInt32(reader["total_kills"]);
                                seasonStats.TotalDeaths = reader["total_deaths"] == DBNull.Value ? 0 : Convert.ToInt32(reader["total_deaths"]);
                                seasonStats.TotalAssists = reader["total_assists"] == DBNull.Value ? 0 : Convert.ToInt32(reader["total_assists"]);
                                seasonStats.TotalHeadshots = reader["total_headshots"] == DBNull.Value ? 0 : Convert.ToInt32(reader["total_headshots"]);
                                seasonStats.TotalEscapes = reader["total_escapes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["total_escapes"]);

                                // Calcular puntos ganados basado en victorias/derrotas
                                seasonStats.PointsGained = (seasonStats.MatchWins * 50) - (seasonStats.MatchLoses * 25);

                                // Calcular medallas basado en rendimiento
                                seasonStats.Medals = CalculateMedals(seasonStats);
                            }
                        }
                    }

                    connection.Close();
                }

                // Obtener ranking estacional en una conexión separada
                seasonStats.SeasonRank = GetClanSeasonRank(clanId);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error calculating season stats: {ex.Message}", LoggerType.Error, ex);
            }

            return seasonStats;
        }
        /// <summary>
        /// Calcula automáticamente las medallas semanales del clan
        /// </summary>
        public static ClanWeeklyStats CalculateClanWeeklyStats(int clanId)
        {
            var stats = new ClanWeeklyStats();

            if (clanId == 0)
                return stats;

            try
            {
                DateTime now = DateTime.Now;

                // Semana actual (lunes a domingo)
                int daysFromMonday = ((int)now.DayOfWeek - 1 + 7) % 7;
                DateTime currentWeekStart = now.Date.AddDays(-daysFromMonday);
                DateTime currentWeekEnd = currentWeekStart.AddDays(7);

                // Semana anterior
                DateTime previousWeekStart = currentWeekStart.AddDays(-7);
                DateTime previousWeekEnd = currentWeekStart;

                // Medallas de la semana actual
                stats.CurrentWeekMedals = CalculateWeeklyMedals(clanId, currentWeekStart, currentWeekEnd);

                // Medallas de la semana anterior
                stats.PreviousWeekMedals = CalculateWeeklyMedals(clanId, previousWeekStart, previousWeekEnd);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error calculating clan weekly stats: {ex.Message}", LoggerType.Error, ex);
            }

            return stats;
        }

        /// <summary>
        /// Calcula las medallas de una semana específica - VERSIÓN CORREGIDA
        /// </summary>
        /// <summary>
        /// Calcula las medallas de una semana específica - VERSIÓN CORREGIDA PARA DBNull
        /// </summary>
        private static int CalculateWeeklyMedals(int clanId, DateTime startDate, DateTime endDate)
        {
            int medals = 0;

            try
            {
                long startTimestamp = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
                long endTimestamp = ((DateTimeOffset)endDate).ToUnixTimeSeconds();

                using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
            SELECT 
                COALESCE(COUNT(*), 0) as matches_played,
                COALESCE(SUM(CASE WHEN clan_winner = @clanId THEN 1 ELSE 0 END), 0) as matches_won
            FROM clan_matches 
            WHERE (clan_1 = @clanId OR clan_2 = @clanId)
            AND match_date >= @startDate 
            AND match_date < @endDate";

                        command.Parameters.AddWithValue("@clanId", clanId);
                        command.Parameters.AddWithValue("@startDate", startTimestamp);
                        command.Parameters.AddWithValue("@endDate", endTimestamp);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Protección contra DBNull
                                int matchesPlayed = reader["matches_played"] == DBNull.Value ? 0 : Convert.ToInt32(reader["matches_played"]);
                                int matchesWon = reader["matches_won"] == DBNull.Value ? 0 : Convert.ToInt32(reader["matches_won"]);

                                // Algoritmo simple para calcular medallas
                                medals = matchesWon;

                                // Bonus por ratio de victorias
                                if (matchesPlayed > 0)
                                {
                                    double winRatio = (double)matchesWon / matchesPlayed;
                                    if (winRatio >= 0.8) medals += 5;
                                    else if (winRatio >= 0.6) medals += 3;
                                    else if (winRatio >= 0.4) medals += 1;
                                }

                                // Bonus por actividad
                                if (matchesPlayed >= 20) medals += 3;
                                else if (matchesPlayed >= 10) medals += 2;
                                else if (matchesPlayed >= 5) medals += 1;
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error calculating weekly medals: {ex.Message}", LoggerType.Error, ex);
            }

            return medals;
        }
        /// <summary>
        /// Calcula medallas basado en estadísticas de temporada
        /// </summary>
        private static int CalculateMedals(SeasonStats stats)
        {
            int medals = 0;

            // Medallas por victorias
            medals += stats.MatchWins;

            // Medallas por KDR
            if (stats.TotalDeaths > 0)
            {
                double kdr = (double)stats.TotalKills / stats.TotalDeaths;
                if (kdr >= 2.0) medals += 10;
                else if (kdr >= 1.5) medals += 5;
                else if (kdr >= 1.0) medals += 2;
            }

            // Medallas por headshots
            if (stats.TotalKills > 0)
            {
                double headshotRatio = (double)stats.TotalHeadshots / stats.TotalKills;
                if (headshotRatio >= 0.3) medals += 5;
                else if (headshotRatio >= 0.2) medals += 3;
                else if (headshotRatio >= 0.1) medals += 1;
            }

            // Medallas por actividad
            if (stats.Matches >= 50) medals += 10;
            else if (stats.Matches >= 25) medals += 5;
            else if (stats.Matches >= 10) medals += 2;

            return medals;
        }

        /// <summary>
        /// Obtiene el ranking estacional del clan desde la base de datos - VERSIÓN CORREGIDA
        /// </summary>
        /// <summary>
        /// Obtiene el ranking estacional del clan desde la base de datos - VERSIÓN CORREGIDA PARA DBNull
        /// </summary>
        private static int GetClanSeasonRank(int clanId)
        {
            int rank = 0;

            try
            {
                using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
            SELECT COALESCE(season_rank, 0) as season_rank 
            FROM system_clan 
            WHERE id = @clanId";

                        command.Parameters.AddWithValue("@clanId", clanId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Protección contra DBNull
                                rank = reader["season_rank"] == DBNull.Value ? 0 : Convert.ToInt32(reader["season_rank"]);
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error getting season rank: {ex.Message}", LoggerType.Error, ex);
            }

            return rank;
        }
        /// <summary>
        /// Registra una partida de clan - LLAMAR ESTO CUANDO TERMINE UNA PARTIDA
        /// </summary>
        public static void RegisterClanMatch(int clan1Id, int clan2Id, int winnerId,
            int clan1Kills, int clan1Deaths, int clan1Assists, int clan1Headshots, int clan1Escapes,
            int clan2Kills, int clan2Deaths, int clan2Assists, int clan2Headshots, int clan2Escapes)
        {
            if (clan1Id == 0 || clan2Id == 0)
                return;

            try
            {
                using (NpgsqlConnection connection = ConnectionSQL.GetInstance().Conn())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                    INSERT INTO clan_matches 
                    (clan_1, clan_2, clan_winner, match_date, 
                     clan_1_kills, clan_1_deaths, clan_1_assists, clan_1_headshots, clan_1_escapes,
                     clan_2_kills, clan_2_deaths, clan_2_assists, clan_2_headshots, clan_2_escapes)
                    VALUES (@clan1, @clan2, @winner, @date, 
                            @c1kills, @c1deaths, @c1assists, @c1headshots, @c1escapes,
                            @c2kills, @c2deaths, @c2assists, @c2headshots, @c2escapes)";

                        command.Parameters.AddWithValue("@clan1", clan1Id);
                        command.Parameters.AddWithValue("@clan2", clan2Id);
                        command.Parameters.AddWithValue("@winner", winnerId);
                        command.Parameters.AddWithValue("@date", DateTimeOffset.Now.ToUnixTimeSeconds());
                        command.Parameters.AddWithValue("@c1kills", clan1Kills);
                        command.Parameters.AddWithValue("@c1deaths", clan1Deaths);
                        command.Parameters.AddWithValue("@c1assists", clan1Assists);
                        command.Parameters.AddWithValue("@c1headshots", clan1Headshots);
                        command.Parameters.AddWithValue("@c1escapes", clan1Escapes);
                        command.Parameters.AddWithValue("@c2kills", clan2Kills);
                        command.Parameters.AddWithValue("@c2deaths", clan2Deaths);
                        command.Parameters.AddWithValue("@c2assists", clan2Assists);
                        command.Parameters.AddWithValue("@c2headshots", clan2Headshots);
                        command.Parameters.AddWithValue("@c2escapes", clan2Escapes);

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                CLogger.Print($"Clan match registered: {clan1Id} vs {clan2Id}, winner: {winnerId}", LoggerType.Info);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error registering clan match: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}