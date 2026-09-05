using Executable.Forms;
using Executable.UDP;
using Executable.UDP.Server;
using Executable.Utility;
using Plugin.Core;
using Plugin.Core.Colorful;
using Plugin.Core.Enums;
using Plugin.Core.Filters;
using Plugin.Core.JSON;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Console = Plugin.Core.Colorful.Console;

namespace Executable
{
    public class Program
    {
        #region Constants

        internal static readonly string BUILD_STAMP = "FrontLine-BR";
        private const int CONSOLE_WIDTH = 160;
        private const int CONSOLE_HEIGHT = 40;
        private const int COMMUNICATION_PORT = 1909;
        internal static readonly string INSTANCE_ID = "ae67c130-c5ef-d321-5e6b-686759584eab";
        private const int STARTUP_DELAY_MS = 250;
        private const int TITLE_UPDATE_INTERVAL_MS = 1000;
        private const int DAILY_RESET_CHECK_TIME = 000000;
        private const int SHUTDOWN_DELAY_MS = 1000;
        private const string ADMINISTRATOR_TITLE = "ADMINISTRADOR";
        private const string SUPERUSER_TITLE = "SUPERUSUARIO";
        private const string MONITOR_ARGUMENT = "-supc";
        private const string SERVICE_ARGUMENT = "--service";
        private const string PARENT_ARGUMENT = "--parent";
        private const string TAKEOVER_ARGUMENT = "--takeover";
        private const string NO_TAKEOVER_ARGUMENT = "--no-takeover";
        private const int TAKEOVER_WAIT_MS = 5000;
        private const int SW_RESTORE = 9;

        #endregion Constants

        #region Windows API

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);

        private enum CtrlType
        { CTRL_C_EVENT = 0, CTRL_BREAK_EVENT = 1, CTRL_CLOSE_EVENT = 2, CTRL_LOGOFF_EVENT = 5, CTRL_SHUTDOWN_EVENT = 6 }

        private static bool WindowsEventHandler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    CleanupFirewallRules();
                    return true;

