using Plugin.Core.Enums;
using Plugin.Core;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using System;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_DETAIL_INFO_ACK : GameServerPacket
    {
        private readonly ClanModel Clan;
        private readonly int Error;
        private readonly Account Player;
        private readonly int Players;
        private readonly ClanSeasonalStats SeasonalStats;
        private readonly ClanWeeklyStats WeeklyStats;

        public PROTOCOL_CS_DETAIL_INFO_ACK(int Error, ClanModel Clan)
        {
            this.Error = Error;
            this.Clan = Clan;
            if (Clan != null)
            {
                Player = AccountManager.GetAccount(Clan.OwnerId, 31);
                Players = DaoManagerSQL.GetClanPlayers(Clan.Id);
                SeasonalStats = ClanManager.CalculateClanSeasonalStats(Clan.Id);
                WeeklyStats = ClanManager.CalculateClanWeeklyStats(Clan.Id);
            }
        }

        public override void Write()
        {
            WriteH(801);
            WriteD(Error);

            // Client (Clan__OnDetailInfoAck @0xEDEE90 / 122 sub_EE8707) reads result(4),
            // then memcpy of a fixed 1607-byte ClanDetailInfo blob (i3NetworkPacket::ReadData
            // is atomic + bound-checked: a short packet copies NOTHING). Body MUST be 1607B.
            // Error is the echoed client byte (0-255, always >= 0 -> client always reads the blob),
            // NOT a server error code, so never branch on it. Only guard a genuinely null clan.
            if (Clan == null)
            {
                WriteB(new byte[1607]);
                return;
            }

            // --- Header [0,1202) --- (display-verified layout)
            WriteD(Clan.Id);                  // @0    clan id (read by Clan__OnDetailInfoAck)
            WriteU(Clan.Name, 34);            // @4    name (17 wchars)
            WriteC((byte)Clan.Rank);          // @38   clan grade
            WriteC((byte)Players);            // @39   member count
            WriteC((byte)Clan.MaxPlayers);    // @40
            WriteD(Clan.CreationDate);        // @41
            WriteD(Clan.Logo);                // @45
            WriteC((byte)Clan.NameColor);     // @49
            WriteC((byte)Clan.Effect);        // @50
            WriteC((byte)Clan.GetClanUnit()); // @51
            WriteD(Clan.Exp);                 // @52
            WriteQ(Clan.OwnerId);             // @56
            WriteC(0);
            WriteC(0);
            WriteC(0);
            WriteC(0);                        // @64
            WriteB(ClanOwnerData(Player));    // @68   owner nick(66)+color+rank = 68B
            WriteU(Clan.Info, 510);           // @136  intro (255 wchars)
            WriteB(new byte[41]);             // @646
            WriteC((byte)Clan.JoinType);      // @687
            WriteC((byte)Clan.RankLimit);     // @688
            WriteC((byte)Clan.MaxAgeLimit);   // @689
            WriteC((byte)Clan.MinAgeLimit);   // @690
            WriteC((byte)Clan.Authority);     // @691
            WriteU(Clan.News, 510);           // @692  -> ends @1202

            // --- Summary [1202,1214) ---
            WriteD((int)Clan.Points);         // @1202
            WriteD(Clan.MatchWins);           // @1206
            WriteD(Clan.MatchLoses);          // @1210

            // --- Reserved pad to record base: 24 dwords so Matches lands @1310 ---
            // (client record struct base is blob+1302; UITabClanRecord__BuildStats reads
            //  matches@+1310, wins@+1314, loses@+1318, kills@+1330 ... drops@+1346)
            WriteB(new byte[96]);             // @1214 -> @1310

            // --- Record struct (fields the client actually reads) ---
            WriteD(Clan.Matches);             // @1310 TotalMatchCount
            WriteD(Clan.MatchWins);           // @1314 TotalWinCount
            WriteD(Clan.MatchLoses);          // @1318 TotalLoseCount
            WriteD(0);                        // @1322 (unread)
            WriteD(0);                        // @1326 (unread)
            WriteD(Clan.TotalKills);          // @1330 TotalKillCount
            WriteD(Clan.TotalAssists);        // @1334 TotalAssistCount
            WriteD(Clan.TotalDeaths);         // @1338 TotalDeathCount
            WriteD(Clan.TotalHeadshots);      // @1342 TotalHeadShotRatio input
            WriteD(Clan.TotalEscapes);        // @1346 TotalDropCount  -> ends @1350

            // --- Season / weekly / rank block [1350,1459) ---
            // Client does NOT read season from the blob (it comes via ClanContext+2228,
            // a separate packet); kept for fidelity. Client DOES read the clan-buff gauge
            // at blob+1446 (sub_9C349A / UITabClanTrophy): dword@1446, dword@1450, word@1454.
            WriteD(0);                                                // @1350
            WriteD(SeasonalStats?.CurrentSeason?.Medals ?? 0);        // @1354
            WriteD(SeasonalStats?.CurrentSeason?.Matches ?? 0);
            WriteD(SeasonalStats?.CurrentSeason?.MatchWins ?? 0);
            WriteD(SeasonalStats?.CurrentSeason?.MatchLoses ?? 0);
            WriteD(SeasonalStats?.CurrentSeason?.SeasonRank ?? 0);
            WriteD(SeasonalStats?.CurrentSeason?.PointsGained ?? 0);
            WriteD(SeasonalStats?.CurrentSeason?.TotalKills ?? 0);
            WriteD(SeasonalStats?.CurrentSeason?.TotalAssists ?? 0);
            WriteD(SeasonalStats?.CurrentSeason?.TotalDeaths ?? 0);
            WriteD(SeasonalStats?.CurrentSeason?.TotalHeadshots ?? 0);
            WriteD(SeasonalStats?.CurrentSeason?.TotalEscapes ?? 0);  // -> ends @1398
            WriteD(0);                                                // @1398
            WriteD(SeasonalStats?.PreviousSeason?.Medals ?? 0);       // @1402
            WriteD(SeasonalStats?.PreviousSeason?.Matches ?? 0);
            WriteD(SeasonalStats?.PreviousSeason?.MatchWins ?? 0);
            WriteD(SeasonalStats?.PreviousSeason?.MatchLoses ?? 0);
            WriteD(SeasonalStats?.PreviousSeason?.SeasonRank ?? 0);
            WriteD(SeasonalStats?.PreviousSeason?.PointsGained ?? 0);
            WriteD(SeasonalStats?.PreviousSeason?.TotalKills ?? 0);
            WriteD(SeasonalStats?.PreviousSeason?.TotalAssists ?? 0);
            WriteD(SeasonalStats?.PreviousSeason?.TotalDeaths ?? 0);
            WriteD(SeasonalStats?.PreviousSeason?.TotalHeadshots ?? 0);
            WriteD(SeasonalStats?.PreviousSeason?.TotalEscapes ?? 0); // -> ends @1446
            WriteD(WeeklyStats?.CurrentWeekMedals ?? 0);              // @1446 buff gauge
            WriteD(WeeklyStats?.PreviousWeekMedals ?? 0);             // @1450
            WriteC(0);                                                // @1454
            WriteD(Clan.Rank);                                        // @1455 -> ends @1459

            // --- Tail pad + clan-war region [1459,1607) ---
            // UITabClanInfo__UpdateClanResult reads @1541 war-declared flag, @1542 last result,
            // then 8 x 8-byte war-opponent entries at [1543,1607). No war data here -> zeros.
            WriteB(new byte[82]);             // @1459 -> @1541
            WriteC(0);                        // @1541 war declared
            WriteC(0);                        // @1542 last war result
            WriteB(new byte[64]);             // @1543 -> @1607  (8 x 8B opponents)
        }

        private byte[] ClanOwnerData(Account Player)
        {
            using (SyncServerPacket S = new SyncServerPacket())
            {
                if (Player != null)
                {
                    S.WriteU(Player.Nickname, 66);
                    S.WriteC((byte)Player.NickColor);
                    S.WriteC((byte)Player.Rank);
                }
                else
                {
                    S.WriteU("", 66);
                    S.WriteC(0);
                    S.WriteC(0);
                }
                return S.ToArray();
            }
        }
    }
}