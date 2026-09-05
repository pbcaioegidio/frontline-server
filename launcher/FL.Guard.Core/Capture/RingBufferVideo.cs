using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FL.Guard.Core.Capture
{
    /// <summary>
    /// Buffer circular em RAM dos últimos N segundos da janela do jogo.
    /// Não grava em disco até DumpClipAsync ser chamado (GM / flag).
    /// Resolve o problema do menu de hack sumir na captura: o buffer já tem o comportamento anterior.
    /// </summary>
    public sealed class RingBufferVideo : IDisposable
    {
        private readonly object _gate = new object();
        private readonly LinkedList<Frame> _frames = new LinkedList<Frame>();
        private readonly TimeSpan _window;
        private readonly int _maxWidth;
        private readonly long _jpegQuality;
        private readonly int _intervalMs;
        private readonly string _processName;

        private CancellationTokenSource _cts;
        private Task _loop;
        private int _blockedStreak;
        private bool _disposed;

        private sealed class Frame
        {
            public DateTime Utc;
            public byte[] Jpeg;
            public int Width;
            public int Height;
        }

        public RingBufferVideo(TimeSpan? window = null, int fps = 5, int maxWidth = 640, long jpegQuality = 45L, string processName = null)
        {
            _window = window ?? TimeSpan.FromSeconds(20);
            _intervalMs = Math.Max(100, 1000 / Math.Max(1, fps));
            _maxWidth = maxWidth;
            _jpegQuality = jpegQuality;
            _processName = processName;
        }

        public int FrameCount { get { lock (_gate) return _frames.Count; } }
        public int BlockedStreak => _blockedStreak;
        public bool IsRunning => _loop != null && !_loop.IsCompleted;

        public void Start()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RingBufferVideo));
            if (_loop != null) return;
            _cts = new CancellationTokenSource();
            _loop = Task.Run(() => LoopAsync(_cts.Token));
        }

        public void Stop()
        {
            try { _cts?.Cancel(); } catch { }
            try { _loop?.Wait(2000); } catch { }
            _loop = null;
        }

        private async Task LoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var shot = ScreenCapture.CaptureGameWindow(_processName, _maxWidth, _jpegQuality, allowOccluded: false);
                    if (shot.Ok && shot.Jpeg != null)
                    {
                        Interlocked.Exchange(ref _blockedStreak, 0);
                        lock (_gate)
                        {
                            _frames.AddLast(new Frame
                            {
                                Utc = shot.CapturedAtUtc,
                                Jpeg = shot.Jpeg,
                                Width = shot.Width,
                                Height = shot.Height
                            });
                            PruneLocked();
                        }
                    }
                    else if (shot.Blocked)
                    {
                        Interlocked.Increment(ref _blockedStreak);
                    }
                }
                catch
                {
                    Interlocked.Increment(ref _blockedStreak);
                }

                try { await Task.Delay(_intervalMs, ct).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }

        private void PruneLocked()
        {
            DateTime cutoff = DateTime.UtcNow - _window;
            while (_frames.Count > 0 && _frames.First.Value.Utc < cutoff)
                _frames.RemoveFirst();
        }

        /// <summary>Screenshot imediato (qualidade maior; permite overlay se outra janela cobrir).</summary>
        public ScreenCapture.CaptureResult TakeScreenshot(int maxWidth = 1600, long quality = 82L)
            => ScreenCapture.CaptureGameWindow(_processName, maxWidth, quality, allowOccluded: true);

        /// <summary>
        /// Empacota os frames do buffer em um ZIP (meta.json + frame_0000.jpg...).
        /// Retorna null se não houver frames; Blocked=true se captura está bloqueada.
        /// </summary>
        public ClipDump DumpClip()
        {
            var dump = new ClipDump();
            List<Frame> copy;
            lock (_gate)
            {
                PruneLocked();
                copy = new List<Frame>(_frames);
            }

            if (copy.Count == 0)
            {
                dump.Blocked = _blockedStreak >= 3;
                dump.Error = dump.Blocked ? "captura bloqueada / sem frames" : "buffer vazio";
                return dump;
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                    {
                        var meta = new StringBuilder();
                        meta.Append("{\"version\":1,\"kind\":\"clip\",\"fps_target\":");
                        meta.Append(1000 / _intervalMs);
                        meta.Append(",\"frames\":").Append(copy.Count);
                        meta.Append(",\"from\":\"").Append(copy[0].Utc.ToString("o")).Append("\"");
                        meta.Append(",\"to\":\"").Append(copy[copy.Count - 1].Utc.ToString("o")).Append("\"");
                        meta.Append(",\"width\":").Append(copy[0].Width);
                        meta.Append(",\"height\":").Append(copy[0].Height);
                        meta.Append(",\"watermark\":\"FL GUARD\"}");
                        ZipEntryBytes(zip, "meta.json", Encoding.UTF8.GetBytes(meta.ToString()));

                        for (int i = 0; i < copy.Count; i++)
                        {
                            string name = "frame_" + i.ToString("D4") + ".jpg";
                            ZipEntryBytes(zip, name, copy[i].Jpeg);
                        }
                    }
                    dump.ZipBytes = ms.ToArray();
                    dump.Ok = dump.ZipBytes != null && dump.ZipBytes.Length > 0;
                    dump.FrameCount = copy.Count;
                    dump.DurationSeconds = (copy[copy.Count - 1].Utc - copy[0].Utc).TotalSeconds;
                }
            }
            catch (Exception ex)
            {
                dump.Error = ex.Message;
                dump.Blocked = true;
            }
            return dump;
        }

        public Task<ClipDump> DumpClipAsync() => Task.Run(DumpClip);

        private static void ZipEntryBytes(ZipArchive zip, string name, byte[] data)
        {
            ZipArchiveEntry entry = zip.CreateEntry(name, CompressionLevel.Fastest);
            using (Stream s = entry.Open())
                s.Write(data, 0, data.Length);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
            lock (_gate) _frames.Clear();
            _cts?.Dispose();
        }

        public sealed class ClipDump
        {
            public bool Ok;
            public bool Blocked;
            public string Error = "";
            public byte[] ZipBytes;
            public int FrameCount;
            public double DurationSeconds;
        }
    }
}
