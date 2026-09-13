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
    /// heartbeat a cada 15s (liveness) + poll de capture a cada 2.5s + ring buffer 20s em RAM.
    /// </summary>
    public sealed class GuardSession : IDisposable
    {
        private const double CapturePollSeconds = 2.5;

        private readonly string _host;
        private readonly int _port;
        private readonly long _playerId;
        private readonly string _sessionId;
        private readonly string _username;
        private readonly string _gameExeName;
        private readonly SemaphoreSlim _captureGate = new SemaphoreSlim(1, 1);

        private RingBufferVideo _ring;
        private CancellationTokenSource _cts;
        private Task _heartbeatLoop;
        private Task _capturePollLoop;
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
            if (_heartbeatLoop != null) return;
            _hw = HardwareFingerprint.Collect();
            _ring = new RingBufferVideo(TimeSpan.FromSeconds(20), fps: 8, maxWidth: 1280, jpegQuality: 72L, processName: _gameExeName);
            _ring.Start();
            _cts = new CancellationTokenSource();
            _heartbeatLoop = Task.Run(() => HeartbeatLoopAsync(_cts.Token));
            _capturePollLoop = Task.Run(() => CapturePollLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            // Goodbye best-effort antes de cancelar o loop — invalida live_sessions na hora.
            try
            {
                Task closed = SendClosedAsync();
                closed.Wait(TimeSpan.FromSeconds(1.2));
            }
            catch { }

            try { _cts?.Cancel(); } catch { }
            try { _heartbeatLoop?.Wait(3000); } catch { }
            try { _capturePollLoop?.Wait(3000); } catch { }
            _heartbeatLoop = null;
            _capturePollLoop = null;
            _ring?.Dispose();
            _ring = null;
        }

        /// <summary>
        /// Último heartbeat com status=closed para o Socket marcar a sessão como morta
        /// sem esperar o timeout do HeartbeatGuard.
        /// </summary>
        private async Task SendClosedAsync()
        {
            if (_playerId <= 0 || string.IsNullOrEmpty(_host) || _port <= 0)
                return;

            string hbJson = JsonConvert.SerializeObject(new
            {
                player_id = _playerId,
                session_id = _sessionId,
                fingerprint = _hw?.Fingerprint ?? "",
                status = "closed",
                status_reason = "guard_stop",
                modules_hash = "",
                capture_blocked_streak = 0
            });

            using (var client = new LAUNCHER_TCP_CLIENT_REQ())
            {
                await client.ConnectAsync(_host, _port).ConfigureAwait(false);
                await client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_CONNECT_REQ).ConfigureAwait(false);
                await client.ReceiveAsync().ConfigureAwait(false);
                await client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_HEARTBEAT_REQ,
                    System.Text.Encoding.UTF8.GetBytes(hbJson)).ConfigureAwait(false);
                try { await client.ReceiveAsync().ConfigureAwait(false); } catch { }
            }
        }

        private async Task HeartbeatLoopAsync(CancellationToken ct)
        {
            // 1º heartbeat na hora — evita FG-110 ao entrar no Game antes do poll antigo de 3s.
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
                            await HandleCaptureJobsAck(client, payload).ConfigureAwait(false);
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

        /// <summary>
        /// Canal dedicado: pergunta capture_requests a cada ~2.5s sem tocar no ring
        /// até haver job. Heartbeat 15s continua só como liveness + fallback.
        /// </summary>
        private async Task CapturePollLoopAsync(CancellationToken ct)
        {
            // Leve atraso só para o ring começar a encher; poll não afeta liveness.
            try { await Task.Delay(1500, ct).ConfigureAwait(false); }
            catch (OperationCanceledException) { return; }

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (_playerId > 0 && !string.IsNullOrEmpty(_host) && _port > 0)
                    {
                        using (var client = new LAUNCHER_TCP_CLIENT_REQ())
                        {
                            await client.ConnectAsync(_host, _port).ConfigureAwait(false);
                            await client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_CONNECT_REQ).ConfigureAwait(false);
                            await client.ReceiveAsync().ConfigureAwait(false);

                            string pollJson = JsonConvert.SerializeObject(new
                            {
                                player_id = _playerId,
                                session_id = _sessionId
                            });

                            await client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_CAPTURE_POLL_REQ,
                                System.Text.Encoding.UTF8.GetBytes(pollJson)).ConfigureAwait(false);

                            LAUNCHER_PACKET_REQ ack = await client.ReceiveAsync().ConfigureAwait(false);
                            if (ack.Opcode == (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_CAPTURE_POLL_ACK)
                            {
                                string payload = client.GetPayloadAsString(ack);
                                await HandleCaptureJobsAck(client, payload).ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[FL GUARD] capture poll: " + ex.Message);
                }

                try { await Task.Delay(TimeSpan.FromSeconds(CapturePollSeconds), ct).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task HandleCaptureJobsAck(LAUNCHER_TCP_CLIENT_REQ client, string payload)
        {
            if (string.IsNullOrWhiteSpace(payload) || _ring == null) return;
            JObject obj;
            try { obj = JObject.Parse(payload); }
            catch { return; }

            JToken captures = obj["captures"];
            if (captures == null || captures.Type != JTokenType.Array) return;
            if (!captures.HasValues) return;

            await _captureGate.WaitAsync().ConfigureAwait(false);
            try
            {
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
            finally
            {
                _captureGate.Release();
            }
        }

        public void Dispose() => Stop();
    }
}
