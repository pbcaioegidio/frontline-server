// Código desofuscado
// Tipo: RandomHostChangePacket (anteriormente Class11)
// Assembly: Server.Game, Version=1.1.25163.0

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Maneja las peticiones para cambiar el host/líder de una sala de forma aleatoria
/// </summary>
public class RandomHostChangePacket : GameClientPacket
{
    /// <summary>
    /// Lista de slots elegibles para convertirse en el nuevo líder
    /// </summary>
    private List<SlotModel> _eligibleSlots = new List<SlotModel>();

    /// <summary>
    /// Lee los datos del paquete desde el cliente
    /// </summary>
    public override void Read()
    {
        // No hay datos que leer para este paquete
    }

    /// <summary>
    /// Procesa la petición de cambio aleatorio de host
    /// </summary>
    
    public override void Run()
    {
        try
        {
            // Obtener el jugador que envió la petición
            Account currentPlayer = this.Client.GetAccount();
            if (currentPlayer == null)
                return;

            // Obtener la sala del jugador
            RoomModel room = currentPlayer.Room;

            // Verificar condiciones necesarias:
            // - La sala existe
            // - El jugador actual es el líder
            // - La sala está en estado READY
            if (room == null || room.Leader != currentPlayer.SlotId || room.State != RoomState.READY)
                return;

            // Buscar candidatos elegibles para ser el nuevo líder
            lock (room.Slots)
            {
                // Recorrer todos los slots de la sala (máximo 18 jugadores)
                for (int slotIndex = 0; slotIndex < 18; ++slotIndex)
                {
                    SlotModel slot = room.Slots[slotIndex];

                    // Agregar slot si tiene un jugador y no es el líder actual
                    if (slot.PlayerId > 0L && slotIndex != room.Leader)
                        this._eligibleSlots.Add(slot);
                }
            }

            // Si hay candidatos disponibles
            if (this._eligibleSlots.Count > 0)
            {
                // Seleccionar un slot aleatorio de la lista de elegibles
                SlotModel newLeaderSlot = this._eligibleSlots[new Random().Next(this._eligibleSlots.Count)];

                // Verificar que el jugador seleccionado aún esté en la sala
                if (room.GetPlayerBySlot(newLeaderSlot) != null)
                {
                    // Cambiar el liderazgo al nuevo slot
                    room.SetNewLeader(newLeaderSlot.Id, SlotState.EMPTY, room.Leader, false);

                    // Notificar a todos los jugadores sobre el cambio de líder
                    using (PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_ACK packet = new PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_ACK(newLeaderSlot.Id))
                        room.SendPacketToPlayers(packet);

                    // Actualizar información de slots
                    room.UpdateSlotsInfo();
                }
                else
                {
                    // Error: El jugador seleccionado ya no está disponible
                    this.Client.SendPacket(new PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_ACK(0x80000000));
                }

                // Limpiar la lista de candidatos
                this._eligibleSlots = null;
            }
            else
            {
                // No hay candidatos elegibles para ser líder
                this.Client.SendPacket(new PROTOCOL_ROOM_REQUEST_MAIN_CHANGE_ACK(0x80000000));
            }
        }
        catch (Exception ex)
        {
            // Log del error para debugging
            CLogger.Print("ROOM_RANDOM_HOST_CHANGE: " + ex.Message, LoggerType.Error, ex);
        }
    }
}