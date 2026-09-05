using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;

namespace Plugin.Core.Managers
{
    #region BattlePass Manager

    /// <summary>
    /// Clase para gestionar la carga y acceso centralizado a los datos del Pase de Batalla
    /// </summary>
    public static class BattlePassManager
    {
        // Almacenamiento de datos cargados
        public static List<BattlePassCardData> _loadedCards;
        public static List<BattlePassSeason> _loadedSeasons;
        public static BattlePassSeason _activeSeason;
        public static bool _isInitialized = false;
        public static readonly object _lockObject = new object();

        /// <summary>
        /// Carga todos los datos del pase de batalla en memoria para su uso posterior
        /// </summary>
        public static void Load()
        {
            // Usar lock para garantizar inicialización segura en entornos multihilo
            lock (_lockObject)
            {
                if (_isInitialized)
                {
                    CLogger.Print("BattlePassManager: The data is already loaded", LoggerType.Info);
                    return;
                }

                try
                {
                    CLogger.Print("BattlePassManager: Starting Battle Pass data loading", LoggerType.Info);

                    // Cargar todas las temporadas
                    _loadedSeasons = BattlePassLoader.LoadAllSeasons();

                    if (_loadedSeasons == null || _loadedSeasons.Count == 0)
                    {
                        //CLogger.Print("BattlePassManager: No se encontraron temporadas", LoggerType.Error);
                        _isInitialized = true; // Marcamos como inicializado aunque no haya datos
                        return;
                    }

                    CLogger.Print($"BattlePassManager: They were loaded {_loadedSeasons.Count} seasons", LoggerType.Info);

                    // Identificar la temporada activa
                    _activeSeason = BattlePassLoader.GetActiveSeasons();

                    if (_activeSeason != null)
                    {
                        //CLogger.Print($"BattlePassManager: Temporada activa: {_activeSeason.SeasonName} (ID: {_activeSeason.SeasonId})", LoggerType.Info);

                        // Cargar las cartas de la temporada activa
                        _loadedCards = BattlePassLoader.ConvertCardsFromSeason(_activeSeason);

                        // Configurar datos estáticos para las cartas
                        BattlePassCardData.EnablePremium = _activeSeason.SeasonEnabledForPremium;
                        BattlePassCardData.StartDate = DateConverter.ParseOrZero(_activeSeason.SeasonStartDate, _activeSeason.SeasonId, nameof(BattlePassSeason.SeasonStartDate));
                        BattlePassCardData.EndDate = DateConverter.ParseOrZero(_activeSeason.SeasonEndDate, _activeSeason.SeasonId, nameof(BattlePassSeason.SeasonEndDate));

                        CLogger.Print($"BattlePassManager: They were loaded {_loadedCards.Count} cards for the active season", LoggerType.Info);
                    }
                    else
                    {
                        CLogger.Print("BattlePassManager: There is no active season at this time", LoggerType.Warning);
                        // Cargar la primera temporada como fallback
                        _activeSeason = _loadedSeasons.FirstOrDefault();
                        if (_activeSeason != null)
                        {
                            _loadedCards = BattlePassLoader.ConvertCardsFromSeason(_activeSeason);
                            CLogger.Print($"BattlePassManager: Using the first season as a fallback: {_activeSeason.SeasonName}", LoggerType.Info);
                        }
                        else
                        {
                            _loadedCards = new List<BattlePassCardData>();
                        }
                    }

                    _isInitialized = true;
                    CLogger.Print("BattlePassManager: Data load completed successfully", LoggerType.Info);
                }
                catch (Exception ex)
                {
                    CLogger.Print($"BattlePassManager: Error loading data: {ex.Message}", LoggerType.Error, ex);
                    // Inicializar con listas vacías para evitar null references
                    _loadedSeasons = new List<BattlePassSeason>();
                    _loadedCards = new List<BattlePassCardData>();
                    _isInitialized = true; // Marcamos como inicializado para evitar intentos repetidos
                }
            }
        }

        /// <summary>
        /// Obtiene las cartas cargadas del pase de batalla
        /// </summary>
        public static List<BattlePassCardData> GetCards()
        {
            EnsureInitialized();
            return _loadedCards;
        }

