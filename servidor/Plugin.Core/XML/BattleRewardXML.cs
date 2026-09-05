// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.BattleRewardXML
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
    public class BattleRewardXML
    {
        public static readonly List<BattleRewardModel> Rewards = new List<BattleRewardModel>();

        
        private const string FallbackPath = "Data/BattleRewards.xml";

        public static void Load()
        {
            var Rows = DaoManagerSQL.GetBattleRewards();
            int Count;
            lock (Rewards)
            {
                Rewards.Clear();
                if (Rows != null && Rows.Count > 0)
                {
                    Rewards.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_battle_rewards unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Rewards.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} recompensas de batalha", LoggerType.Info);
        }

        public static void Reload()
        {
            BattleRewardXML.Load();
        }

        public static BattleRewardModel GetRewardType(BattleRewardType RewardType)
        {
            lock (BattleRewardXML.Rewards)
            {
                foreach (BattleRewardModel reward in BattleRewardXML.Rewards)
                {
                    if (reward.Type == RewardType)
                        return reward;
                }
                return (BattleRewardModel)null;
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
                                    if (A_0_1.Name.Equals("Item"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)A_0_1.Attributes;
                                        int length = int.Parse(attributes.GetNamedItem("Count").Value);
                                        BattleRewardModel A_1 = new BattleRewardModel()
                                        {
                                            Type = ComDiv.ParseEnum<BattleRewardType>(attributes.GetNamedItem("Type").Value),
                                            Percentage = int.Parse(attributes.GetNamedItem("Percentage").Value),
                                            Rewards = new int[length]
                                        };
                                        BattleRewardXML.StaticMethod1(A_0_1, A_1);
                                        BattleRewardXML.Rewards.Add(A_1);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                    }
                }
                inStream.Dispose();
                inStream.Close();
            }
        }

        
        private static void StaticMethod1(XmlNode A_0, BattleRewardModel A_1)
        {
            for (XmlNode xmlNode1 = A_0.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
            {
                if (xmlNode1.Name.Equals("Rewards"))
                {
                    for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                    {
                        if (xmlNode2.Name.Equals("Good"))
                        {
                            XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                            BattleRewardItem battleRewardItem = new BattleRewardItem()
                            {
                                Index = int.Parse(attributes.GetNamedItem("Index").Value),
                                GoodId = int.Parse(attributes.GetNamedItem("Id").Value)
                            };
                            A_1.Rewards[battleRewardItem.Index] = battleRewardItem.GoodId;
                        }
                    }
                }
            }
        }
    }
}