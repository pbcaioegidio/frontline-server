using Plugin.Core.Enums;
using Plugin.Core.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Plugin.Core
{
    public static class ConfigLoader
    {
        public static string[] HOST = new string[3]
        {
        "0.0.0.0",
        "0.0.0.0",
        "0.0.0.0"
        };

        public static readonly int[] DEFAULT_PORT = new int[3]
        {
        39190,
        39191,
        40009
        };

        public static readonly int[] PROXY_PORT = new int[3]
        {
        23850,
        23851,
        24669
        };

        internal static readonly byte[] PROTO_SEED =
        {
            0x30, 0xc1, 0x67, 0xae, 0xef, 0xc5, 0x21, 0xd3,
            0x5e, 0x6b, 0x68, 0x67, 0x59, 0x58, 0x4e, 0xab
        };

        public static string RconIp;
        public static string RconPassword;
        public static int RconPort;

        public static bool RconEnable;
        public static bool StatusFeedEnable;
        public static string StatusFeedBindHost;
        public static string StatusFeedToken;
        public static int StatusFeedPort;
        public static int StatusFeedHeartbeatSeconds;
        public static bool RconInfoCommand;
        public static bool RconPrintNotValidIp;
        public static bool RconNotValidIpEnable;
        public static List<string> RconValidIps;

        public static string DatabaseName;
        public static string DatabaseHost;
        public static string DatabaseUsername;
        public static string DatabasePassword;
        public static string UdpVersion;
        public static string RandomPasswordChars;
        public static string CryptedPasswordSalt;
        public static bool IsUseProxy;
        public static bool IsTestMode;
        public static bool ShowMoreInfo;
        public static bool ProcessSplit;
        public static bool AutoAccount;
        public static bool DebugMode;
        public static bool TraceEnabled;
        public static string CaptureLevelRaw;
        public static string ConsoleLevelRaw;
        public static string TraceOpcodesRaw;
        public static bool WinCashPerBattle;
        public static bool ShowCashReceiveWarn;
        public static bool AutoBan;
        public static bool SendInfoToServ;
        public static bool SendFailMsg;
        public static bool EnableLog;
        public static bool UseMaxAmmoInDrop;
        public static bool UseHitMarker;
        public static bool ICafeSystem;
        public static bool IsDebugPing;
        public static bool CustomYear;
        public static bool AntiScript;
        public static bool SendPingSync;
        public static bool TournamentRule;
        public static bool RandomPassword;
        public static bool UseCryptedPassword;
        public static int DatabasePort;
        public static int ConfigId;
        public static int MaxNickSize;
        public static int MinNickSize;
        public static int MaxUserSize;
        public static int MinUserSize;
        public static int MaxPassSize;
        public static int MinPassSize;
        public static int BackLog;
        public static int MaxLatency;
        public static int MaxRepeatLatency;
        public static int MaxActiveClans;
        public static int MinRankVote;
        public static int MaxExpReward;
        public static int MaxGoldReward;
        public static int MaxCashReward;
        public static int MinCreateGold;
        public static int MinCreateRank;
        public static int InternetCafeId;
        public static int BackYear;
        public static int PingUpdateTimeSeconds;
        public static int PlayersServerUpdateTimeSeconds;
        public static int UpdateIntervalPlayersServer;
        public static int EmptyRoomRemovalInterval;
        public static int ConsoleTitleUpdateTimeSeconds;
        public static int IntervalEnterRoomAfterKickSeconds;
        public static int MaxBuyItemDays;
        public static int MaxBuyItemUnits;
        public static int BattleRewardId;
        public static int MaxConnectionPerIp;
        public static int ConnectionThrottleSeconds;
        /// <summary>Auth só aceita token emitido pelo launcher (token_expires no futuro). Liga quando o Socket estiver no ar.</summary>
        public static bool RequireOtpToken;
        /// <summary>Queima o token logo após o login OK (senão fica válido até expirar / até o logout).</summary>
        public static bool OtpOneShot;
        /// <summary>Hard ban também bloqueia o /24 do IP (cuidado com NAT de operadora).</summary>
        public static bool HardBanSubnet;
        /// <summary>Game kicka jogador se o launcher parar de mandar heartbeat (live_sessions).</summary>
        public static bool RequireLauncherHeartbeat;
        /// <summary>Segundos sem heartbeat antes do kick (padrão 45 = 3 miss).</summary>
        public static int HeartbeatTimeoutSeconds;
        /// <summary>Probation: horas após criar a conta sem ranked/clan.</summary>
        public static int ProbationHours;
        /// <summary>Limite de partidas por hora em probation (0 = sem limite extra).</summary>
        public static int ProbationMaxMatchesPerHour;
        /// <summary>Violações de speed no Match antes de kick da partida.</summary>
        public static int SpeedKickViolations;
        /// <summary>Violações de speed antes de auto-ban (se AutoBan).</summary>
        public static int SpeedBanViolations;
        /// <summary>Janela em segundos para contar kills rápidas (anti-burst).</summary>
        public static int KillBurstWindowSeconds;
        /// <summary>Kills na janela que disparam flag/clip/kick na partida.</summary>
        public static int KillBurstMaxKills;
        /// <summary>Se false (padrão), não aplica FG-124/clip/kick no modo bot/desafio.</summary>
        public static bool KillBurstInBotMode;
        /// <summary>Heurísticas aimbot/FOV no Match (FG-130/132).</summary>
        public static bool AimbotDetect;
        /// <summary>Amostras mínimas de hit em jogador antes de avaliar HS%.</summary>
        public static int AimbotHsMinHits;
        /// <summary>Taxa HS/hits que dispara flag (armas normais).</summary>
        public static float AimbotHsRatioFlag;
        /// <summary>Taxa HS/hits para armas de alto dano (sniper/RPG etc.).</summary>
        public static float AimbotHsRatioFlagHighDmg;
        /// <summary>Damage base &gt;= isto → limiares mais brandos.</summary>
        public static int AimbotHighDamageThreshold;
        /// <summary>Ângulo (graus) entre tiros consecutivos considerado snap.</summary>
        public static float AimbotSnapDegrees;
        /// <summary>Janela ms em que o snap é avaliado.</summary>
        public static int AimbotSnapMaxIntervalMs;
        /// <summary>Violações aimbot antes de congelar na partida (+clip).</summary>
        public static int AimbotFreezeViolations;
        /// <summary>Se true, hardban após AimbotBanViolations (default false — preferir GM).</summary>
        public static bool AimbotAutoBan;
        /// <summary>Violações para hardban automático (só se AimbotAutoBan).</summary>
        public static int AimbotBanViolations;
        /// <summary>Pacotes UDP/s por IP no Match antes de dropar (0 = off).</summary>
        public static int MatchUdpMaxPacketsPerSecond;
        public static float MaxClanPoints;
        public static float PlantDuration;
        public static float DefuseDuration;
        public static ushort MaxDropWpnCount;
        public static UdpState UdpType;
        public static NationsEnum National;
        public static List<ClientLocale> GameLocales;

        static ConfigLoader()
        {
            LoadTimeline();
            Load();
            LoadRcon();
        }

        private static void Load()
        {
            ConfigEngine configEngine = new ConfigEngine("Config/Settings.ini", FileAccess.Read);
            HOST = new string[3]
            {
              configEngine.ReadS("PrivateIp4Address", "127.0.0.1", "Server"),
              configEngine.ReadS("ProxyIp4Address", "127.0.0.1", "Server"),
              configEngine.ReadS("PublicIp4Address", "127.0.0.1", "Server")
            };
            HOST[0] = GetEnvironment("PB_BIND_HOST", HOST[0]);
            HOST[2] = GetEnvironment("PB_ADVERTISE_HOST", HOST[2]);
            DatabaseHost = configEngine.ReadS("Host", "localhost", "Database");
            DatabaseName = configEngine.ReadS("Name", "", "Database");
            DatabaseUsername = configEngine.ReadS("User", "root", "Database");
            DatabasePassword = configEngine.ReadS("Pass", "", "Database");
            DatabasePort = configEngine.ReadD("Port", 0, "Database");
            DatabaseHost = GetEnvironment("PB_DB_HOST", DatabaseHost);
            DatabaseName = GetEnvironment("PB_DB_NAME", DatabaseName);
            DatabaseUsername = GetEnvironment("PB_DB_USER", DatabaseUsername);
            DatabasePassword = GetEnvironment("PB_DB_PASS", DatabasePassword);
            DatabasePort = GetEnvironmentInt("PB_DB_PORT", DatabasePort);
            ConfigId = configEngine.ReadD("ConfigId", 1, "Server");
            BackLog = configEngine.ReadD("BackLog", 3, "Server");
            ConnectionThrottleSeconds = configEngine.ReadD("ConnectionThrottleSeconds", 5, "Server");
            DebugMode = configEngine.ReadX("Debug", false, "Server");
            bool legacyTracePref = configEngine.ReadX("Sniffer", true, "Server");
            TraceEnabled = configEngine.ReadX("Trace", legacyTracePref, "Server");
            Plugin.Core.Logging.TraceSink.Enabled = TraceEnabled;
            CaptureLevelRaw = configEngine.ReadS("CaptureLevel", "full", "Server");
            ConsoleLevelRaw = configEngine.ReadS("ConsoleLevel", "packets", "Server");
            TraceOpcodesRaw = configEngine.ReadS("TraceOpcodes", "", "Server");
            Plugin.Core.Logging.LogConfig.ConfigureTrace(TraceOpcodesRaw);
            IsTestMode = configEngine.ReadX("Test", false, "Server");
            ShowMoreInfo = configEngine.ReadX("MoreInfo", false, "Server");
            ProcessSplit = configEngine.ReadX("ProcessSplit", false, "Server");
            IsDebugPing = configEngine.ReadX("DebugPing", false, "Server");
            AutoBan = configEngine.ReadX("AutoBan", false, "Server");
            ICafeSystem = configEngine.ReadX("ICafeSystem", true, "Server");
            InternetCafeId = configEngine.ReadD("InternetCafeId", 1, "Server");
            IsUseProxy = configEngine.ReadX("UseProxy", true, "Server");
            AutoAccount = configEngine.ReadX("AutoAccount", false, "Essentials");
            TournamentRule = configEngine.ReadX("TournamentRule", false, "Essentials");
            RandomPassword = configEngine.ReadX("RandomPassword", false, "Essentials");
            RandomPasswordChars = configEngine.ReadS("RandomPasswordChars", "", "Essentials");
            UseCryptedPassword = configEngine.ReadX("UseCryptedPassword", true, "Essentials");
            CryptedPasswordSalt = configEngine.ReadS("CryptedPasswordSalt", "", "Essentials");
            MinRankVote = configEngine.ReadD("MinRankVote", 0, "Internal");
            WinCashPerBattle = configEngine.ReadX("WinCashPerBattle", true, "Internal");
            ShowCashReceiveWarn = configEngine.ReadX("ShowCashReceiveWarn", true, "Internal");
            MaxExpReward = configEngine.ReadD("MaxExpReward", 1000, "Internal");
            MaxGoldReward = configEngine.ReadD("MaxGoldReward", 1000, "Internal");
            MaxCashReward = configEngine.ReadD("MaxCashReward", 1000, "Internal");
            MinCreateRank = configEngine.ReadD("MinCreateRank", 15, "Internal");
            MinCreateGold = configEngine.ReadD("MinCreateGold", 7500, "Internal");
            MaxClanPoints = configEngine.ReadT("MaxClanPoints", 0.0f, "Internal");
            MaxActiveClans = configEngine.ReadD("MaxActiveClans", 0, "Internal");
            MaxLatency = configEngine.ReadD("MaxLatency", 0, "Internal");
            MaxRepeatLatency = configEngine.ReadD("MaxRepeatLatency", 0, "Internal");
            BattleRewardId = configEngine.ReadD("BattleRewardId", 1, "Internal");
            UdpType = (UdpState)configEngine.ReadC("UdpType", (byte)3, "Others");
            UdpVersion = configEngine.ReadS("UdpVersion", "1508.7", "Others");
            SendInfoToServ = configEngine.ReadX("SendInfoToServ", true, "Others");
            SendPingSync = configEngine.ReadX("SendPingSync", true, "Others");
            EnableLog = configEngine.ReadX("EnableLog", false, "Others");
            SendFailMsg = configEngine.ReadX("SendFailMsg", true, "Others");
            UseHitMarker = configEngine.ReadX("UseHitMarker", false, "Others");
            UseMaxAmmoInDrop = configEngine.ReadX("UseMaxAmmoInDrop", false, "Others");
            MaxDropWpnCount = configEngine.ReadUH("MaxDropWpnCount", (ushort)0, "Others");
            AntiScript = configEngine.ReadX("AntiScript", true, "Others");
            // [Security] FL Guard
            RequireOtpToken = configEngine.ReadX("RequireOtpToken", false, "Security");
            OtpOneShot = configEngine.ReadX("OtpOneShot", false, "Security");
            HardBanSubnet = configEngine.ReadX("HardBanSubnet", false, "Security");
            RequireLauncherHeartbeat = configEngine.ReadX("RequireLauncherHeartbeat", false, "Security");
            HeartbeatTimeoutSeconds = configEngine.ReadD("HeartbeatTimeoutSeconds", 45, "Security");
            ProbationHours = configEngine.ReadD("ProbationHours", 48, "Security");
            ProbationMaxMatchesPerHour = configEngine.ReadD("ProbationMaxMatchesPerHour", 10, "Security");
            SpeedKickViolations = configEngine.ReadD("SpeedKickViolations", 8, "Security");
            SpeedBanViolations = configEngine.ReadD("SpeedBanViolations", 20, "Security");
            KillBurstWindowSeconds = configEngine.ReadD("KillBurstWindowSeconds", 8, "Security");
            KillBurstMaxKills = configEngine.ReadD("KillBurstMaxKills", 5, "Security");
            KillBurstInBotMode = configEngine.ReadX("KillBurstInBotMode", false, "Security");
            AimbotDetect = configEngine.ReadX("AimbotDetect", true, "Security");
            AimbotHsMinHits = configEngine.ReadD("AimbotHsMinHits", 14, "Security");
            AimbotHsRatioFlag = configEngine.ReadT("AimbotHsRatioFlag", 0.88f, "Security");
            AimbotHsRatioFlagHighDmg = configEngine.ReadT("AimbotHsRatioFlagHighDmg", 0.96f, "Security");
            AimbotHighDamageThreshold = configEngine.ReadD("AimbotHighDamageThreshold", 180, "Security");
            AimbotSnapDegrees = configEngine.ReadT("AimbotSnapDegrees", 62f, "Security");
            AimbotSnapMaxIntervalMs = configEngine.ReadD("AimbotSnapMaxIntervalMs", 90, "Security");
            AimbotFreezeViolations = configEngine.ReadD("AimbotFreezeViolations", 3, "Security");
            AimbotAutoBan = configEngine.ReadX("AimbotAutoBan", false, "Security");
            AimbotBanViolations = configEngine.ReadD("AimbotBanViolations", 8, "Security");
            MatchUdpMaxPacketsPerSecond = configEngine.ReadD("MatchUdpMaxPacketsPerSecond", 140, "Security");
            GameLocales = new List<ClientLocale>();
            National = (NationsEnum)Enum.Parse(typeof(NationsEnum), configEngine.ReadS("National", "Global", "Essentials"));
            string str1 = configEngine.ReadS("Region", "None", "Essentials");
            char[] chArray = new char[1] { ',' };
            foreach (string str2 in str1.Split(chArray))
            {
                ClientLocale result;
                Enum.TryParse<ClientLocale>(str2, out result);
                GameLocales.Add(result);
            }
            MinUserSize = configEngine.ReadD("MinUserSize", 4, "Essentials");
            MaxUserSize = configEngine.ReadD("MaxUserSize", 16, "Essentials");
            MinPassSize = configEngine.ReadD("MinPassSize", 4, "Essentials");
            MaxPassSize = configEngine.ReadD("MaxPassSize", 16, "Essentials");
            MinNickSize = configEngine.ReadD("MinNickSize", 4, "Essentials");
            MaxNickSize = configEngine.ReadD("MaxNickSize", 16, "Essentials");
            PingUpdateTimeSeconds = configEngine.ReadD("PingUpdateTimeSeconds", 7, "Internal");
            PlayersServerUpdateTimeSeconds = configEngine.ReadD("PlayersServerUpdateTimeSeconds", 7, "Internal");
            UpdateIntervalPlayersServer = configEngine.ReadD("UpdateIntervalPlayersServer", 2, "Internal");
            EmptyRoomRemovalInterval = configEngine.ReadD("EmptyRoomRemovalInterval", 2, "Internal");
            ConsoleTitleUpdateTimeSeconds = configEngine.ReadD("ConsoleTitleUpdateTimeSeconds", 3, "Internal");
            IntervalEnterRoomAfterKickSeconds = configEngine.ReadD("IntervalEnterRoomAfterKickSeconds", 30, "Internal");
            MaxBuyItemDays = configEngine.ReadD("MaxBuyItemDays", 365, "Internal");
            MaxBuyItemUnits = configEngine.ReadD("MaxBuyItemUnits", 100000, "Internal");
            PlantDuration = configEngine.ReadT("PlantDuration", 5.5f, "Internal");
            DefuseDuration = configEngine.ReadT("DefuseDuration", 7.1f, "Internal");
            MaxConnectionPerIp = configEngine.ReadD("MaxConnectionPerIp", 2, "Server");
        }

        private static string GetEnvironment(string name, string fallback)
        {
            string value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static int GetEnvironmentInt(string name, int fallback)
        {
            string value = Environment.GetEnvironmentVariable(name);
            return int.TryParse(value, out int parsed) ? parsed : fallback;
        }

        public static void LoadRcon()
        {
            /** Rcon Section **/
            ConfigEngine CFG = new ConfigEngine("Config/Rcon.ini", FileAccess.Read);
            RconEnable = CFG.ReadX("RconEnable", false, "Rcon");
            RconIp = CFG.ReadS("RconIp", "127.0.0.1", "Rcon");
            RconPassword = CFG.ReadS("RconPassword", "", "Rcon");
            RconPort = CFG.ReadD("RconPort", 39189, "Rcon");
            RconIp = GetEnvironment("PB_RCON_BIND_HOST", RconIp);
            RconPort = GetEnvironmentInt("PB_RCON_BIND_PORT", RconPort);
            RconInfoCommand = CFG.ReadX("RconInfo", false, "Rcon");
            RconPrintNotValidIp = CFG.ReadX("RconPrintNotValidIp", false, "Rcon");
            RconNotValidIpEnable = CFG.ReadX("RconNotValidIpEnable", false, "Rcon");
            RconValidIps = new List<string>();
            string Ips = CFG.ReadS("RconValidIps", "127.0.0.1", "Rcon");
            if (Ips.Contains(";"))
            {
                RconValidIps.AddRange(Ips.Split(';'));
            }
            else RconValidIps.Add(Ips);

            // StatusFeed (Discord #status) — separado do RCON
            StatusFeedEnable = CFG.ReadX("StatusFeedEnable", false, "Rcon")
                || string.Equals(GetEnvironment("PB_STATUS_FEED_ENABLE", ""), "true", StringComparison.OrdinalIgnoreCase)
                || GetEnvironment("PB_STATUS_FEED_ENABLE", "") == "1";
            StatusFeedBindHost = GetEnvironment("PB_STATUS_FEED_BIND_HOST", CFG.ReadS("StatusFeedBindHost", "0.0.0.0", "Rcon"));
            StatusFeedPort = GetEnvironmentInt("PB_STATUS_FEED_PORT", CFG.ReadD("StatusFeedPort", 30001, "Rcon"));
            StatusFeedToken = GetEnvironment("PB_STATUS_FEED_TOKEN", CFG.ReadS("StatusFeedToken", "", "Rcon"));
            StatusFeedHeartbeatSeconds = GetEnvironmentInt("PB_STATUS_FEED_HEARTBEAT_SECONDS", CFG.ReadD("StatusFeedHeartbeatSeconds", 120, "Rcon"));
            Plugin.Core.StatusFeed.StatusFeedHub.Configure(StatusFeedEnable, StatusFeedHeartbeatSeconds);
        }

        private static void LoadTimeline()
        {
            ConfigEngine configEngine = new ConfigEngine("Config/Timeline.ini", FileAccess.Read);
            CustomYear = configEngine.ReadX("CustomYear", false, "Addons");
            BackYear = configEngine.ReadD("BackYear", 10, "Runtime");
        }
    }
}
