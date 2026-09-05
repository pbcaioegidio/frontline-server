using System.Collections.Generic;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System.IO;
using System.Xml;
using System;
using Plugin.Core.Enums;
using Plugin.Core.Utility;

namespace Plugin.Core.XML
{
    public class SChannelXML
    {
        private const string FallbackPath = "Data/Server/SChannels.xml";

        public static List<SChannelModel> Servers = new List<SChannelModel>();
        public static void Load(bool Update = false)
        {
            List<SChannelModel> Rows = DaoManagerSQL.GetSystemServers();
            if (Rows != null && Rows.Count > 0)
            {
                foreach (SChannelModel Row in Rows)
                {
                    Apply(Row, Update);
                }
            }
            else
            {
                CLogger.Print($"system_servers unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                LoadFromFile(Update);
            }
            CLogger.Print($"Plugin carregado: {Servers.Count} canais de servidor", LoggerType.Info);
        }
        public static void UpdateServer(int ServerId)
        {
            List<SChannelModel> Rows = DaoManagerSQL.GetSystemServers();
            if (Rows != null && Rows.Count > 0)
            {
                foreach (SChannelModel Row in Rows)
                {
                    if (Row.Id == ServerId)
                    {
                        Apply(Row, true);
                        return;
                    }
                }
            }
            else if (File.Exists(FallbackPath))
            {
                ParseReload(FallbackPath, ServerId);
            }
            else
            {
                CLogger.Print($"File not found: {FallbackPath}", LoggerType.Warning);
            }
        }
        public static void Reload()
        {
            Servers.Clear();
            Load(true);
        }
        public static SChannelModel GetServer(int id)
        {
            lock (Servers)
            {
                foreach (SChannelModel server in Servers)
                {
                    if (server.Id == id)
                    {
                        return server;
                    }
                }
                return null;
            }
        }
        private static void Apply(SChannelModel Row, bool Update)
        {
            string advertisedHost = Environment.GetEnvironmentVariable("PB_ADVERTISE_HOST");
            if (!string.IsNullOrWhiteSpace(advertisedHost))
                Row.Host = advertisedHost; // fix container handoff: advertise the Windows-reachable host instead of the database address
            if (Update)
            {
                SChannelModel Server = GetServer(Row.Id);
                if (Server != null)
                {
                    lock (Servers)
                    {
                        Server.State = Row.State;
                        Server.Host = Row.Host;
                        Server.Port = Row.Port;
                        Server.Type = Row.Type;
                        Server.IsMobile = Row.IsMobile;
                        Server.MaxPlayers = Row.MaxPlayers;
                        Server.ChannelPlayers = Row.ChannelPlayers;
                    }
                    return;
                }
            }
            Servers.Add(Row);
        }
        private static void LoadFromFile(bool Update)
        {
            if (File.Exists(FallbackPath))
            {
                ParseLoad(FallbackPath, Update);
            }
            else
            {
                CLogger.Print($"File not found: {FallbackPath}", LoggerType.Warning);
            }
        }
        private static void ParseLoad(string Path, bool Update)
        {
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
                                    if ("Server".Equals(Node2.Name))
                                    {
                                        XmlNamedNodeMap xml = Node2.Attributes;
                                        SChannelModel SChannel = new SChannelModel(xml.GetNamedItem("Host").Value, ushort.Parse(xml.GetNamedItem("Port").Value))
                                        {
                                            Id = int.Parse(xml.GetNamedItem("Id").Value),
                                            State = bool.Parse(xml.GetNamedItem("State").Value),
                                            Type = ComDiv.ParseEnum<SChannelType>(xml.GetNamedItem("Type").Value),
                                            IsMobile = bool.Parse(xml.GetNamedItem("Mobile").Value),
                                            MaxPlayers = int.Parse(xml.GetNamedItem("MaxPlayers").Value),
                                            ChannelPlayers = int.Parse(xml.GetNamedItem("ChannelPlayers").Value)
                                        };
                                        Apply(SChannel, Update);
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
        }
        private static void ParseReload(string Path, int ServerId)
        {
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
                                    if ("Server".Equals(Node2.Name))
                                    {
                                        XmlNamedNodeMap xml = Node2.Attributes;
                                        SChannelModel Server = GetServer(ServerId);
                                        if (Server != null)
                                        {
                                            Server.State = bool.Parse(xml.GetNamedItem("State").Value);
                                            Server.Host = xml.GetNamedItem("Host").Value;
                                            Server.Port = ushort.Parse(xml.GetNamedItem("Port").Value);
                                            Server.Type = ComDiv.ParseEnum<SChannelType>(xml.GetNamedItem("Type").Value);
                                            Server.IsMobile = bool.Parse(xml.GetNamedItem("Mobile").Value);
                                            Server.MaxPlayers = int.Parse(xml.GetNamedItem("MaxPlayers").Value);
                                            Server.ChannelPlayers = int.Parse(xml.GetNamedItem("ChannelPlayers").Value);
                                        }
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
        }
    }
}
