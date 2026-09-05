using Plugin.Core;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Data.Utils
{
    /// <summary>
    /// Simulacoes de eventos de jogo disparadas fora do fluxo natural, para teste e diagnostico.
    /// Cada funcao aqui orquestra as MESMAS primitivas de producao que o fluxo real usa,
    /// para que o resultado observado no cliente valha como prova do caminho real.
    /// </summary>
    public static class AdvancedSimulation
    {
        /// <summary>
        /// Executa uma promocao de rank sem partida, reproduzindo o bloco de rank-up de
        /// RoomModel.CalculateBattleResults: recompensas do tier concluido, ouro do rank,
        /// incremento do rank, persistencia e o PROTOCOL_BASE_RANK_UP_ACK.
        /// Ignora de proposito o gate de experiencia, que e justamente o que se quer contornar.
        /// </summary>
        /// <returns>Mensagem de resultado para o painel de log.</returns>
        public static string SimulateRankUp(Account player)
        {
            if (player == null)
                return "SimulateRankUp: player nao encontrado ou offline.";

            int previousRank = player.Rank;
            if (previousRank > 50)
                return $"SimulateRankUp: rank {previousRank} acima do teto de promocao (50).";

            RankModel rank = PlayerRankXML.GetRank(previousRank);
            if (rank == null)
                return $"SimulateRankUp: rank {previousRank} nao existe em Player.xml.";

            int granted = AllUtils.GrantRankRewards(player, previousRank);

            player.Gold += rank.OnGoldUp;
            player.LastRankUpDate = uint.Parse(DateTime.Now.ToString("yyMMddHHmm"));
            player.Rank = previousRank + 1;

            // ponytail: tres updates de coluna unica, mesma forma ja usada por RconRankAdmin.
            ComDiv.UpdateDB("accounts", "rank", player.Rank, "player_id", player.PlayerId);
            ComDiv.UpdateDB("accounts", "gold", player.Gold, "player_id", player.PlayerId);
            ComDiv.UpdateDB("accounts", "last_rank_update", (long)player.LastRankUpDate, "player_id", player.PlayerId);

            SendItemInfo.LoadGoldCash(player);
            player.SendPacket(new PROTOCOL_BASE_RANK_UP_ACK(player.Rank, previousRank, rank.OnGoldUp));
            player.Room?.UpdateSlotsInfo();

            // ponytail: experiencia nao e tocada, entao a barra de exp fica atras do novo rank.
            return $"SimulateRankUp: {player.Nickname} {previousRank} -> {player.Rank}, {granted} item(s) entregue(s), +{rank.OnGoldUp} gold.";
        }

        private static readonly Dictionary<long, KeyValuePair<object, int>> SimulatedInventoryPlus =
            new Dictionary<long, KeyValuePair<object, int>>();

        public static string SimulateInventoryExtend(Account player, int slots)
        {
            if (player == null)
                return "SimulateInventoryExtend: player nao encontrado ou offline.";

            if (player.Connection == null)
                return $"SimulateInventoryExtend: {player.Nickname} nao esta conectado ao Game, nada foi enviado.";

            if (slots <= 0)
                return $"SimulateInventoryExtend: slots {slots} precisa ser positivo.";

            lock (SimulatedInventoryPlus)
            {
                KeyValuePair<object, int> tracked;
                int alreadySimulated = SimulatedInventoryPlus.TryGetValue(player.PlayerId, out tracked)
                    && ReferenceEquals(tracked.Key, player.Connection)
                    ? tracked.Value
                    : 0;

                if (!AllUtils.CanExtendInventory(player, alreadySimulated + slots))
                {
                    int headroom = AllUtils.InventoryMaxPlus - player.InventoryPlus - alreadySimulated;
                    return $"SimulateInventoryExtend: slots {slots} fora da faixa 1..{headroom} (plus {player.InventoryPlus}, ja simulado nesta sessao {alreadySimulated}, teto {AllUtils.InventoryMaxPlus}).";
                }

                player.SendPacket(new PACKET_INVENTORY_MAX_UP_ACK(player, (ushort)slots));
                SimulatedInventoryPlus[player.PlayerId] =
                    new KeyValuePair<object, int>(player.Connection, alreadySimulated + slots);
                return $"SimulateInventoryExtend: 3337 enviado para {player.Nickname} com delta {slots}, total simulado nesta sessao {alreadySimulated + slots} (nada gravado).";
            }
        }

        /// <summary>
        /// Envia os dois carriers do bloco USER_INFO_BASIC que so aparecem quando um jogador
        /// clica em OUTRO jogador dentro de uma sala: ROOM_GET_PLAYERINFO_ACK (3597) e
        /// ROOM_GET_ACEMODE_PLAYERINFO_ACK (3682). O alvo e o proprio jogador, entao nenhum
        /// estado muda; o objetivo e por os bytes reais desses writers na captura para o
        /// oraculo validar (check_live_myinfo_2371.py --require 3597,3682) sem precisar de um
        /// segundo cliente na sala. So o gatilho e artificial: os writers sao os de producao.
        ///
        /// `only` restringe a um dos dois. Isso importa porque os dois handlers do cliente
        /// (Room__HandleGetPlayerInfoAck 0xEDC0B0 e sub_EDC108) copiam o mesmo bloco de 185B
        /// para o mesmo singleton dword_15E9C38: mandar os dois juntos torna impossivel saber
        /// qual deles produziu o conteudo. Cada um grava tambem um segundo bloco em offset
        /// proprio (3597: 96B em +185; 3682: 36B em +281), e e por esse segundo bloco que se
        /// distingue quem rodou.
        /// </summary>
        public static string SimulateUserInfoCarriers(Account player, int only = 0)
        {
            if (player == null)
                return "SimulateUserInfoCarriers: player nao encontrado ou offline.";

            if (player.Connection == null)
                return $"SimulateUserInfoCarriers: {player.Nickname} nao esta conectado ao Game.";

            if (only != 0 && only != 3597 && only != 3682)
                return $"SimulateUserInfoCarriers: only={only} invalido, use 3597, 3682 ou 0 para os dois.";

            if (only != 3682)
                player.SendPacket(new PROTOCOL_ROOM_GET_PLAYERINFO_ACK(player));
            if (only != 3597)
                player.SendPacket(new PROTOCOL_ROOM_GET_ACEMODE_PLAYERINFO_ACK(player));

            string sent = only == 0 ? "3597 e 3682" : only.ToString();
            return $"SimulateUserInfoCarriers: {sent} enviado(s) para {player.Nickname} (nada gravado).";
        }
    }
}