        /// <summary>
        /// Obtiene todas las temporadas cargadas
        /// </summary>
        public static List<BattlePassSeason> GetAllSeasons()
        {
            EnsureInitialized();
            return _loadedSeasons;
        }

        /// <summary>
        /// Obtiene la temporada activa actual
        /// </summary>
        public static BattlePassSeason GetActiveSeason()
        {
            EnsureInitialized();
            return _activeSeason;
        }

        /// <summary>
        /// Obtiene una temporada específica por su ID
        /// </summary>
        public static BattlePassSeason GetSeasonById(string seasonId)
        {
            EnsureInitialized();
            return _loadedSeasons.FirstOrDefault(s => s.SeasonId == seasonId);
        }

        /// <summary>
        /// Asegura que los datos estén inicializados antes de cualquier operación
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Load();
            }
        }

        /// <summary>
        /// Fuerza una recarga de todos los datos
        /// </summary>
        public static void Reload()
        {
            lock (_lockObject)
            {
                _isInitialized = false;
                Load();
            }
        }
    }

    #endregion

    #region Models

    /// <summary>
    /// Clase para convertir entre formatos de fecha
    /// </summary>
    public static class DateConverter
    {
        /// <summary>
        /// Lee una fecha de temporada sin lanzar excepciones ante datos malformados
        /// </summary>
        public static uint ParseOrZero(string value, string seasonId, string fieldName)
        {
            if (uint.TryParse(value, out uint parsed))
            {
                return parsed;
            }

            CLogger.Print($"BattlePass: season '{seasonId}' has malformed {fieldName} ('{value}'), using 0", LoggerType.Warning);
            return 0;
        }

        /// <summary>
        /// Convierte una fecha en formato YYMMDDHHSS a timestamp Unix
        /// </summary>
        /// <param name="yymmddhhss">Fecha en formato YYMMDDHHSS</param>
        /// <returns>Timestamp Unix equivalente</returns>
        public static uint YYMMDDHHSSToUnixTimestamp(string yymmddhhss)
        {
            try
            {
                if (string.IsNullOrEmpty(yymmddhhss) || yymmddhhss.Length != 10)
                {
                    throw new ArgumentException("El formato debe ser YYMMDDHHSS (10 dígitos)");
                }

                int yy = int.Parse(yymmddhhss.Substring(0, 2));
                int mm = int.Parse(yymmddhhss.Substring(2, 2));
                int dd = int.Parse(yymmddhhss.Substring(4, 2));
                int hh = int.Parse(yymmddhhss.Substring(6, 2));
                int ss = int.Parse(yymmddhhss.Substring(8, 2));

                // Validar componentes
                if (mm < 1 || mm > 12 || dd < 1 || dd > 31 || hh < 0 || hh > 23 || ss < 0 || ss > 59)
                {
                    throw new ArgumentException("Componentes de fecha inválidos");
                }

                // Convertir a fecha completa (asumiendo 2000 + yy para el año)
                DateTime dateTime = new DateTime(2000 + yy, mm, dd, hh, ss, 0);

                // Convertir a timestamp Unix
                return (uint)new DateTimeOffset(dateTime).ToUnixTimeSeconds();
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error al convertir fecha YYMMDDHHSS: {ex.Message}", LoggerType.Error, ex);
                return 0;
            }
        }

        /// <summary>
        /// Convierte un timestamp Unix a fecha en formato YYMMDDHHSS
        /// </summary>
        /// <param name="unixTimestamp">Timestamp Unix</param>
        /// <returns>Fecha en formato YYMMDDHHSS</returns>
        public static string UnixTimestampToYYMMDDHHSS(uint unixTimestamp)
        {
            try
            {
                DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);

                // Extraer componentes
                int yy = dateTime.Year - 2000; // Obtener solo los últimos 2 dígitos del año
                int mm = dateTime.Month;
                int dd = dateTime.Day;
                int hh = dateTime.Hour;
                int ss = dateTime.Minute;

                // Formatear como YYMMDDHHSS
                return $"{yy:D2}{mm:D2}{dd:D2}{hh:D2}{ss:D2}";
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error al convertir timestamp Unix a YYMMDDHHSS: {ex.Message}", LoggerType.Error, ex);
                return string.Empty;
            }
        }
    }
    /// <summary>
    /// Representa una carta del Pase de Batalla
    /// </summary>
    public class BattlePassCardData
    {
        // Propiedades estáticas compartidas entre todas las cartas
        public static bool EnablePremium { get; set; }
        public static uint StartDate { get; set; }
        public static uint EndDate { get; set; }

        // Propiedades de cada carta individual
        public int Number { get; set; }
        public int RequiredExp { get; set; }
        public int NormalCard { get; set; }
        public int PremiumCardA { get; set; }
        public int PremiumCardB { get; set; }
    }

    /// <summary>
    /// Clase raíz para deserializar el JSON de temporadas
    /// </summary>
    public class BattlePassSeasons
    {
        public List<BattlePassSeason> Seasons { get; set; }
    }

    /// <summary>
    /// Representa una temporada completa del Pase de Batalla
    /// </summary>
    public class BattlePassSeason
    {
        public string SeasonId { get; set; }
        public int SeasonEnabled { get; set; }
        // Optional server-owned daily season-point cap (0 = no cap). Not read by the
        // 122 client; purely a balance lever enforced in CalculateSeasonExpPoints.
        public int MaxDailyPoints { get; set; }
        public bool SeasonEnabledForFree { get; set; }
        public bool SeasonEnabledForPremium { get; set; }
        public string SeasonName { get; set; }
        public string SeasonDescription { get; set; }
        public string SeasonPrice { get; set; }
        public string SeasonStartDate { get; set; }
        public string SeasonEndDate { get; set; }
        public SeasonExpValuesJson SeasonExpValues { get; set; }
        public List<BattlePassCardData> Cards { get; set; } // Add this property
    }

    /// <summary>
    /// Representa los valores de experiencia de una temporada
    /// </summary>
    public class SeasonExpValuesJson
    {
        public List<CardJson> Card { get; set; }
    }

    /// <summary>
    /// Representa una carta en formato JSON
    /// </summary>
    public class CardJson
    {
        public string RequiredExp { get; set; }
        public string NormalCard { get; set; }
        public string PremiumCardA { get; set; }
        public string PremiumCardB { get; set; }
        public string _Number { get; set; }
    }

    #endregion

    #region BattlePass Core

    /// <summary>
    /// Clase principal que contiene la lógica del sistema de Pase de Batalla
    /// </summary>
    public static class BattlePass
    {
        private const string DEFAULT_SEASON_ID = "1";
        private const int DEFAULT_LEVEL = 1;
        private const int DEFAULT_EXP = 500;

        /// <summary>
        /// Obtiene el Pase de Batalla de una temporada específica
        /// </summary>
        /// <param name="SeasonId">ID de la temporada</param>
        /// <returns>Instancia de BattlePassModel correspondiente a la temporada</returns>
        public static BattlePassSeason GetSeasonPass(int SeasonId)
        {
            lock (new object()) // Cambiado para evitar el bloqueo en una lista nula
            {
                foreach (BattlePassSeason season in BattlePassLoader.LoadAllSeasons())
                {
                    if (int.TryParse(season.SeasonId, out int parsedSeasonId) && parsedSeasonId == SeasonId)
                    {
                        // Convertir BattlePassSeason a BattlePassModel
                        return new BattlePassSeason
                        {
                            SeasonId = season.SeasonId, // Use SeasonId instead of Id
                            SeasonName = season.SeasonName,
                            SeasonStartDate = season.SeasonStartDate,
                            SeasonEndDate = season.SeasonEndDate,
                            SeasonEnabled = season.SeasonEnabled,
                            Cards = BattlePassManager.GetCards()
                        };
                    }
                }

                return null;
            }
        }


        /// <summary>
        /// Carga los datos del Pase de Batalla para la temporada actual (por defecto la primera)
        /// </summary>
        /// <returns>Lista de cartas/niveles del Pase de Batalla</returns>
        public static List<BattlePassCardData> LoadBattlePassData()
        {
            return BattlePassLoader.Load();
        }

        /// <summary>
        /// Obtiene el último nivel completado por el jugador
        /// </summary>
        /// <param name="playerExp">Experiencia total del jugador</param>
        /// <param name="cardDataList">Lista de cartas/niveles del pase de batalla</param>
        /// <returns>El número del último nivel completado, o 0 si no ha completado ninguno</returns>

        /// <summary>
        /// Obtiene el último nivel completado por el jugador basado en su experiencia total
        /// </summary>
        public static int GetCompletedLevel(int playerExp, List<BattlePassCardData> cardDataList)
        {
            if (playerExp <= 0 || cardDataList == null || cardDataList.Count == 0)
                return 0;

            int lastCompletedLevel = 0;
            var orderedCards = cardDataList.OrderBy(card => card.Number).ToList();

            foreach (var card in orderedCards)
            {
                // RequiredExp es acumulativo, comparar directamente
                if (playerExp >= card.RequiredExp)
                {
                    lastCompletedLevel = card.Number;
                }
                else
                {
                    // No tiene suficiente exp para este nivel
                    break;
                }
            }

            return lastCompletedLevel;
        }

        /// <summary>
        /// Obtiene el nivel actual en el que está trabajando el jugador
        /// </summary>
        public static int GetCurrentLevel(int playerExp, List<BattlePassCardData> cardDataList)
        {
            try
            {
                if (playerExp <= 0 || cardDataList == null || cardDataList.Count == 0)
                    return DEFAULT_LEVEL;

                var orderedCards = cardDataList.OrderBy(card => card.Number).ToList();

                // Si el jugador completó todos los niveles
                var lastCard = orderedCards.Last();
                if (playerExp >= lastCard.RequiredExp)
                {
                    return lastCard.Number; // Nivel máximo alcanzado
                }

                // Buscar el nivel en el que está trabajando
                foreach (var card in orderedCards)
                {
                    if (playerExp < card.RequiredExp)
                    {
                        return card.Number; // Este es el nivel que está intentando alcanzar
                    }
                }

                return DEFAULT_LEVEL;
            }
            catch (Exception ex)
            {
                CLogger.Print("Error en GetCurrentLevel: " + ex.Message, LoggerType.Error, ex);
                return DEFAULT_LEVEL;
            }
        }

        /// <summary>
        /// Obtiene información detallada del nivel actual del jugador
        /// </summary>
        public static (int currentLevel, int completedLevel, int expInCurrentLevel, int expNeededForLevel, int totalExpRequired) GetLevelInfo(int playerExp, List<BattlePassCardData> cardDataList)
        {
            if (cardDataList == null || cardDataList.Count == 0)
                return (1, 0, 0, 500, 500);

            var orderedCards = cardDataList.OrderBy(card => card.Number).ToList();

            int currentLevel = GetCurrentLevel(playerExp, cardDataList);
            int completedLevel = GetCompletedLevel(playerExp, cardDataList);

            // Si está en nivel máximo
            if (currentLevel == orderedCards.Last().Number && playerExp >= orderedCards.Last().RequiredExp)
            {
                return (currentLevel, completedLevel, 0, 0, orderedCards.Last().RequiredExp);
            }

            // Obtener experiencia requerida para el nivel actual
            var currentCard = orderedCards.FirstOrDefault(c => c.Number == currentLevel);
            if (currentCard == null)
                return (1, 0, 0, 500, 500);

            // Experiencia del nivel anterior (si existe)
            int previousLevelExp = 0;
            if (currentLevel > 1)
            {
                var previousCard = orderedCards.FirstOrDefault(c => c.Number == currentLevel - 1);
                if (previousCard != null)
                    previousLevelExp = previousCard.RequiredExp;
            }

            // Calcular experiencia dentro del nivel actual
            int expInCurrentLevel = playerExp - previousLevelExp;
            int expNeededForLevel = currentCard.RequiredExp - previousLevelExp;
            int totalExpRequired = currentCard.RequiredExp;

            return (currentLevel, completedLevel, expInCurrentLevel, expNeededForLevel, totalExpRequired);
        }

        /// <summary>
        /// Calcula el porcentaje de progreso en el nivel actual
        /// </summary>
        public static float GetProgressPercentage(int playerExp, List<BattlePassCardData> cardDataList)
        {
            var (currentLevel, _, expInCurrentLevel, expNeededForLevel, _) = GetLevelInfo(playerExp, cardDataList);

            if (expNeededForLevel == 0)
                return 1.0f; // Nivel máximo alcanzado

            return (float)expInCurrentLevel / expNeededForLevel;
        }

        /// <summary>
        /// Obtiene la información para la temporada específica (compatible con código existente)
        /// </summary>
        public static (int currentLevel, int completedLevel, float progressPercentage, int expInCurrentLevel, int requiredExp) GetLevelInfoForSeason(int playerExp)
        {
            // Cargar los datos del Battle Pass actual
            var cardDataList = BattlePass.LoadBattlePassData();

            var (currentLevel, completedLevel, expInCurrentLevel, expNeededForLevel, totalExpRequired) = GetLevelInfo(playerExp, cardDataList);
            float progressPercentage = GetProgressPercentage(playerExp, cardDataList);

            return (currentLevel, completedLevel, progressPercentage, expInCurrentLevel, totalExpRequired);
        }

        /// <summary>
        /// Verifica si una temporada está activa actualmente
        /// </summary>
        /// <param name="seasonId">ID de la temporada a verificar</param>
        /// <returns>True si la temporada está activa, False en caso contrario</returns>
        public static bool IsSeasonActive(string seasonId = DEFAULT_SEASON_ID)
        {
            try
            {
                var season = BattlePassLoader.GetSeasonById(seasonId);
                if (season == null)
                {
                    return false;
                }

                uint currentTime = GetCurrentUnixTimestamp();

                if (!uint.TryParse(season.SeasonStartDate, out uint startDate) ||
                    !uint.TryParse(season.SeasonEndDate, out uint endDate))
                {
                    CLogger.Print($"BattlePass: season '{seasonId}' has malformed dates ('{season.SeasonStartDate}'..'{season.SeasonEndDate}'), treating as inactive", LoggerType.Warning);
                    return false;
                }

                return currentTime >= startDate && currentTime <= endDate;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error en IsSeasonActive: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        /// <summary>
        /// Obtiene el timestamp Unix actual
        /// </summary>
        /// <returns>Timestamp Unix actual en segundos</returns>
        private static uint GetCurrentUnixTimestamp()
        {
            return (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    #endregion

    #region BattlePass Loader

    /// <summary>
    /// Clase responsable de cargar y deserializar los datos del Pase de Batalla
    /// </summary>
    public static class BattlePassLoader
    {
        private const string FILE_PATH = "Data/BattlepassInfo.json";

        /// <summary>
        /// Carga los datos del Pase de Batalla para la temporada actual (por defecto la primera)
        /// </summary>
        /// <returns>Lista de cartas/niveles del Pase de Batalla</returns>
        public static List<BattlePassCardData> Load()
        {
            try
            {
                List<BattlePassSeason> seasons = ReadSeasons();

                if (seasons.Count == 0)
                {
                    CLogger.Print("BattlePass: No seasons found in the DB or the JSON file", LoggerType.Error);
                    return new List<BattlePassCardData>();
                }

                // Por ahora tomamos solo la primera temporada (se puede modificar para manejar múltiples temporadas)
                BattlePassSeason currentSeason = seasons[0];

                // Configuramos datos estáticos

                BattlePassCardData.EnablePremium = currentSeason.SeasonEnabledForPremium;
                BattlePassCardData.StartDate = DateConverter.ParseOrZero(currentSeason.SeasonStartDate, currentSeason.SeasonId, nameof(BattlePassSeason.SeasonStartDate));
                BattlePassCardData.EndDate = DateConverter.ParseOrZero(currentSeason.SeasonEndDate, currentSeason.SeasonId, nameof(BattlePassSeason.SeasonEndDate));


                //CLogger.Print($"Success load Season {currentSeason.SeasonName} {currentSeason.SeasonId}",LoggerType.Debug);
                // Convertimos las tarjetas del JSON a nuestro formato interno
                return ConvertCardsFromSeason(currentSeason);
            }
            catch (Exception ex)
            {
                CLogger.Print($"BattlePass {ex}", LoggerType.Error, ex);
                return new List<BattlePassCardData>(); // Devolvemos lista vacía para evitar referencias nulas
            }
        }

        /// <summary>
        /// Carga y deserializa el archivo JSON de temporadas
        /// </summary>
        /// <param name="filePath">Ruta al archivo JSON</param>
        /// <returns>Objeto BattlePassSeasons con todas las temporadas</returns>
        private static BattlePassSeasons LoadSeasonsFromJson(string filePath)
        {
            string jsonContent = File.ReadAllText(filePath);
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<BattlePassSeasons>(jsonContent, options);
        }

        /// <summary>
        /// Fuente única de temporadas: primero el banco, y el JSON sólo como respaldo para una
        /// instalación cuya migración todavía no rodó.
        /// </summary>
        private static List<BattlePassSeason> ReadSeasons()
        {
            List<BattlePassSeason> fromDb = DaoManagerSQL.GetBattlePassSeasons();

            if (fromDb != null && fromDb.Count > 0)
            {
                return fromDb;
            }

            CLogger.Print($"BattlePass: no seasons in system_battlepass_seasons, falling back to {FILE_PATH}", LoggerType.Warning);
            BattlePassSeasons seasons = LoadSeasonsFromJson(FILE_PATH);
            return seasons?.Seasons ?? new List<BattlePassSeason>();
        }

        /// <summary>
        /// Convierte las cartas de una temporada desde formato JSON al formato interno
        /// </summary>
        /// <param name="season">Temporada con las cartas a convertir</param>
        /// <returns>Lista de cartas en formato interno</returns>
        public static List<BattlePassCardData> ConvertCardsFromSeason(BattlePassSeason season)
        {
            List<BattlePassCardData> cardDataList = new List<BattlePassCardData>();

            if (season == null || season.SeasonExpValues == null || season.SeasonExpValues.Card == null)
            {
                return cardDataList;
            }

            foreach (var jsonCard in season.SeasonExpValues.Card)
            {
                BattlePassCardData cardData = new BattlePassCardData
                {
                    Number = int.Parse(jsonCard._Number),
                    RequiredExp = int.Parse(jsonCard.RequiredExp),
                    NormalCard = int.Parse(jsonCard.NormalCard),
                    PremiumCardA = int.Parse(jsonCard.PremiumCardA),
                    PremiumCardB = int.Parse(jsonCard.PremiumCardB)
                };

                cardDataList.Add(cardData);
            }

            return cardDataList;
        }

        /// <summary>
        /// Obtiene una temporada específica por su ID
        /// </summary>
        /// <param name="seasonId">ID de la temporada a buscar</param>
        /// <returns>La temporada con el ID especificado, o null si no se encuentra</returns>
        public static BattlePassSeason GetSeasonById(string seasonId)
        {
            return ReadSeasons().FirstOrDefault(s => s.SeasonId == seasonId);
        }

        /// <summary>
        /// Carga todas las temporadas disponibles
        /// </summary>
        /// <returns>Lista de todas las temporadas</returns>
        public static List<BattlePassSeason> LoadAllSeasons()
        {
            return ReadSeasons();
        }

        /// <summary>
        /// Verifica si una cadena está en formato YYMMDDHHSS (10 dígitos)
        /// </summary>
        private static bool IsYYMMDDHHSSFormat(string date)
        {
            if (string.IsNullOrEmpty(date) || date.Length != 10)
                return false;

            // Verificar que son todo dígitos
            foreach (char c in date)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            // Extraer componentes
            int yy = int.Parse(date.Substring(0, 2));
            int mm = int.Parse(date.Substring(2, 2));
            int dd = int.Parse(date.Substring(4, 2));
            int hh = int.Parse(date.Substring(6, 2));
            int ss = int.Parse(date.Substring(8, 2));

            // Verificar que los componentes son válidos para una fecha
            return mm >= 1 && mm <= 12 && dd >= 1 && dd <= 31 && hh >= 0 && hh <= 23 && ss >= 0 && ss <= 59;
        }

        /// <summary>
        /// Obtiene la temporada activa actualmente
        /// </summary>
        /// <returns>La primera temporada activa encontrada, o null si no hay ninguna activa</returns>
        public static BattlePassSeason GetActiveSeasons()
        {
            try
            {
                List<BattlePassSeason> allSeasons = LoadAllSeasons();
                uint currentTime = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();

               // CLogger.Print($"GetActiveSeasons -> Tiempo actual: {currentTime} | Fecha: {DateTimeOffset.Now}", LoggerType.Debug);

                foreach (var season in allSeasons)
                {
                    if (string.IsNullOrEmpty(season.SeasonStartDate) || string.IsNullOrEmpty(season.SeasonEndDate))
                    {
                        //CLogger.Print($"Temporada {season.SeasonId} tiene fechas inválidas", LoggerType.Warning);
                        continue;
                    }

                    // Convertir fechas si están en formato YYMMDDHHSS
                    uint startDate, endDate;

                    if (season.SeasonStartDate.Length == 10 && IsYYMMDDHHSSFormat(season.SeasonStartDate))
                    {
                        startDate = DateConverter.YYMMDDHHSSToUnixTimestamp(season.SeasonStartDate);
                    }
                    else if (!uint.TryParse(season.SeasonStartDate, out startDate))
                    {
                        CLogger.Print($"BattlePass: season '{season.SeasonId}' has malformed SeasonStartDate ('{season.SeasonStartDate}'), skipping season", LoggerType.Warning);
                        continue;
                    }

                    if (season.SeasonEndDate.Length == 10 && IsYYMMDDHHSSFormat(season.SeasonEndDate))
                    {
                        endDate = DateConverter.YYMMDDHHSSToUnixTimestamp(season.SeasonEndDate);
                    }
                    else if (!uint.TryParse(season.SeasonEndDate, out endDate))
                    {
                        CLogger.Print($"BattlePass: season '{season.SeasonId}' has malformed SeasonEndDate ('{season.SeasonEndDate}'), skipping season", LoggerType.Warning);
                        continue;
                    }

                    // Convertir a fechas legibles para una mejor depuración
                    DateTimeOffset startDateTime = DateTimeOffset.FromUnixTimeSeconds(startDate);
                    DateTimeOffset endDateTime = DateTimeOffset.FromUnixTimeSeconds(endDate);

                    //CLogger.Print($"GetActiveSeasons -> Analizando: {season.SeasonName} (ID: {season.SeasonId})", LoggerType.Debug);
                    //CLogger.Print($"  - Tiempo actual: {currentTime} ({DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss})", LoggerType.Debug);
                    //CLogger.Print($"  - StartDate: {startDate} ({startDateTime:yyyy-MM-dd HH:mm:ss})", LoggerType.Debug);
                    //CLogger.Print($"  - EndDate: {endDate} ({endDateTime:yyyy-MM-dd HH:mm:ss})", LoggerType.Debug);
                    //CLogger.Print($"  - ¿Enabled?: {season.SeasonEnabled}", LoggerType.Debug);

                    // Verificar si la fecha actual está entre el inicio y fin, y si la temporada está habilitada
                    bool isActive = currentTime >= startDate && currentTime <= endDate && season.SeasonEnabled == 1;
                    //CLogger.Print($"  - ¿Activa?: {isActive}", LoggerType.Debug);

                    if (isActive)
                    {
                        CLogger.Print($"active season found: {season.SeasonName} (ID: {season.SeasonId})", LoggerType.Info);
                        return season;
                    }
                }

                CLogger.Print("No active season was found at this time", LoggerType.Warning);
                return null;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error en GetActiveSeasons: {ex.Message}", LoggerType.Error, ex);
                return null;
            }
        }
    }

    #endregion
}