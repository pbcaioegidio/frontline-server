// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.GameRuleXML
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;


namespace Plugin.Core.XML
{
    public class GameRuleXML
    {
        private const string FallbackPath = "Data/ClassicMode.xml";

        public static readonly List<TRuleModel> GameRules = new List<TRuleModel>();

        public static void Load()
        {
            List<KeyValuePair<string, List<string>>> Rows = DaoManagerSQL.GetGameRuleFilters();
            int Count;
            lock (GameRules)
            {
                GameRules.Clear();
                if (Rows != null && Rows.Count > 0)
                {
                    foreach (KeyValuePair<string, List<string>> Row in Rows)
                    {
                        TRuleModel Rule = new TRuleModel
                        {
                            Name = Row.Key,
                            BanIndexes = new List<int>()
                        };
                        foreach (string Filter in Row.Value)
                            ShopManager.IsBlocked(Filter, Rule.BanIndexes);
                        GameRules.Add(Rule);
                    }
                }
                else
                {
                    CLogger.Print($"system_game_rules unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = GameRules.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} regras de jogo", LoggerType.Info);
        }

        public static void Reload()
        {
            GameRuleXML.Load();
        }

        public static TRuleModel CheckTRuleByRoomName(string RoomName)
        {
            lock (GameRuleXML.GameRules)
            {
                foreach (TRuleModel gameRule in GameRuleXML.GameRules)
                {
                    if (RoomName.ToLower().Contains(gameRule.Name.ToLower()))
                        return gameRule;
                }
                return (TRuleModel)null;
            }
        }

        public static bool IsBlocked(int ListId, int ItemId) => ListId == ItemId;

        public static bool IsBlocked(int ListId, int ItemId,  List<string> List, string Category)
        {
            if (ListId != ItemId)
                return false;
            List.Add(Category);
            return true;
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
                                    if (A_0_1.Name.Equals("Rule"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)A_0_1.Attributes;
                                        TRuleModel A_1 = new TRuleModel()
                                        {
                                            Name = attributes.GetNamedItem("Name").Value,
                                            BanIndexes = new List<int>()
                                        };
                                        GameRuleXML.StaticMethod1(A_0_1, A_1);
                                        GameRuleXML.GameRules.Add(A_1);
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

        
        private static void StaticMethod1(XmlNode A_0, TRuleModel A_1)
        {
            for (XmlNode xmlNode1 = A_0.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
            {
                if (xmlNode1.Name.Equals("Extensions"))
                {
                    for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                    {
                        if (xmlNode2.Name.Equals("Ban"))
                            ShopManager.IsBlocked(xmlNode2.Attributes.GetNamedItem("Filter").Value, A_1.BanIndexes);
                    }
                }
            }
        }
    }
}