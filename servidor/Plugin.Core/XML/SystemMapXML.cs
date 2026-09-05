using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Models.Map;
using Plugin.Core.SQL;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Plugin.Core.XML
{
    public static class SystemMapXML
    {
        public static List<MapRule> Rules = new List<MapRule>();
        public static List<MapMatch> Matches = new List<MapMatch>();
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int limit)
        {
            return list.Select((item, inx) => new { item, inx }).GroupBy(x => x.inx / limit).Select(g => g.Select(x => x.item));
        }
        private static readonly object SyncRoot = new object();

        public static void Load()
        {
            lock (SyncRoot)
            {
                List<MapRule> LoadedRules = DaoManagerSQL.GetMapRules();
                if (LoadedRules == null || LoadedRules.Count == 0)
                {
                    CLogger.Print("Map Rules: sem dados no banco, falling back para Data/Maps", LoggerType.Warning);
                    LoadedRules = LoadMapRule();
                }

                List<MapMatch> LoadedMatches = DaoManagerSQL.GetMapMatches();
                if (LoadedMatches == null || LoadedMatches.Count == 0)
                {
                    CLogger.Print("Map Matches: sem dados no banco, falling back para Data/Maps", LoggerType.Warning);
                    LoadedMatches = LoadMapMatch();
                }

                Rules = LoadedRules;
                Matches = LoadedMatches;

                CLogger.Print($"Plugin carregado: {LoadedRules.Count} regras de mapa", LoggerType.Info);
                CLogger.Print($"Plugin carregado: {LoadedMatches.Count} partidas de mapa", LoggerType.Info);
            }
        }
        public static void Reload()
        {
            Load();
        }
        private static List<MapRule> LoadMapRule()
        {
            string Path = "Data/Maps/Rules.xml";
            if (!File.Exists(Path))
            {
                CLogger.Print($"File not found: {Path}", LoggerType.Warning);
                return new List<MapRule>();
            }
            return ParseMapRule(Path);
        }
        private static List<MapMatch> LoadMapMatch()
        {
            string Path = "Data/Maps/Matches.xml";
            if (!File.Exists(Path))
            {
                CLogger.Print($"File not found: {Path}", LoggerType.Warning);
                return new List<MapMatch>();
            }
            return ParseMapMatch(Path);
        }
        public static MapRule GetMapRule(int RuleId)
        {
            foreach (MapRule Rule in Rules)
            {
                if (Rule.Id == RuleId)
                {
                    return Rule;
                }
            }
            return null;
        }
        public static MapMatch GetMapLimit(int MapId, int RuleId)
        {
            List<MapRule> RuleSnapshot = Rules;
            foreach (MapMatch Match in Matches)
            {
                if (Match.Id != MapId)
                {
                    continue;
                }
                foreach (MapRule Rule in RuleSnapshot)
                {
                    if (Rule.Id == Match.Mode && Rule.Rule == RuleId)
                    {
                        return Match;
                    }
                }
            }
            return null;
        }
        private static List<MapRule> ParseMapRule(string Path)
        {
            List<MapRule> Result = new List<MapRule>();
            XmlDocument Document = new XmlDocument();
            using (FileStream Stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (Stream.Length == 0)
                {
                    CLogger.Print($"File is empty: {Path}", LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        Document.Load(Stream);
                        for (XmlNode Node1 = Document.FirstChild; Node1 != null; Node1 = Node1.NextSibling)
                        {
                            if ("List".Equals(Node1.Name))
                            {
                                for (XmlNode Node2 = Node1.FirstChild; Node2 != null; Node2 = Node2.NextSibling)
                                {
                                    if ("Mode".Equals(Node2.Name))
                                    {
                                        XmlNamedNodeMap Xml = Node2.Attributes;
                                        MapRule Rule = new MapRule()
                                        {
                                            Id = int.Parse(Xml.GetNamedItem("Id").Value),
                                            Rule = byte.Parse(Xml.GetNamedItem("Rule").Value),
                                            StageOptions = byte.Parse(Xml.GetNamedItem("StageOptions").Value),
                                            Conditions = byte.Parse(Xml.GetNamedItem("Conditions").Value),
                                            Name = Xml.GetNamedItem("Name").Value
                                        };
                                        Result.Add(Rule);
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException Ex)
                    {
                        CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                    }
                }
                Stream.Dispose();
                Stream.Close();
            }
            return Result;
        }
        private static List<MapMatch> ParseMapMatch(string Path)
        {
            List<MapMatch> Result = new List<MapMatch>();
            XmlDocument XmlDocument = new XmlDocument();
            using (FileStream Stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (Stream.Length == 0)
                {
                    CLogger.Print($"File is empty: {Path}", LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        XmlDocument.Load(Stream);
                        for (XmlNode NodeList = XmlDocument.FirstChild; NodeList != null; NodeList = NodeList.NextSibling)
                        {
                            if ("List".Equals(NodeList.Name))
                            {
                                for (XmlNode Node = NodeList.FirstChild; Node != null; Node = Node.NextSibling)
                                {
                                    if ("Match".Equals(Node.Name))
                                    {
                                        XmlNamedNodeMap Xml = Node.Attributes;
                                        int RuleId = int.Parse(Xml.GetNamedItem("Rule").Value);
                                        string Mode = Xml.GetNamedItem("Mode").Value;
                                        if (RuleId == 0 || string.IsNullOrEmpty(Mode))
                                        {
                                            CLogger.Print($"Invalid Mode: {RuleId}; Mode Name: {Mode}; Please check and try again!", LoggerType.Warning);
                                            return Result;
                                        }
                                        ListMaps(Node, RuleId, Result);
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException Ex)
                    {
                        CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                    }
                }
                Stream.Dispose();
                Stream.Close();
            }
            return Result;
        }
        private static void ListMaps(XmlNode XmlNode, int RuleId, List<MapMatch> Result)
        {
            for (XmlNode NodeList = XmlNode.FirstChild; NodeList != null; NodeList = NodeList.NextSibling)
            {
                if ("Count".Equals(NodeList.Name))
                {
                    for (XmlNode Node = NodeList.FirstChild; Node != null; Node = Node.NextSibling)
                    {
                        if ("Map".Equals(Node.Name))
                        {
                            XmlNamedNodeMap Xml = Node.Attributes;
                            MapMatch Match = new MapMatch(RuleId)
                            {
                                Id = byte.Parse(Xml.GetNamedItem("Id").Value),
                                Limit = byte.Parse(Xml.GetNamedItem("Limit").Value),
                                Tag = byte.Parse(Xml.GetNamedItem("Tag").Value),
                                Name = Xml.GetNamedItem("Name").Value
                            };
                            Result.Add(Match);
                        }
                    }
                }
            }
        }
    }
}