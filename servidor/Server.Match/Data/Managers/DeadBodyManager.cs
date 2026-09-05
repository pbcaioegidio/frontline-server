using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Match.Data.Models;
using System;

namespace Server.Match.Data.Managers
{
    /// <summary>
    /// Maneja la colisión física de jugadores muertos
    /// Especialmente diseñado para modos tácticos como Bomb donde la colisión es problemática
    /// </summary>
    public static class DeadBodyCollisionManager
    {
        /// <summary>
        /// Tiempo en segundos antes de que un cadáver deje de tener colisión
        /// En modo Bomb, se recomienda un valor bajo (0.5-2 segundos)
        /// </summary>
        public static float DeadBodyCollisionTime = 1.0f; // ⚙️ Configura aquí el tiempo

        /// <summary>
        /// Si es true, los jugadores muertos en modo Bomb nunca se sincronizan (sin colisión instantánea)
        /// Si es false, se sincronizan por DeadBodyCollisionTime segundos
        /// </summary>
        public static bool DisableCollisionInstantly = false; // ⚙️ true = sin colisión inmediata

        /// <summary>
        /// Verifica si un jugador muerto debe sincronizarse según el modo de juego
        /// </summary>
        /// <param name="player">Jugador a verificar</param>
        /// <param name="room">Sala actual</param>
        /// <returns>True si debe sincronizarse (tener colisión), False si no</returns>
        public static bool ShouldSyncDeadPlayer(PlayerModel player, RoomModel room)
        {
            // Si el jugador no está muerto, siempre sincronizar
            if (!player.Dead)
                return true;

            // Si LastDie no está establecido, no sincronizar
            if (player.LastDie == new DateTime())
                return false;

            // ✅ MODOS TÁCTICOS: Bomb, Annihilation, Convoy, Ace
            // En estos modos, la colisión de cadáveres es especialmente problemática
            if (IsTacticalMode(room.RoomType))
            {
                // Opción 1: Desactivar colisión instantáneamente
                if (DisableCollisionInstantly)
                    return false; // No sincronizar = sin colisión

                // Opción 2: Desactivar colisión después del tiempo configurado
                float timeSinceDeath = (float)ComDiv.GetDuration(player.LastDie);
                return timeSinceDeath < DeadBodyCollisionTime;
            }

            // ✅ OTROS MODOS: Permitir sincronización normal
            // En modos como DeathMatch, la colisión temporal es menos problemática
            return true;
        }

        /// <summary>
        /// Verifica si el modo de juego es táctico (sin respawn inmediato)
        /// </summary>
        /// <param name="roomType">Tipo de sala</param>
        /// <returns>True si es modo táctico</returns>
        private static bool IsTacticalMode(RoomCondition roomType)
        {
            return roomType == RoomCondition.Bomb ||
                   roomType == RoomCondition.Annihilation ||
                   roomType == RoomCondition.Convoy ||
                   roomType == RoomCondition.Ace;
        }

        /// <summary>
        /// Obtiene el tiempo de colisión recomendado según el modo de juego
        /// </summary>
        /// <param name="roomType">Tipo de sala</param>
        /// <returns>Tiempo en segundos</returns>
        public static float GetRecommendedCollisionTime(RoomCondition roomType)
        {
            if (IsTacticalMode(roomType))
                return 0.5f; // Muy corto en modos tácticos
            else
                return 3.0f; // Más tiempo en otros modos
        }

        /// <summary>
        /// Configuración dinámica del tiempo según el modo de juego
        /// </summary>
        /// <param name="room">Sala actual</param>
        public static void AdjustCollisionTimeByMode(RoomModel room)
        {
            if (room == null)
                return;

            DeadBodyCollisionTime = GetRecommendedCollisionTime(room.RoomType);
        }
    }
}