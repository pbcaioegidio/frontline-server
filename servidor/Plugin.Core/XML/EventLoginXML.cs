// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.EventLoginXML
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
    public class EventLoginXML
    {
        public static readonly List<EventLoginModel> Events = new List<EventLoginModel>();


        private const string FallbackPath = "Data/Events/Login.xml";

        public static void Load()
        {
            var Rows = DaoManagerSQL.GetLoginEvents();
            int Count;
            lock (Events)
            {
                Events.Clear();
                if (Rows != null)
                {
                    Events.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_event_login unreachable, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Events.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Event Login", LoggerType.Info);
        }

        public static void Reload()
        {
            EventLoginXML.Load();
        }


        public static EventLoginModel GetRunningEvent()
        {
            lock (EventLoginXML.Events)
            {
                uint num = uint.Parse(DateTimeUtil.Now("yyMMddHHmm"));
                foreach (EventLoginModel runningEvent in EventLoginXML.Events)
                {
                    if (runningEvent.BeginDate <= num && num < runningEvent.EndedDate)
                        return runningEvent;
                }
                return (EventLoginModel)null;
            }
        }

        public static EventLoginModel GetEvent(int EventId)
        {
            lock (EventLoginXML.Events)
            {
                foreach (EventLoginModel eventLoginModel in EventLoginXML.Events)
                {
                    if (eventLoginModel.Id == EventId)
                        return eventLoginModel;
                }
                return (EventLoginModel)null;
            }
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
                        for (XmlNode xmlNode = xmlDocument.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                        {
                            if (xmlNode.Name.Equals("List"))
                            {
                                for (XmlNode A_0_1 = xmlNode.FirstChild; A_0_1 != null; A_0_1 = A_0_1.NextSibling)
                                {
                                    if (A_0_1.Name.Equals("Event"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)A_0_1.Attributes;
                                        EventLoginModel A_1 = new EventLoginModel()
                                        {
                                            Id = int.Parse(attributes.GetNamedItem("Id").Value),
                                            BeginDate = uint.Parse(attributes.GetNamedItem("Begin").Value),
                                            EndedDate = uint.Parse(attributes.GetNamedItem("Ended").Value),
                                            Name = attributes.GetNamedItem("Name").Value,
                                            Description = attributes.GetNamedItem("Description").Value,
                                            Period = bool.Parse(attributes.GetNamedItem("Period").Value),
                                            Priority = bool.Parse(attributes.GetNamedItem("Priority").Value),
                                            Goods = new List<int>()
                                        };
                                        // Image escolhe Gui/EventPortal/img_*_<N>.i3i (+0x153). Default no ctor = 1.
                                        var imageAttr = attributes.GetNamedItem("Image");
                                        if (imageAttr != null && byte.TryParse(imageAttr.Value, out byte imageIdx))
                                            A_1.Image = imageIdx;
                                        EventLoginXML.StaticMethod1(A_0_1, A_1);
                                        EventLoginXML.Events.Add(A_1);
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


        private static void StaticMethod1(XmlNode A_0, EventLoginModel A_1)
        {
            for (XmlNode xmlNode1 = A_0.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
            {
                if (xmlNode1.Name.Equals("Rewards"))
                {
                    for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                    {
                        if (xmlNode2.Name.Equals("Item"))
                        {
                            int num = int.Parse(xmlNode2.Attributes.GetNamedItem("GoodId").Value);
                            if (A_1.Goods.Count <= 4)
                            {
                                A_1.Goods.Add(num);
                            }
                            else
                            {
                                CLogger.Print("Max that can be listed on Login Event was 4!", LoggerType.Warning);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}