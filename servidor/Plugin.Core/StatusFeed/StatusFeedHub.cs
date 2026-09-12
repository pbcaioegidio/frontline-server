using Plugin.Core.Enums;
using System;
using System.Text.Json;
using System.Threading;

namespace Plugin.Core.StatusFeed
{
    /// <summary>
    /// Hub de eventos de presença para o Discord #status.
    /// O servidor WS (Fleck) registra-se em <see cref="SetBroadcaster"/> no processo Game;
    /// Auth pode chamar Publish sem ter listener (NOTIFY Postgres cobre o bot).
    /// </summary>
    public static class StatusFeedHub
    {
        private static Action<string> _broadcaster;
        private static int _heartbeatSeconds = 120;
        private static Timer _heartbeat;

        public static bool Enabled { get; private set; }

        public static void Configure(bool enabled, int heartbeatSeconds = 120)
        {
            Enabled = enabled;
            _heartbeatSeconds = Math.Max(30, heartbeatSeconds);
        }

        public static void SetBroadcaster(Action<string> broadcaster)
        {
            _broadcaster = broadcaster;
            _heartbeat?.Dispose();
            if (broadcaster != null && Enabled)
            {
                _heartbeat = new Timer(_ =>
                {
                    try { PublishSnapshot("up"); }
                    catch { /* ignore */ }
                }, null, TimeSpan.FromSeconds(_heartbeatSeconds), TimeSpan.FromSeconds(_heartbeatSeconds));
            }
        }

        public static void PublishPlayer(long playerId, string nickname, bool online, int onlineCount)
        {
            if (!Enabled || _broadcaster == null)
                return;
            Broadcast(new
            {
                type = online ? "player.online" : "player.offline",
                playerId,
                nickname = nickname ?? "",
                online = onlineCount,
                ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }

        public static void PublishSnapshot(string server = "up", int? onlineCount = null)
        {
            if (!Enabled || _broadcaster == null)
                return;
            int count = onlineCount ?? SafeOnlineCount();
            Broadcast(new
            {
                type = "snapshot",
                online = count,
                server,
                ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        }

        public static int SafeOnlineCount()
        {
            try
            {
                return Utility.ComDiv.CountDB("SELECT COUNT(*) FROM accounts WHERE online = true");
            }
            catch
            {
                return 0;
            }
        }

        private static void Broadcast(object payload)
        {
            try
            {
                string json = JsonSerializer.Serialize(payload);
                _broadcaster?.Invoke(json);
            }
            catch (Exception ex)
            {
                CLogger.Print($"StatusFeedHub broadcast: {ex.Message}", LoggerType.Warning);
            }
        }
    }
}
