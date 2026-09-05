using System;
using System.IO;
using System.Text;

namespace Plugin.Core.Logging
{
    public sealed class TraceSink : ILogSink
    {
        private readonly object _lock = new object();
        public string CurrentPath { get; }

        public static bool Enabled = true;

        public TraceSink(string dir)
        {
            try { Directory.CreateDirectory(dir); } catch { }
            CurrentPath = Path.Combine(dir, "trace-" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + ".log");
        }

        public void Write(LogEvent e)
        {
            try
            {
                if (!Enabled) return;
                bool isPacket = e.Cat == LogCat.Packet;
                bool isUnhandled = e.Cat == LogCat.Opcode && e.Dir.HasValue;
                if (!isPacket && !isUnhandled) return;
                int op = e.Op ?? 0;
                string dir = e.Dir == Direction.In ? "C2S" : "S2C";
                string srv = (e.Srv?.ToString() ?? "?").ToUpperInvariant();
                string name = e.Pkt ?? (isUnhandled ? "UNHANDLED" : "?");
                string line = string.Format("{0:HH:mm:ss.fff} | {1,-4} | {2} | op={3,-5} 0x{4:X4} | {5} | len={6} | {7}",
                    e.TsUtc.ToLocalTime(), srv, dir, op, op, name, e.Len ?? 0, e.Hex ?? "");
                lock (_lock)
                {
                    using (var fs = new FileStream(CurrentPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var w = new StreamWriter(fs, new UTF8Encoding(false)))
                        w.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("[TraceSink failed] " + ex.Message);
            }
        }
    }
}