                default:
                    return false;
            }
        }

        #endregion Windows API

        #region Fields

        internal static string ServiceMode = null;
        private static int parentPid = 0;
        private static bool takeoverDisabled = true;
        private static bool takeoverEnabled = false;
        private static string titleSuffix = "";
        private static Mutex applicationMutex = null;
        private static readonly FileInfo executableInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
        private static readonly int currentProcessId = Process.GetCurrentProcess().Id;
        private static readonly DateTime processStartedAt = DateTime.Now;
        internal static DateTime ProcessStartedAt => processStartedAt;
        private static readonly ServerManager serverManager = new ServerManager();

        #endregion Fields

        #region Main Entry

        [STAThread]
        protected static void Main(string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == SERVICE_ARGUMENT && i + 1 < args.Length) ServiceMode = args[i + 1].ToLowerInvariant();
                    if (args[i] == PARENT_ARGUMENT && i + 1 < args.Length) int.TryParse(args[i + 1], out parentPid);
                    if (args[i] == TAKEOVER_ARGUMENT) takeoverEnabled = true;
                    if (args[i] == NO_TAKEOVER_ARGUMENT) takeoverDisabled = true;
                }

                SetupExceptionHandlers();
                InitializeConsole();

                if (!ValidateSingleInstance()) return;

                ConfigureSystemSettings();

                bool includeMonitor = ServiceMode == null && ShouldIncludeMonitorForm(args);
                if (includeMonitor) StartMonitorFormAsync();

                string fileVersion = GetApplicationVersion();
                StartServerSystems(includeMonitor, fileVersion);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Critical startup error: {ex.Message}", LoggerType.Error, ex);
            }
            finally
            {
                CleanupResources();
            }
        }

        #endregion Main Entry

        #region Initialization

        private static void SetupExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Console.CancelKeyPress += OnCancelKeyPress;
        }

        private static void InitializeConsole()
        {
            try
            {
                // WinExe: sem console — painel de Logs na UI substitui o terminal
                if (GetConsoleWindow() == IntPtr.Zero) return;

                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;
                Console.SetWindowSize(CONSOLE_WIDTH, CONSOLE_HEIGHT);
                Console.CursorVisible = false;
                Console.TreatControlCAsInput = false;
                WindowUtility.MoveWindowToCenter();

                Console.Title = BuildTitlePrefix(GetApplicationVersion());
            }
            catch (Exception ex)
            {
                CLogger.Print($"Console initialization error: {ex.Message}", LoggerType.Warning, ex);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private static bool ValidateSingleInstance()
        {
            // Nome fixo: Assembly.Location pode ser .dll e variar entre builds.
            string mutexName = ServiceMode == null
                ? $"Global\\FrontLine-FLMonitor-{INSTANCE_ID}"
                : $"Global\\FrontLine-FLMonitor-{INSTANCE_ID}-{ServiceMode}";
            applicationMutex = new Mutex(true, mutexName, out bool isFirstInstance);

            if (isFirstInstance) return true;

            // Só toma o lugar da instância anterior com --takeover (útil p/ restart).
            if (ServiceMode == null && takeoverEnabled && !takeoverDisabled && TakeOverExistingInstances())
                return true;

            CLogger.Print("O FLMonitor ja esta em execucao! Saindo...", LoggerType.Warning);
            if (ServiceMode == null)
            {
                BringExistingMonitorToFront();
                MessageBox.Show("O FLMonitor ja esta em execucao.", "FrontLine", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return false;
        }

        private static void BringExistingMonitorToFront()
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName("FLMonitor"))
                {
                    if (process.Id == currentProcessId) continue;

                    IntPtr handle = process.MainWindowHandle;
                    if (handle == IntPtr.Zero) continue;

                    if (IsIconic(handle))
                        ShowWindow(handle, SW_RESTORE);
                    else
                        ShowWindow(handle, SW_RESTORE);

                    SetForegroundWindow(handle);
                    return;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"BringExistingMonitorToFront: {ex.Message}", LoggerType.Warning);
            }
        }

        private static bool TakeOverExistingInstances()
        {
            Process[] running = Process.GetProcessesByName("FLMonitor").Where(process => process.Id != currentProcessId).ToArray();

            if (running.Length == 0) return false;

            CLogger.Print($"Server already running; terminating {running.Length} instance(s) and taking over...", LoggerType.Warning);

            foreach (Process process in running)
            {
                try { WindowUtility.KillProcessAndChildren(process.Id); }
                catch (Exception ex) { CLogger.Print($"Failed to terminate pid {process.Id}: {ex.Message}", LoggerType.Warning, ex); }
            }

            // the killed owner leaves the mutex abandoned, which surfaces as an exception on the wait that grants us ownership
            try
            {
                if (applicationMutex.WaitOne(TAKEOVER_WAIT_MS)) return true;
            }
            catch (AbandonedMutexException) { return true; }

            CLogger.Print("Takeover failed: previous instance did not release the lock.", LoggerType.Error);
            return false;
        }

        private static void ConfigureSystemSettings()
        {
            if (MemoryUtility.IsAdministrator())
            {
                SetConsoleCtrlHandler(new EventHandler(WindowsEventHandler), true);
                FirewallUtil.AddFirewallRule(executableInfo.FullName);
                titleSuffix = ADMINISTRATOR_TITLE;
            }
            else
            {
                titleSuffix = SUPERUSER_TITLE;
            }
        }

        private static string GetApplicationVersion() => FileVersionInfo.GetVersionInfo(executableInfo.Name).FileVersion;

        private static string BuildTitlePrefix(string version) => $"FrontLine Monitor {version} ({BUILD_STAMP})";

        #endregion Initialization

        #region Server Startup

        private static void StartServerSystems(bool includeMonitor, string fileVersion)
        {
            serverManager.DisplayStartupBanner();

            if (ServiceMode != null)
            {
                StartChildService(fileVersion);
                return;
            }

            serverManager.LoadAllComponents(!ConfigLoader.ProcessSplit);
            Plugin.Core.CLogger.Init();

            Thread.Sleep(STARTUP_DELAY_MS);

            InitializeCommunication();

            bool serverStarted = ValidateAndStartServers();
            FinalizeStartup(serverStarted, includeMonitor, fileVersion);
        }

        private static void StartChildService(string fileVersion)
        {
            serverManager.LoadAllComponents(ServiceMode == "game");
            // split mode: the whole process belongs to one service, so anything the thread
            // stamping misses (timers, background workers) still gets attributed correctly
            switch (ServiceMode)
            {
                case "auth": Plugin.Core.CLogger.ProcessKind = Plugin.Core.Logging.ServerKind.Auth; break;
                case "game": Plugin.Core.CLogger.ProcessKind = Plugin.Core.Logging.ServerKind.Game; break;
                case "match": Plugin.Core.CLogger.ProcessKind = Plugin.Core.Logging.ServerKind.Match; break;
            }
            Plugin.Core.CLogger.Init();
            Thread.Sleep(STARTUP_DELAY_MS);

            StartParentWatchdog();

            bool started;
            switch (ServiceMode)
            {
                case "auth": started = AuthServerManager.Start(); break;
                case "game": started = GameServerManager.Start(); break;
                case "match": started = BattleServerManager.Start(); break;
                default:
                    CLogger.Print($"Unknown service mode '{ServiceMode}'", LoggerType.Error);
                    return;
            }

            CLogger.Print(started
                ? $"Service '{ServiceMode}' started (child process, pid {currentProcessId})"
                : $"Service '{ServiceMode}' FAILED to start", started ? LoggerType.Info : LoggerType.Error);
            if (!started) Environment.Exit(1);

            Console.Title = $"FL Servico [{ServiceMode.ToUpper()}] {fileVersion} - pid {currentProcessId}";
        }

        private static void StartParentWatchdog()
        {
            if (parentPid <= 0) return;
            new Thread(() =>
            {
                try
                {
                    Process parent = Process.GetProcessById(parentPid);
                    parent.WaitForExit();
                }
                catch { }
                CLogger.Print("Supervisor exited; shutting down service.", LoggerType.Warning);
                Environment.Exit(0);
            })
            { IsBackground = true }.Start();
        }

        private static void InitializeCommunication()
        {
            serverManager.PrintSection("Plugin Status", true);
            Communication.Start(new IPEndPoint(IPAddress.Parse(ConfigLoader.HOST[1]), COMMUNICATION_PORT));
            CLogger.Print("Todos os plugins do servidor carregados", LoggerType.Info);
            serverManager.PrintSection("Plugin Status", false);
        }

        private static bool ValidateAndStartServers() => DatabaseValidator.ValidateAllConnections() && StartAllServerInstances();

        private static bool StartAllServerInstances()
        {
            try
            {
                if (ConfigLoader.ProcessSplit)
                {
                    CLogger.Print("ProcessSplit enabled: launching services as child processes.", LoggerType.Info);
                    return Supervision.ProcessSupervisor.Instance.StartAll();
                }
                return AuthServerManager.Start() && GameServerManager.Start() && BattleServerManager.Start();
            }
            catch (Exception ex)
            {
                CLogger.Print($"Server startup error: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        private static void FinalizeStartup(bool serverStarted, bool includeMonitor, string fileVersion)
        {
            UpdateServerStatus(serverStarted, fileVersion);
            if (serverStarted) BeginServerOperation(includeMonitor, fileVersion);
        }

        private static void UpdateServerStatus(bool isOnline, string version)
        {
            StringUtility.ServerVersionL = version;
            StringUtility.ServerStatusL = isOnline ? "SERVIDOR ONLINE" : "SERVIDOR OFFLINE";

            string message = isOnline
                ? $"Inicializacao OK — FrontLine ativo ({DateTimeUtil.Now("yyyy")})"
                : $"Inicializacao FALHOU — FrontLine ({DateTimeUtil.Now("yyyy")})";
            LoggerType logType = isOnline ? LoggerType.Info : LoggerType.Warning;

            serverManager.PrintSection("Server Status", true);
            CLogger.Print(message, logType);
            serverManager.PrintSection("Server Status", false);
            try { Console.WriteLine(""); } catch { }
        }

        private static async void BeginServerOperation(bool includeMonitor, string version)
        {
            try { await StartTitleUpdateLoop(includeMonitor, version); }
            catch (Exception ex) { CLogger.Print($"Server operation error: {ex.Message}", LoggerType.Error, ex); }
        }

        #endregion Server Startup

        #region Monitor and Title Management

        // UI do monitor: padrao ao abrir FLMonitor.exe (sem --service).
        // -supc mantido por compatibilidade; --headless sobe so o servidor sem janela.
        private static bool ShouldIncludeMonitorForm(string[] args)
        {
            if (args == null || args.Length == 0) return true;
            if (args.Any(a => string.Equals(a, "--headless", StringComparison.OrdinalIgnoreCase))) return false;
            return true;
        }

        private static void StartMonitorFormAsync()
        {
            var thread = new Thread(StartMonitorForm)
            {
                IsBackground = false,
                Name = "FLMonitor-UI"
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private static void StartMonitorForm()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var monitor = new MainForm(currentProcessId, new DirectoryInfo($"{executableInfo.Directory}/Logs")) { TopMost = true };
                Application.Run(monitor);
            }
            catch (Exception ex) { CLogger.Print($"Monitor form error: {ex.Message}", LoggerType.Error, ex); }
        }

        private static async Task StartTitleUpdateLoop(bool monitorMode, string version)
        {
            while (true)
            {
                try
                {
                    UpdateFormInformation();
                    UpdateConsoleTitle(monitorMode, version);
                    CheckDailyReset();
                    await Task.Delay(TITLE_UPDATE_INTERVAL_MS);
                }
                catch (Exception ex)
                {
                    CLogger.Print($"Title update error: {ex.Message}", LoggerType.Warning, ex);
                    await Task.Delay(TITLE_UPDATE_INTERVAL_MS);
                }
            }
        }

        // Set when the process has no console window to write a title to (launched detached, or as a
        // supervised child). Writing the title then throws once per tick, which used to bury the log in
        // "Title update error" warnings AND skip CheckDailyReset for the rest of the run.
        private static bool titleWritable = true;

        private static void UpdateConsoleTitle(bool monitorMode, string version)
        {
            if (!titleWritable) return;
            var statistics = ServerStatistics.GetCurrent();
            string titleInfo = monitorMode ? $"RAM Usage: {statistics.MemoryUsageMB:0.0} MB)" : $"Users: {statistics.TotalUsers}; Online: {statistics.OnlineUsers}; RAM Usage: {statistics.MemoryUsageMB:0.0} MB ({statistics.MemoryUsagePercent:0.0}%)";
            try
            {
                Console.Title = $"{BuildTitlePrefix(version)} </> {titleInfo} -{titleSuffix} </> {DateTimeUtil.Now("dddd, dd MMMM yyyy - HH:mm:ss")}";
            }
            catch (Exception ex)
            {
                titleWritable = false;
                CLogger.Print($"No console title available ({ex.Message.Trim()}); title updates disabled for this run.", LoggerType.Debug);
            }
        }

        #endregion Monitor and Title Management

        #region Database Management

        private static void CheckDailyReset()
        {
            try
            {
                if (int.Parse(DateTimeUtil.Now("HHmmss")) == DAILY_RESET_CHECK_TIME)
                    DatabaseManager.PerformDailyReset();
            }
            catch (Exception ex) { CLogger.Print($"Daily reset check error: {ex.Message}", LoggerType.Error, ex); }
        }

        private static void UpdateFormInformation()
        {
            var statistics = ServerStatistics.GetCurrent();
            var networkInfo = NetworkInformation.GetCurrent();
            var gameInfo = GameInformation.GetCurrent();
            StringUtilityHelper.UpdateAllStatistics(statistics, networkInfo, gameInfo);
        }

        #endregion Database Management

        #region Event Handlers

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            CLogger.Print("Server shutdown initiated by user.", LoggerType.Info);
            SendMessage.FromServer("Attention! \nThe Server Will Be Restarted!");
            if (ConfigLoader.ProcessSplit && ServiceMode == null)
                Supervision.ProcessSupervisor.Instance.StopAll();
            e.Cancel = true;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            CLogger.Print($"Unhandled Exception - Sender: {sender}, Terminating: {e.IsTerminating}, Exception: {exception.Message}", LoggerType.Error, exception);
        }

        #endregion Event Handlers

        #region Cleanup

        private static void CleanupFirewallRules()
        {
            try { FirewallUtil.RemoveFirewallRule(executableInfo.FullName); }
            catch (Exception ex) { CLogger.Print($"Firewall cleanup error: {ex.Message}", LoggerType.Warning, ex); }
        }

        private static void CleanupResources()
        {
            try
            {
                applicationMutex?.ReleaseMutex();
                Process.GetCurrentProcess().WaitForExit();
            }
            catch (Exception ex) { CLogger.Print($"Resource cleanup error: {ex.Message}", LoggerType.Warning, ex); }
        }

        #endregion Cleanup
    }

    #region Server Manager - Unified Class

    public class ServerManager
    {
        private const int LINE_WIDTH = 100;

        private static readonly string[] TEAM_CREDITS = new string[]
        {
            "FrontLine Server Monitor  |  Auth · Game · Match",
            "Private server BR 122  ·  use FLMonitor.exe para subir tudo",
        };

        public void DisplayStartupBanner()
        {
            Console.WriteLine();
            PrintLine('=');
            Console.WriteLine();

            string[] logo = new string[]
            {
                @"  ███████╗██████╗  ██████╗ ███╗   ██╗████████╗██╗     ██╗███╗   ██╗███████╗",
                @"  ██╔════╝██╔══██╗██╔═══██╗████╗  ██║╚══██╔══╝██║     ██║████╗  ██║██╔════╝",
                @"  █████╗  ██████╔╝██║   ██║██╔██╗ ██║   ██║   ██║     ██║██╔██╗ ██║█████╗  ",
                @"  ██╔══╝  ██╔══██╗██║   ██║██║╚██╗██║   ██║   ██║     ██║██║╚██╗██║██╔══╝  ",
                @"  ██║     ██║  ██║╚██████╔╝██║ ╚████║   ██║   ███████╗██║██║ ╚████║███████╗",
                @"  ╚═╝     ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═══╝   ╚═╝   ╚══════╝╚═╝╚═╝  ╚═══╝╚══════╝",
                @"",
                @"                    ▓▓▓  M O N I T O R   D O   S E R V I D O R  ▓▓▓",
                @"                         ─────────────────────────────",
                @"                              linha de frente · BR",
            };

            int logoWidth = logo.Max(line => line.Length);
            int logoPad = Math.Max(0, (LINE_WIDTH - logoWidth) / 2);
            foreach (string line in logo)
                System.Console.WriteLine(new string(' ', logoPad) + line);

            Console.WriteLine();
            foreach (string credit in TEAM_CREDITS)
                Console.WriteLine(CenterText(credit));

            Console.WriteLine();
            PrintLine('=');
            Console.WriteLine();
        }

        public void PrintSection(string name, bool isBegin)
        {
            if (string.IsNullOrEmpty(name)) return;

            string arrow = isBegin ? ">>>" : "<<<";
            string status = isBegin ? "[CARREGANDO]" : "[PRONTO]";

            string header = $"{arrow} {name} {status}";
            int padding = (LINE_WIDTH - header.Length) / 2;

            if (isBegin)
            {
                Console.WriteLine();
                Console.WriteLine(new string('-', LINE_WIDTH));
                Console.WriteLine(new string(' ', padding) + header);
            }
            else
            {
                Console.WriteLine(new string(' ', padding) + header);
                Console.WriteLine(new string('-', LINE_WIDTH));
            }
        }

        public void LoadAllComponents(bool includeRcon)
        {
            LoadConfigurations();
            LoadEventData();
            LoadPortalData();
            LoadShopData();
            LoadMissionData();
            LoadServerData();
            LoadGameModes();

            if (includeRcon)
            {
                PrintSection("Rcon Status", true);
                RconCommand.Instance();
                PrintSection("Rcon Status", false);
            }
        }

        private void LoadConfigurations()
        {
            PrintSection("Config", true);
            try
            {
                ServerConfigJSON.Load();
                CommandHelperJSON.Load();
                ResolutionJSON.Load();
                ValidateCriticalConfigurations();
            }
            catch (Exception ex)
            {
                CLogger.Print($"Configuration loading failed: {ex.Message}", LoggerType.Error, ex);
                throw;
            }
            PrintSection("Config", false);
        }

        private void ValidateCriticalConfigurations()
        {
            if (ConfigLoader.HOST == null || ConfigLoader.HOST.Length < 3)
                throw new InvalidOperationException("Host configuration is missing or incomplete");

            if (ConfigLoader.DEFAULT_PORT == null || ConfigLoader.DEFAULT_PORT.Length < 3)
                throw new InvalidOperationException("Port configuration is missing or incomplete");

            for (int i = 0; i < ConfigLoader.DEFAULT_PORT.Length; i++)
            {
                if (ConfigLoader.DEFAULT_PORT[i] <= 0)
                    throw new InvalidOperationException($"Invalid port value at index {i}: {ConfigLoader.DEFAULT_PORT[i]}");
            }

            CLogger.Print("All critical configurations validated successfully", LoggerType.Info);
        }

        private void LoadEventData()
        {
            PrintSection("Events Data", true);
            EventLoginXML.Load();
            EventBoostXML.Load();
            EventRankUpXML.Load();
            EventPlaytimeJSON.Load();
            EventQuestXML.Load();
            EventVisitXML.Load();
            EventXmasXML.Load();
            PrintSection("Events Data", false);
        }

        private void LoadShopData()
        {
            PrintSection("Shop Data", true);
            ShopManager.Load(1);
            ShopManager.Load(2);
            PrintSection("Shop Data", false);
        }

        private void LoadMissionData()
        {
            PrintSection("Mission Cards", true);
            MissionCardRAW.LoadBasicCards(1);
            MissionCardRAW.LoadBasicCards(2);
            PrintSection("Mission Cards", false);
        }

        private void LoadServerData()
        {
            PrintSection("Server Data", true);
            LoadServerXMLFiles();
            LoadServerFilters();
            PrintSection("Server Data", false);
        }

        private void LoadGameModes()
        {
            PrintSection("Classic Mode", true);
            ClassicModeManager.LoadList();
            PrintSection("Classic Mode", false);

            PrintSection("Battle Pass", true);
            BattlePassManager.Load();
            PrintSection("Battle Pass", false);

            PrintSection("Competitive", true);
            CompetitiveXML.Load();
            PrintSection("Competitive", false);
        }

        private void LoadPortalData()
        {
            PrintSection("Portal Data", true);
            PortalManager.Load();
            PrintSection("Portal Data", false);
        }

        private void LoadServerXMLFiles()
        {
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
        }

        private void LoadServerFilters() => NickFilter.Load();

        private void PrintLine(char character) => Console.WriteLine(new string(character, LINE_WIDTH));

        private string CenterText(string text)
        {
            int padding = (LINE_WIDTH - text.Length) / 2;
            return new string(' ', Math.Max(0, padding)) + text;
        }
    }

    #endregion Server Manager - Unified Class

    #region Support Classes

    public static class DatabaseValidator
    {
        public static bool ValidateAllConnections() => ComDiv.ValidateAllPlayersAccount();
    }

    public static class AuthServerManager
    {
        public static bool Start()
        {
            try
            {
                var manager = new ServerManager();
                manager.PrintSection("Servidor Auth", true);
                Server.Auth.Data.XML.ChannelsXML.Load();
                AuthXender.GetPlugin(ConfigLoader.HOST[0], ConfigLoader.DEFAULT_PORT[0]);
                manager.PrintSection("Servidor Auth", false);
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Falha ao iniciar Servidor Auth: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }
    }

    public static class GameServerManager
    {
        public static bool Start()
        {
            try
            {
                var manager = new ServerManager();
                manager.PrintSection("Servidor Game", true);
                Server.Game.Data.XML.ChannelsXML.Load();
                Server.Game.Data.Managers.ClanManager.Load();

                foreach (SChannelModel server in SChannelXML.Servers)
                {
                    if (server.Id >= 1 && server.Port > 0)
                        GameXender.GetPlugin(server.Id, ConfigLoader.HOST[0], server.Port);
                }

                manager.PrintSection("Servidor Game", false);
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Falha ao iniciar Servidor Game: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }
    }

    public static class BattleServerManager
    {
        public static bool Start()
        {
            try
            {
                var manager = new ServerManager();
                manager.PrintSection("Servidor Battle", true);
                Server.Match.Data.XML.MapStructureXML.Load();
                Server.Match.Data.XML.CharaStructureXML.Load();
                Server.Match.Data.XML.ItemStatisticXML.Load();
                MatchXender.GetPlugin(ConfigLoader.HOST[0], ConfigLoader.DEFAULT_PORT[2]);
                manager.PrintSection("Servidor Battle", false);
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Falha ao iniciar Servidor Battle: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }
    }

    public static class DatabaseManager
    {
        public static bool PerformDailyReset()
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
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Daily reset failed: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }
    }

    public class ServerStatistics
    {
        public double MemoryUsageMB { get; set; }
        public double MemoryUsagePercent { get; set; }
        public int TotalUsers { get; set; }
        public int OnlineUsers { get; set; }
        public int TotalClans { get; set; }
        public int VipUsers { get; set; }
        public int BannedPlayers { get; set; }

        public static ServerStatistics GetCurrent()
        {
            return new ServerStatistics
            {
                MemoryUsageMB = MemoryUtility.GetMemoryUsage(),
                MemoryUsagePercent = MemoryUtility.GetMemoryUsagePercent(),
                TotalUsers = ComDiv.CountDB("SELECT COUNT(*) FROM accounts"),
                OnlineUsers = ComDiv.CountDB($"SELECT COUNT(*) FROM accounts WHERE online = {true}"),
                TotalClans = ComDiv.CountDB("SELECT COUNT(*) FROM system_clan"),
                VipUsers = ComDiv.CountDB($"SELECT COUNT(*) FROM accounts WHERE pc_cafe = '2' OR pc_cafe = '1'"),
                BannedPlayers = ComDiv.CountDB($"SELECT COUNT(*) FROM base_auto_ban")
            };
        }
    }

    public class NetworkInformation
    {
        public string LocalAddress { get; set; }
        public string GameRegion { get; set; }

        public static NetworkInformation GetCurrent()
        {
            return new NetworkInformation
            {
                LocalAddress = GetLocalAddress(),
                GameRegion = GetGameLocale()
            };
        }

        private static string GetLocalAddress()
        {
            try
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress address in host.AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                        return address.ToString();
                }
            }
            catch (Exception ex) { CLogger.Print($"Failed to get local address: {ex.Message}", LoggerType.Warning, ex); }
            return "Unknown";
        }

        private static string GetGameLocale()
        {
            foreach (ClientLocale region in ConfigLoader.GameLocales)
            {
                if (region == ClientLocale.Russia)
                    return region.ToString();
            }
            return "Outside!";
        }
    }

    public class GameInformation
    {
        public string Version { get; set; }
        public string ConfigId { get; set; }
        public bool TournamentRule { get; set; }
        public bool InternetCafe { get; set; }
        public bool AutoAccount { get; set; }
        public bool AutoBan { get; set; }

        public static GameInformation GetCurrent()
        {
            return new GameInformation
            {
                Version = $"V{ServerConfigJSON.GetConfig(ConfigLoader.ConfigId)?.ClientVersion ?? "Unknown"}",
                ConfigId = ConfigLoader.ConfigId.ToString(),
                TournamentRule = ConfigLoader.TournamentRule,
                InternetCafe = ConfigLoader.ICafeSystem,
                AutoAccount = ConfigLoader.AutoAccount,
                AutoBan = ConfigLoader.AutoBan
            };
        }
    }

    public static class StringUtilityHelper
    {
        public static void UpdateAllStatistics(ServerStatistics stats, NetworkInformation network, GameInformation game)
        {
            StringUtility.MemoryValueVPB = $"{Convert.ToInt32(stats.MemoryUsageMB)}";
            StringUtility.MemoryUsageL = $"{stats.MemoryUsagePercent:0.0}%";
            StringUtility.RegisteredUserL = stats.TotalUsers.ToString();
            StringUtility.OnlineUserL = stats.OnlineUsers.ToString();
            StringUtility.TotalClansL = stats.TotalClans.ToString();
            StringUtility.VipUserL = stats.VipUsers.ToString();
            StringUtility.BannedPlayers = stats.BannedPlayers.ToString();
            StringUtility.LocalAddressL = network.LocalAddress;
            StringUtility.ForGameRegionL = network.GameRegion;
            StringUtility.ForGameVersionL = game.Version;
            StringUtility.SelectedServerConfigL = game.ConfigId;
            StringUtility.TournamentRuleL = game.TournamentRule ? "Ativado" : "Desativado";
            StringUtility.InternetCafeL = game.InternetCafe ? "Ativado" : "Desativado";
            StringUtility.EnableAutoAccountL = game.AutoAccount ? "Ativado" : "Desativado";
            StringUtility.AutoBanPlayerL = game.AutoBan ? "Ativado" : "Desativado";
            var up = DateTime.Now - Program.ProcessStartedAt;
            StringUtility.ServerTimelineL = $"{(int)up.TotalHours:00}:{up.Minutes:00}:{up.Seconds:00}";
            StringUtility.PortsL = $"Auth {ConfigLoader.DEFAULT_PORT[0]} · Game {ConfigLoader.DEFAULT_PORT[1]} · Match {ConfigLoader.DEFAULT_PORT[2]}";
            StringUtility.ProcessSplitL = ConfigLoader.ProcessSplit ? "Ativado" : "Desativado";
            StringUtility.DbHostL = $"{ConfigLoader.DatabaseHost}:{ConfigLoader.DatabasePort}/{ConfigLoader.DatabaseName}";
            StringUtility.LogFileSize = CalculateLogFileSize();
            StringUtility.RegShopItems = CalculateShopItems().ToString();
            StringUtility.ShopCafeItems = CalculateCafeItems().ToString();
            StringUtility.RepairableItems = ComDiv.CountDB("SELECT COUNT(*) FROM system_shop_repair").ToString();
            StringUtility.UnknownUserL = ComDiv.CountDB("SELECT COUNT(*) FROM accounts WHERE nickname = ''").ToString();
        }

        private static string CalculateLogFileSize()
        {
            try
            {
                var executableInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
                double size = MemoryUtility.GetDirectorySize(new DirectoryInfo($"{executableInfo.Directory}/Logs"), true);
                return $"{(size / (1024 * 1024)):N2}MB";
            }
            catch { return "0.00MB"; }
        }

        private static int CalculateShopItems()
        {
            return ComDiv.CountDB("SELECT COUNT(*) FROM system_shop") +
                   ComDiv.CountDB("SELECT COUNT(*) FROM system_shop_effects") +
                   ComDiv.CountDB("SELECT COUNT(*) FROM system_shop_sets");
        }

        private static int CalculateCafeItems()
        {
            return ComDiv.CountDB($"SELECT COUNT(*) FROM system_shop WHERE item_visible = '{false}'") +
                   ComDiv.CountDB($"SELECT COUNT(*) FROM system_shop_effects WHERE coupon_visible = '{false}'");
        }
    }

    #endregion Support Classes
}