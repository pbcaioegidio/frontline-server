using System;
using System.IO;
using System.Text;

namespace Plugin.Core.Logging
{
    public sealed class SlimLogSink : ILogSink
    {
        private readonly object _lock = new object();
        public string CurrentPath { get; }

        public SlimLogSink(string dir)
        {
            try { Directory.CreateDirectory(dir); } catch { }
            CurrentPath = Path.Combine(dir, DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + ".log");
        }

        public void Write(LogEvent e)
        {
            try
            {
                if (e.Cat == LogCat.Packet || e.Cat == LogCat.Opcode) return;
                if (e.Level < LogLevel.Info) return;
                string line = string.Format("{0:yyyy-MM-dd HH:mm:ss} [{1}] {2}",
                    e.TsUtc.ToLocalTime(),
                    e.Level.ToString().ToUpperInvariant(),
                    ConsoleSink.Render(e));
                lock (_lock)
                {
                    using (var fs = new FileStream(CurrentPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var w = new StreamWriter(fs, new UTF8Encoding(false)))
                        w.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("[SlimLogSink failed] " + ex.Message);
            }
        }
    }
}
