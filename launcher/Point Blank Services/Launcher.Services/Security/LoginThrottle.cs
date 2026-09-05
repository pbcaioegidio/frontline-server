using System;
using System.Collections.Concurrent;

namespace Launcher.Services.Security
{
    /// <summary>
    /// Brute force no login do launcher: N falhas por IP dentro da janela → bloqueio temporário.
    /// Em memória (processo único do Socket).
    /// </summary>
    public static class LoginThrottle
    {
        private sealed class Entry
        {
            public int Failures;
            public DateTime WindowStart;
            public DateTime BlockedUntil;
        }

        private static readonly ConcurrentDictionary<string, Entry> Entries = new ConcurrentDictionary<string, Entry>();

        public static bool IsBlocked(string ip, out TimeSpan remaining)
        {
            remaining = TimeSpan.Zero;
            if (string.IsNullOrEmpty(ip)) return false;
            if (!Entries.TryGetValue(ip, out Entry e)) return false;
            if (e.BlockedUntil > DateTime.UtcNow)
            {
                remaining = e.BlockedUntil - DateTime.UtcNow;
                return true;
            }
            return false;
        }

        /// <summary>Registra falha. Retorna true se este IP acabou de ser bloqueado.</summary>
        public static bool RegisterFailure(string ip, int maxFailures, TimeSpan window, TimeSpan block)
        {
            if (string.IsNullOrEmpty(ip)) return false;
            DateTime now = DateTime.UtcNow;
            Entry e = Entries.GetOrAdd(ip, _ => new Entry { WindowStart = now });
            lock (e)
            {
                if (now - e.WindowStart > window)
                {
                    e.WindowStart = now;
                    e.Failures = 0;
                }
                e.Failures++;
                if (e.Failures >= maxFailures)
                {
                    e.BlockedUntil = now + block;
                    e.Failures = 0;
                    e.WindowStart = now;
                    return true;
                }
            }
            return false;
        }

        public static void RegisterSuccess(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return;
            Entries.TryRemove(ip, out _);
        }

        /// <summary>Limpa entradas velhas (chamar de vez em quando).</summary>
        public static void Sweep()
        {
            DateTime now = DateTime.UtcNow;
            foreach (var kv in Entries)
            {
                Entry e = kv.Value;
                if (e.BlockedUntil < now && now - e.WindowStart > TimeSpan.FromHours(1))
                    Entries.TryRemove(kv.Key, out _);
            }
        }
    }
}
