using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Security;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Server.Game.Security
{
    /// <summary>
    /// Periodicamente kicka jogadores online cujo launcher parou o heartbeat (live_sessions).
    /// Só age se ConfigLoader.RequireLauncherHeartbeat = true.
    /// Grace no começo da sessão: o Guard manda o 1º heartbeat alguns segundos após Start;
    /// sem isso o poll de 2s kickava na hora ao entrar (FG-110 falso).
    /// </summary>
    public static class HeartbeatGuard
    {
        /// <summary>Segundos online antes de considerar FG-110. Cobre delay do Guard + rede.</summary>
        private const int OnlineGraceSeconds = 25;

        private static Timer _timer;
        private static int _running;
        private static readonly ConcurrentDictionary<long, DateTime> _onlineSince = new ConcurrentDictionary<long, DateTime>();

        public static void Start()
        {
            if (_timer != null) return;
            _timer = new Timer(_ => Tick(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
            CLogger.Print("[FL GUARD] HeartbeatGuard iniciado (checa live_sessions a cada 2s, grace " + OnlineGraceSeconds + "s)", LoggerType.Info);
        }

        public static void Stop()
        {
            try { _timer?.Dispose(); } catch { }
            _timer = null;
            _onlineSince.Clear();
        }

        private static void Tick()
        {
            if (!ConfigLoader.RequireLauncherHeartbeat) return;
            if (Interlocked.Exchange(ref _running, 1) == 1) return;
            try
            {
                var online = new List<long>();
                var seen = new HashSet<long>();
                DateTime now = DateTimeUtil.Now();

                foreach (GameManager mgr in GameXender.All)
                {
                    foreach (GameClient client in mgr.SocketSessions.Values)
                    {
                        Account p = client?.Player;
                        if (p != null && p.IsOnline && p.PlayerId > 0)
                        {
                            online.Add(p.PlayerId);
                            seen.Add(p.PlayerId);
                            _onlineSince.TryAdd(p.PlayerId, now);
                        }
                    }
                }

                foreach (long id in _onlineSince.Keys)
                {
                    if (!seen.Contains(id))
                        _onlineSince.TryRemove(id, out _);
                }

                if (online.Count == 0) return;

                var eligible = new List<long>();
                foreach (long id in online)
                {
                    if (_onlineSince.TryGetValue(id, out DateTime since) &&
                        (now - since).TotalSeconds >= OnlineGraceSeconds)
                        eligible.Add(id);
                }
                if (eligible.Count == 0) return;

                List<long> stale = SecurityDao.ListStaleHeartbeats(eligible, ConfigLoader.HeartbeatTimeoutSeconds);
                foreach (long id in stale)
                {
                    Account victim = AccountManager.GetAccount(id, true);
                    if (victim == null || !victim.IsOnline) continue;

                    SecurityDao.LogEvent(SecurityDao.SourceGame, victim.PlayerId, victim.Username, victim.Nickname,
                        "heartbeat_lost", "HEARTBEAT_LOST",
                        "FL Guard desconectado / sem heartbeat",
                        "{\"timeout_sec\":" + ConfigLoader.HeartbeatTimeoutSeconds +
                        ",\"grace_sec\":" + OnlineGraceSeconds + "}",
                        4, "FG-110");

                    AllUtils.KickPlayer(victim, "FL Guard desconectado (FG-110)", "HEARTBEAT_LOST",
                        SecurityDao.SourceGame, 0, "FG-110", requestClip: false);

                    _onlineSince.TryRemove(id, out _);
                    CLogger.Print($"[FL GUARD] Kick por heartbeat perdido: {victim.Nickname} (#{id})", LoggerType.Warning);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[FL GUARD] HeartbeatGuard: " + ex.Message, LoggerType.Error, ex);
            }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
            }
        }
    }
}
