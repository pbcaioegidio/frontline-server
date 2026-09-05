using Plugin.Core;
using Plugin.Core.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Executable.Supervision
{
    public class ServiceStatus
    {
        public string Name { get; set; }
        public bool Running { get; set; }
        public int Pid { get; set; }
        public DateTime StartedAt { get; set; }
        public int RestartCount { get; set; }
        public double MemoryMB { get; set; }
    }

    public class ProcessSupervisor
    {
        private const int RESTART_DELAY_MS = 5000;
        private const int MAX_RESTARTS_PER_WINDOW = 5;
        private const int RESTART_WINDOW_MINUTES = 10;

        public static readonly ProcessSupervisor Instance = new ProcessSupervisor();
        private static readonly string ExePath = Assembly.GetExecutingAssembly().Location;
        private static readonly int SupervisorPid = Process.GetCurrentProcess().Id;

        private class Entry
        {
            public string Name;
            public Process Process;
            public DateTime StartedAt;
            public int RestartCount;
            public readonly List<DateTime> RecentRestarts = new List<DateTime>();
            public bool StopRequested;
        }

        private readonly object gate = new object();
        private readonly Dictionary<string, Entry> services = new Dictionary<string, Entry>
        {
            { "auth", new Entry { Name = "auth" } },
            { "game", new Entry { Name = "game" } },
            { "match", new Entry { Name = "match" } },
        };

        public bool StartAll()
        {
            bool ok = true;
            foreach (string name in new[] { "auth", "game", "match" })
            {
                ok &= StartInternal(name);
                Thread.Sleep(1500);
            }
            return ok;
        }

        public void Start(string name)
        {
            lock (gate)
            {
                services[name].StopRequested = false;
            }
            StartInternal(name);
        }

        public void Stop(string name)
        {
            lock (gate)
            {
                Entry e = services[name];
                e.StopRequested = true;
                KillEntry(e);
            }
        }

        public void Restart(string name)
        {
            Stop(name);
            Thread.Sleep(1000);
            Start(name);
        }

        public void StopAll()
        {
            foreach (string name in services.Keys.ToList()) Stop(name);
        }

        public List<ServiceStatus> GetStatus()
        {
            lock (gate)
            {
                return services.Values.Select(e =>
                {
                    bool running = e.Process != null && !SafeHasExited(e.Process);
                    double mem = 0;
                    if (running)
                    {
                        try { e.Process.Refresh(); mem = e.Process.WorkingSet64 / (1024.0 * 1024.0); }
                        catch { }
                    }
                    return new ServiceStatus
                    {
                        Name = e.Name,
                        Running = running,
                        Pid = running ? e.Process.Id : 0,
                        StartedAt = e.StartedAt,
                        RestartCount = e.RestartCount,
                        MemoryMB = mem
                    };
                }).ToList();
            }
        }

        private bool StartInternal(string name)
        {
            lock (gate)
            {
                Entry e = services[name];
                if (e.Process != null && !SafeHasExited(e.Process)) return true;
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = ExePath,
                        Arguments = $"--service {name} --parent {SupervisorPid}",
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(ExePath)
                    };
                    Process p = Process.Start(psi);
                    p.EnableRaisingEvents = true;
                    p.Exited += (s, a) => OnServiceExited(name);
                    e.Process = p;
                    e.StartedAt = DateTime.Now;
                    CLogger.Print($"[Supervisor] Started service '{name}' (pid {p.Id})", LoggerType.Info);
                    return true;
                }
                catch (Exception ex)
                {
                    CLogger.Print($"[Supervisor] Failed to start '{name}': {ex.Message}", LoggerType.Error, ex);
                    return false;
                }
            }
        }

        private void OnServiceExited(string name)
        {
            Entry e;
            lock (gate)
            {
                e = services[name];
                if (e.StopRequested)
                {
                    CLogger.Print($"[Supervisor] Service '{name}' stopped by operator.", LoggerType.Info);
                    return;
                }
                e.RecentRestarts.RemoveAll(t => t < DateTime.Now.AddMinutes(-RESTART_WINDOW_MINUTES));
                if (e.RecentRestarts.Count >= MAX_RESTARTS_PER_WINDOW)
                {
                    CLogger.Print($"[Supervisor] Service '{name}' crashed {e.RecentRestarts.Count} times in {RESTART_WINDOW_MINUTES}min; giving up. Use the Services tab to start it manually.", LoggerType.Error);
                    return;
                }
                e.RecentRestarts.Add(DateTime.Now);
                e.RestartCount++;
            }
            CLogger.Print($"[Supervisor] Service '{name}' exited unexpectedly; restarting in {RESTART_DELAY_MS / 1000}s (restart #{e.RestartCount})", LoggerType.Warning);
            new Thread(() =>
            {
                Thread.Sleep(RESTART_DELAY_MS);
                StartInternal(name);
            })
            { IsBackground = true }.Start();
        }

        private static void KillEntry(Entry e)
        {
            if (e.Process == null) return;
            try
            {
                if (!SafeHasExited(e.Process)) e.Process.Kill();
            }
            catch (Exception ex)
            {
                CLogger.Print($"[Supervisor] Kill '{e.Name}' failed: {ex.Message}", LoggerType.Warning);
            }
            e.Process = null;
        }

        private static bool SafeHasExited(Process p)
        {
            try { return p.HasExited; }
            catch { return true; }
        }
    }
}
