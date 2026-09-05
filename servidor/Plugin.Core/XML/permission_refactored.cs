// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.PermissionXML
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

#nullable disable
namespace Plugin.Core.XML
{
    public class PermissionXML
    {
        private static readonly SortedList<int, string> permissionNames = new SortedList<int, string>();
        private static readonly SortedList<AccessLevel, List<string>> accessLevelPermissions = new SortedList<AccessLevel, List<string>>();
        private static readonly SortedList<int, int> rankMappings = new SortedList<int, int>();

        public static void Load()
        {
            LoadPermissions();
            LoadPermissionLevels();
            LoadPermissionRights();
        }

        public static void Reload()
        {
            permissionNames.Clear();
            accessLevelPermissions.Clear();
            rankMappings.Clear();
            Load();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadPermissions()
        {
            string xmlFilePath = "Data/Access/Permission.xml";
            if (!File.Exists(xmlFilePath))
                CLogger.Print("File not found: " + xmlFilePath, LoggerType.Warning);
            else
                ParsePermissionXml(xmlFilePath);
            CLogger.Print($"Plugin carregado: {permissionNames.Count} permissoes", LoggerType.Info);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadPermissionLevels()
        {
            string xmlFilePath = "Data/Access/PermissionLevel.xml";
            if (!File.Exists(xmlFilePath))
                CLogger.Print("File not found: " + xmlFilePath, LoggerType.Warning);
            else
                ParsePermissionLevelXml(xmlFilePath);
            CLogger.Print($"Plugin carregado: {rankMappings.Count} Permission Ranks", LoggerType.Info);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadPermissionRights()
        {
            string xmlFilePath = "Data/Access/PermissionRight.xml";
            if (!File.Exists(xmlFilePath))
                CLogger.Print("File not found: " + xmlFilePath, LoggerType.Warning);
            else
                ParsePermissionRightXml(xmlFilePath);
            CLogger.Print($"Plugin carregado: {accessLevelPermissions.Count} Level Permission", LoggerType.Info);
        }

        public static int GetFakeRank(int Level)
        {
            lock (rankMappings)
                return !rankMappings.ContainsKey(Level) ? 0 : rankMappings[Level];
        }

        public static string GetPermissionName(int permissionId)
        {
            lock (permissionNames)
                return permissionNames.ContainsKey(permissionId) ? permissionNames[permissionId] : string.Empty;
        }

        public static List<string> GetPermissionsForLevel(AccessLevel level)
        {
            lock (accessLevelPermissions)
                return accessLevelPermissions.ContainsKey(level) ? accessLevelPermissions[level] : new List<string>();
        }

        public static bool HasPermission(AccessLevel level, string permissionName)
        {
            List<string> permissions = GetPermissionsForLevel(level);
            return permissions.Contains(permissionName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ParsePermissionXml(string xmlFilePath)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlFilePath);
                
                XmlNodeList permissionNodes = xmlDocument.SelectNodes("//Permission");
                foreach (XmlNode permissionNode in permissionNodes)
                {
                    if (permissionNode.Attributes != null)
                    {
                        XmlNode idAttribute = permissionNode.Attributes["Id"];
                        XmlNode nameAttribute = permissionNode.Attributes["Name"];
                        
                        if (idAttribute != null && nameAttribute != null)
                        {
                            if (int.TryParse(idAttribute.Value, out int permissionId))
                            {
                                lock (permissionNames)
                                {
                                    if (!permissionNames.ContainsKey(permissionId))
                                        permissionNames.Add(permissionId, nameAttribute.Value);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error parsing permission XML: {ex.Message}", LoggerType.Error, ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ParsePermissionLevelXml(string xmlFilePath)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlFilePath);
                
                XmlNodeList levelNodes = xmlDocument.SelectNodes("//Level");
                foreach (XmlNode levelNode in levelNodes)
                {
                    if (levelNode.Attributes != null)
                    {
                        XmlNode levelAttribute = levelNode.Attributes["Level"];
                        XmlNode rankAttribute = levelNode.Attributes["Rank"];
                        
                        if (levelAttribute != null && rankAttribute != null)
                        {
                            if (int.TryParse(levelAttribute.Value, out int level) && 
                                int.TryParse(rankAttribute.Value, out int rank))
                            {
                                lock (rankMappings)
                                {
                                    if (!rankMappings.ContainsKey(level))
                                        rankMappings.Add(level, rank);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error parsing permission level XML: {ex.Message}", LoggerType.Error, ex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ParsePermissionRightXml(string xmlFilePath)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlFilePath);
                
                XmlNodeList accessLevelNodes = xmlDocument.SelectNodes("//AccessLevel");
                foreach (XmlNode accessLevelNode in accessLevelNodes)
                {
                    if (accessLevelNode.Attributes != null)
                    {
                        XmlNode levelAttribute = accessLevelNode.Attributes["Level"];
                        
                        if (levelAttribute != null && Enum.TryParse<AccessLevel>(levelAttribute.Value, out AccessLevel accessLevel))
                        {
                            List<string> permissions = new List<string>();
                            
                            XmlNodeList permissionNodes = accessLevelNode.SelectNodes("Permission");
                            foreach (XmlNode permissionNode in permissionNodes)
                            {
                                if (permissionNode.Attributes != null)
                                {
                                    XmlNode nameAttribute = permissionNode.Attributes["Name"];
                                    if (nameAttribute != null)
                                    {
                                        permissions.Add(nameAttribute.Value);
                                    }
                                }
                            }
                            
                            lock (accessLevelPermissions)
                            {
                                if (!accessLevelPermissions.ContainsKey(accessLevel))
                                    accessLevelPermissions.Add(accessLevel, permissions);
                                else
                                    accessLevelPermissions[accessLevel].AddRange(permissions);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error parsing permission rights XML: {ex.Message}", LoggerType.Error, ex);
            }
        }
    }
}