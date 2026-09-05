//// Decompiled with JetBrains decompiler
//// Type: Plugin.Core.Models.ClanModel
//// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
//// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
//// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

//using Plugin.Core.Enums;
//using Plugin.Core.SQL;

//namespace Plugin.Core.Models
//{
//    public class ClanModel
//    {
//        public int Id;
//        public int Matches;
//        public int MatchWins;
//        public int MatchLoses;
//        public int TotalKills;
//        public int TotalHeadshots;
//        public int TotalDeaths;
//        public int TotalAssists;
//        public int TotalEscapes;
//        public int Authority;
//        public int RankLimit;
//        public int MinAgeLimit;
//        public int MaxAgeLimit;
//        public int Exp;
//        public int Rank;
//        public int NameColor;
//        public int MaxPlayers;
//        public int Effect;
//        public string Name;
//        public string Info;
//        public string News;
//        public long OwnerId;
//        public uint Logo;
//        public uint CreationDate;
//        public float Points;
//        public JoinClanType JoinType;
//        public ClanBestPlayers BestPlayers = new ClanBestPlayers();
//        public int SeasonRank { get; set; }
//        public int PreviousSeasonRank { get; set; }

//        public ClanModel()
//        {
//            this.MaxPlayers = 50;
//            this.Logo = uint.MaxValue;
//            this.Name = "";
//            this.Info = "";
//            this.News = "";
//            this.Points = 1000f;
//        }

//        public int GetClanUnit() => this.GetClanUnit(DaoManagerSQL.GetClanPlayers(this.Id));

//        public int GetClanUnit(int Count)
//        {
//            if (Count >= 250)
//                return 7;
//            if (Count >= 200)
//                return 6;
//            if (Count >= 150)
//                return 5;
//            if (Count >= 100)
//                return 4;
//            if (Count >= 50)
//                return 3;
//            if (Count >= 30)
//                return 2;
//            return Count >= 10 ? 1 : 0;
//        }
//    }
//}

// PASO 4: Reemplazar tu ClanModel.cs con este código completo

using Plugin.Core.Enums;
using Plugin.Core.SQL;
using System;

namespace Plugin.Core.Models
{
    public class ClanModel
    {
        // Propiedades principales existentes
        public int Id;
        public int Matches;
        public int MatchWins;
        public int MatchLoses;
        public int Authority;
        public int RankLimit;
        public int MinAgeLimit;
        public int MaxAgeLimit;
        public int Exp;
        public int Rank;
        public int NameColor;
        public int MaxPlayers;
        public int Effect;
        public string Name;
        public string Info;
        public string News;
        public long OwnerId;
        public uint Logo;
        public uint CreationDate;
        public float Points;
        public JoinClanType JoinType;
        public ClanBestPlayers BestPlayers = new ClanBestPlayers();

        // Estadísticas históricas totales (desde la BD)
        public int TotalKills { get; set; }
        public int TotalAssists { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalHeadshots { get; set; }
        public int TotalEscapes { get; set; }

        // Rankings de temporadas (desde la BD)
        public int SeasonRank { get; set; }
        public int PreviousSeasonRank { get; set; }

        // Cache de estadísticas recientes (opcional para optimización)
        public DateTime LastStatsUpdate { get; set; }

        public ClanModel()
        {
            MaxPlayers = 50;
            Logo = 0xFFFFFFFF;
            Name = "";
            Info = "";
            News = "";
            Points = 1000;
            LastStatsUpdate = DateTime.MinValue;
        }

        public int GetClanUnit() => GetClanUnit(DaoManagerSQL.GetClanPlayers(Id));

        public int GetClanUnit(int Count)
        {
            if (Count >= 250)
            {
                return 7;
            }
            else if (Count >= 200)
            {
                return 6;
            }
            else if (Count >= 150)
            {
                return 5;
            }
            else if (Count >= 100)
            {
                return 4;
            }
            else if (Count >= 50)
            {
                return 3;
            }
            else if (Count >= 30)
            {
                return 2;
            }
            else if (Count >= 10)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}