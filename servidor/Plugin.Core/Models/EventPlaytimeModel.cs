using Plugin.Core.Enums;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;

namespace Plugin.Core.Models
{
    public class EventPlaytimeModel
    {
        public int Id { get; set; }
        public uint BeginDate { get; set; }
        public uint EndedDate { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Segundo texto del bloque comun (+0x4F, 60 bytes), separado del nombre en el
        // cliente (BoostEvent__BuildDescriptionList @0xBF8BD4).
        public string Subtitle { get; set; } = "";

        // Indice de Gui/EventPortal/img_*_<N>.i3i, u8 en +0x153. Clave ausente -> fallback 1.
        public byte Image { get; set; }
        public bool Period { get; set; }
        public bool Priority { get; set; }

        // Lista de niveles de tiempo
        public List<PlaytimeLevel> Levels { get; set; }

        public EventPlaytimeModel()
        {
            Name = "";
            Description = "";
            Levels = new List<PlaytimeLevel>();
        }

        // Propiedades calculadas para compatibilidad con código existente
        public int Minutes1 => Levels.Count > 0 ? Levels[0].PlayMinutes : 0;
        public int Minutes2 => Levels.Count > 1 ? Levels[1].PlayMinutes : 0;
        public int Minutes3 => Levels.Count > 2 ? Levels[2].PlayMinutes : 0;

        // Listas de bienes para compatibilidad con código existente
        public List<int> Goods1
        {
            get
            {
                List<int> goods = new List<int>();
                if (Levels.Count > 0)
                {
                    foreach (var good in Levels[0].Rewards)
                    {
                        goods.Add(good.Id);
                    }
                }
                return goods;
            }
        }

        public List<int> Goods2
        {
            get
            {
                List<int> goods = new List<int>();
                if (Levels.Count > 1)
                {
                    foreach (var good in Levels[1].Rewards)
                    {
                        goods.Add(good.Id);
                    }
                }
                return goods;
            }
        }

        public List<int> Goods3
        {
            get
            {
                List<int> goods = new List<int>();
                if (Levels.Count > 2)
                {
                    foreach (var good in Levels[2].Rewards)
                    {
                        goods.Add(good.Id);
                    }
                }
                return goods;
            }
        }

        public bool EventIsEnabled()
        {
            try
            {
                // Obtener la fecha actual con formato yyMMdd (sin hora)
                uint currentDate = uint.Parse(DateTimeUtil.Now("yyMMdd") + "0000");

                // Normalizar las fechas del evento para comparación (quitar horas/minutos)
                uint beginDateNormalized = BeginDate / 10000 * 10000;
                uint endedDateNormalized = EndedDate / 10000 * 10000;

                // Verificar si la fecha actual está dentro del rango del evento
                bool isEnabled = (beginDateNormalized <= currentDate && currentDate < endedDateNormalized);

                // Registrar información detallada para depuración
                if (isEnabled)
                {
                   // CLogger.Print($"Evento de tiempo ID={Id} está ACTIVO: Fecha actual={currentDate}, Inicio={beginDateNormalized}, Fin={endedDateNormalized}", LoggerType.Debug);
                }
                else
                {
                    //CLogger.Print($"Evento de tiempo ID={Id} NO está activo: Fecha actual={currentDate}, Inicio={beginDateNormalized}, Fin={endedDateNormalized}", LoggerType.Debug);
                }

                return isEnabled;
            }
            catch (Exception ex)
            {
               // CLogger.Print($"Error en EventPlaytimeModel.EventIsEnabled: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        // Método para obtener las recompensas según el nivel completado
        public List<int> GetRewards(int level)
        {
            List<int> rewards = new List<int>();

            // Ajustar índice de nivel (tu JSON usa índices base 1)
            int levelIndex = level - 1;

            if (levelIndex >= 0 && levelIndex < Levels.Count)
            {
                foreach (var good in Levels[levelIndex].Rewards)
                {
                    rewards.Add(good.Id);
                }
            }

            return rewards;
        }

        // Método para obtener los minutos requeridos según el nivel
        // === STEP 2: Debug en EventPlaytimeModel.GetRequiredMinutes ===
        public int GetRequiredMinutes(int level)
        {
            int levelIndex = level - 1;

            CLogger.Print($"GetRequiredMinutes: Buscando nivel {level} (índice {levelIndex})", LoggerType.Debug);
            CLogger.Print($"  Tengo {Levels.Count} niveles en total", LoggerType.Debug);

            if (levelIndex >= 0 && levelIndex < Levels.Count)
            {
                PlaytimeLevel targetLevel = Levels[levelIndex];
                //CLogger.Print($"  Encontrado: Posición {levelIndex} tiene Índice {targetLevel.Index} = {targetLevel.PlayMinutes} min", LoggerType.Debug);
                return targetLevel.PlayMinutes;
            }

            CLogger.Print($"  No encontrado, devolviendo 0", LoggerType.Debug);
            return 0;
        }

        // Método para verificar si un nivel es válido
        public bool IsValidLevel(int level)
        {
            // Ajustar índice de nivel (tu JSON usa índices base 1)
            int levelIndex = level - 1;

            return levelIndex >= 0 && levelIndex < Levels.Count;
        }

        // Método para obtener el número de niveles
        public int GetLevelCount()
        {
            return Levels.Count;
        }

        // Método para depuración - muestra información detallada del evento
        public void LogEventDetails()
        {
            //CLogger.Print($"=== Detalles del Evento de Tiempo ID={Id} ===", LoggerType.Debug);
            //CLogger.Print($"Nombre: {Name}", LoggerType.Debug);
            //CLogger.Print($"Descripción: {Description}", LoggerType.Debug);
            ////CLogger.Print($"Fechas: {BeginDate} - {EndedDate}", LoggerType.Debug);
            //CLogger.Print($"Periodo: {Period}, Prioridad: {Priority}", LoggerType.Debug);
            //CLogger.Print($"Evento Activo: {EventIsEnabled()}", LoggerType.Debug);

            for (int i = 0; i < Levels.Count; i++)
            {
                //CLogger.Print($"Nivel {i + 1}: {Levels[i].PlayMinutes} minutos, {Levels[i].Rewards.Count} recompensas", LoggerType.Debug);
            }
        }
    }

    // Clase para representar un nivel de tiempo
    public class PlaytimeLevel
    {
        public int Index { get; set; }
        public int PlayMinutes { get; set; }
        public List<PlaytimeReward> Rewards { get; set; }

        public PlaytimeLevel()
        {
            Rewards = new List<PlaytimeReward>();
        }
    }

    // Clase para representar una recompensa
    public class PlaytimeReward
    {
        public int Id { get; set; }
    }
}