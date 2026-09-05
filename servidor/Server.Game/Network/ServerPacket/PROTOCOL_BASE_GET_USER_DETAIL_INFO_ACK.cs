using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using System;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_GET_USER_DETAIL_INFO_ACK : GameServerPacket
    {
        private const ushort PROTOCOL_ID = 2438;
        private const int NICKNAME_LENGTH = 66;
        private const int CLAN_NAME_LENGTH = 34;
        private const int STRUCT_TAIL_LENGTH = 1840;

        private readonly uint _errorCode;
        private readonly Account _account;
        private readonly StatisticSeason _seasonStats;

        public PROTOCOL_BASE_GET_USER_DETAIL_INFO_ACK(uint error, Account account, int characterId)
        {
            _errorCode = error;
            _account = account;

            if (account != null)
            {
                PlayerStatistic statistic = account.Statistic ?? new PlayerStatistic();
                _seasonStats = statistic.Season ?? new StatisticSeason();
            }
        }

        // Client 122 contract (S2MO PACKET_BASE_GET_USER_DETAIL_INFO_ACK, ctor 0xEB47A4,
        // handler 0xEC0C88): after H opcode, H pad, D status, US2_USER_DETAIL_INFO is a
        // 2319-byte flat struct consumed at fixed offsets. Offsets below are the wire
        // offsets the handler reads (object-relative, verified against the decompile):
        //   12 Q playerId, 20 nickname[66], 86 clanName[34], 120 D clanLogo,
        //   124 C clanEffect, 125 C clanRole, 134 season stats[10 D],
        //   174 equipment grid[6x45], 444-446 colors, 447 H crosshair, 449 H muzzle,
        //   470 C nickBorder, 471/475 D fakeRank. Everything else is unread.
        // The previous writer emitted 1460 struct bytes with a field order that matched
        // none of these offsets, shorting the client 859 bytes and desyncing the stream.
        public override void Write()
        {
            try
            {
                WriteH(PROTOCOL_ID);
                WriteH(0);
                WriteD(_errorCode);

                if (_errorCode != 0)
                    return;

                WriteB(new byte[12]);
                WriteQ(_account.PlayerId);
                WriteU(_account.Nickname, NICKNAME_LENGTH);

                ClanModel clan = ClanManager.GetClan(_account.ClanId);
                WriteU(clan?.Name ?? string.Empty, CLAN_NAME_LENGTH);
                WriteD(clan?.Logo ?? 0);
                WriteC((byte)(clan?.Effect ?? 0));
                WriteC(0);

                WriteD(0);
                WriteD(0);

                WriteSeasonStatistics();

                WriteB(new byte[270]);

                WriteC((byte)_account.NickColor);
                WriteC(0);
                WriteC(0);
                WriteH((short)_account.Bonus.CrosshairColor);
                WriteH((short)_account.Bonus.MuzzleColor);
                WriteB(new byte[19]);
                WriteC((byte)_account.Bonus.NickBorderColor);
                WriteD(_account.Bonus.FakeRank);
                WriteD(_account.Bonus.FakeRank);

                WriteB(new byte[STRUCT_TAIL_LENGTH]);
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_BASE_GET_USER_DETAIL_INFO_ACK: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private void WriteSeasonStatistics()
        {
            WriteD(_seasonStats.Matches);
            WriteD(_seasonStats.MatchWins);
            WriteD(_seasonStats.MatchDraws);
            WriteD(_seasonStats.MatchLoses);
            WriteD(_seasonStats.EscapesCount);
            WriteD(_seasonStats.KillsCount);
            WriteD(_seasonStats.DeathsCount);
            WriteD(_seasonStats.HeadshotsCount);
            WriteD(_seasonStats.AssistsCount);
            WriteD(_seasonStats.MvpCount);
        }
    }
}
