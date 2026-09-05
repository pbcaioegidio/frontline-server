using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Plugin.Core.XML
{
    public class EventPlaytimeJSON
    {
        public static readonly List<EventPlaytimeModel> Events = new List<EventPlaytimeModel>();

        public static void Load()
        {
            string Path = "Data/Events/Play.json";
            if (File.Exists(Path))
            {
                Parse(Path);
            }
            else
            {
                CLogger.Print($"Archivo no encontrado: {Path}", LoggerType.Warning);
            }
            CLogger.Print($"Plugin carregado: {Events.Count} Time Events", LoggerType.Info);
        }

        public static void Reload()
        {
            Events.Clear();
            Load();
        }

        public static EventPlaytimeModel GetEvent(int EventId)
        {
            lock (Events)
            {
                foreach (EventPlaytimeModel Event in Events)
                {
                    if (Event.Id == EventId)
                    {
                        return Event;
                    }
                }
                return null;
            }
        }

        public static EventPlaytimeModel GetRunningEvent()
        {
            lock (Events)
            {
                // Obtener la fecha actual sin hora para una comparación más precisa
                uint Date = uint.Parse(DateTimeUtil.Now("yyMMdd") + "0000");
                EventPlaytimeModel activeEvent = null;

                foreach (EventPlaytimeModel Event in Events)
                {
                    if (Event == null)
                        continue;

                    // Normalizar las fechas del evento para comparación
                    uint beginDateNormalized = Event.BeginDate / 10000 * 10000;
                    uint endedDateNormalized = Event.EndedDate / 10000 * 10000;

                    // Verificar si está activo
                    if (beginDateNormalized <= Date && Date < endedDateNormalized)
                    {
                        // Si es el primer evento encontrado o tiene prioridad sobre el anterior
                        if (activeEvent == null || (Event.Priority && !activeEvent.Priority))
                        {
                            activeEvent = Event;

                            // Si este evento tiene prioridad, lo elegimos inmediatamente
                            if (Event.Priority)
                            {
                                break;
                            }
                        }
                    }
                }

                if (activeEvent != null)
                {
                    //CLogger.Print($"Evento de tiempo activo encontrado: ID={activeEvent.Id}, Nombre={activeEvent.Name}", LoggerType.Debug);
                    activeEvent.LogEventDetails();
                }
                else
                {
                    //CLogger.Print("No se encontró ningún evento de tiempo activo", LoggerType.Debug);
                }

                return activeEvent;
            }
        }

        // Método para obtener el ID del evento activo
        public static int GetRunningEventId()
        {
            EventPlaytimeModel runningEvent = GetRunningEvent();
            return runningEvent != null ? runningEvent.Id : 0;
        }

        // Método ResetPlayerEvent actualizado para EventPlaytimeJSON.cs
        public static void ResetPlayerEvent(long PlayerId, PlayerEvent Event, bool includeCompletedLevels = false)
        {
            if (PlayerId == 0 || Event == null)
            {
                //CLogger.Print($"ResetPlayerEvent: PlayerId o Event inválido", LoggerType.Warning);
                return;
            }

            try
            {
                // Registrar lo que estamos actualizando
                //CLogger.Print($"Actualizando evento de tiempo para jugador {PlayerId}:", LoggerType.Debug);
                //CLogger.Print($"  LastPlaytimeDate: {Event.LastPlaytimeDate}", LoggerType.Debug);
                //CLogger.Print($"  LastPlaytimeFinish: {Event.LastPlaytimeFinish}", LoggerType.Debug);
                //CLogger.Print($"  LastPlaytimeValue: {Event.LastPlaytimeValue}", LoggerType.Debug);
                //CLogger.Print($"  CurrentPlaytimeEventId: {Event.CurrentPlaytimeEventId}", LoggerType.Debug);
                //CLogger.Print($"  IncludeCompletedLevels: {includeCompletedLevels}", LoggerType.Debug);

                bool success = false;

                if (includeCompletedLevels)
                {
                    // Actualizar todos los valores, incluido playtime_completed_levels
                    success = DaoManagerSQL.UpdatePlaytimeEventData(PlayerId, Event.LastPlaytimeDate, Event.LastPlaytimeValue, Event.LastPlaytimeFinish, Event.CurrentPlaytimeEventId, Event.PlaytimeCompletedLevels);
                }
                else
                {
                    // Solo actualizar progreso básico, mantener niveles completados
                    success = ComDiv.UpdateDB("player_events", "owner_id", PlayerId, new string[] { "last_playtime_date", "last_playtime_finish", "last_playtime_value", "current_playtime_event_id" }, Event.LastPlaytimeDate, Event.LastPlaytimeFinish, Event.LastPlaytimeValue, Event.CurrentPlaytimeEventId);
                }

                if (success)
                {
                   // CLogger.Print($"Evento de tiempo actualizado exitosamente en BD para jugador {PlayerId}", LoggerType.Debug);
                }
                else
                {
                    //CLogger.Print($"Error actualizando evento de tiempo en BD para jugador {PlayerId}", LoggerType.Warning);
                }
            }
            catch (Exception ex)
            {
                //CLogger.Print($"Error actualizando evento de jugador {PlayerId}: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private static void Parse(string Path)
        {
            try
            {
                string jsonContent = File.ReadAllText(Path);
                if (string.IsNullOrEmpty(jsonContent))
                {
                    CLogger.Print($"Archivo vacío: {Path}", LoggerType.Warning);
                    return;
                }

                using (JsonDocument document = JsonDocument.Parse(jsonContent))
                {
                    if (document.RootElement.TryGetProperty("List", out JsonElement listElement))
                    {
                        if (listElement.TryGetProperty("Event", out JsonElement eventArray))
                        {
                            // Verificar que Event sea un array
                            if (eventArray.ValueKind == JsonValueKind.Array)
                            {
                                foreach (JsonElement eventElement in eventArray.EnumerateArray())
                                {
                                    ParseEventElement(eventElement);
                                }
                            }
                            else
                            {
                                CLogger.Print("Error: 'Event' debe ser un array en el archivo JSON.", LoggerType.Error);
                            }
                        }
                        else
                        {
                            CLogger.Print("Error: Elemento 'Event' no encontrado en el archivo JSON.", LoggerType.Error);
                        }
                    }
                    else
                    {
                        CLogger.Print("Error: Elemento 'List' no encontrado en el archivo JSON.", LoggerType.Error);
                    }
                }
            }
            catch (JsonException Ex)
            {
                CLogger.Print($"Error al analizar JSON: {Ex.Message}", LoggerType.Error, Ex);
            }
            catch (IOException Ex)
            {
                CLogger.Print($"Error al leer archivo: {Ex.Message}", LoggerType.Error, Ex);
            }
        }

        private static void ParseEventElement(JsonElement eventElement)
        {
            try
            {
                EventPlaytimeModel Event = new EventPlaytimeModel();

                // Propiedades básicas
                if (eventElement.TryGetProperty("Id", out JsonElement idElement))
                    Event.Id = idElement.GetInt32();

                if (eventElement.TryGetProperty("Begin", out JsonElement beginElement))
                    Event.BeginDate = beginElement.GetUInt32();

                if (eventElement.TryGetProperty("Ended", out JsonElement endedElement))
                    Event.EndedDate = endedElement.GetUInt32();

                if (eventElement.TryGetProperty("Name", out JsonElement nameElement))
                    Event.Name = nameElement.GetString();

                if (eventElement.TryGetProperty("Description", out JsonElement descElement))
                    Event.Description = descElement.GetString();

                if (eventElement.TryGetProperty("Period", out JsonElement periodElement))
                    Event.Period = periodElement.GetBoolean();

                if (eventElement.TryGetProperty("Priority", out JsonElement priorityElement))
                    Event.Priority = priorityElement.GetBoolean();

                // Parsear la estructura de niveles y recompensas
                if (eventElement.TryGetProperty("Minutes", out JsonElement minutesElement))
                {
                    if (minutesElement.TryGetProperty("Time", out JsonElement timeArray) &&
                        timeArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement timeElement in timeArray.EnumerateArray())
                        {
                            PlaytimeLevel level = new PlaytimeLevel();

                            if (timeElement.TryGetProperty("Index", out JsonElement indexElement))
                                level.Index = indexElement.GetInt32();

                            if (timeElement.TryGetProperty("Play", out JsonElement playElement))
                            {
                                level.PlayMinutes = playElement.GetInt32();

                                // *** DEBUG CRÍTICO ***
                                //CLogger.Print($"JSON PARSE: Índice {level.Index} = {level.PlayMinutes} minutos RAW", LoggerType.Debug);
                            }

                            // Parsear recompensas...
                            if (timeElement.TryGetProperty("Reward", out JsonElement rewardElement))
                            {
                                if (rewardElement.TryGetProperty("Goods", out JsonElement goodsArray) &&
                                    goodsArray.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (JsonElement goodElement in goodsArray.EnumerateArray())
                                    {
                                        if (goodElement.TryGetProperty("Id", out JsonElement goodIdElement))
                                        {
                                            PlaytimeReward reward = new PlaytimeReward
                                            {
                                                Id = goodIdElement.GetInt32()
                                            };
                                            level.Rewards.Add(reward);
                                        }
                                    }
                                }
                            }

                            Event.Levels.Add(level);
                            //CLogger.Print($"JSON PARSE: Agregado nivel {level.Index} con {level.PlayMinutes} min y {level.Rewards.Count} recompensas", LoggerType.Debug);
                        }
                    }
                }

                Event.Levels.Sort((a, b) => a.Index.CompareTo(b.Index));

                // *** DEBUG FINAL ***
                //CLogger.Print($"JSON PARSE FINAL: Evento {Event.Id} tiene {Event.Levels.Count} niveles", LoggerType.Debug);
                for (int i = 0; i < Event.Levels.Count; i++)
                {
                    //CLogger.Print($"  Posición {i}: Índice {Event.Levels[i].Index} = {Event.Levels[i].PlayMinutes} min", LoggerType.Debug);
                }

                Events.Add(Event);
            }
            catch (Exception Ex)
            {
                CLogger.Print($"Error al analizar evento: {Ex.Message}", LoggerType.Error, Ex);
            }
        }
    }
}