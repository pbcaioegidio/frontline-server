using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_ATTENDANCE_REQ : GameClientPacket
    {
        private EventErrorEnum error = EventErrorEnum.VISIT_EVENT_SUCCESS;
        private int eventId;
        private int dayCheckedIdx;
        private EventVisitModel eventData;

        public override void Read()
        {
            this.eventId = this.ReadD();
            this.dayCheckedIdx = this.ReadC();
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();

                if (player == null)
                    return;

                // Validar que el jugador tenga un nickname válido
                if (string.IsNullOrEmpty(player.Nickname))
                {
                    this.error = EventErrorEnum.VISIT_EVENT_USERFAIL;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_ACK(this.error, null, null));
                    return;
                }

                PlayerEvent playerEvent = player.Event;

                // Verificar si el jugador tiene datos de evento
                if (playerEvent == null)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_UNKNOWN;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_ACK(this.error, null, null));
                    return;
                }

                // Obtener información del evento
                this.eventData = EventVisitXML.GetEvent(this.eventId);

                if (this.eventData == null)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_UNKNOWN;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_ACK(this.error, null, null));
                    return;
                }

                // Verificar si el evento está activo
                if (!this.eventData.EventIsEnabled())
                {
                    this.error = EventErrorEnum.VISIT_EVENT_WRONGVERSION;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_ACK(this.error, null, playerEvent));
                    return;
                }

                // Verificar si el índice del día es válido
                if (this.dayCheckedIdx < 0 || this.dayCheckedIdx >= this.eventData.Boxes.Count)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_WRONGVERSION;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_ACK(this.error, null, playerEvent));
                    return;
                }

                // Obtener la fecha actual en el formato adecuado (solo fecha, sin hora ni minutos)
                uint currentDate = uint.Parse(DateTimeUtil.Now("yyMMdd") + "0000");

                // Convertir LastVisitDate a solo fecha (sin hora ni minutos)
                uint lastVisitDateOnly = (uint)(playerEvent.LastVisitDate / 10000 * 10000);

                // Verificar lógica de asistencia diaria
                // Si la última fecha de visita (ignorando hora y minutos) es igual a la fecha actual,
                // significa que el usuario ya verificó hoy
                if (lastVisitDateOnly == currentDate)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_ALREADYCHECK;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_ACK(this.error, this.eventData, playerEvent));
                    return;
                }

                // Verificar que el día que se está intentando marcar sea el siguiente en secuencia
                if (this.dayCheckedIdx != playerEvent.LastVisitCheckDay)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_WRONGVERSION;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_ACK(this.error, null, playerEvent));
                    return;
                }

                // Obtener la caja de recompensa para el día
                VisitBoxModel rewardBox = this.eventData.Boxes[this.dayCheckedIdx];

                if (rewardBox == null)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_WRONGVERSION;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_ACK(this.error, null, playerEvent));
                    return;
                }

                // Marcar el día como reclamado
                playerEvent.LastVisitCheckDay = this.dayCheckedIdx + 1;
                playerEvent.LastVisitDate = (uint)currentDate;

                // Actualizar la base de datos
                ComDiv.UpdateDB("player_events", "owner_id", (object)player.PlayerId,
                    new string[]
                    {
                        "last_visit_check_day",
                        "last_visit_date"
                    },
                    (object)playerEvent.LastVisitCheckDay,
                    (object)playerEvent.LastVisitDate);

                // Enviar respuesta de éxito
                this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_ACK(this.error, this.eventData, playerEvent));
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_BASE_ATTENDANCE_REQ: {ex.Message}", LoggerType.Error, ex);
                // En caso de excepción, enviar error genérico
                this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_ACK(EventErrorEnum.VISIT_EVENT_UNKNOWN, null, null));
            }
        }
    }
}