using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Filters;
using Plugin.Core.JSON;
using Plugin.Core.Logging;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.RAW;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Auth;
using Server.Game;
using Server.Game.Data.Managers;
using Server.Game.Rcon;
using Server.Match;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Server.Host
{
    internal static class Program
    {
        private const string MutexName = "FrontLine-FLMonitor";
        private const int DailyResetHms = 0;
        private static readonly ManualResetEventSlim Shutdown = new ManualResetEventSlim(false);
        private static readonly List<Process> Children = new List<Process>();
        private static Mutex _mutex;

        private static int Main(string[] args)
        {
            string contentRoot = GetOption(args, "--content-root") ?? Environment.GetEnvironmentVariable("PB_CONTENT_ROOT") ?? AppContext.BaseDirectory;
            RuntimePaths.SetContentRoot(contentRoot);
            string service = GetOption(args, "--service");

            if (!TryAcquireMutex(service))
                return 1;

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                Shutdown.Set();
            };
            PosixSignalRegistration sigterm = RegisterSignal(PosixSignal.SIGTERM);
            PosixSignalRegistration sigint = RegisterSignal(PosixSignal.SIGINT);

            try
            {
                if (args.Any(arg => string.Equals(arg, "--wait-for-signal", StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine("FrontLine signal probe ready.");
                    Shutdown.Wait();
                    return 0;
                }

                StampProcessKind(service);
                if (service == null)
                    return RunSupervisor();
                return RunService(service);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Critical startup error: {ex.Message}", LoggerType.Error, ex);
                return 1;
            }
            finally
            {
                Shutdown.Set();
                StopServices();
                StopChildren();
                sigterm?.Dispose();
                sigint?.Dispose();
                ReleaseMutex();
            }
        }

        private static bool TryAcquireMutex(string service)
        {
            string name = string.IsNullOrEmpty(service) ? MutexName : MutexName + "-" + service;
            _mutex = new Mutex(true, name, out bool created);
            if (created)
                return true;
            try
            {
                if (_mutex.WaitOne(0))
                    return true;
            }
            catch (AbandonedMutexException)
            {
                return true;
            }
            Console.Error.WriteLine("FLMonitor ja esta em execucao.");
            return false;
        }

        private static void ReleaseMutex()
        {
            try { _mutex?.ReleaseMutex(); } catch { }
            _mutex?.Dispose();
        }

        private static void StampProcessKind(string service)
        {
            switch (service)
            {
                case "auth": CLogger.ProcessKind = ServerKind.Auth; break;
                case "game": CLogger.ProcessKind = ServerKind.Game; break;
                case "match": CLogger.ProcessKind = ServerKind.Match; break;
            }
        }

        private static int RunSupervisor()
        {
            ServerBootstrap.LoadAll(true);
            CLogger.Init();
            StartDailyResetLoop();
            if (!ComDiv.ValidateAllPlayersAccount())
                return 1;

            if (ConfigLoader.ProcessSplit)
            {
                foreach (string service in new[] { "auth", "game", "match" })
                {
                    Children.Add(StartChild(service));
                    Thread.Sleep(1500);
                }
            }
            else if (!StartAllServices())
                return 1;

            CLogger.Print("FrontLine server online (Linux).", LoggerType.Info);
            Shutdown.Wait();
            return 0;
        }

        private static int RunService(string service)
        {
            ServerBootstrap.LoadAll(service == "game");
            CLogger.Init();
            if (!ComDiv.ValidateAllPlayersAccount())
                return 1;
            if (!StartService(service))
                return 1;
            CLogger.Print($"Service '{service}' online.", LoggerType.Info);
            Shutdown.Wait();
            return 0;
        }

        private static bool StartAllServices()
        {
            return StartAuth() && StartGame() && StartMatch();
        }

        private static bool StartService(string service)
        {
            switch (service)
            {
                case "auth": return StartAuth();
                case "game": return StartGame();
                case "match": return StartMatch();
                default: throw new ArgumentException($"Unknown service '{service}'.");
            }
        }

        private static bool StartAuth()
        {
            Server.Auth.Data.XML.ChannelsXML.Load();
            return AuthXender.GetPlugin(ConfigLoader.HOST[0], ConfigLoader.DEFAULT_PORT[0]);
        }

        private static bool StartGame()
        {
            Server.Game.Data.XML.ChannelsXML.Load();
            ClanManager.Load();
            bool started = false;
            foreach (SChannelModel server in SChannelXML.Servers)
            {
                if (server.Id < 1 || server.Port <= 0)
                    continue;
                started |= GameXender.GetPlugin(server.Id, ConfigLoader.HOST[0], server.Port);
            }
            return started;
        }

        private static bool StartMatch()
        {
            Server.Match.Data.XML.MapStructureXML.Load();
            Server.Match.Data.XML.CharaStructureXML.Load();
            Server.Match.Data.XML.ItemStatisticXML.Load();
            return MatchXender.GetPlugin(ConfigLoader.HOST[0], ConfigLoader.DEFAULT_PORT[2]);
        }

        private static Process StartChild(string service)
        {
            string processPath = Environment.ProcessPath;
            string entryAssembly = Assembly.GetEntryAssembly().Location;
            bool usesDotnetHost = string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase);
            string prefix = usesDotnetHost ? $"\"{entryAssembly}\" " : string.Empty;
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = processPath,
                Arguments = $"{prefix}--service {service} --content-root \"{RuntimePaths.ContentRoot}\"",
                WorkingDirectory = RuntimePaths.ContentRoot,
                UseShellExecute = false
            };
            Process process = Process.Start(info);
            process.EnableRaisingEvents = true;
            process.Exited += (sender, eventArgs) =>
            {
                if (!Shutdown.IsSet)
                {
                    CLogger.Print($"Service '{service}' exited with code {process.ExitCode}; stopping supervisor.", LoggerType.Error);
                    Shutdown.Set();
                }
            };
            return process;
        }

        private static void StartDailyResetLoop()
        {
            new Thread(() =>
            {
                while (!Shutdown.IsSet)
                {
                    try
                    {
                        if (int.Parse(DateTime.Now.ToString("HHmmss")) == DailyResetHms)
                            PerformDailyReset();
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print($"Daily reset check error: {ex.Message}", LoggerType.Error, ex);
                    }
                    Shutdown.Wait(1000);
                }
            })
            { IsBackground = true, Name = "FL-DailyReset" }.Start();
        }

        private static void PerformDailyReset()
        {
            try
            {
                int playerDailies = ComDiv.CountDB("SELECT COUNT(*) FROM player_stat_dailies");
                if (playerDailies > 0)
                {
                    ComDiv.UpdateDB("player_stat_dailies",
                        new string[] { "matches", "match_wins", "match_loses", "match_draws", "kills_count", "deaths_count", "headshots_count", "exp_gained", "point_gained", "playtime" },
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                }

                int playerReports = ComDiv.CountDB("SELECT COUNT(*) FROM player_reports");
                if (playerReports > 0)
                    ComDiv.UpdateDB("player_reports", new string[] { "ticket_count" }, 3);

                CLogger.Print("Daily database reset completed successfully", LoggerType.Info);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Daily reset failed: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private static void StopServices()
        {
            try { AuthXender.Sync?.Close(); } catch { }
            try { AuthXender.Client?.MainSocket?.Close(); } catch { }
            foreach (GameManager manager in GameXender.All)
            {
                try { manager.Sync?.Close(); } catch { }
                try { manager.MainSocket?.Close(); } catch { }
            }
            try { MatchXender.Sync?.Close(); } catch { }
            try { MatchXender.Client?.Shutdown(); } catch { }
        }

        private static void StopChildren()
        {
            foreach (Process child in Children)
            {
                try
                {
                    if (!child.HasExited)
                    {
                        if (!OperatingSystem.IsWindows())
                        {
                            using (Process signal = Process.Start("/bin/kill", $"-TERM {child.Id}"))
                                signal.WaitForExit(2000);
                            child.WaitForExit(5000);
                        }
                        if (!child.HasExited)
                        {
                            child.Kill(true);
                            child.WaitForExit(5000);
                        }
                    }
                }
                catch { }
                child.Dispose();
            }
        }

        private static PosixSignalRegistration RegisterSignal(PosixSignal signal)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return null;
            return PosixSignalRegistration.Create(signal, context =>
            {
                context.Cancel = true;
                Shutdown.Set();
            });
        }

        private static string GetOption(string[] args, string option)
        {
            for (int i = 0; i + 1 < args.Length; i++)
                if (string.Equals(args[i], option, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            return null;
        }
    }

    internal static class ServerBootstrap
    {
        public static void LoadAll(bool includeRcon)
        {
            ServerConfigJSON.Load();
            CommandHelperJSON.Load();
            ResolutionJSON.Load();
            EventLoginXML.Load();
            EventBoostXML.Load();
            EventRankUpXML.Load();
            EventPlaytimeJSON.Load();
            EventQuestXML.Load();
            EventVisitXML.Load();
            EventXmasXML.Load();
            PortalManager.Load();
            ShopManager.Load(1);
            ShopManager.Load(2);
            MissionCardRAW.LoadBasicCards(1);
            MissionCardRAW.LoadBasicCards(2);
            TemplatePackXML.Load();
            TitleSystemXML.Load();
            TitleAwardXML.Load();
            MissionAwardXML.Load();
            MissionConfigXML.Load();
            MissionStreamXML.Load();
            SChannelXML.Load();
            ChannelTypeConditionManager.Load();
            SynchronizeXML.Load();
            SystemMapXML.Load();
            ClanRankXML.Load();
            PlayerRankXML.Load();
            CouponEffectXML.Load();
            PermissionXML.Load();
            RandomBoxXML.Load();
            BattleBoxXML.Load();
            DirectLibraryXML.Load();
            InternetCafeXML.Load();
            RedeemCodeXML.Load();
            BattleRewardXML.Load();
            NickFilter.Load();
            ClassicModeManager.LoadList();
            BattlePassManager.Load();
            CompetitiveXML.Load();
            if (includeRcon)
                RconCommand.Instance();
        }
    }
}
