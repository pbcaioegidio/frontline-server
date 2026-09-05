using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Server.Game.Network.ClientPacket
{
    /// <summary>
    /// Maneja las peticiones del cliente para cambiar el estado de un slot en una sala.
    /// Implementa protección contra procesamiento duplicado mediante debouncing.
    /// </summary>
    public class PROTOCOL_ROOM_CHANGE_SLOT_REQ : GameClientPacket
    {
        #region Constantes

        private const int SLOT_ID_MASK = 0x0FFFFFFF;
        private const int OPEN_SLOT_FLAG = 0x10000000;
        private const uint ERROR_CODE = 0x80000401;
        private const int DEBOUNCE_MILLISECONDS = 500; // 500ms entre peticiones del mismo jugador

        #endregion Constantes

        #region Sistema Anti-Duplicación

        // Almacena el timestamp de la última petición por jugador
        private static readonly ConcurrentDictionary<long, DateTime> _lastRequestTime =
            new ConcurrentDictionary<long, DateTime>();

        // Locks por jugador para evitar condiciones de carrera
        private static readonly ConcurrentDictionary<long, object> _playerLocks =
            new ConcurrentDictionary<long, object>();

        #endregion Sistema Anti-Duplicación

        #region Campos Privados

        private int _slotChangeRequest;
        private uint _errorCode;

        #endregion Campos Privados

        #region Métodos Públicos

        public override void Read()
        {
            _slotChangeRequest = ReadD();
        }

        public override void Run()
        {
            try
            {
                Account player = Client.GetAccount();
                if (!ValidatePlayer(player))
                {
                    SendErrorResponse();
                    return;
                }

                // Sistema de debouncing: prevenir procesamiento duplicado
                if (!TryAcquireRequestPermission(player.PlayerId))
                {
                    CLogger.Print(
                        $"[DEBOUNCE] Petición duplicada ignorada - Player: {player.PlayerId}, Slot: {_slotChangeRequest}",
                        LoggerType.Debug
                    );
                    return;
                }

                RoomModel room = player.Room;
                if (!ValidateRoomAndLeadership(room, player))
                {
                    SendErrorResponse();
                    return;
                }

                // Lock adicional a nivel de sala para prevenir cambios concurrentes
                if (room.ChangingSlots)
                {
                    CLogger.Print(
                        $"[CONCURRENCY] Sala ocupada procesando cambios - Room: {room.RoomId}",
                        LoggerType.Debug
                    );
                    SendErrorResponse();
                    return;
                }

                int slotId = ExtractSlotId(_slotChangeRequest);
                if (!room.GetSlot(slotId, out SlotModel slot))
                {
                    SendErrorResponse();
                    return;
                }

                ProcessSlotStateChange(room, slot, player);
                Client.SendPacket(new PROTOCOL_ROOM_CHANGE_SLOT_ACK(_errorCode));
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_ROOM_CHANGE_SLOT_REQ: {ex.Message}", LoggerType.Error, ex);
            }
        }

        #endregion Métodos Públicos

        #region Sistema Anti-Duplicación

        /// <summary>
        /// Verifica si el jugador puede hacer una nueva petición según el debounce time.
        /// Usa locks por jugador para thread-safety.
        /// </summary>
        private bool TryAcquireRequestPermission(long playerId)
        {
            // Obtener o crear un lock específico para este jugador
            object playerLock = _playerLocks.GetOrAdd(playerId, new object());

            lock (playerLock)
            {
                DateTime now = DateTimeUtil.Now();

                if (_lastRequestTime.TryGetValue(playerId, out DateTime lastRequest))
                {
                    TimeSpan timeSinceLastRequest = now - lastRequest;

                    // Si han pasado menos de DEBOUNCE_MILLISECONDS, rechazar
                    if (timeSinceLastRequest.TotalMilliseconds < DEBOUNCE_MILLISECONDS)
                    {
                        return false;
                    }
                }

                // Actualizar el timestamp de la última petición
                _lastRequestTime[playerId] = now;
                return true;
            }
        }

        /// <summary>
        /// Método estático para limpiar locks y timestamps de jugadores desconectados.
        /// Debe ser llamado periódicamente (ej: cada 5 minutos) para prevenir memory leaks.
        /// </summary>
        public static void CleanupInactivePlayers(TimeSpan inactivityThreshold)
        {
            DateTime cutoffTime = DateTimeUtil.Now() - inactivityThreshold;

            foreach (var kvp in _lastRequestTime)
            {
                if (kvp.Value < cutoffTime)
                {
                    _lastRequestTime.TryRemove(kvp.Key, out _);
                    _playerLocks.TryRemove(kvp.Key, out _);
                }
            }
        }

        #endregion Sistema Anti-Duplicación

        #region Validaciones

        private bool ValidatePlayer(Account player)
        {
            return player != null;
        }

        private bool ValidateRoomAndLeadership(RoomModel room, Account player)
        {
            return room != null && room.Leader == player.SlotId;
        }

        #endregion Validaciones

        #region Procesamiento de Estados

        private void ProcessSlotStateChange(RoomModel room, SlotModel slot, Account player)
        {
            // Marcar que se están procesando cambios (prevenir concurrencia a nivel de sala)
            lock (room.Slots)
            {
                if (room.ChangingSlots)
                {
                    _errorCode = ERROR_CODE;
                    return;
                }

                try
                {
                    room.ChangingSlots = true;

                    switch (slot.State)
                    {
                        case SlotState.EMPTY:
                            HandleEmptySlot(room, slot, player);
                            break;

                        case SlotState.CLOSE:
                            HandleClosedSlot(room, slot, player);
                            break;

                        case SlotState.UNKNOWN:
                        case SlotState.SHOP:
                        case SlotState.INFO:
                        case SlotState.CLAN:
                        case SlotState.INVENTORY:
                        case SlotState.GACHA:
                        case SlotState.GIFTSHOP:
                        case SlotState.NORMAL:
                        case SlotState.READY:
                            HandleOccupiedSlot(room, slot, player);
                            break;

                        case SlotState.LOAD:
                        case SlotState.RENDEZVOUS:
                        case SlotState.PRESTART:
                        case SlotState.BATTLE_LOAD:
                        case SlotState.BATTLE_READY:
                        case SlotState.BATTLE:
                            _errorCode = ERROR_CODE;
                            break;
                    }
                }
                finally
                {
                    room.ChangingSlots = false;
                }
            }
        }

        private void HandleEmptySlot(RoomModel room, SlotModel slot, Account player)
        {
            if (room.Competitive && !AllUtils.CanCloseSlotCompetitive(room, slot))
            {
                SendCompetitiveWarning(player, "CompetitiveSlotMin");
                return;
            }

            room.ChangeSlotState(slot, SlotState.CLOSE, true);
        }

        private void HandleClosedSlot(RoomModel room, SlotModel slot, Account player)
        {
            MapMatch mapLimit = SystemMapXML.GetMapLimit((int)room.MapId, (int)room.Rule);

            if (!CanOpenSlot(mapLimit, slot))
                return;

            if (room.Competitive && !AllUtils.CanOpenSlotCompetitive(room, slot))
            {
                SendCompetitiveWarning(player, "CompetitiveSlotMax");
                return;
            }

            if (CanOpenSlotInBotMode(room, slot))
            {
                room.ChangeSlotState(slot, SlotState.EMPTY, true);
            }
        }

        private void HandleOccupiedSlot(RoomModel room, SlotModel slot, Account player)
        {
            Account targetPlayer = room.GetPlayerBySlot(slot);

            if (targetPlayer == null || targetPlayer.AntiKickGM)
                return;

            if (CanKickPlayer(room, slot))
            {
                KickPlayer(room, slot, targetPlayer, player);
            }
        }

        #endregion Procesamiento de Estados

        #region Métodos Auxiliares

        private int ExtractSlotId(int request)
        {
            return request & SLOT_ID_MASK;
        }

        private bool CanOpenSlot(MapMatch mapLimit, SlotModel slot)
        {
            return mapLimit != null
                && (_slotChangeRequest & OPEN_SLOT_FLAG) == OPEN_SLOT_FLAG
                && slot.Id < mapLimit.Limit;
        }

        private bool CanOpenSlotInBotMode(RoomModel room, SlotModel slot)
        {
            return !room.IsBotMode() || room.Leader % 2 == slot.Id % 2;
        }

        private bool CanKickPlayer(RoomModel room, SlotModel slot)
        {
            bool isNotReady = slot.State != SlotState.READY;
            bool isClanChannel = room.ChannelType == ChannelType.Clan;
            bool isReady = slot.State == SlotState.READY;

            return (isNotReady && (isClanChannel && room.State != RoomState.COUNTDOWN || !isClanChannel))
                || (isReady && (isClanChannel && room.State == RoomState.READY || !isClanChannel));
        }

        private void KickPlayer(RoomModel room, SlotModel slot, Account targetPlayer, Account hostPlayer)
        {
            targetPlayer.SendPacket(new PROTOCOL_SERVER_MESSAGE_KICK_PLAYER_ACK());
            UpdateKickedPlayersList(room, hostPlayer);
            room.RemovePlayer(targetPlayer, slot, false);
        }

        private void UpdateKickedPlayersList(RoomModel room, Account hostPlayer)
        {
            if (!room.KickedPlayersHost.ContainsKey(hostPlayer.PlayerId))
            {
                room.KickedPlayersHost.Add(hostPlayer.PlayerId, DateTimeUtil.Now());
            }
            else
            {
                room.KickedPlayersHost[hostPlayer.PlayerId] = DateTimeUtil.Now();
            }
        }

        private void SendCompetitiveWarning(Account player, string translationKey)
        {
            player.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK(
                Translation.GetLabel("Competitive"),
                player.Session.SessionId,
                player.NickColor,
                true,
                Translation.GetLabel(translationKey)
            ));
        }

        private void SendErrorResponse()
        {
            _errorCode = ERROR_CODE;
            Client.SendPacket(new PROTOCOL_ROOM_CHANGE_SLOT_ACK(_errorCode));
        }

        #endregion Métodos Auxiliares
    }
}