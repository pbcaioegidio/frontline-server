using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Security;
using Plugin.Core.Utility;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace Server.Match.Data.Utils
{
    /// <summary>
    /// Rate-limit UDP por IP no Match (anti-flood). Drop silencioso + FG-140 ocasional.
    /// </summary>
    public static class UdpFloodGuard
    {
        private sealed class Bucket
        {
            public int Count;
            public DateTime WindowStart;
            public DateTime LastLog;
            public int Dropped;
        }

        private static readonly ConcurrentDictionary<string, Bucket> ByIp =
            new ConcurrentDictionary<string, Bucket>(StringComparer.Ordinal);

        public static bool Allow(IPEndPoint ep)
        {
            int max = ConfigLoader.MatchUdpMaxPacketsPerSecond;
            if (max <= 0 || ep?.Address == null)
                return true;

            string key = ep.Address.ToString();
            DateTime now = DateTimeUtil.Now();
            Bucket b = ByIp.GetOrAdd(key, _ => new Bucket { WindowStart = now });

            lock (b)
            {
                if ((now - b.WindowStart).TotalSeconds >= 1.0)
                {
                    b.WindowStart = now;
                    b.Count = 0;
                    b.Dropped = 0;
                }
                b.Count++;
                if (b.Count <= max)
                    return true;

                b.Dropped++;
                if (b.Dropped == 1 || (now - b.LastLog).TotalSeconds >= 30)
                {
                    b.LastLog = now;
                    CLogger.Print($"[UdpFlood] drop IP={key} pps>{max} dropped={b.Dropped}", LoggerType.Hack);
                    SecurityDao.LogEvent(
                        SecurityDao.SourceMatch, 0, "", "",
                        "flag", "FLOOD",
                        $"Match UDP flood IP={key} limit={max}/s",
                        "{\"ip\":\"" + key + "\",\"limit\":" + max + ",\"dropped\":" + b.Dropped + "}",
                        3, "FG-140");
                }
                return false;
            }
        }
    }
}
