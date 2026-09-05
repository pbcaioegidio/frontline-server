// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.EventVisitXML
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
    public class EventVisitXML
    {
        public static readonly List<EventVisitModel> Events = new List<EventVisitModel>();


        private const string FallbackPath = "Data/Events/Visit.xml";

        public static void Load()
        {
            var Rows = DaoManagerSQL.GetVisitEvents();
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
                    CLogger.Print($"system_event_visit unreachable, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Events.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Event Visit", LoggerType.Info);
        }

        public static void Reload()
        {
            EventVisitXML.Load();
        }

        public static EventVisitModel GetEvent(int EventId)
        {
            lock (EventVisitXML.Events)
            {
                foreach (EventVisitModel eventVisitModel in EventVisitXML.Events)
                {
                    if (eventVisitModel.Id == EventId)
                        return eventVisitModel;
                }
                return (EventVisitModel)null;
            }
        }


        public static EventVisitModel GetRunningEvent()
        {
            lock (EventVisitXML.Events)
            {
                uint num = uint.Parse(DateTimeUtil.Now("yyMMddHHmm"));
                foreach (EventVisitModel runningEvent in EventVisitXML.Events)
                {
                    if (runningEvent.BeginDate <= num && num < runningEvent.EndedDate)
                        return runningEvent;
                }
                return (EventVisitModel)null;
            }
        }


        public static void ResetPlayerEvent(long PlayerId, PlayerEvent Event)
        {
            if (PlayerId == 0L)
                return;
            ComDiv.UpdateDB("player_events", "owner_id", (object)PlayerId, new string[4]
            {
      "last_visit_check_day",
      "last_visit_seq_type",
      "last_visit_date",
      "last_visit_reward_day"
            }, (object)Event.LastVisitCheckDay, (object)Event.LastVisitSeqType, (object)(long)Event.LastVisitDate, (object)Event.LastVisitRewardDay);
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
                                        EventVisitModel A_1 = new EventVisitModel()
                                        {
                                            Id = int.Parse(attributes.GetNamedItem("Id").Value),
                                            BeginDate = uint.Parse(attributes.GetNamedItem("Begin").Value),
                                            EndedDate = uint.Parse(attributes.GetNamedItem("Ended").Value),
                                            Title = attributes.GetNamedItem("Title").Value,
                                            Checks = int.Parse(attributes.GetNamedItem("Days").Value),
                                            Boxes = new List<VisitBoxModel>()
                                        };
                                        for (int index = 0; index < 31 /*0x1F*/; ++index)
                                            A_1.Boxes.Add(new VisitBoxModel());
                                        EventVisitXML.StaticMethod1(A_0_1, A_1);
                                        A_1.SetBoxCounts();
                                        EventVisitXML.Events.Add(A_1);
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


        private static void StaticMethod1(XmlNode A_0, EventVisitModel A_1)
        {
            for (XmlNode xmlNode1 = A_0.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
            {
                if (xmlNode1.Name.Equals("Rewards"))
                {
                    for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                    {
                        if (xmlNode2.Name.Equals("Box"))
                        {
                            XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                            int index = int.Parse(attributes.GetNamedItem("Day").Value) - 1;
                            A_1.Boxes[index].Reward1.SetGoodId(int.Parse(attributes.GetNamedItem("GoodId1").Value));
                            A_1.Boxes[index].Reward2.SetGoodId(int.Parse(attributes.GetNamedItem("GoodId2").Value));
                            A_1.Boxes[index].IsBothReward = bool.Parse(attributes.GetNamedItem("Both").Value);
                        }
                    }
                }
            }
        }
    }
}