// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.EventBoostXML
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
    public class EventBoostXML
    {
        public static readonly List<EventBoostModel> Events = new List<EventBoostModel>();


        private const string FallbackPath = "Data/Events/Boost.xml";

        public static void Load()
        {
            var Rows = DaoManagerSQL.GetBoostEvents();
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
                    CLogger.Print($"system_event_boost unreachable, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Events.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Event Boost Bonus", LoggerType.Info);
        }

        public static void Reload()
        {
            EventBoostXML.Load();
        }


        public static EventBoostModel GetRunningEvent()
        {
            lock (EventBoostXML.Events)
            {
                uint num = uint.Parse(DateTimeUtil.Now("yyMMddHHmm"));
                foreach (EventBoostModel runningEvent in EventBoostXML.Events)
                {
                    if (runningEvent.BeginDate <= num && num < runningEvent.EndedDate)
                        return runningEvent;
                }
                return (EventBoostModel)null;
            }
        }

        public static EventBoostModel GetEvent(int EventId)
        {
            lock (EventBoostXML.Events)
            {
                foreach (EventBoostModel eventBoostModel in EventBoostXML.Events)
                {
                    if (eventBoostModel.Id == EventId)
                        return eventBoostModel;
                }
                return (EventBoostModel)null;
            }
        }

        public static bool EventIsValid(
          EventBoostModel Event,
          PortalBoostEvent BoostType,
          int BoostValue)
        {
            if (Event == null)
                return false;
            return Event.BoostType == BoostType || Event.BoostValue == BoostValue;
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
                                        EventBoostModel eventBoostModel = new EventBoostModel()
                                        {
                                            Id = int.Parse(attributes.GetNamedItem("Id").Value),
                                            BeginDate = uint.Parse(attributes.GetNamedItem("Begin").Value),
                                            EndedDate = uint.Parse(attributes.GetNamedItem("Ended").Value),
                                            BoostType = ComDiv.ParseEnum<PortalBoostEvent>(attributes.GetNamedItem("BoostType").Value),
                                            BoostValue = int.Parse(attributes.GetNamedItem("BoostValue").Value),
                                            BonusExp = int.Parse(attributes.GetNamedItem("BonusExp").Value),
                                            BonusGold = int.Parse(attributes.GetNamedItem("BonusGold").Value),
                                            Percent = int.Parse(attributes.GetNamedItem("Percent").Value),
                                            Name = attributes.GetNamedItem("Name").Value,
                                            Description = attributes.GetNamedItem("Description").Value,
                                            Period = bool.Parse(attributes.GetNamedItem("Period").Value),
                                            Priority = bool.Parse(attributes.GetNamedItem("Priority").Value)
                                        };
                                        var imageAttr = attributes.GetNamedItem("Image");
                                        if (imageAttr != null && byte.TryParse(imageAttr.Value, out byte imageIdx))
                                            eventBoostModel.Image = imageIdx;
                                        EventBoostXML.Events.Add(eventBoostModel);
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