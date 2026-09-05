using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace Plugin.Core.JSON
{
    /// <summary>
    /// Handles loading and management of server configuration data from JSON files
    /// Provides thread-safe access to server configurations and supports hot-reloading
    /// </summary>
    public static class ServerConfigJSON
    {
        #region Constants

        /// <summary>Default path to the server configuration file</summary>
        private const string CONFIG_FILE_PATH = "Data/ServerConfig.json";

        /// <summary>JSON property name for the server array</summary>
        private const string SERVER_ARRAY_PROPERTY = "Server";

        /// <summary>Minimum valid configuration ID</summary>
        private const int MIN_CONFIG_ID = 1;

        /// <summary>Maximum file size in bytes (10MB) to prevent memory issues</summary>
        private const long MAX_FILE_SIZE = 10 * 1024 * 1024;

        #endregion

        #region Private Fields

        /// <summary>Thread-safe collection of loaded server configurations</summary>
        private static readonly List<ServerConfig> configurations = new List<ServerConfig>();

        /// <summary>Lock object for thread-safe operations</summary>
        private static readonly object configLock = new object();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a read-only copy of all loaded server configurations
        /// </summary>
        public static IReadOnlyList<ServerConfig> Configs
        {
            get
            {
                lock (configLock)
                {
                    return configurations.ToList().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// Gets the count of loaded configurations
        /// </summary>
        public static int ConfigCount
        {
            get
            {
                lock (configLock)
                {
                    return configurations.Count;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads server configurations from the default JSON file
        /// </summary>
        /// <returns>True if loading was successful, false otherwise</returns>
        public static bool Load()
        {
            return LoadFromFile(CONFIG_FILE_PATH);
        }

        /// <summary>
        /// Loads server configurations from a specific JSON file
        /// </summary>
        /// <param name="filePath">Path to the JSON configuration file</param>
        /// <returns>True if loading was successful, false otherwise</returns>
        public static bool LoadFromFile(string filePath)
        {
            try
            {
                if (!ValidateFilePath(filePath))
                    return false;

                if (!File.Exists(filePath))
                {
                    CLogger.Print($"Configuration file not found: {filePath}", LoggerType.Warning);
                    return false;
                }

                bool result = ParseConfigurationFile(filePath);

                CLogger.Print($"Server configurations loaded: {ConfigCount} configs from {filePath}", LoggerType.Info);
                return result;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Failed to load server configurations: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        /// <summary>
        /// Reloads all server configurations from the default file
        /// Clears existing configurations and loads fresh data
        /// </summary>
        /// <returns>True if reload was successful, false otherwise</returns>
        public static bool Reload()
        {
            try
            {
                lock (configLock)
                {
                    configurations.Clear();
                }

                CLogger.Print("Server configurations cleared, reloading...", LoggerType.Info);
                return Load();
            }
            catch (Exception ex)
            {
                CLogger.Print($"Failed to reload server configurations: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        /// <summary>
        /// Retrieves a server configuration by its ID
        /// </summary>
        /// <param name="configId">The configuration ID to search for</param>
        /// <returns>ServerConfig object if found, null otherwise</returns>
        public static ServerConfig GetConfig(int configId)
        {
            if (configId < MIN_CONFIG_ID)
            {
                CLogger.Print($"Invalid configuration ID: {configId}", LoggerType.Warning);
                return null;
            }

            lock (configLock)
            {
                return configurations.FirstOrDefault(config => config.ConfigId == configId);
            }
        }

        /// <summary>
        /// Checks if a configuration with the specified ID exists
        /// </summary>
        /// <param name="configId">Configuration ID to check</param>
        /// <returns>True if configuration exists, false otherwise</returns>
        public static bool ConfigExists(int configId)
        {
            return GetConfig(configId) != null;
        }

        /// <summary>
        /// Gets all configuration IDs currently loaded
        /// </summary>
        /// <returns>Array of configuration IDs</returns>
        public static int[] GetAllConfigIds()
        {
            lock (configLock)
            {
                return configurations.Select(config => config.ConfigId).ToArray();
            }
        }

        /// <summary>
        /// Validates all loaded configurations for completeness and consistency
        /// </summary>
        /// <returns>True if all configurations are valid, false otherwise</returns>
        public static bool ValidateConfigurations()
        {
            lock (configLock)
            {
                bool allValid = true;

                foreach (var config in configurations)
                {
                    if (!ValidateConfiguration(config))
                    {
                        allValid = false;
                    }
                }

                CLogger.Print($"Configuration validation completed: {(allValid ? "All valid" : "Some invalid configurations found")}",
                             allValid ? LoggerType.Info : LoggerType.Warning);

                return allValid;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates the provided file path
        /// </summary>
        /// <param name="filePath">File path to validate</param>
        /// <returns>True if path is valid, false otherwise</returns>
        private static bool ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                CLogger.Print("Configuration file path cannot be null or empty", LoggerType.Error);
                return false;
            }

            try
            {
                // Check if path is valid
                Path.GetFullPath(filePath);
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Invalid file path '{filePath}': {ex.Message}", LoggerType.Error);
                return false;
            }
        }

        /// <summary>
        /// Parses the configuration file and loads all server configurations
        /// </summary>
        /// <param name="filePath">Path to the configuration file</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        private static bool ParseConfigurationFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            // Check file size to prevent memory issues
            if (fileInfo.Length == 0)
            {
                CLogger.Print($"Configuration file is empty: {filePath}", LoggerType.Warning);
                return false;
            }

            if (fileInfo.Length > MAX_FILE_SIZE)
            {
                CLogger.Print($"Configuration file too large: {fileInfo.Length} bytes (max: {MAX_FILE_SIZE})", LoggerType.Error);
                return false;
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);
                return ParseJsonContent(jsonContent, filePath);
            }
            catch (IOException ex)
            {
                CLogger.Print($"IO error reading configuration file: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                CLogger.Print($"Access denied reading configuration file: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        /// <summary>
        /// Parses JSON content and extracts server configurations
        /// </summary>
        /// <param name="jsonContent">JSON content as string</param>
        /// <param name="sourceFile">Source file name for error reporting</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        private static bool ParseJsonContent(string jsonContent, string sourceFile)
        {
            try
            {
                using (JsonDocument document = JsonDocument.Parse(jsonContent, new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                }))
                {
                    if (!document.RootElement.TryGetProperty(SERVER_ARRAY_PROPERTY, out JsonElement serverArray))
                    {
                        CLogger.Print($"Missing '{SERVER_ARRAY_PROPERTY}' property in configuration file", LoggerType.Error);
                        return false;
                    }

                    if (serverArray.ValueKind != JsonValueKind.Array)
                    {
                        CLogger.Print($"'{SERVER_ARRAY_PROPERTY}' property must be an array", LoggerType.Error);
                        return false;
                    }

                    return ProcessServerConfigurations(serverArray, sourceFile);
                }
            }
            catch (JsonException ex)
            {
                CLogger.Print($"JSON parsing error in {sourceFile}: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        /// <summary>
        /// Processes the server configurations array from JSON
        /// </summary>
        /// <param name="serverArray">JSON array containing server configurations</param>
        /// <param name="sourceFile">Source file name for error reporting</param>
        /// <returns>True if processing was successful, false otherwise</returns>
        private static bool ProcessServerConfigurations(JsonElement serverArray, string sourceFile)
        {
            List<ServerConfig> newConfigurations = new List<ServerConfig>();
            int processedCount = 0;
            int errorCount = 0;

            foreach (JsonElement serverElement in serverArray.EnumerateArray())
            {
                try
                {
                    ServerConfig config = ParseServerConfiguration(serverElement);
                    if (config != null)
                    {
                        // Check for duplicate configuration IDs
                        if (newConfigurations.Any(c => c.ConfigId == config.ConfigId))
                        {
                            CLogger.Print($"Duplicate configuration ID found: {config.ConfigId}", LoggerType.Warning);
                            errorCount++;
                            continue;
                        }

                        newConfigurations.Add(config);
                        processedCount++;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    CLogger.Print($"Error processing server configuration: {ex.Message}", LoggerType.Warning, ex);
                    errorCount++;
                }
            }

            // Update the global configuration list
            lock (configLock)
            {
                configurations.AddRange(newConfigurations);
            }

            CLogger.Print($"Processed {processedCount} configurations, {errorCount} errors from {sourceFile}",
                         errorCount > 0 ? LoggerType.Warning : LoggerType.Info);

            return errorCount == 0;
        }

        /// <summary>
        /// Parses a single server configuration from JSON element
        /// </summary>
        /// <param name="serverElement">JSON element containing server configuration</param>
        /// <returns>ServerConfig object if successful, null otherwise</returns>
        private static ServerConfig ParseServerConfiguration(JsonElement serverElement)
        {
            try
            {
                // Parse and validate configuration ID
                if (!TryGetIntProperty(serverElement, "ConfigId", out int configId) || configId < MIN_CONFIG_ID)
                {
                    CLogger.Print($"Invalid or missing ConfigId: {configId}", LoggerType.Warning);
                    return null;
                }

                // Create configuration object with all properties
                ServerConfig config = new ServerConfig
                {
                    ConfigId = configId,
                    OnlyGM = GetBoolProperty(serverElement, "ChannelOnlyGM", false),
                    Missions = GetBoolProperty(serverElement, "EnableMissions", true),
                    AccessUFL = GetBoolProperty(serverElement, "AccessUFL", false),
                    UserFileList = GetStringProperty(serverElement, "UserFileList", string.Empty),
                    ClientVersion = GetStringProperty(serverElement, "ClientVersion", "1.0.0"),
                    GiftSystem = GetBoolProperty(serverElement, "EnableGiftSystem", true),
                    EnableClan = GetBoolProperty(serverElement, "EnableClan", true),
                    EnableTicket = GetBoolProperty(serverElement, "EnableTicket", true),
                    EnablePlaytime = GetBoolProperty(serverElement, "EnablePlaytime", true),
                    EnableTags = GetBoolProperty(serverElement, "EnableTags", true),
                    EnableBlood = GetBoolProperty(serverElement, "EnableBlood", true),
                    ExitURL = GetStringProperty(serverElement, "ExitURL", string.Empty),
                    ShopURL = GetStringProperty(serverElement, "ShopURL", string.Empty),
                    OfficialSteam = GetStringProperty(serverElement, "OfficialSteam", string.Empty),
                    OfficialBanner = GetStringProperty(serverElement, "OfficialBanner", string.Empty),
                    OfficialBannerEnabled = GetBoolProperty(serverElement, "OfficialBannerEnabled", false),
                    OfficialAddress = GetStringProperty(serverElement, "OfficialAddress", string.Empty),
                    ChatAnnounceColor = GetIntProperty(serverElement, "ChatAnnoucementColor", 0),
                    ChannelAnnounceColor = GetIntProperty(serverElement, "ChannelAnnoucementColor", 0),
                    ChatAnnounce = GetStringProperty(serverElement, "ChatAnnountcement", string.Empty),
                    ChannelAnnounce = GetStringProperty(serverElement, "ChannelAnnouncement", string.Empty),
                    Showroom = GetEnumProperty<ShowroomView>(serverElement, "Showroom", ShowroomView.S_Default)
                };

                return config;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error parsing server configuration: {ex.Message}", LoggerType.Warning, ex);
                return null;
            }
        }

        /// <summary>
        /// Safely gets a string property from JSON element with fallback
        /// </summary>
        private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue = "")
        {
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                return property.GetString() ?? defaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Safely gets a boolean property from JSON element with fallback
        /// </summary>
        private static bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue = false)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                string value = property.GetString();
                if (bool.TryParse(value, out bool result))
                    return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Safely gets an integer property from JSON element with fallback
        /// </summary>
        private static int GetIntProperty(JsonElement element, string propertyName, int defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                string value = property.GetString();
                if (int.TryParse(value, out int result))
                    return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// Safely tries to get an integer property from JSON element
        /// </summary>
        private static bool TryGetIntProperty(JsonElement element, string propertyName, out int value)
        {
            value = 0;
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                string stringValue = property.GetString();
                return int.TryParse(stringValue, out value);
            }
            return false;
        }

        /// <summary>
        /// Safely gets an enum property from JSON element with fallback
        /// </summary>
        private static T GetEnumProperty<T>(JsonElement element, string propertyName, T defaultValue) where T : struct, Enum
        {
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                string value = property.GetString();
                if (!string.IsNullOrEmpty(value))
                {
                    // Cambiar a la sobrecarga correcta de ParseEnum
                    T parsed;
                    if (Enum.TryParse<T>(value, true, out parsed))
                        return parsed;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Validates a single server configuration for completeness
        /// </summary>
        /// <param name="config">Configuration to validate</param>
        /// <returns>True if configuration is valid, false otherwise</returns>
        private static bool ValidateConfiguration(ServerConfig config)
        {
            if (config == null)
            {
                CLogger.Print("Configuration object is null", LoggerType.Warning);
                return false;
            }

            if (config.ConfigId < MIN_CONFIG_ID)
            {
                CLogger.Print($"Invalid ConfigId: {config.ConfigId}", LoggerType.Warning);
                return false;
            }

            // Validate URLs if they're provided
            if (!string.IsNullOrEmpty(config.ExitURL) && !IsValidUrl(config.ExitURL))
            {
                CLogger.Print($"Invalid ExitURL in config {config.ConfigId}: {config.ExitURL}", LoggerType.Warning);
                return false;
            }

            if (!string.IsNullOrEmpty(config.ShopURL) && !IsValidUrl(config.ShopURL))
            {
                CLogger.Print($"Invalid ShopURL in config {config.ConfigId}: {config.ShopURL}", LoggerType.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates if a string is a valid URL
        /// </summary>
        /// <param name="url">URL string to validate</param>
        /// <returns>True if URL is valid, false otherwise</returns>
        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result) &&
                   (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        #endregion
    }
}