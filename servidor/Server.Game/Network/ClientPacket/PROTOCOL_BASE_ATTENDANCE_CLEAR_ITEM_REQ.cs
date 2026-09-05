// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_REQ : GameClientPacket
    {
        private EventErrorEnum error = EventErrorEnum.VISIT_EVENT_SUCCESS;
        private int eventId;
        private int rewardType;
        private int dayCheckedIdx;

        public override void Read()
        {
            this.eventId = this.ReadD();
            this.rewardType = this.ReadC();
            this.dayCheckedIdx = this.ReadC();
        }
        public static bool IsClaimable(int day, int checkDay, int rewardDay)
        {
            return day >= 0 && day < checkDay && day == rewardDay;
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();

                if (player == null)
                    return;

                // Validar parámetros básicos
                if (string.IsNullOrEmpty(player.Nickname) || this.rewardType > 2)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_USERFAIL;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
                    return;
                }

                PlayerEvent playerEvent = player.Event;

                // Verificar si el jugador tiene datos de evento
                if (playerEvent == null)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_UNKNOWN;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
                    return;
                }

                // Obtener información del evento
                EventVisitModel eventData = EventVisitXML.GetEvent(this.eventId);

                if (eventData == null)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_UNKNOWN;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
                    return;
                }

                // Verificar si el evento está activo
                if (!eventData.EventIsEnabled())
                {
                    this.error = EventErrorEnum.VISIT_EVENT_WRONGVERSION;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
                    return;
                }

                // Verificar si el día seleccionado está dentro del calendario del evento
                if (this.dayCheckedIdx < 0 || this.dayCheckedIdx >= eventData.Checks ||
                    this.dayCheckedIdx >= eventData.Boxes.Count)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_WRONGVERSION;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
                    return;
                }

                // Obtener la caja de recompensa para el día
                VisitBoxModel rewardBox = eventData.Boxes[this.dayCheckedIdx];

                if (rewardBox == null)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_WRONGVERSION;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
                    return;
                }

                // El día debe estar marcado como asistido y ser exactamente el siguiente
                // pendiente de cobro: sin esto el mismo REQ se puede repetir sin límite.
                if (!IsClaimable(this.dayCheckedIdx, playerEvent.LastVisitCheckDay, playerEvent.LastVisitRewardDay))
                {
                    this.error = EventErrorEnum.VISIT_EVENT_ALREADYCHECK;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
                    return;
                }

                // Cobrar las dos recompensas solo si la caja lo permite. No se exige lo
                // contrario (tipo 0/1 sobre una caja is_both): el cliente puede elegir una sola.
                if (this.rewardType == 2 && !rewardBox.IsBothReward)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_WRONGVERSION;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
                    return;
                }

                // Obtener las recompensas para el día seleccionado y el tipo seleccionado
                List<VisitItemModel> rewards = eventData.GetReward(this.dayCheckedIdx, this.rewardType);

                if (rewards.Count == 0)
                {
                    this.error = EventErrorEnum.VISIT_EVENT_UNKNOWN;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
                    return;
                }

                // Intentar enviar las recompensas al jugador
                if (!this.SendRewardsToPlayer(player, rewards))
                {
                    this.error = EventErrorEnum.VISIT_EVENT_NOTENOUGH;
                    this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
                    return;
                }

                // Actualizar información del evento para el jugador
                playerEvent.LastVisitSeqType = this.rewardType;
                playerEvent.LastVisitRewardDay = this.dayCheckedIdx + 1;

                // Actualizar la base de datos
                ComDiv.UpdateDB("player_events", "owner_id", (object)player.PlayerId,
                    new string[] { "last_visit_seq_type", "last_visit_reward_day" },
                    (object)playerEvent.LastVisitSeqType, (object)playerEvent.LastVisitRewardDay);

                // Enviar respuesta de éxito
                this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(this.error));
            }
            catch (Exception ex)
            {
                CLogger.Print($"PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_REQ: {ex.Message}", LoggerType.Error, ex);
                // En caso de excepción, enviar error genérico
                this.Client.SendPacket(new PROTOCOL_BASE_ATTENDANCE_CLEAR_ITEM_ACK(EventErrorEnum.VISIT_EVENT_UNKNOWN));
            }
        }

        private bool SendRewardsToPlayer(Account player, List<VisitItemModel> rewards)
        {
            try
            {
                int rewardCount = 0;

                foreach (VisitItemModel reward in rewards)
                {
                    GoodsItem goodItem = ShopManager.GetGood(reward.GoodId);

                    if (goodItem != null)
                    {
                        // Si es un personaje (tipo 6) y el jugador no lo tiene, crearlo
                        if (ComDiv.GetIdStatics(goodItem.Item.Id, 1) == 6 &&
                            player.Character.GetCharacter(goodItem.Item.Id) == null)
                        {
                            AllUtils.CreateCharacter(player, goodItem.Item);
                        }
                        else
                        {
                            // Enviar el item al inventario del jugador
                            player.SendPacket(new PROTOCOL_INVENTORY_GET_INFO_ACK(0, player, goodItem.Item));
                        }
                        rewardCount++;
                    }
                }

                return rewardCount > 0;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }
    }
}