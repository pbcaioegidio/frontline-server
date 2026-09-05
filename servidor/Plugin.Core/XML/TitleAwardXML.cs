using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Plugin.Core.XML
{
    public class TitleAwardXML
    {
        private const string FallbackPath = "Data/Titles/Rewards.xml";

        public static readonly List<TitleAward> Awards = new List<TitleAward>();

        public static void Load()
        {
            List<TitleAward> Rows = DaoManagerSQL.GetTitleAwards();
            int Count;
            lock (Awards)
            {
                Awards.Clear();
                if (Rows != null && Rows.Count > 0)
                {
                    Awards.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_title_awards unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Awards.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Title Awards", LoggerType.Info);
        }

        public static void Reload()
        {
            TitleAwardXML.Load();
        }

        public static List<ItemsModel> GetAwards(int titleId)
        {
            List<ItemsModel> awards = new List<ItemsModel>();
            lock (TitleAwardXML.Awards)
            {
                foreach (TitleAward award in TitleAwardXML.Awards)
                {
                    if (award.Id == titleId)
                        awards.Add(award.Item);
                }
            }
            return awards;
        }

        public static bool Contains(int TitleId, int ItemId)
        {
            if (ItemId == 0)
                return false;
            lock (TitleAwardXML.Awards)
            {
                foreach (TitleAward award in TitleAwardXML.Awards)
                {
                    if (award.Id == TitleId && award.Item.Id == ItemId)
                        return true;
                }
            }
            return false;
        }

        private static void ParseFile(string Path)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream inStream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (inStream.Length > 0L)
                {
                    try
                    {
                        xmlDocument.Load((Stream)inStream);
                        for (XmlNode xmlNode = xmlDocument.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                        {
                            if (xmlNode.Name.Equals("List"))
                            {
                                for (XmlNode AwardNode = xmlNode.FirstChild; AwardNode != null; AwardNode = AwardNode.NextSibling)
                                {
                                    if (AwardNode.Name.Equals("Award"))
                                    {
                                        ParseItems(AwardNode, int.Parse(AwardNode.Attributes.GetNamedItem("TitleId").Value));
                                    }
                                    else if (AwardNode.Name.Equals("Title"))
                                    {
                                        ParseItems(AwardNode, int.Parse(AwardNode.Attributes.GetNamedItem("Id").Value));
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

        private static void ParseItems(XmlNode AwardNode, int TitleId)
        {
            for (XmlNode Block = AwardNode.FirstChild; Block != null; Block = Block.NextSibling)
            {
                for (XmlNode ItemNode = Block.FirstChild; ItemNode != null; ItemNode = ItemNode.NextSibling)
                {
                    if (!ItemNode.Name.Equals("Item"))
                        continue;

                    XmlNamedNodeMap attributes = (XmlNamedNodeMap)ItemNode.Attributes;
                    int ItemId = int.Parse(attributes.GetNamedItem("Id").Value);
                    ItemsModel Item = new ItemsModel(ItemId)
                    {
                        Name = attributes.GetNamedItem("Name").Value,
                        Count = uint.Parse(attributes.GetNamedItem("Count").Value),
                        Equip = (ItemEquipType)int.Parse(attributes.GetNamedItem("Equip").Value)
                    };
                    if (Item.Equip == ItemEquipType.Permanent)
                        Item.ObjectId = (long)ComDiv.ValidateStockId(ItemId);
                    TitleAwardXML.Awards.Add(new TitleAward
                    {
                        Id = TitleId,
                        Item = Item
                    });
                }
            }
        }
    }
}
