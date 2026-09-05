// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.MissionConfigXML
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;


namespace Plugin.Core.XML
{
    public class MissionConfigXML
    {
        public static uint MissionPage1;
        public static uint MissionPage2;
        private const string FallbackPath = "Data/MissionConfig.xml";

        private static readonly List<MissionStore> Field0 = new List<MissionStore>();

        public static void Load()
        {
            List<MissionStore> Rows = DaoManagerSQL.GetMissionStores();
            int Count;
            lock (Field0)
            {
                Field0.Clear();
                MissionConfigXML.MissionPage1 = 0U;
                MissionConfigXML.MissionPage2 = 0U;
                if (Rows != null && Rows.Count > 0)
                {
                    foreach (MissionStore Mission in Rows)
                    {
                        AddMission(Mission);
                    }
                }
                else
                {
                    CLogger.Print($"system_mission_stores unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Field0.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Mission Stores", LoggerType.Info);
        }

        public static void Reload()
        {
            MissionConfigXML.Load();
        }

        private static void AddMission(MissionStore Mission)
        {
            if (Mission.Enable)
            {
                uint Bit = (uint)(1 << Mission.Id);
                switch ((int)Math.Ceiling((double)Mission.Id / 32.0))
                {
                    case 1:
                        MissionConfigXML.MissionPage1 += Bit;
                        break;
                    case 2:
                        MissionConfigXML.MissionPage2 += Bit;
                        break;
                }
            }
            MissionConfigXML.Field0.Add(Mission);
        }

        public static MissionStore GetMission(int MissionId)
        {
            lock (MissionConfigXML.Field0)
            {
                foreach (MissionStore mission in MissionConfigXML.Field0)
                {
                    if (mission.Id == MissionId)
                        return mission;
                }
                return (MissionStore)null;
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
                        for (XmlNode xmlNode1 = xmlDocument.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
                        {
                            if (xmlNode1.Name.Equals("List"))
                            {
                                for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                                {
                                    if (xmlNode2.Name.Equals("Mission"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                                        AddMission(new MissionStore()
                                        {
                                            Id = int.Parse(attributes.GetNamedItem("Id").Value),
                                            ItemId = int.Parse(attributes.GetNamedItem("ItemId").Value),
                                            Enable = bool.Parse(attributes.GetNamedItem("Enable").Value)
                                        });
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