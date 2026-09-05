using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Plugin.Core.Enums;
using Plugin.Core.Logging;

namespace Plugin.Core
{
    public static class CLogger
    {
        private static readonly object Sync = new object();
        private static readonly List<ILogSink> Sinks = new List<ILogSink>();
        private static CaptureLevel _capture = CaptureLevel.Full;
        private static bool _ready;
        private static bool _initializing;
        public static long LastSeq;
        private static string _lastSig; private static DateTime _lastSigAt;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private static bool HasConsoleSink()
        {
            if (!OperatingSystem.IsWindows())
                return true;
            try { return GetConsoleWindow() != IntPtr.Zero; }
            catch { return false; }
        }

        // ---- ambient service attribution ----
        // Only CLogger.Packet ever received an explicit ServerKind, so every Info/Warn/Error/Event
        // line landed with srv=null (~68% of the corpus). The receive callbacks stamp the thread on
        // entry; ProcessKind is the fallback that makes attribution exact under ProcessSplit=true.
        [ThreadStatic] private static ServerKind? _threadKind;
        public static ServerKind? ProcessKind;

        public static void SetThreadKind(ServerKind? kind) => _threadKind = kind;
        public static ServerKind? CurrentKind => _threadKind ?? ProcessKind;

        // Pool threads keep whatever kind the previous work item left behind, so an unstamped
        // callback (the connection watchdogs) logged under a random service. Stamp on entry and
        // clear on exit: work that never declares a kind now ends up srv=null instead of wrong.
        public static void QueueWork(ServerKind kind, Action work)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                _threadKind = kind;
                try { work(); }
                finally { _threadKind = null; }
            });
        }

        // ---- existing exception-timestamp state kept for back-compat ----
        public static DateTime LastAuthException = DateTime.MinValue;
        public static DateTime LastGameException = DateTime.MinValue;
        public static DateTime LastGameSession = DateTime.MinValue;
        public static DateTime LastMatchSocket = DateTime.MinValue;
        public static DateTime LastMatchBuffer = DateTime.MinValue;
        public static DateTime LatchAuthSession = DateTime.MinValue;
        public static DateTime SetLastGameException(DateTime d) => LastGameException = d;
        public static DateTime SetLastAuthException(DateTime d) => LastAuthException = d;
        public static DateTime SetLastGameSession(DateTime d) => LastGameSession = d;
        public static DateTime SetLastMatchSocket(DateTime d) => LastMatchSocket = d;
        public static DateTime SetLastMatchBuffer(DateTime d) => LastMatchBuffer = d;
        public static DateTime SetLatchAuthSession(DateTime d) => LatchAuthSession = d;
        public static DateTime GetLastGameException() => LastGameException;
        public static DateTime GetLastAuthException() => LastAuthException;
        public static DateTime GetLastGameSession() => LastGameSession;
        public static DateTime GetLastMatchSocket() => LastMatchSocket;
        public static DateTime GetLastMatchBuffer() => LastMatchBuffer;
        public static DateTime GetLatchAuthSession() => LatchAuthSession;
        public static DateTime UpdateLastMatchSocket() => LastMatchSocket = DateTime.Now;
        public static DateTime UpdateLastMatchBuffer() => LastMatchBuffer = DateTime.Now;

        public static void Init()
        {
            lock (Sync)
            {
                if (_ready || _initializing) return;
                _initializing = true;
                try
                {
                    string capRaw = null, conRaw = null;
                    try { capRaw = ConfigLoader.CaptureLevelRaw; conRaw = ConfigLoader.ConsoleLevelRaw; } catch (Exception ex) { System.Console.WriteLine($"[CLogger] ConfigLoader unavailable, using defaults (capture=Full, console=Packets): {ex.Message}"); }
                    _capture = LogConfig.ParseCapture(capRaw);
                    var console = LogConfig.ParseConsole(conRaw);
                    try { LogConfig.ConfigureTrace(ConfigLoader.TraceOpcodesRaw); } catch { }
                    // Windows WinExe sem console: só painel UI + arquivos. Linux: stdout (journald).
                    if (HasConsoleSink())
                        Sinks.Add(new ConsoleSink(LogLevel.Debug, console == ConsoleLevel.Packets));
                    Sinks.Add(new UiLogSink(LogLevel.Info, showPackets: false));
                    Sinks.Add(new SlimLogSink("Logs"));
                    if (_capture != CaptureLevel.Off) Sinks.Add(new JsonlSink("Logs/events"));
                    Sinks.Add(new TraceSink("Logs"));
                    _ready = true;
                }
                finally { _initializing = false; }
            }
        }

        public static void Emit(LogEvent e)
        {
            try
            {
                if (!_ready) Init();
                if (!e.Srv.HasValue) e.Srv = _threadKind ?? ProcessKind;
                if (e.Cat == LogCat.Packet)
                {
                    if (_capture == CaptureLevel.Off) { /* still allow console */ }
                    if (_capture != CaptureLevel.Full) e.Hex = null; // omit hex unless Full
                }
                e.Seq = LogEvent.NextSeq();
                System.Threading.Interlocked.Exchange(ref LastSeq, e.Seq);
                List<ILogSink> snapshot;
                lock (Sync) snapshot = new List<ILogSink>(Sinks);
                string sig = $"{e.Level}|{e.Cat}|{e.ErrType}|{e.ErrMessage}";
                bool dup = sig == _lastSig && (DateTime.Now - _lastSigAt).TotalSeconds < 2 && e.Cat != LogCat.Packet;
                _lastSig = sig; _lastSigAt = DateTime.Now;
                foreach (var s in snapshot)
                {
                    if (dup && (s is ConsoleSink || s is SlimLogSink)) continue;
                    s.Write(e);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[CLogger.Emit failed] {ex.Message}");
            }
        }

        public static void Packet(ServerKind srv, Direction dir, ushort op, string pkt, int conn, int len, byte[] payload)
            => Packet(srv, dir, op, pkt, conn, len, payload, null);

        public static void Packet(ServerKind srv, Direction dir, ushort op, string pkt, int conn, int len, byte[] payload, IReadOnlyList<FieldTrace> schema)
        {
            string conRaw = null;
            try { conRaw = ConfigLoader.ConsoleLevelRaw; } catch (Exception ex) { System.Console.WriteLine($"[CLogger] ConfigLoader unavailable, using defaults (capture=Full, console=Packets): {ex.Message}"); }
            if (_capture == CaptureLevel.Off && (LogConfig.ParseConsole(conRaw) == ConsoleLevel.Quiet))
                return; // nothing would consume it
            Emit(new LogEvent
            {
                Level = LogLevel.Debug, Cat = LogCat.Packet, Srv = srv, Dir = dir,
                Op = op, Pkt = pkt, Conn = conn, Len = len,
                Schema = (schema != null && schema.Count > 0 && LogConfig.ShouldEmitSchema(op)) ? schema : null,
                Hex = payload != null ? BitConverter.ToString(payload) : null
            });
        }

        public static void Event(LogCat cat, object fields, string msg = null, LogLevel level = LogLevel.Info)
            => Emit(new LogEvent { Level = level, Cat = cat, ErrMessage = msg, Fields = ToDict(fields) });

        public static void Info(string msg)  => Emit(new LogEvent { Level = LogLevel.Info, Cat = LogCat.System, ErrMessage = msg });
        public static void Warn(string msg)  => Emit(new LogEvent { Level = LogLevel.Warn, Cat = LogCat.System, ErrMessage = msg });
        public static void DebugMsg(string msg) => Emit(new LogEvent { Level = LogLevel.Debug, Cat = LogCat.System, ErrMessage = msg });

        public static void Error(string msg, Exception ex = null)
        {
            var e = new LogEvent { Level = LogLevel.Error, Cat = LogCat.System, ErrMessage = msg };
            if (ex != null)
            {
                e.ErrType = ex.GetType().Name;
                var tr = new StackTrace(ex, true);
                var f = tr.FrameCount > 0 ? tr.GetFrame(0) : null;
                int line = f?.GetFileLineNumber() ?? 0;
                if (line > 0) e.ErrAt = (f.GetMethod()?.ReflectedType?.Name ?? "?") + ":" + line;
                LastGameException = DateTime.Now;
            }
            Emit(e);
        }

        // ---- back-compat shim: old call sites keep working ----
        public static void Print(string text, LoggerType type, Exception ex = null)
        {
            switch (type)
            {
                case LoggerType.Error: Error(text, ex); break;
                case LoggerType.Warning: Warn(text); break;
                case LoggerType.Debug: DebugMsg(text); break;
                case LoggerType.Opcode: Emit(new LogEvent { Level = LogLevel.Info, Cat = LogCat.Opcode, ErrMessage = text }); break;
                case LoggerType.Hack: Emit(new LogEvent { Level = LogLevel.Hack, Cat = LogCat.Hack, ErrMessage = text }); break;
                default: Info(text); break;
            }
        }

        private static IReadOnlyDictionary<string, object> ToDict(object fields)
        {
            if (fields == null) return null;
            if (fields is IReadOnlyDictionary<string, object> rod) return rod;
            if (fields is IDictionary d)
            {
                var m = new Dictionary<string, object>();
                foreach (DictionaryEntry kv in d) m[kv.Key.ToString()] = kv.Value;
                return m;
            }
            var map = new Dictionary<string, object>();
            foreach (var p in fields.GetType().GetProperties()) map[p.Name] = p.GetValue(fields, null);
            return map;
        }
    }
}
