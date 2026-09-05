// Código desofuscado
// Tipo: CheckRandomHostPacket (anteriormente Class10)
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
/// Maneja las peticiones para verificar quién sería el próximo host aleatorio 
/// sin realizar el cambio real
/// </summary>
public class CheckRandomHostPacket : GameClientPacket
{
    /// <summary>
    /// ID del slot que sería seleccionado como nuevo líder, o código de error
    /// </summary>
    private uint _candidateSlotId;

    /// <summary>
    /// Lista temporal de slots elegibles para ser el nuevo líder
    /// </summary>
    private List<SlotModel> _eligibleSlots = new List<SlotModel>();

    /// <summary>
    /// Lee los datos del paquete desde el cliente
    /// </summary>
    public override void Read()
    {
        // No hay datos que leer para este paquete de verificación
    }

    /// <summary>
    /// Procesa la petición de verificación de cambio aleatorio de host
    /// Este método simula la selección sin hacer el cambio real
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

            // Verificar condiciones necesarias para el cambio de host
            if (room != null && room.Leader == currentPlayer.SlotId && room.State == RoomState.READY)
            {
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

                // Determinar el resultado de la verificación
                if (this._eligibleSlots.Count > 0)
                {
                    // Simular selección aleatoria
                    SlotModel candidateSlot = this._eligibleSlots[new Random().Next(this._eligibleSlots.Count)];

                    // Verificar si el candidato seleccionado es válido
                    if (room.GetPlayerBySlot(candidateSlot) != null)
                    {
                        // Retornar el ID del slot que sería seleccionado
                        this._candidateSlotId = (uint)candidateSlot.Id;
                    }
                    else
                    {
                        // Error: El candidato ya no está disponible
                        this._candidateSlotId = 0x80000000;
                    }

                    // Limpiar la lista temporal
                    this._eligibleSlots = null;
                }
                else
                {
                    // No hay candidatos elegibles
                    this._candidateSlotId = 0x80000000;
                }
            }
            else
            {
                // Condiciones no cumplidas para cambio de host
                this._candidateSlotId = 0x80000000;
            }

            // Enviar resultado de la verificación al cliente
            this.Client.SendPacket(new PROTOCOL_ROOM_CHECK_MAIN_ACK(this._candidateSlotId));
        }
        catch (Exception ex)
        {
            // Log del error para debugging
            CLogger.Print("PROTOCOL_ROOM_CHECK_MAIN_REQ: " + ex.Message, LoggerType.Error, ex);
        }
    }
}