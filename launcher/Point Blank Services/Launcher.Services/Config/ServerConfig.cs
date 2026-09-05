using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Services.Config
{
   public class ServerConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string LauncherVersion { get; set; }
        public string ClientVersion { get; set; }
        public bool Maintenance { get; set; }
        public string MaintenanceMessage { get; set; }
        public string ManifestPath { get; set; }
        public string ManifestUrl { get; set; }
        public string ClientFilesPath { get; set; }
        public string LauncherFilesPath { get; set; }
        public int MaxPacketSize { get; set; }
        public bool EnablePacketLog { get; set; }
        public string DbHost { get; set; }
        public int DbPort { get; set; }
        public string DbName { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }

        // [Security] - FL Guard
        /// <summary>Versão atual do termo de uso. 0 desliga a exigência.</summary>
        public int TosVersion { get; set; }
        /// <summary>Validade do token OTP em minutos.</summary>
        public int TokenMinutes { get; set; }
        /// <summary>Recusar login sem fingerprint (ligar quando todos usarem o launcher novo).</summary>
        public bool RequireFingerprint { get; set; }
        /// <summary>Score de evasão a partir do qual a conta nova é banida automaticamente.</summary>
        public int EvasionAutoBanScore { get; set; }
        /// <summary>Score a partir do qual marca como suspeito (sem bloquear).</summary>
        public int EvasionSuspectScore { get; set; }
        public int BruteForceMaxFailures { get; set; }
        public int BruteForceWindowMinutes { get; set; }
        public int BruteForceBlockMinutes { get; set; }
        /// <summary>Pasta de evidências (screenshot/clip). Vazio = BaseDirectory/Evidence. Ou env FL_EVIDENCE_ROOT.</summary>
        public string EvidenceRoot { get; set; }

        public ServerConfig()
        {
            TosVersion = 0;
            TokenMinutes = 10;
            RequireFingerprint = false;
            EvasionAutoBanScore = 4;
            EvasionSuspectScore = 2;
            BruteForceMaxFailures = 5;
            BruteForceWindowMinutes = 5;
            BruteForceBlockMinutes = 15;
            EvidenceRoot = "";

            Host = "0.0.0.0";
            Port = 9000;

            LauncherVersion = "202605";
            ClientVersion = "20260514";
            Maintenance = false;
            MaintenanceMessage = "Servidor em manutenção.";

            ManifestPath = @"Info\manifest.json";
            ManifestUrl = "http://localhost/patch/manifest.json";
            ClientFilesPath = @"Data\Client";
            LauncherFilesPath = @"Data\Launcher";

            MaxPacketSize = 10485760;
            EnablePacketLog = true;

            DbHost = string.Empty;
            DbPort = 0;
            DbName = string.Empty;
            DbUser = string.Empty;
            DbPassword = string.Empty;
        }
    }
}
