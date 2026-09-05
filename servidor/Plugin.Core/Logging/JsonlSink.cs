using System;
using System.IO;
using System.Text;

namespace Plugin.Core.Logging
{
    public sealed class JsonlSink : ILogSink
    {
        private readonly object _lock = new object();
        public string CurrentPath { get; }

        public JsonlSink(string dir)
        {
            try { Directory.CreateDirectory(dir); } catch { }
            string date = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss");
            CurrentPath = Path.Combine(dir, date + ".jsonl");
        }

        public void Write(LogEvent e)
        {
            try
            {
                string line = LogEventJson.ToLine(e);
                lock (_lock)
                {
                    using (var fs = new FileStream(CurrentPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var w = new StreamWriter(fs, new UTF8Encoding(false)))
                    {
                        w.Write(line);
                        w.Write('\n');
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[JsonlSink failed] {ex.Message}");
            }
        }
    }
}
