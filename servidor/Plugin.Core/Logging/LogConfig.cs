using System;
using System.Collections.Generic;

namespace Plugin.Core.Logging
{
    public enum CaptureLevel { Off, Packets, Full }
    public enum ConsoleLevel { Quiet, Packets }

    public static class LogConfig
    {
        // ---- packet schema tracing (pb-dev packet oracle) ----
        // Settings.ini [Server] TraceOpcodes = "" (off) | "all" | "2453,1037,..."
        public static bool SchemaTraceEnabled;          // build the trace at write time?
        private static bool _traceAll;
        private static readonly HashSet<int> _traceOps = new HashSet<int>();

        public static void ConfigureTrace(string raw)
        {
            _traceAll = false; _traceOps.Clear();
            if (string.IsNullOrWhiteSpace(raw)) { SchemaTraceEnabled = false; return; }
            string s = raw.Trim().ToLowerInvariant();
            if (s == "all") { _traceAll = true; SchemaTraceEnabled = true; return; }
            foreach (var part in raw.Split(','))
            {
                int op;
                if (int.TryParse(part.Trim(), out op)) _traceOps.Add(op);
            }
            SchemaTraceEnabled = _traceAll || _traceOps.Count > 0;
        }

        public static bool ShouldEmitSchema(int op) => _traceAll || _traceOps.Contains(op);

        public static CaptureLevel ParseCapture(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return CaptureLevel.Full;
            switch (s.Trim().ToLowerInvariant())
            {
                case "off": return CaptureLevel.Off;
                case "packets": return CaptureLevel.Packets;
                default: return CaptureLevel.Full;
            }
        }

        public static ConsoleLevel ParseConsole(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return ConsoleLevel.Packets;
            return s.Trim().ToLowerInvariant() == "quiet" ? ConsoleLevel.Quiet : ConsoleLevel.Packets;
        }
    }
}
