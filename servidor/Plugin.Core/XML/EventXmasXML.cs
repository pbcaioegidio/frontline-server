// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.EventXmasXML
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;


namespace Plugin.Core.XML
{
    public class EventXmasXML
    {
        private static readonly List<EventXmasModel> Field0 = new List<EventXmasModel>();


        private const string FallbackPath = "Data/Events/Xmas.xml";

        public static void Load()
        {
            var Rows = DaoManagerSQL.GetXmasEvents();
            int Count;
            lock (Field0)
            {
                Field0.Clear();
                if (Rows != null)
                {
                    Field0.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_event_xmas unreachable, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Field0.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Event X-Mas", LoggerType.Info);
        }

        public static void Reload()
        {
            EventXmasXML.Load();
        }


        public static EventXmasModel GetRunningEvent()
        {
            uint num = uint.Parse(DateTimeUtil.Now("yyMMddHHmm"));
            foreach (EventXmasModel runningEvent in EventXmasXML.Field0)
            {
                if (runningEvent.BeginDate <= num && num < runningEvent.EndedDate)
                    return runningEvent;
            }
            return (EventXmasModel)null;
        }


        private static void ParseFile(string A_0)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream inStream = new FileStream(A_0, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (inStream.Length == 0L)
                {
                    CLogger.Print("File is empty: " + A_0, LoggerType.Warning);
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
                                    if (xmlNode2.Name.Equals("Event"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                                        EventXmasModel eventXmasModel = new EventXmasModel()
                                        {
                                            BeginDate = uint.Parse(attributes.GetNamedItem("Begin").Value),
                                            EndedDate = uint.Parse(attributes.GetNamedItem("Ended").Value),
                                            GoodId = int.Parse(attributes.GetNamedItem("GoodId").Value)
                                        };
                                        EventXmasXML.Field0.Add(eventXmasModel);
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