using Plugin.Core.Enums;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;


namespace Plugin.Core.XML
{
    public class PermissionXML
    {
        private const string PermissionPath = "Data/Access/Permission.xml";
        private const string LevelPath = "Data/Access/PermissionLevel.xml";
        private const string RightPath = "Data/Access/PermissionRight.xml";

        private static readonly SortedList<int, string> Permissions = new SortedList<int, string>();
        private static readonly SortedList<AccessLevel, List<string>> RightsByLevel = new SortedList<AccessLevel, List<string>>();
        private static readonly SortedList<int, int> FakeRanks = new SortedList<int, int>();

        public static void Load()
        {
            LoadPermissions();
            LoadLevels();
            LoadRights();
        }

        public static void Reload()
        {
            PermissionXML.Permissions.Clear();
            PermissionXML.RightsByLevel.Clear();
            PermissionXML.FakeRanks.Clear();
            PermissionXML.Load();
        }

        public static int GetFakeRank(int Level)
        {
            lock (PermissionXML.FakeRanks)
                return !PermissionXML.FakeRanks.ContainsKey(Level) ? -1 : PermissionXML.FakeRanks[Level];
        }

        public static bool HavePermission(string Permission, AccessLevel Level)
        {
            return PermissionXML.RightsByLevel.ContainsKey(Level) && PermissionXML.RightsByLevel[Level].Contains(Permission);
        }

        private static void LoadPermissions()
        {
            SortedList<int, string> Rows = DaoManagerSQL.GetPermissionNames();
            if (Rows != null && Rows.Count > 0)
            {
                foreach (KeyValuePair<int, string> Row in Rows)
                {
                    if (!Permissions.ContainsKey(Row.Key))
                        Permissions.Add(Row.Key, Row.Value);
                }
            }
            else
            {
                CLogger.Print($"system_permissions unreachable or empty, falling back to {PermissionPath}", LoggerType.Error);
                if (!File.Exists(PermissionPath))
                    CLogger.Print("File not found: " + PermissionPath, LoggerType.Warning);
                else
                    ParsePermissions(PermissionPath);
            }
            CLogger.Print($"Plugin carregado: {PermissionXML.Permissions.Count} permissoes", LoggerType.Info);
        }

        private static void LoadLevels()
        {
            SortedList<int, int> Rows = DaoManagerSQL.GetAccessLevelFakeRanks();
            if (Rows != null && Rows.Count > 0)
            {
                foreach (KeyValuePair<int, int> Row in Rows)
                {
                    AddLevel(Row.Key, Row.Value);
                }
            }
            else
            {
                CLogger.Print($"system_access_levels unreachable or empty, falling back to {LevelPath}", LoggerType.Error);
                if (!File.Exists(LevelPath))
                    CLogger.Print("File not found: " + LevelPath, LoggerType.Warning);
                else
                    ParseLevels(LevelPath);
            }
            CLogger.Print($"Plugin carregado: {PermissionXML.FakeRanks.Count} Permission Ranks", LoggerType.Info);
        }

        private static void LoadRights()
        {
            List<KeyValuePair<int, int>> Rows = DaoManagerSQL.GetAccessRights();
            if (Rows != null && Rows.Count > 0)
            {
                foreach (KeyValuePair<int, int> Row in Rows)
                {
                    AddRight((AccessLevel)Row.Key, Row.Value);
                }
            }
            else
            {
                CLogger.Print($"system_access_rights unreachable or empty, falling back to {RightPath}", LoggerType.Error);
                if (!File.Exists(RightPath))
                    CLogger.Print("File not found: " + RightPath, LoggerType.Warning);
                else
                    ParseRights(RightPath);
            }
            CLogger.Print($"Plugin carregado: {PermissionXML.RightsByLevel.Count} Level Permission", LoggerType.Info);
        }

        private static void AddLevel(int Key, int FakeRank)
        {
            if (!FakeRanks.ContainsKey(Key))
                FakeRanks.Add(Key, FakeRank);
            if (!RightsByLevel.ContainsKey((AccessLevel)Key))
                RightsByLevel.Add((AccessLevel)Key, new List<string>());
        }

        private static void AddRight(AccessLevel Level, int PermissionKey)
        {
            if (!RightsByLevel.ContainsKey(Level))
                RightsByLevel.Add(Level, new List<string>());
            if (Permissions.ContainsKey(PermissionKey))
                RightsByLevel[Level].Add(Permissions[PermissionKey]);
        }

        private static void ParsePermissions(string Path)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream inStream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (inStream.Length == 0L)
                {
                    CLogger.Print("File is empty: " + Path, LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        xmlDocument.Load((Stream)inStream);
                        for (XmlNode xmlNode1 = xmlDocument.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
                        {
                            if (xmlNode1.Name.Equals("List"))
                            {
                                for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                                {
                                    if (xmlNode2.Name.Equals("Permission"))
                                    {
                                        XmlAttributeCollection attributes = xmlNode2.Attributes;
                                        int key = int.Parse(attributes.GetNamedItem("Key").Value);
                                        string name = attributes.GetNamedItem("Name").Value;
                                        if (!PermissionXML.Permissions.ContainsKey(key))
                                            PermissionXML.Permissions.Add(key, name);
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, (Exception)ex);
                    }
                }
                inStream.Dispose();
                inStream.Close();
            }
        }

        private static void ParseLevels(string Path)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream inStream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (inStream.Length == 0L)
                {
                    CLogger.Print("File is empty: " + Path, LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        xmlDocument.Load((Stream)inStream);
                        for (XmlNode xmlNode1 = xmlDocument.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
                        {
                            if (xmlNode1.Name.Equals("List"))
                            {
                                for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                                {
                                    if (xmlNode2.Name.Equals("Permission"))
                                    {
                                        XmlAttributeCollection attributes = xmlNode2.Attributes;
                                        int key = int.Parse(attributes.GetNamedItem("Key").Value);
                                        int fakeRank = int.Parse(attributes.GetNamedItem("FakeRank").Value);
                                        AddLevel(key, fakeRank);
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, (Exception)ex);
                    }
                }
                inStream.Dispose();
                inStream.Close();
            }
        }

        private static void ParseRights(string Path)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream inStream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (inStream.Length == 0L)
                {
                    CLogger.Print("File is empty: " + Path, LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        xmlDocument.Load((Stream)inStream);
                        for (XmlNode xmlNode = xmlDocument.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                        {
                            if (xmlNode.Name.Equals("List"))
                            {
                                for (XmlNode AccessNode = xmlNode.FirstChild; AccessNode != null; AccessNode = AccessNode.NextSibling)
                                {
                                    if (AccessNode.Name.Equals("Access"))
                                    {
                                        AccessLevel Level = ComDiv.ParseEnum<AccessLevel>(AccessNode.Attributes.GetNamedItem("Level").Value);
                                        ParseAccessRights(AccessNode, Level);
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, (Exception)ex);
                    }
                }
                inStream.Dispose();
                inStream.Close();
            }
        }

        private static void ParseAccessRights(XmlNode AccessNode, AccessLevel Level)
        {
            for (XmlNode xmlNode1 = AccessNode.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
            {
                if (xmlNode1.Name.Equals("Permission"))
                {
                    for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                    {
                        if (xmlNode2.Name.Equals("Right"))
                        {
                            AddRight(Level, int.Parse(xmlNode2.Attributes.GetNamedItem("LevelKey").Value));
                        }
                    }
                }
            }
        }
    }
}
