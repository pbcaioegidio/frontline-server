using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;


namespace Plugin.Core.XML
{
    public class PlayerRankXML
    {
        private const string FallbackPath = "Data/Ranks/Player.xml";

        public static readonly List<RankModel> Ranks = new List<RankModel>();

        public static void Load()
        {
            List<RankModel> Rows = DaoManagerSQL.GetPlayerRanks();
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
                    CLogger.Print($"system_player_ranks unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Ranks.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} ranks de jogador", LoggerType.Info);
        }

        public static void Reload()
        {
            PlayerRankXML.Load();
        }

        public static RankModel GetRank(int Id)
        {
            lock (PlayerRankXML.Ranks)
            {
                foreach (RankModel rank in PlayerRankXML.Ranks)
                {
                    if (rank.Id == Id)
                        return rank;
                }
                return (RankModel)null;
            }
        }

        public static List<int> GetRewards(int RankId)
        {
            RankModel Rank = PlayerRankXML.GetRank(RankId);
            if (Rank == null || Rank.Rewards == null)
                return new List<int>();
            lock (Rank.Rewards)
                return new List<int>(Rank.Rewards);
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
                        for (XmlNode xmlNode = xmlDocument.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                        {
                            if (xmlNode.Name.Equals("List"))
                            {
                                for (XmlNode RankNode = xmlNode.FirstChild; RankNode != null; RankNode = RankNode.NextSibling)
                                {
                                    if (RankNode.Name.Equals("Rank"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)RankNode.Attributes;
                                        RankModel Rank = new RankModel(int.Parse(attributes.GetNamedItem("Id").Value))
                                        {
                                            Title = attributes.GetNamedItem("Title").Value,
                                            OnNextLevel = int.Parse(attributes.GetNamedItem("OnNextLevel").Value),
                                            OnGoldUp = int.Parse(attributes.GetNamedItem("OnGoldUp").Value),
                                            OnAllExp = int.Parse(attributes.GetNamedItem("OnAllExp").Value),
                                            Rewards = new List<int>()
                                        };
                                        ParseRewards(RankNode, Rank);
                                        PlayerRankXML.Ranks.Add(Rank);
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

        private static void ParseRewards(XmlNode RankNode, RankModel Rank)
        {
            for (XmlNode xmlNode1 = RankNode.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
            {
                if (xmlNode1.Name.Equals("Rewards"))
                {
                    for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                    {
                        if (xmlNode2.Name.Equals("Good") || xmlNode2.Name.Equals("Item"))
                        {
                            Rank.Rewards.Add(int.Parse(xmlNode2.Attributes.GetNamedItem("Id").Value));
                        }
                    }
                }
            }
        }
    }
}
