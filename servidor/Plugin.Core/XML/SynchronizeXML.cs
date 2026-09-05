using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;


namespace Plugin.Core.XML
{
    public class SynchronizeXML
    {
        private const string FallbackPath = "Data/Synchronize.xml";

        public static List<Synchronize> Servers = new List<Synchronize>();

        public static void Load()
        {
            List<Synchronize> Rows = DaoManagerSQL.GetSyncEndpoints();
            if (Rows != null && Rows.Count > 0)
            {
                Servers.AddRange(Rows);
            }
            else
            {
                CLogger.Print($"system_sync_endpoints unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                LoadFromFile();
            }
        }

        public static void Reload()
        {
            SynchronizeXML.Servers.Clear();
            SynchronizeXML.Load();
        }

        public static Synchronize GetServer(int Port)
        {
            if (SynchronizeXML.Servers.Count == 0)
                return (Synchronize)null;
            try
            {
                lock (SynchronizeXML.Servers)
                {
                    foreach (Synchronize server in SynchronizeXML.Servers)
                    {
                        if (server.RemotePort == Port)
                            return server;
                    }
                    return (Synchronize)null;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return (Synchronize)null;
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
                        for (XmlNode xmlNode1 = xmlDocument.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
                        {
                            if (xmlNode1.Name.Equals("List"))
                            {
                                for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                                {
                                    if (xmlNode2.Name.Equals("Sync"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                                        Synchronize synchronize = new Synchronize(attributes.GetNamedItem("Host").Value, int.Parse(attributes.GetNamedItem("Port").Value))
                                        {
                                            RemotePort = int.Parse(attributes.GetNamedItem("RemotePort").Value)
                                        };
                                        SynchronizeXML.Servers.Add(synchronize);
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
