using Fleck;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.StatusFeed;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Server.Game.StatusFeed
{
    /// <summary>
    /// WebSocket dedicado ao bot Discord (#status). Não misturar com RCON (:30000).
    /// Bind interno Docker; auth por token na query (?token=) ou 1ª mensagem {"auth":"..."}.
    /// </summary>
    public sealed class StatusFeedServer
    {
        private static StatusFeedServer _instance;

        public static StatusFeedServer Instance()
            => _instance ?? (_instance = new StatusFeedServer());

        private readonly ConcurrentDictionary<Guid, IWebSocketConnection> _clients =
            new ConcurrentDictionary<Guid, IWebSocketConnection>();
        private readonly HashSet<Guid> _authed = new HashSet<Guid>();
        private readonly object _authLock = new object();
        private WebSocketServer _server;
        private readonly string _token;
        private readonly bool _enabled;

        private StatusFeedServer()
        {
            _enabled = ConfigLoader.StatusFeedEnable;
            _token = ConfigLoader.StatusFeedToken ?? "";
            if (!_enabled)
            {
                CLogger.Print("StatusFeed desligado (PB_STATUS_FEED_ENABLE / StatusFeedEnable)", LoggerType.Info);
                return;
            }
            if (string.IsNullOrWhiteSpace(_token))
            {
                CLogger.Print("StatusFeed: token vazio — recusando start", LoggerType.Warning);
                return;
            }

            try
            {
                FleckLog.Level = Fleck.LogLevel.Error;
                string host = ConfigLoader.StatusFeedBindHost;
                int port = ConfigLoader.StatusFeedPort;
                _server = new WebSocketServer($"ws://{host}:{port}");
                _server.Start(socket =>
                {
                    socket.OnOpen = () => OnOpen(socket);
                    socket.OnClose = () => OnClose(socket);
                    socket.OnMessage = msg => OnMessage(socket, msg);
                });
                StatusFeedHub.Configure(true, ConfigLoader.StatusFeedHeartbeatSeconds);
                StatusFeedHub.SetBroadcaster(Broadcast);
                CLogger.Print($"StatusFeed ws://{host}:{port} (token required)", LoggerType.Info);
            }
            catch (Exception ex)
            {
                CLogger.Print("StatusFeed start: " + ex.Message, LoggerType.Warning, ex);
            }
        }

        private void OnOpen(IWebSocketConnection socket)
        {
            _clients[socket.ConnectionInfo.Id] = socket;
            // Token na query: ws://host:30001/?token=...
            string path = socket.ConnectionInfo.Path ?? "";
            if (TryAuthFromPath(path))
            {
                MarkAuthed(socket.ConnectionInfo.Id);
                SendSnapshot(socket);
            }
        }

        private void OnClose(IWebSocketConnection socket)
        {
            Guid id = socket.ConnectionInfo.Id;
            _clients.TryRemove(id, out _);
            lock (_authLock)
                _authed.Remove(id);
        }

        private void OnMessage(IWebSocketConnection socket, string message)
        {
            try
            {
                Guid id = socket.ConnectionInfo.Id;
                bool isAuthed;
                lock (_authLock)
                    isAuthed = _authed.Contains(id);

                if (!isAuthed)
                {
                    if (message != null && message.IndexOf(_token, StringComparison.Ordinal) >= 0)
                    {
                        MarkAuthed(id);
                        SendSnapshot(socket);
                    }
                    else
                    {
                        socket.Close();
                    }
                    return;
                }

                if (string.Equals(message, "ping", StringComparison.OrdinalIgnoreCase))
                    socket.Send("{\"type\":\"pong\",\"ts\":" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "}");
            }
            catch (Exception ex)
            {
                CLogger.Print("StatusFeed message: " + ex.Message, LoggerType.Warning);
            }
        }

        private bool TryAuthFromPath(string path)
        {
            int q = path.IndexOf('?');
            if (q < 0)
                return false;
            string query = path.Substring(q + 1);
            foreach (string part in query.Split('&'))
            {
                string[] kv = part.Split(new[] { '=' }, 2);
                if (kv.Length == 2 && kv[0] == "token" && kv[1] == _token)
                    return true;
            }
            return false;
        }

        private void MarkAuthed(Guid id)
        {
            lock (_authLock)
                _authed.Add(id);
        }

        private void SendSnapshot(IWebSocketConnection socket)
        {
            try
            {
                int count = StatusFeedHub.SafeOnlineCount();
                long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                socket.Send($"{{\"type\":\"snapshot\",\"online\":{count},\"server\":\"up\",\"ts\":{ts}}}");
            }
            catch { /* ignore */ }
        }

        private void Broadcast(string json)
        {
            foreach (var kv in _clients)
            {
                bool ok;
                lock (_authLock)
                    ok = _authed.Contains(kv.Key);
                if (!ok)
                    continue;
                try
                {
                    if (kv.Value.IsAvailable)
                        kv.Value.Send(json);
                }
                catch
                {
                    /* drop */
                }
            }
        }
    }
}
