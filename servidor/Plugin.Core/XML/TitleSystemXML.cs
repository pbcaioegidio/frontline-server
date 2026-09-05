// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.TitleSystemXML
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
    public class TitleSystemXML
    {
        private const string FallbackPath = "Data/Titles/System.xml";

        private static readonly List<TitleModel> Field0 = new List<TitleModel>();

        public static void Load()
        {
            List<TitleModel> Rows = DaoManagerSQL.GetTitles();
            int Count;
            lock (Field0)
            {
                Field0.Clear();
                if (Rows != null && Rows.Count > 0)
                {
                    Field0.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_titles unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Field0.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Title System", LoggerType.Info);
        }

        public static void Reload()
        {
            TitleSystemXML.Load();
        }

        public static TitleModel GetTitle(int titleId, bool ReturnNull = true)
        {
            if (titleId == 0)
                return ReturnNull ? (TitleModel)null : new TitleModel();
            foreach (TitleModel title in TitleSystemXML.Field0)
            {
                if (title.Id == titleId)
                    return title;
            }
            return (TitleModel)null;
        }

        public static void Get2Titles(
          int titleId1,
          int titleId2,
          out TitleModel title1,
          out TitleModel title2,
          bool ReturnNull = true)
        {
            if (!ReturnNull)
            {
                title1 = new TitleModel();
                title2 = new TitleModel();
            }
            else
            {
                title1 = (TitleModel)null;
                title2 = (TitleModel)null;
            }
            if (titleId1 == 0 && titleId2 == 0)
                return;
            foreach (TitleModel titleModel in TitleSystemXML.Field0)
            {
                if (titleModel.Id == titleId1)
                    title1 = titleModel;
                else if (titleModel.Id == titleId2)
                    title2 = titleModel;
            }
        }

        public static void Get3Titles(
          int titleId1,
          int titleId2,
          int titleId3,
          out TitleModel title1,
          out TitleModel title2,
          out TitleModel title3,
          bool ReturnNull)
        {
            if (!ReturnNull)
            {
                title1 = new TitleModel();
                title2 = new TitleModel();
                title3 = new TitleModel();
            }
            else
            {
                title1 = (TitleModel)null;
                title2 = (TitleModel)null;
                title3 = (TitleModel)null;
            }
            if (titleId1 == 0 && titleId2 == 0 && titleId3 == 0)
                return;
            foreach (TitleModel titleModel in TitleSystemXML.Field0)
            {
                if (titleModel.Id != titleId1)
                {
                    if (titleModel.Id == titleId2)
                        title2 = titleModel;
                    else if (titleModel.Id == titleId3)
                        title3 = titleModel;
                }
                else
                    title1 = titleModel;
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
                                    if (xmlNode2.Name.Equals("Title"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                                        TitleModel titleModel = new TitleModel(int.Parse(attributes.GetNamedItem("Id").Value))
                                        {
                                            ClassId = int.Parse(attributes.GetNamedItem("List").Value),
                                            Ribbon = int.Parse(attributes.GetNamedItem("Ribbon").Value),
                                            Ensign = int.Parse(attributes.GetNamedItem("Ensign").Value),
                                            Medal = int.Parse(attributes.GetNamedItem("Medal").Value),
                                            MasterMedal = int.Parse(attributes.GetNamedItem("MasterMedal").Value),
                                            Rank = int.Parse(attributes.GetNamedItem("Rank").Value),
                                            Slot = int.Parse(attributes.GetNamedItem("Slot").Value),
                                            Req1 = int.Parse(attributes.GetNamedItem("ReqT1").Value),
                                            Req2 = int.Parse(attributes.GetNamedItem("ReqT2").Value)
                                        };
                                        TitleSystemXML.Field0.Add(titleModel);
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