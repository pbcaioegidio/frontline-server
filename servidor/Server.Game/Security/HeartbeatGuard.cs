using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Security;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Server.Game.Security
{
    /// <summary>
    /// Periodicamente kicka jogadores online cujo launcher parou o heartbeat (live_sessions).
    /// Só age se ConfigLoader.RequireLauncherHeartbeat = true.
    /// </summary>
    public static class HeartbeatGuard
    {
        private static Timer _timer;
        private static int _running;

        public static void Start()
        {
            if (_timer != null) return;
            _timer = new Timer(_ => Tick(), null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
            CLogger.Print("[FL GUARD] HeartbeatGuard iniciado (checa live_sessions a cada 20s)", LoggerType.Info);
        }

        public static void Stop()
        {
            try { _timer?.Dispose(); } catch { }
            _timer = null;
        }

        private static void Tick()
        {
            if (!ConfigLoader.RequireLauncherHeartbeat) return;
            if (Interlocked.Exchange(ref _running, 1) == 1) return;
            try
            {
                var online = new List<long>();
                foreach (GameManager mgr in GameXender.All)
                {
                    foreach (GameClient client in mgr.SocketSessions.Values)
                    {
                        Account p = client?.Player;
                        if (p != null && p.IsOnline && p.PlayerId > 0)
                            online.Add(p.PlayerId);
                    }
                }
                if (online.Count == 0) return;

                List<long> stale = SecurityDao.ListStaleHeartbeats(online, ConfigLoader.HeartbeatTimeoutSeconds);
                foreach (long id in stale)
                {
                    Account victim = AccountManager.GetAccount(id, true);
                    if (victim == null || !victim.IsOnline) continue;

                    SecurityDao.LogEvent(SecurityDao.SourceGame, victim.PlayerId, victim.Username, victim.Nickname,
                        "heartbeat_lost", "HEARTBEAT_LOST",
                        "FL Guard desconectado / sem heartbeat",
                        "{\"timeout_sec\":" + ConfigLoader.HeartbeatTimeoutSeconds + "}",
                        4, "FG-110");

                    AllUtils.KickPlayer(victim, "FL Guard desconectado (FG-110)", "HEARTBEAT_LOST",
                        SecurityDao.SourceGame, 0, "FG-110", requestClip: false);

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
