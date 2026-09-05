using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Plugin.Core.XML
{
    public class ClanRankXML
    {
        private const string FallbackPath = "Data/Ranks/Clan.xml";

        private static readonly List<RankModel> Ranks = new List<RankModel>();

        public static void Load()
        {
            List<RankModel> Rows = DaoManagerSQL.GetClanRanks();
            int Count;
            lock (Ranks)
            {
                Ranks.Clear();
                if (Rows != null && Rows.Count > 0)
                {
                    Ranks.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_clan_ranks unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Ranks.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} ranks de clan", LoggerType.Info);
        }

        public static void Reload()
        {
            ClanRankXML.Load();
        }

        public static RankModel GetRank(int Id)
        {
            lock (ClanRankXML.Ranks)
            {
                foreach (RankModel rank in ClanRankXML.Ranks)
                {
                    if (rank.Id == Id)
                        return rank;
                }
                return (RankModel)null;
            }
        }

        private static void ParseFile(string Path)
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
                                    if (xmlNode2.Name.Equals("Rank"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                                        RankModel rankModel = new RankModel((int)byte.Parse(attributes.GetNamedItem("Id").Value))
                                        {
                                            Title = attributes.GetNamedItem("Title").Value,
                                            OnNextLevel = int.Parse(attributes.GetNamedItem("OnNextLevel").Value),
                                            OnGoldUp = 0,
                                            OnAllExp = int.Parse(attributes.GetNamedItem("OnAllExp").Value),
                                            Rewards = new List<int>()
                                        };
                                        ClanRankXML.Ranks.Add(rankModel);
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
    }
}
