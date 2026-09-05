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
    public class RedeemCodeXML
    {
        private const string FallbackPath = "Data/RedeemCodes.xml";

        public static List<TicketModel> Tickets = new List<TicketModel>();

        public static void Load()
        {
            List<TicketModel> Rows = DaoManagerSQL.GetRedeemTickets();
            int Count;
            lock (Tickets)
            {
                Tickets.Clear();
                if (Rows != null && Rows.Count > 0)
                {
                    Tickets.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_redeem_codes unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                    LoadFromFile();
                }
                Count = Tickets.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Redeem Codes", LoggerType.Info);
        }

        public static void Reload()
        {
            RedeemCodeXML.Load();
        }

        public static TicketModel GetTicket(string Token, TicketType Type)
        {
            lock (RedeemCodeXML.Tickets)
            {
                foreach (TicketModel ticket in RedeemCodeXML.Tickets)
                {
                    if (ticket.Token == Token && ticket.Type == Type)
                        return ticket;
                }
                return (TicketModel)null;
            }
        }

        private static void LoadFromFile()
        {
            if (File.Exists(FallbackPath))
                ParseFile(FallbackPath);
            else
                CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
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
                                for (XmlNode TicketNode = xmlNode.FirstChild; TicketNode != null; TicketNode = TicketNode.NextSibling)
                                {
                                    if (TicketNode.Name.Equals("Ticket"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)TicketNode.Attributes;
                                        TicketModel Ticket = new TicketModel()
                                        {
                                            Token = attributes.GetNamedItem("Token").Value,
                                            Type = ComDiv.ParseEnum<TicketType>(attributes.GetNamedItem("Type").Value),
                                            TicketCount = uint.Parse(attributes.GetNamedItem("Count").Value),
                                            PlayerRation = uint.Parse(attributes.GetNamedItem("PlayerRation").Value),
                                            Rewards = new List<int>()
                                        };
                                        if (Ticket.Type == TicketType.VOUCHER)
                                        {
                                            Ticket.GoldReward = int.Parse(attributes.GetNamedItem("GoldReward").Value);
                                            Ticket.CashReward = int.Parse(attributes.GetNamedItem("CashReward").Value);
                                            Ticket.TagsReward = int.Parse(attributes.GetNamedItem("TagsReward").Value);
                                        }
                                        if (Ticket.Type == TicketType.COUPON)
                                            ParseRewards(TicketNode, Ticket);
                                        RedeemCodeXML.Tickets.Add(Ticket);
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

        private static void ParseRewards(XmlNode TicketNode, TicketModel Ticket)
        {
            for (XmlNode xmlNode1 = TicketNode.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
            {
                if (xmlNode1.Name.Equals("Rewards"))
                {
                    for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                    {
                        if (xmlNode2.Name.Equals("Goods"))
                        {
                            XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                            Ticket.Rewards.Add(int.Parse(attributes.GetNamedItem("Id").Value));
                        }
                    }
                }
            }
        }
    }
}
