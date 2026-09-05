using Plugin.Core.Enums;
using Plugin.Core.Utility;
using System;
using System.Collections.Concurrent;

namespace Plugin.Core.Security
{
    /// <summary>
    /// Ticket one-shot Auth→Game: impede USER_ENTER só com PlayerId/Username spoofados.
    /// </summary>
    public sealed class LoginSessionTicket
    {
        public long PlayerId;
        public string Username;
        public string IpAddress;
        public uint SessionKey;
        public DateTime ExpiresAt;
        public bool Consumed;

        public bool IsValid(DateTime now) =>
            !Consumed && now <= ExpiresAt;
    }

    public static class LoginSessionStore
    {
        /// <summary>Validade padrão do ticket após login no Auth (segundos).</summary>
        public static int TicketLifetimeSeconds = 180;

        /// <summary>Se true, USER_ENTER exige ticket válido.</summary>
        public static bool RequireTicket = true;

        /// <summary>Se true, IP do Game deve bater com o IP do Auth.</summary>
        public static bool RequireIpMatch = true;

        private static readonly ConcurrentDictionary<long, LoginSessionTicket> Tickets =
            new ConcurrentDictionary<long, LoginSessionTicket>();

        public static void Upsert(long playerId, string username, string ipAddress, uint sessionKey, int ttlSeconds = 0)
        {
            if (playerId <= 0)
                return;

            int ttl = ttlSeconds > 0 ? ttlSeconds : TicketLifetimeSeconds;
            DateTime now = DateTimeUtil.Now();
            var ticket = new LoginSessionTicket
            {
                PlayerId = playerId,
                Username = username ?? "",
                IpAddress = ipAddress ?? "",
                SessionKey = sessionKey,
                ExpiresAt = now.AddSeconds(ttl),
                Consumed = false
            };
            Tickets[playerId] = ticket;
            PurgeExpired(now);
        }

        public static bool TryConsume(long playerId, string username, string ipAddress, out string reason)
        {
            reason = null;
            if (!RequireTicket)
                return true;

            if (playerId <= 0)
            {
                reason = "player_id_invalido";
                return false;
            }

            DateTime now = DateTimeUtil.Now();
            if (!Tickets.TryGetValue(playerId, out LoginSessionTicket ticket))
            {
                reason = "ticket_ausente";
                return false;
            }

            if (ticket.Consumed)
            {
                reason = "ticket_ja_usado";
                return false;
            }

            if (now > ticket.ExpiresAt)
            {
                Tickets.TryRemove(playerId, out _);
                reason = "ticket_expirado";
                return false;
            }

            if (!string.IsNullOrEmpty(ticket.Username) &&
                !string.IsNullOrEmpty(username) &&
                !string.Equals(ticket.Username, username, StringComparison.OrdinalIgnoreCase))
            {
                reason = "username_divergente";
                return false;
            }

            if (RequireIpMatch &&
                !string.IsNullOrEmpty(ticket.IpAddress) &&
                !string.IsNullOrEmpty(ipAddress) &&
                !string.Equals(ticket.IpAddress, ipAddress, StringComparison.OrdinalIgnoreCase))
            {
                reason = "ip_divergente";
                CLogger.Print(
                    $"[LoginSession] IP divergente PlayerId={playerId} Auth={ticket.IpAddress} Game={ipAddress}",
                    LoggerType.Warning);
                return false;
            }

            ticket.Consumed = true;
            Tickets.TryRemove(playerId, out _);
            return true;
        }

        public static void Invalidate(long playerId)
        {
            Tickets.TryRemove(playerId, out _);
        }

        private static void PurgeExpired(DateTime now)
        {
            if (Tickets.Count < 64)
                return;

            foreach (var kv in Tickets)
            {
                if (kv.Value.Consumed || now > kv.Value.ExpiresAt)
                    Tickets.TryRemove(kv.Key, out _);
            }
        }
    }
}
