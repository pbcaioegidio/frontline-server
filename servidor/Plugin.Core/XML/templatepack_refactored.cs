// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.TemplatePackXML
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

#nullable disable
namespace Plugin.Core.XML
{
    public class TemplatePackXML
    {
        public static List<ItemsModel> Basics = new List<ItemsModel>();
        public static List<ItemsModel> Awards = new List<ItemsModel>();
        public static List<PCCafeModel> Cafes = new List<PCCafeModel>();

        public static void Load()
        {
            LoadBasicTemplates();
            LoadPCCafeTemplates();
            LoadAwardTemplates();
        }

        public static void Reload()
        {
            Basics.Clear();
            Awards.Clear();
            Cafes.Clear();
            Load();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadBasicTemplates()
        {
            string xmlFilePath = "Data/Temps/Basic.xml";
            if (!File.Exists(xmlFilePath))
                CLogger.Print("File not found: " + xmlFilePath, LoggerType.Warning);
            else
                ParseBasicTemplateXml(xmlFilePath);
            CLogger.Print($"Plugin carregado: {Basics.Count} Basic Templates", LoggerType.Info);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadPCCafeTemplates()
        {
            string xmlFilePath = "Data/Temps/CafePC.xml";
            if (File.Exists(xmlFilePath))
                ParsePCCafeXml(xmlFilePath);
            else
                CLogger.Print("File not found: " + xmlFilePath, LoggerType.Warning);
            CLogger.Print($"Plugin carregado: {Cafes.Count} PC Cafes", LoggerType.Info);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void LoadAwardTemplates()
        {
            string xmlFilePath = "Data/Temps/Award.xml";
            if (File.Exists(xmlFilePath))
                ParseAwardTemplateXml(xmlFilePath);
            else
                CLogger.Print("File not found: " + xmlFilePath, LoggerType.Warning);
            CLogger.Print($"Plugin carregado: {Awards.Count} Award Templates", LoggerType.Info);
        }

        public static PCCafeModel GetPCCafe(CafeEnum Type)
        {
            lock (Cafes)
            {
                foreach (PCCafeModel cafe in Cafes)
                {
                    if (cafe.Type == Type)
                        return cafe;
                }
                return null;
            }
        }

        public static List<ItemsModel> GetPCCafeRewards(CafeEnum Type)
        {
            PCCafeModel pcCafe = GetPCCafe(Type);
            if (pcCafe != null)
            {
                lock (pcCafe.Rewards)
                {
                    if (pcCafe.Rewards.TryGetValue(Type, out List<ItemsModel> pcCafeRewards))
                        return pcCafeRewards;
                }
            }
            return new List<ItemsModel>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ParseBasicTemplateXml(string xmlFilePath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open))
            {
                if (fileStream.Length == 0L)
                {
                    CLogger.Print("File is empty: " + xmlFilePath, LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        xmlDocument.Load(fileStream);
                        for (XmlNode rootNode = xmlDocument.FirstChild; rootNode != null; rootNode = rootNode.NextSibling)
                        {
                            if (IsNodeNamed(rootNode.Name, "List"))
                            {
                                for (XmlNode itemNode = rootNode.FirstChild; itemNode != null; itemNode = itemNode.NextSibling)
                                {
                                    if (IsNodeNamed(itemNode.Name, "Item"))
                                    {
                                        XmlNamedNodeMap attributes = itemNode.Attributes;
                                        int itemId = int.Parse(attributes.GetNamedItem("Id").Value);
                                        ItemsModel basicItem = new ItemsModel(itemId)
                                        {
                                            ObjectId = (long)ComDiv.ValidateStockId(itemId),
                                            Name = attributes.GetNamedItem("Name").Value,
                                            Count = 1,
                                            Equip = ItemEquipType.Permanent
                                        };
                                        Basics.Add(basicItem);
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                    }
                }
                fileStream.Dispose();
                fileStream.Close();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ParsePCCafeXml(string xmlFilePath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open))
            {
                if (fileStream.Length == 0L)
                {
                    CLogger.Print("File is empty: " + xmlFilePath, LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        xmlDocument.Load(fileStream);
                        for (XmlNode rootNode = xmlDocument.FirstChild; rootNode != null; rootNode = rootNode.NextSibling)
                        {
                            if (IsNodeNamed(rootNode.Name, "List"))
                            {
                                for (XmlNode cafeNode = rootNode.FirstChild; cafeNode != null; cafeNode = cafeNode.NextSibling)
                                {
                                    if (IsNodeNamed(cafeNode.Name, "Cafe"))
                                    {
                                        XmlNamedNodeMap attributes = cafeNode.Attributes;
                                        PCCafeModel cafeModel = new PCCafeModel(ComDiv.ParseEnum<CafeEnum>(attributes.GetNamedItem("Type").Value))
                                        {
                                            ExpUp = int.Parse(attributes.GetNamedItem("ExpUp").Value),
                                            PointUp = int.Parse(attributes.GetNamedItem("PointUp").Value),
                                            Rewards = new SortedList<CafeEnum, List<ItemsModel>>()
                                        };
                                        ParseCafeRewards(cafeNode, cafeModel);
                                        Cafes.Add(cafeModel);
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                    }
                }
                fileStream.Dispose();
                fileStream.Close();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ParseCafeRewards(XmlNode cafeNode, PCCafeModel cafeModel)
        {
            for (XmlNode childNode = cafeNode.FirstChild; childNode != null; childNode = childNode.NextSibling)
            {
                if (IsNodeNamed(childNode.Name, "Rewards"))
                {
                    for (XmlNode rewardNode = childNode.FirstChild; rewardNode != null; rewardNode = rewardNode.NextSibling)
                    {
                        if (IsNodeNamed(rewardNode.Name, "Item"))
                        {
                            XmlNamedNodeMap attributes = rewardNode.Attributes;
                            int itemId = int.Parse(attributes.GetNamedItem("Id").Value);
                            ItemsModel rewardItem = new ItemsModel(itemId)
                            {
                                ObjectId = (long)ComDiv.ValidateStockId(itemId),
                                Name = attributes.GetNamedItem("Name").Value,
                                Count = 1,
                                Equip = ItemEquipType.CafePC
                            };
                            AddRewardToModel(cafeModel, rewardItem);
                        }
                    }
                }
            }
        }

        private static void AddRewardToModel(PCCafeModel cafeModel, ItemsModel rewardItem)
        {
            lock (cafeModel.Rewards)
            {
                if (cafeModel.Rewards.ContainsKey(cafeModel.Type))
                    cafeModel.Rewards[cafeModel.Type].Add(rewardItem);
                else
                    cafeModel.Rewards.Add(cafeModel.Type, new List<ItemsModel>() { rewardItem });
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ParseAwardTemplateXml(string xmlFilePath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open))
            {
                if (fileStream.Length == 0L)
                {
                    CLogger.Print("File is empty: " + xmlFilePath, LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        xmlDocument.Load(fileStream);
                        for (XmlNode rootNode = xmlDocument.FirstChild; rootNode != null; rootNode = rootNode.NextSibling)
                        {
                            if (IsNodeNamed(rootNode.Name, "List"))
                            {
                                for (XmlNode itemNode = rootNode.FirstChild; itemNode != null; itemNode = itemNode.NextSibling)
                                {
                                    if (IsNodeNamed(itemNode.Name, "Item"))
                                    {
                                        XmlNamedNodeMap attributes = itemNode.Attributes;
                                        ItemsModel awardItem = new ItemsModel(int.Parse(attributes.GetNamedItem("Id").Value))
                                        {
                                            Name = attributes.GetNamedItem("Name").Value,
                                            Count = uint.Parse(attributes.GetNamedItem("Count").Value),
                                            Equip = ItemEquipType.Durable
                                        };
                                        Awards.Add(awardItem);
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException ex)
                    {
                        CLogger.Print(ex.Message, LoggerType.Error, ex);
                    }
                }
                fileStream.Dispose();
                fileStream.Close();
            }
        }

        // Helper method to replace the cryptic Method0 calls
        private static bool IsNodeNamed(string nodeName, string expectedName) 
        {
            return string.Equals(nodeName, expectedName, StringComparison.OrdinalIgnoreCase);
        }
    }
}