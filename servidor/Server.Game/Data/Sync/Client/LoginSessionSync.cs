using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Security;
using System;

namespace Server.Game.Data.Sync.Client
{
    /// <summary>
    /// Recebe ticket de sessão do Auth (opcode sync 33).
    /// </summary>
    public static class LoginSessionSync
    {
        public static void Load(SyncClientPacket C)
        {
            try
            {
                long playerId = C.ReadQ();
                uint sessionKey = (uint)C.ReadD();
                string username = ReadLenString(C);
                string ipAddress = ReadLenString(C);
                int ttl = C.ReadD();
                if (ttl <= 0)
                    ttl = LoginSessionStore.TicketLifetimeSeconds;

                LoginSessionStore.Upsert(playerId, username, ipAddress, sessionKey, ttl);
                CLogger.Print(
                    $"[LoginSession] Ticket recebido PlayerId={playerId} User={username} IP={ipAddress} TTL={ttl}s",
                    LoggerType.Debug);
            }
            catch (Exception ex)
            {
                CLogger.Print($"LoginSessionSync.Load: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private static string ReadLenString(SyncClientPacket C)
        {
            int len = C.ReadC();
            return len <= 0 ? "" : C.ReadS(len);
        }
    }
}
