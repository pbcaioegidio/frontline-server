using System;
using System.IO;

namespace Launcher.Services
{
    internal static class ServiceLog
    {
        private static readonly object Sync = new object();
        private static string _path;

        public static void Init(string baseDirectory)
        {
            string dir = Path.Combine(baseDirectory, "logs");
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, "socket.log");
        }

        public static void Write(string message)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message;
            try
            {
                lock (Sync)
                {
                    if (!string.IsNullOrEmpty(_path))
                        File.AppendAllText(_path, line + Environment.NewLine);
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
