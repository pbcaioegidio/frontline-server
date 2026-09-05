using FL.Guard.Core.Capture;
using FL.Guard.Core.Identity;
using FL.Guard.Core.Protection;
using Launcher.PointBlank.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Launcher.PointBlank.Services
{
    /// <summary>
    /// Sessão FL GUARD enquanto o jogo está aberto:
    /// heartbeat a cada 15s + ring buffer 20s em RAM + responde a pedidos de screenshot/clip.
    /// </summary>
    public sealed class GuardSession : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly long _playerId;
        private readonly string _sessionId;
        private readonly string _username;
        private readonly string _gameExeName;

        private RingBufferVideo _ring;
        private CancellationTokenSource _cts;
        private Task _loop;
        private HardwareComponents _hw;

        public GuardSession(string host, int port, long playerId, string sessionId, string username, string gameExeName = "FrontLine")
        {
            _host = host;
            _port = port;
            _playerId = playerId;
            _sessionId = string.IsNullOrEmpty(sessionId) ? Guid.NewGuid().ToString("N") : sessionId;
            _username = username ?? "";
            _gameExeName = gameExeName;
        }

        public void Start()
        {
            if (_loop != null) return;
            _hw = HardwareFingerprint.Collect();
            _ring = new RingBufferVideo(TimeSpan.FromSeconds(20), fps: 8, maxWidth: 1280, jpegQuality: 72L, processName: _gameExeName);
            _ring.Start();
            _cts = new CancellationTokenSource();
            _loop = Task.Run(() => LoopAsync(_cts.Token));
        }

        public void Stop()
        {
            try { _cts?.Cancel(); } catch { }
            try { _loop?.Wait(3000); } catch { }
            _loop = null;
            _ring?.Dispose();
            _ring = null;
        }

        private async Task LoopAsync(CancellationToken ct)
        {
            await Task.Delay(3000, ct).ConfigureAwait(false);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    RuntimeScan.ScanResult scan = RuntimeScan.Scan(_gameExeName);

                    using (var client = new LAUNCHER_TCP_CLIENT_REQ())
                    {
                        await client.ConnectAsync(_host, _port).ConfigureAwait(false);
                        await client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_CONNECT_REQ).ConfigureAwait(false);
                        await client.ReceiveAsync().ConfigureAwait(false);

                        string status = scan.Status;
                        string statusReason = scan.StatusReason ?? "";
                        if (_ring != null && _ring.BlockedStreak >= 5 && status == "ok")
                        {
                            status = "suspect";
                            statusReason = "captura bloqueada (" + _ring.BlockedStreak + ")";
                        }

                        string hbJson = JsonConvert.SerializeObject(new
                        {
                            player_id = _playerId,
                            session_id = _sessionId,
                            fingerprint = _hw?.Fingerprint ?? "",
                            status,
                            status_reason = statusReason,
                            modules_hash = scan.ModulesHash ?? "",
                            modules = scan.Modules,
                            suspicious = scan.Suspicious,
                            capture_blocked_streak = _ring?.BlockedStreak ?? 0
                        });

                        await client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_HEARTBEAT_REQ,
                            System.Text.Encoding.UTF8.GetBytes(hbJson)).ConfigureAwait(false);

                        LAUNCHER_PACKET_REQ ack = await client.ReceiveAsync().ConfigureAwait(false);
                        if (ack.Opcode == (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_HEARTBEAT_ACK)
                        {
                            string payload = client.GetPayloadAsString(ack);
                            await HandleHeartbeatAck(client, payload).ConfigureAwait(false);
                        }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[FL GUARD] heartbeat: " + ex.Message);
                }

                try { await Task.Delay(TimeSpan.FromSeconds(15), ct).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task HandleHeartbeatAck(LAUNCHER_TCP_CLIENT_REQ client, string payload)
        {
            if (string.IsNullOrWhiteSpace(payload) || _ring == null) return;
            JObject obj;
            try { obj = JObject.Parse(payload); }
            catch { return; }

            JToken captures = obj["captures"];
            if (captures == null || captures.Type != JTokenType.Array) return;

            foreach (JToken job in captures)
            {
                long requestId = job.Value<long?>("request_id") ?? 0;
                string kind = (job.Value<string>("kind") ?? "screenshot").ToLowerInvariant();
                if (requestId <= 0) continue;

                bool blocked = false;
                string error = "";
                string b64 = "";
                string filename = "";

                if (kind == "clip")
                {
                    var dump = await _ring.DumpClipAsync().ConfigureAwait(false);
                    if (dump.Ok && dump.ZipBytes != null)
                    {
                        b64 = Convert.ToBase64String(dump.ZipBytes);
                        filename = "clip_" + requestId + ".zip";
                    }
                    else
                    {
                        blocked = dump.Blocked || dump.FrameCount == 0;
                        error = dump.Error;
                    }
                }
                else
                {
                    var shot = _ring.TakeScreenshot();
                    if (shot.Ok && shot.Jpeg != null)
                    {
                        b64 = Convert.ToBase64String(shot.Jpeg);
                        filename = "shot_" + requestId + ".jpg";
                    }
                    else
                    {
                        blocked = true;
                        error = shot.Error;
                    }
                }

                string upload = JsonConvert.SerializeObject(new
                {
                    request_id = requestId,
                    player_id = _playerId,
                    session_id = _sessionId,
                    username = _username,
                    kind,
                    blocked,
                    error,
                    filename,
                    data_base64 = b64
                });

                await client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_CAPTURE_UPLOAD,
                    System.Text.Encoding.UTF8.GetBytes(upload)).ConfigureAwait(false);
                await client.ReceiveAsync().ConfigureAwait(false);
            }
        }

        public void Dispose() => Stop();
    }
}
