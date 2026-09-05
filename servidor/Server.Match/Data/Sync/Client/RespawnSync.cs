using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Match.Data.Managers;
using Server.Match.Data.Models;
using Server.Match.Data.Utils;
using Server.Match.Data.XML;
using Server.Match.Network.Packets;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Match.Data.Sync.Client
{
    /// <summary>
    /// Maneja la sincronización de respawn de jugadores entre servidores
    /// </summary>
    public class RespawnSync
    {
        #region Constants

        private const int BASIC_PACKET_LENGTH = 23;
        private const int FULL_PACKET_LENGTH = 53;
        private const int RESPAWN_MODE_FULL = 0;
        private const int RESPAWN_MODE_SYNC = 2;

        #endregion Constants

        #region Load Method

        /// <summary>
        /// Procesa un paquete de sincronización de respawn
        /// </summary>
        public static void Load(SyncClientPacket packet)
        {
            try
            {
                // Leer datos básicos del paquete
                uint uniqueRoomId = packet.ReadUD();
                uint seed = packet.ReadUD();
                long startTick = packet.ReadQ();
                int respawnMode = packet.ReadC();
                int round = packet.ReadC();
                int slotId = packet.ReadC();
                int respawnCount = packet.ReadC();
                int accountId = packet.ReadC();

                // Datos extendidos (solo para ciertos modos)
                int characterId = 0;
                int hpBonusPercent = 0;
                int primaryWeapon = 0;
                int secondaryWeapon = 0;
                int meleeWeapon = 0;
                int explosiveWeapon = 0;
                int specialWeapon = 0;
                int accessory = 0;
                bool hasBombDefuseBonus = false;

                // Validar longitud del paquete según el modo
                if (respawnMode != RESPAWN_MODE_FULL && respawnMode != RESPAWN_MODE_SYNC)
                {
                    if (packet.ToArray().Length > BASIC_PACKET_LENGTH)
                        CLogger.Print($"RespawnSync (Length > {BASIC_PACKET_LENGTH}): {packet.ToArray().Length}", LoggerType.Warning);
                }
                else
                {
                    // Leer datos extendidos del equipamiento y personaje
                    characterId = packet.ReadD();
                    hpBonusPercent = packet.ReadC();
                    hasBombDefuseBonus = packet.ReadC() == 1;
                    primaryWeapon = packet.ReadD();
                    secondaryWeapon = packet.ReadD();
                    meleeWeapon = packet.ReadD();
                    explosiveWeapon = packet.ReadD();
                    specialWeapon = packet.ReadD();
                    accessory = packet.ReadD();

                    if (packet.ToArray().Length > FULL_PACKET_LENGTH)
                        CLogger.Print($"RespawnSync (Length > {FULL_PACKET_LENGTH}): {packet.ToArray().Length}", LoggerType.Warning);
                }

                // Obtener la sala
                var room = RoomsManager.GetRoom(uniqueRoomId, seed);
                if (room == null)
                    return;

                // Resincronizar tick de la sala
                room.ResyncTick(startTick, seed);

                // Obtener el jugador
                PlayerModel player = room.GetPlayer(slotId, true);

                // Validar ID de cuenta
                if (player != null && player.PlayerIdByUser != accountId)
                {
                    CLogger.Print($"Invalid User Ids: [By User: {player.PlayerIdByUser} / Server: {accountId}]", LoggerType.Warning);
                }

                if (player == null || player.PlayerIdByUser != accountId)
                    return;

                // ✅ SOLUCIÓN: Actualizar datos del jugador y resetear estado Dead SIEMPRE
                player.PlayerIdByServer = accountId;
                player.RespawnByServer = respawnCount;
                player.Integrity = false;
                player.Dead = false; // ✅ FIX: Resetear Dead aquí para TODOS los modos de respawn

                // Actualizar ronda del servidor si es necesaria
                if (round > room.ServerRound)
                {
                    room.ServerRound = round;
                }

                // Procesar respawn completo o sincronización
                if (respawnMode == RESPAWN_MODE_FULL || respawnMode == RESPAWN_MODE_SYNC)
                {
                    // Limpiar asistencias del jugador
                    AssistServerData existingAssist = AssistManager.GetAssist(player.Slot, room.RoomId);
                    if (existingAssist != null && AssistManager.RemoveAssist(existingAssist))
                    {
                        // Capturar variables locales para el predicado
                        int playerSlot = player.Slot;
                        uint roomId = (uint)room.RoomId;

                        // Remover todas las asistencias relacionadas con este jugador y sala
                        List<AssistServerData> relatedAssists = AssistManager.Assists.FindAll(
                            assist => assist.Victim == playerSlot && assist.RoomId == roomId
                        );

                        foreach (AssistServerData assist in relatedAssists)
                            AssistManager.RemoveAssist(assist);
                    }

                    // Configurar equipamiento
                    Equipment equipment = new Equipment()
                    {
                        WpnPrimary = primaryWeapon,
                        WpnSecondary = secondaryWeapon,
                        WpnMelee = meleeWeapon,
                        WpnExplosive = explosiveWeapon,
                        WpnSpecial = specialWeapon,
                        Accessory = accessory
                    };

                    player.PlantDuration = ConfigLoader.PlantDuration;
                    player.DefuseDuration = ConfigLoader.DefuseDuration;
                    player.Equip = equipment;

                    // Aplicar bonos de desactivación de bomba si es necesario
                    if (hasBombDefuseBonus)
                    {
                        player.PlantDuration -= ComDiv.Percentage(ConfigLoader.PlantDuration, 50);
                        player.DefuseDuration -= ComDiv.Percentage(ConfigLoader.DefuseDuration, 25);
                    }

                    // Resetear sala según el modo
                    if (!room.BotMode)
                    {
                        if (room.SourceToMap == -1)
                        {
                            room.RoundResetRoomF1(round);
                        }
                        else
                        {
                            room.RoundResetRoomS1(round);
                        }
                    }

                    // Configurar vida del personaje
                    if (characterId == -1)
                    {
                        player.Immortal = true;
                    }
                    else
                    {
                        player.Immortal = false;
                        int baseHp = CharaStructureXML.GetCharaHP(characterId);
                        int totalHp = baseHp + ComDiv.Percentage(baseHp, hpBonusPercent);
                        player.MaxLife = totalHp;
                        player.ResetLife();
                    }
                }

                // Sincronizar objetos y jugadores si no es modo bot
                if (room.BotMode || respawnMode == RESPAWN_MODE_SYNC || !room.ObjectsIsValid())
                    return;

                List<ObjectHitInfo> objectsToSync = new List<ObjectHitInfo>();

                // Sincronizar objetos destruibles
                foreach (ObjectInfo obj in room.Objects)
                {
                    ObjectModel model = obj.Model;

                    if (model != null &&
                        (respawnMode != RESPAWN_MODE_SYNC && model.Destroyable && obj.Life != model.Life || model.NeedSync))
                    {
                        ObjectHitInfo hitInfo = new ObjectHitInfo(3)
                        {
                            ObjSyncId = model.NeedSync ? 1 : 0,
                            AnimId1 = model.Animation,
                            AnimId2 = obj.Animation != null ? obj.Animation.Id : 255,
                            DestroyState = obj.DestroyState,
                            ObjId = model.Id,
                            ObjLife = obj.Life,
                            SpecialUse = AllUtils.GetDuration(obj.UseDate)
                        };
                        objectsToSync.Add(hitInfo);
                    }
                }

                // Sincronizar estado de otros jugadores
                foreach (PlayerModel otherPlayer in room.Players)
                {
                    if (otherPlayer.Slot != slotId &&
                        otherPlayer.AccountIdIsValid() &&
                        !otherPlayer.Immortal &&
                        otherPlayer.StartTime != new DateTime() &&
                        (otherPlayer.MaxLife != otherPlayer.Life || otherPlayer.Dead))
                    {
                        ObjectHitInfo hitInfo = new ObjectHitInfo(4)
                        {
                            ObjId = otherPlayer.Slot,
                            ObjLife = otherPlayer.Life
                        };
                        objectsToSync.Add(hitInfo);
                    }
                }

                // Enviar paquete de sincronización si hay objetos
                if (objectsToSync.Count > 0)
                {
                    byte[] syncPacket = AllUtils.BaseWriteCode(
                        4,
                        PROTOCOL_EVENTS_ACTION.GET_CODE(objectsToSync),
                        255,
                        AllUtils.GetDuration(room.StartTime),
                        round,
                        respawnCount,
                        0,
                        accountId
                    );

                    MatchXender.Client.SendPacket(syncPacket, player.Client);
                }

                objectsToSync.Clear();
            }
            catch (Exception ex)
            {
                CLogger.Print($"RespawnSync.Load: {ex.Message}", LoggerType.Error, ex);
            }
        }

        #endregion Load Method
    }
}