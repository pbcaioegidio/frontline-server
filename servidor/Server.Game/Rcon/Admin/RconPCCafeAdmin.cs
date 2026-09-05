using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using Server.Game.Rcon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game.Rcon.Admin
{
    public class RconPCCafeAdmin : RconReceive
    {
        private string Token;
        private int vip;
        private long username, gm_id;

        public override void Run()
        {
            if (IsJsonMode)
            {
                Token = PopString("token");
                username = PopLong("player_id");
                vip = PopInt("pc_cafe");
                gm_id = PopLong("gm");
            }

            // Validación del token
            if (!RconCommand.CheckToken(Token) || Token == "")
            {
                RconLogger.LogsPanel("Error: Token inválido o vacío. Por favor, intente nuevamente.", 1);
                return;
            }

            // Determinar el costo basado en el nivel VIP usando el enum CafeEnum
            int cost = 0;
            string vipTypeName = "";
            CafeEnum cafeType = (CafeEnum)vip;

            switch (cafeType)
            {
                case CafeEnum.Silver:
                    cost = 1;
                    vipTypeName = "Silver";
                    break;

                case CafeEnum.Gold:
                    cost = 1;
                    vipTypeName = "Gold";
                    break;

                case CafeEnum.None:
                case CafeEnum.Unk:
                default:
                    RconLogger.LogsPanel($"Error: Tipo de VIP inválido ({vip}). Solo se permiten: Silver (1) o Gold (2).", 1);
                    return;
            }

            // Obtener cuentas del GM y del jugador objetivo
            Account GM = AccountManager.GetAccount(gm_id, 0);
            Account pR = AccountManager.GetAccount(username, 0);

            if (GM == null)
            {
                RconLogger.LogsPanel($"Error: No se encontró la cuenta del GM con ID {gm_id}", 1);
                return;
            }

            if (pR == null)
            {
                RconLogger.LogsPanel($"Error: No se encontró ningún jugador con ID {username}", 1);
                return;
            }

            // Verificar balance del GM
            int vipBalance = RconManager.GetVipBalance(gm_id);
            if (vipBalance < cost)
            {
                RconLogger.LogsPanel($"Error: {GM.Nickname} no tiene suficiente balance ({vipBalance}). Requiere: {cost}", 1);
                return;
            }

            // Calcular fecha de expiración (1 mes desde ahora)
            DateTime expirationDate = DateTime.Now.AddMonths(1);
            string formattedDate = expirationDate.ToString("dd/MM/yyyy HH:mm tt");
            long expirationTimestamp = ((DateTimeOffset)expirationDate).ToUnixTimeSeconds();

            try
            {
                // Actualizar nivel VIP en la tabla accounts
                if (!ComDiv.UpdateDB("accounts", "pc_cafe", vip, "player_id", pR.PlayerId))
                {
                    RconLogger.LogsPanel($"Error: No se pudo actualizar el nivel VIP para {pR.Nickname}", 1);
                    return;
                }

                // Establecer access_level a 1 (usuario con privilegios)
                if (!ComDiv.UpdateDB("accounts", "access_level", 1, "player_id", pR.PlayerId))
                {
                    RconLogger.LogsPanel($"Advertencia: No se pudo actualizar access_level para {pR.Nickname}", 1);
                }

                // Verificar si el jugador ya tiene un registro VIP existente
                bool hasExistingVIP = ComDiv.CheckIfPlayerHasVIP(pR.PlayerId);

                if (hasExistingVIP)
                {
                    // CASO 1: Actualizar VIP existente (extender duración)
                    if (!ComDiv.UpsertPlayerVIP(pR.PlayerId, pR.PublicIP?.ToString() ?? "0.0.0.0", vipTypeName, expirationTimestamp))
                    {
                        RconLogger.LogsPanel($"Error: No se pudo actualizar el registro VIP existente para {pR.Nickname}", 1);
                        return;
                    }

                    RconLogger.LogsPanel($"[✓] VIP extendido para {pR.Nickname} ({vipTypeName}) hasta {formattedDate}", 0);
                }
                else
                {
                    // CASO 2: Crear nuevo registro VIP
                    if (!ComDiv.InsertPlayerVIP(pR.PlayerId, pR.PublicIP?.ToString() ?? "0.0.0.0", vipTypeName, expirationTimestamp))
                    {
                        RconLogger.LogsPanel($"Error: No se pudo crear el registro VIP para {pR.Nickname}", 1);
                        return;
                    }

                    RconLogger.LogsPanel($"[✓] Nuevo VIP creado para {pR.Nickname} ({vipTypeName}) hasta {formattedDate}", 0);
                }

                // Desconectar al jugador para aplicar cambios
                pR.SendPacket(new PROTOCOL_AUTH_ACCOUNT_KICK_ACK(2), false);
                pR.Close(1000, true);

                // Enviar mensaje al jugador
                MessageModel msgF9 = CreateMessage(
                    "Sistema VIP",
                    pR.PlayerId,
                    0,
                    $"¡Felicidades {pR.Nickname}!\n\nTe has suscrito a VIP {vipTypeName}.\nTu membresía es válida hasta: {formattedDate}\n\n¡Disfruta de tus beneficios exclusivos!"
                );

                if (msgF9 != null)
                {
                    pR.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(msgF9));
                }

                // Descontar el costo del balance del GM
                int finalBalance = vipBalance - cost;
                if (!ComDiv.UpdateDB("accounts", "vip_balance", finalBalance, "player_id", GM.PlayerId))
                {
                    RconLogger.LogsPanel($"Advertencia: No se pudo actualizar el balance del GM {GM.Nickname}", 1);
                }

                // Log final de éxito
                RconLogger.LogsPanel(
                    $"[✓] Operación completada exitosamente:\n" +
                    $"    - Jugador: {pR.Nickname} (ID: {pR.PlayerId})\n" +
                    $"    - VIP: {vipTypeName} (Nivel {vip})\n" +
                    $"    - Válido hasta: {formattedDate}\n" +
                    $"    - GM: {GM.Nickname} (Balance restante: {finalBalance})",
                    0
                );
            }
            catch (Exception ex)
            {
                RconLogger.LogsPanel($"Error crítico al procesar VIP: {ex.Message}", 1);
                CLogger.Print($"[RconPCCafeAdmin.Run] {ex.Message}", LoggerType.Error, ex);
            }
        }

        private static MessageModel CreateMessage(string senderName, long owner, long senderId, string text)
        {
            MessageModel msg = new MessageModel(15)
            {
                SenderName = senderName,
                SenderId = senderId,
                Text = text,
                State = NoteMessageState.Unreaded
            };

            if (!DaoManagerSQL.CreateMessage(owner, msg))
                return null;

            return msg;
        }
    }
}