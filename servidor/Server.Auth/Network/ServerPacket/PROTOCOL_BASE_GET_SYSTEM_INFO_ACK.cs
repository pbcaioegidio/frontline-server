using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Auth.Data.Models;
using Server.Auth.Data.XML;
using System;
using System.Collections.Generic;

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_GET_SYSTEM_INFO_ACK : AuthServerPacket
    {
        private const int DismantleSettingSize = 4;

        private readonly ServerConfig Field0;
        private readonly List<SChannelModel> Field1;
        private readonly List<RankModel> Field2;
        private readonly string[] Field4;
        private readonly int CardSetMask;

        public PROTOCOL_BASE_GET_SYSTEM_INFO_ACK(ServerConfig A_1)
            : this(A_1, null)
        {
        }

        public PROTOCOL_BASE_GET_SYSTEM_INFO_ACK(ServerConfig A_1, PlayerMissions A_2)
        {
            this.Field0 = A_1;
            this.CardSetMask = ComDiv.GetOwnedCardSetMask(A_2);
            if (A_1 != null)
            {
                this.Field1 = SChannelXML.Servers;
                this.Field2 = PlayerRankXML.Ranks;
            }
            this.Field4 = new string[2]
            {
                "ded9a5bc68c44c6b885ac376be4f08c6",
                "5c67549f9ea01f1c7429d2a6bb121844"
            };
        }

        // Wire layout reverse-engineered from the 117 RU client
        // (ClientTCPSocket::__Parse_Base_GetSystemInfo / S2MOPackable field chain).
        // Field order below = the client's sequential read order. Sizes are exact;
        // mismatching any size desyncs the whole packet (silent unpack failure).
        public override void Write()
        {
            this.WriteH((short)2315);   // opcode (source[2..3])
            this.WriteH((short)0);      // preamble word (source[4..5])

            // >0 deveria liberar o cadeado de Arma Especial 2 (Throw2PointSlotMaxDays).
            // Com 100 aqui o seletor do Aviso continua travado em 0, ou seja o campo que
            // o client usa como máximo está em outro lugar deste pacote. PB_SYSINFO_FILL
            // permite preencher blocos reservados para localizar por eliminação.
            this.WriteC(EnvByte("PB_THROW2_MAXDAYS", 100));             // head  throw2PointSlotMax    -> UISystemCtx, Throw2PointSlotMaxDays
            this.WriteB(Block("dismantle", DismantleSettingSize));      // 00    dismantleSetting      DISMANTLE_SETTING (4B)
            this.WriteC((byte)0);                                       // 01    reserved01            byte (client ignores)
            this.WriteC((byte)this.Field4[0].Length);                   // 02    missionCardFixHash    StringA<33> (MissionCardFix.dat)
            this.WriteS(this.Field4[0], this.Field4[0].Length);
            this.WriteC((byte)this.Field4[1].Length);                   // 03    missionCardChangeHash StringA<33> (MissionCardChange.dat)
            this.WriteS(this.Field4[1], this.Field4[1].Length);
            this.WriteD(0);                                             // 04    gameFrameworkDword    -> CGameFramework/match ctx +2252
            this.WriteC((byte)0);                                       // 05    gameFrameworkArr16    -> CGameFramework/match ctx (uint[16], empty)
            this.WriteH((short)0);                                      // 06    clanWord06            -> ClanContext +24
            this.WriteH((ushort)ConfigLoader.MaxActiveClans);           // 07    clanWord07            -> ClanContext +28 = clan-list slot-pool capacity (client pre-allocs N row slots at login; 0 = empty pool -> clan browse always shows "no result")
            this.WriteC((byte)0);                                       // 08    clanListFilter        -> ClanContext +56 (CLAN_LIST_FILTER_TYPE)
            this.WriteB(Block("shopctx", 5));                           // 09    shopCtxBlock5         -> Shop ctx (fixed 5B)
            this.WriteB(Block("penalty", 12));                          // 10    penalty               -> UISystemCtx +2072 (PENALTY, 3 dwords)
            this.WriteC((byte)0);                                       // 11    battleInviteByte      -> Battle ctx +508 & MainFrame/invite ctx +11818
            this.WriteH((short)0);                                      // 12    battleInviteWord      -> Battle ctx +504 & MainFrame/invite ctx +11816
            this.WriteB(Block("ticket", 6));                            // 13    baseInfoTicket        -> UISystemCtx +2066 (BASE_INFO_TICKET, gated bit19)
            this.WriteC((byte)0);                                       // 14    lobbySceneOpt         -> Lobby/Showroom ctx +232 (gated bit14, sibling of showroom+228)
            this.WriteB(Block("promotion", 373));                       // 15    promotionBlock        fixed 373B (FreeItemMgr promotion/event)
            this.WriteC((byte)this.Field0.Showroom);                    // 16    showroomTheme         -> Lobby/Showroom ctx +228 (gated bit3; ShowroomView 0-15)
            this.WriteC((byte)0);                                       // 17    uiSystemByte17        -> UISystemCtx [21] (+84)
            this.WriteC((byte)0);                                       // 18    reservedArr18         int[9] array (client ignores)
            this.WriteB(Block("reserved19", 3));                        // 19    reserved19            fixed 3B (client ignores)
            this.WriteB(Block("match", 84));                            // 20    matchSetting          -> CGameFramework/match ctx +809 (MATCH_SETTING, 84B)
            this.WriteC((byte)0);                                       // 21    randomBoxByte         -> RandomBox/box ctx +20
            this.WriteC((byte)0);                                       // 22    uiSystemByte22        -> UISystemCtx +1331
            this.WriteC((byte)8);                                       // 23    multiWeaponCount      -> UISystemCtx +1192 (121 = 8 slots, client clamps [1,10])
            this.WriteB(Block("gift", 109));                            // 24    giftBuyRanking        GIFT_BUY_RANKING (109B)
            this.WriteB(Block("clanseason", 120));                      // 25    clanMatchSeason       CLAN_MATCH_SEASON_EXT (120B)
            this.WriteC((byte)0);                                       // 26    uiSystemArr3          -> UISystemCtx (uint[3], empty)
            this.WriteC((byte)0);                                       // 27    reserved27            byte (client ignores)
            this.WriteD(this.CardSetMask);                              // 28    missionCardDword      -> MCardMgr +372 = owned card-set bitmask (bit i = CardSetId i); drives the mission-card tab combo list
            this.WriteH((short)0);                                      // 29    gamePort              m_ConnectionInfo.m_GamePort (0 = set at channel-select)
            this.WriteB(this.Method0(this.Field1));                     // 30    serverList            SChannel[max100,elem36]
            this.WriteC((byte)1);                                       // 31    nationList            SERVICE_COUNTRY_INFO[max7,elem2]: count
            this.WriteC((byte)this.NATIONS);                            //                             nation code low byte
            this.WriteC((byte)0);                                       //                             nation code high byte
            this.WriteH((ushort)this.Field0.ShopURL.Length);           // 32    shopUrl               StringA<255>: client reads a 2-byte length prefix (S2MOStringA<255>, max>=255 -> WORD count), NOT 1 byte
            this.WriteS(this.Field0.ShopURL, this.Field0.ShopURL.Length);
            this.WriteB(this.Method1(this.Field2));                      // 33    rankList              rank[56]: count byte + 56 slots x 18B (ushort rankId + 4x uint goodId); slot index = rank id
            this.WriteC((byte)0);                                       // 34    reserved34            byte/bool (client ignores)
        }

        private static byte EnvByte(string name, byte fallback)
        {
            string raw = Environment.GetEnvironmentVariable(name);
            return byte.TryParse(raw, out byte value) ? value : fallback;
        }

        /// <summary>
        /// Bloco reservado do SYSTEM_INFO. Normalmente zeros; se o nome estiver em
        /// PB_SYSINFO_FILL (lista separada por vírgula) sai preenchido com
        /// PB_SYSINFO_FILL_VALUE (padrão 99). Serve para localizar, por eliminação,
        /// qual campo o client usa como máximo do seletor de dias do Arma Especial 2 —
        /// sem precisar de rebuild, só restart do container.
        /// </summary>
        private static byte[] Block(string name, int size)
        {
            byte[] buffer = new byte[size];
            string list = Environment.GetEnvironmentVariable("PB_SYSINFO_FILL");
            if (string.IsNullOrEmpty(list))
                return buffer;

            if (Array.IndexOf(list.Split(','), name) < 0)
                return buffer;

            byte fill = EnvByte("PB_SYSINFO_FILL_VALUE", 99);
            for (int i = 0; i < size; i++)
                buffer[i] = fill;

            CLogger.Print($"SYSTEM_INFO bloco '{name}' preenchido com {fill} ({size}B)", LoggerType.Info);
            return buffer;
        }

        private byte[] Method0(List<SChannelModel> A_1)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)A_1.Count);
                foreach (SChannelModel schannelModel in A_1)
                {
                    syncServerPacket.WriteD(schannelModel.State ? 1 : 0);
                    syncServerPacket.WriteB(ComDiv.AddressBytes(schannelModel.Host));
                    syncServerPacket.WriteB(ComDiv.AddressBytes(schannelModel.Host));
                    syncServerPacket.WriteH(schannelModel.Port);
                    syncServerPacket.WriteC((byte)schannelModel.Type);
                    syncServerPacket.WriteH((ushort)schannelModel.MaxPlayers);
                    syncServerPacket.WriteD(schannelModel.LastPlayers);
                    if (schannelModel.Id == 0)
                    {
                        syncServerPacket.WriteB(Bitwise.HexStringToByteArray("01 01 01 01 01 01 01 01 01 01 0E 00 00 00 00"));
                    }
                    else
                    {
                        foreach (ChannelModel channel in ChannelsXML.GetChannels(schannelModel.Id))
                            syncServerPacket.WriteC((byte)channel.Type);
                        syncServerPacket.WriteC((byte)schannelModel.Type);
                        syncServerPacket.WriteC((byte)0);
                        syncServerPacket.WriteH((short)0);
                    }
                }
                return syncServerPacket.ToArray();
            }
        }

        // RANKUP_REWARD_ITEM[56], client __Parse_Base_GetSystemInfo (sub_EC01E6):
        // count byte + 56 fixed slots, slot index = rank id. Each slot = 18B:
        // ushort rankId (+0) then 4x uint goodId (+2/+6/+10/+14). The client reads the
        // 4 goodIds and resolves each in its goods catalog for the 2x2 rank-up reward grid.
        private byte[] Method1(List<RankModel> A_1)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                const int RankSlots = 56;
                syncServerPacket.WriteC((byte)RankSlots);
                for (int rankId = 0; rankId < RankSlots; ++rankId)
                {
                    syncServerPacket.WriteH((ushort)rankId);
                    int written = 0;
                    foreach (int itemId in PlayerRankXML.GetRewards(rankId))
                    {
                        if (written >= 4)
                            break;
                        GoodsItem good = ShopManager.GetItemId(itemId);
                        syncServerPacket.WriteD(good == null ? 0 : good.Id);
                        ++written;
                    }
                    for (; written < 4; ++written)
                        syncServerPacket.WriteD(0);
                }
                return syncServerPacket.ToArray();
            }
        }

    }
}
