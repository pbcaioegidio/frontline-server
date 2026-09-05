using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Security;
using Plugin.Core.XML;
using System;
using System.Net;

namespace Server.Auth.Data.Sync.Server
{
    /// <summary>
    /// Envia ticket de sessão Auth→Game (opcode sync 33).
    /// </summary>
    public static class LoginSessionSync
    {
        public const short Opcode = 33;

        public static void BroadcastTicket(long playerId, string username, string ipAddress, uint sessionKey)
        {
            try
            {
                if (playerId <= 0)
                    return;

                // Espelho local (mesmo processo Auth) — Game recebe via UDP.
                LoginSessionStore.Upsert(playerId, username, ipAddress, sessionKey);

                foreach (SChannelModel server in SChannelXML.Servers)
                {
                    if (server == null || server.Id == 0)
                        continue;

                    Synchronize sync = SynchronizeXML.GetServer((int)server.Port);
                    if (sync?.Connection == null)
                        continue;

                    using (SyncServerPacket packet = new SyncServerPacket())
                    {
                        packet.WriteH(Opcode);
                        packet.WriteQ(playerId);
                        packet.WriteD(sessionKey);
                        WriteLenString(packet, username ?? "");
                        WriteLenString(packet, ipAddress ?? "");
                        packet.WriteD(LoginSessionStore.TicketLifetimeSeconds);
                        AuthXender.Sync.SendPacket(packet.ToArray(), sync.Connection);
                    }
                }

                CLogger.Print(
                    $"[LoginSession] Ticket enviado PlayerId={playerId} User={username} IP={ipAddress}",
                    LoggerType.Debug);
            }
            catch (Exception ex)
            {
                CLogger.Print($"LoginSessionSync.BroadcastTicket: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private static void WriteLenString(SyncServerPacket packet, string text)
        {
            if (text == null)
                text = "";
            if (text.Length > 255)
                text = text.Substring(0, 255);
            packet.WriteC((byte)text.Length);
            if (text.Length > 0)
                packet.WriteS(text, text.Length);
        }
    }
}
