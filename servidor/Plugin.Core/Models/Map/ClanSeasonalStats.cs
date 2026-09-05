// PASO 2: Crear este archivo o agregar estas clases a tu proyecto
// Ubicación sugerida: Plugin.Core.Models o donde tengas tus modelos

using System;

namespace Plugin.Core.Models
{
    /// <summary>
    /// Estadísticas estacionales del clan (mes actual y anterior)
    /// </summary>
    public class ClanSeasonalStats
    {
        public SeasonStats CurrentSeason { get; set; } = new SeasonStats();
        public SeasonStats PreviousSeason { get; set; } = new SeasonStats();
    }

    /// <summary>
    /// Estadísticas de una temporada específica
    /// </summary>
    public class SeasonStats
    {
        public int Matches { get; set; }
        public int MatchWins { get; set; }
        public int MatchLoses { get; set; }
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalAssists { get; set; }
        public int TotalHeadshots { get; set; }
        public int TotalEscapes { get; set; }
        public int Medals { get; set; }
        public int PointsGained { get; set; }
        public int SeasonRank { get; set; }
    }

    /// <summary>
    /// Estadísticas semanales del clan (semana actual y anterior)
    /// </summary>
    public class ClanWeeklyStats
    {
        public int CurrentWeekMedals { get; set; }
        public int PreviousWeekMedals { get; set; }
    }
}