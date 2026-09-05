using Launcher.Services.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace Launcher.Server.Config
{
    public static class ConfigFile
    {
        public static ServerConfig Load(string path)
        {
            if (!File.Exists(path))
                CreateDefault(path);

            Dictionary<string, Dictionary<string, string>> ini = Parse(path);

            ServerConfig config = new ServerConfig();

            config.Host = GetString(ini, "Server", "Host", config.Host);
            config.Port = GetInt(ini, "Server", "Port", config.Port);

            config.LauncherVersion = GetString(ini, "Launcher", "LauncherVersion", config.LauncherVersion);
            config.ClientVersion = GetString(ini, "Launcher", "ClientVersion", config.ClientVersion);
            config.Maintenance = GetBool(ini, "Launcher", "Maintenance", config.Maintenance);
            config.MaintenanceMessage = GetString(ini, "Launcher", "MaintenanceMessage", config.MaintenanceMessage);

            config.ManifestPath = GetString(ini, "Patch", "ManifestPath", config.ManifestPath);
            config.ManifestUrl = GetString(ini, "Patch", "ManifestUrl", config.ManifestUrl);
            config.ClientFilesPath = GetString(ini, "Patch", "ClientFilesPath", config.ClientFilesPath);
            config.LauncherFilesPath = GetString(ini, "Patch", "LauncherFilesPath", config.LauncherFilesPath);

            config.MaxPacketSize = GetInt(ini, "Security", "MaxPacketSize", config.MaxPacketSize);
            config.EnablePacketLog = GetBool(ini, "Security", "EnablePacketLog", config.EnablePacketLog);
            config.TosVersion = GetInt(ini, "Security", "TosVersion", config.TosVersion);
            config.TokenMinutes = GetInt(ini, "Security", "TokenMinutes", config.TokenMinutes);
            config.RequireFingerprint = GetBool(ini, "Security", "RequireFingerprint", config.RequireFingerprint);
            config.EvasionAutoBanScore = GetInt(ini, "Security", "EvasionAutoBanScore", config.EvasionAutoBanScore);
            config.EvasionSuspectScore = GetInt(ini, "Security", "EvasionSuspectScore", config.EvasionSuspectScore);
            config.BruteForceMaxFailures = GetInt(ini, "Security", "BruteForceMaxFailures", config.BruteForceMaxFailures);
            config.BruteForceWindowMinutes = GetInt(ini, "Security", "BruteForceWindowMinutes", config.BruteForceWindowMinutes);
            config.BruteForceBlockMinutes = GetInt(ini, "Security", "BruteForceBlockMinutes", config.BruteForceBlockMinutes);
            config.EvidenceRoot = GetString(ini, "Security", "EvidenceRoot", config.EvidenceRoot);
            string envEvidence = Environment.GetEnvironmentVariable("FL_EVIDENCE_ROOT");
            if (!string.IsNullOrWhiteSpace(envEvidence))
                config.EvidenceRoot = envEvidence.Trim();

            config.DbHost = GetString(ini, "Database", "Host", config.DbHost);
            config.DbPort = GetInt(ini, "Database", "Port", config.DbPort);
            config.DbName = GetString(ini, "Database", "Name", config.DbName);
            config.DbUser = GetString(ini, "Database", "User", config.DbUser);
            config.DbPassword = GetString(ini, "Database", "Pass", config.DbPassword);

            return config;
        }

        private static Dictionary<string, Dictionary<string, string>> Parse(string path)
        {
            var data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            string currentSection = "Default";

            foreach (string rawLine in File.ReadAllLines(path))
            {
                string line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith(";") || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Substring(1, line.Length - 2).Trim();

                    if (!data.ContainsKey(currentSection))
                    {
                        data[currentSection] =
                            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }

                    continue;
                }

                string[] parts = line.Split(new[] { '=' }, 2);

                if (parts.Length != 2)
                    continue;

                if (!data.ContainsKey(currentSection))
                {
                    data[currentSection] =
                        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                data[currentSection][key] = value;
            }

            return data;
        }

        private static string GetString(
            Dictionary<string, Dictionary<string, string>> ini,
            string section,
            string key,
            string defaultValue)
        {
            if (ini.ContainsKey(section) && ini[section].ContainsKey(key))
                return ini[section][key];

            return defaultValue;
        }

        private static int GetInt(
            Dictionary<string, Dictionary<string, string>> ini,
            string section,
            string key,
            int defaultValue)
        {
            string value = GetString(ini, section, key, null);

            int result;

            if (int.TryParse(value, out result))
                return result;

            return defaultValue;
        }

        private static bool GetBool(
            Dictionary<string, Dictionary<string, string>> ini,
            string section,
            string key,
            bool defaultValue)
        {
            string value = GetString(ini, section, key, null);

            bool result;

            if (bool.TryParse(value, out result))
                return result;

            return defaultValue;
        }
        private static void CreateDefault(string path)
        {
            string directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            string content =
                          @"[Server]
                          Host=0.0.0.0
                          Port=9000

                          [Launcher]
                          LauncherVersion=202605
                          ClientVersion=20260514
                          Maintenance=false
                          MaintenanceMessage=Servidor em manutenção.

                          [Patch]
                          ManifestPath=Info\manifest.json
                          ManifestUrl=http://localhost/patch/manifest.json
                          ClientFilesPath=Data\Client
                          LauncherFilesPath=Data\Launcher

                          [Security]
                          MaxPacketSize=10485760
                          EnablePacketLog=true
                          TosVersion=0
                          TokenMinutes=10
                          RequireFingerprint=false
                          EvasionAutoBanScore=4
                          EvasionSuspectScore=2
                          BruteForceMaxFailures=5
                          BruteForceWindowMinutes=5
                          BruteForceBlockMinutes=15

                          [Database]
                          Host=127.0.0.1
                          Port=5433
                          Name=pb
                          User=postgres
                          Pass=postgres";
            File.WriteAllText(path, content);
        }
    }
}