// Decompiled with JetBrains decompiler
// Type: Plugin.Core.XML.InternetCafeXML
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
    public static class InternetCafeXML
    {
        public static readonly List<InternetCafe> Cafes = new List<InternetCafe>();

        
        private const string FallbackPath = "Data/InternetCafe.xml";

        public static void Load()
        {
            var Rows = DaoManagerSQL.GetInternetCafeBonuses();
            int Count;
            lock (Cafes)
            {
                Cafes.Clear();
                if (Rows != null && Rows.Count > 0)
                {
                    Cafes.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_internet_cafe_bonuses unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Cafes.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Internet Cafe Bonuses", LoggerType.Info);
        }

        public static void Reload()
        {
            InternetCafeXML.Load();
        }

        public static InternetCafe GetICafe(int ConfigId)
        {
            lock (InternetCafeXML.Cafes)
            {
                foreach (InternetCafe cafe in InternetCafeXML.Cafes)
                {
                    if (cafe.ConfigId == ConfigId)
                        return cafe;
                }
                return (InternetCafe)null;
            }
        }

        public static bool IsValidAddress(string PlayerAddress, string RegisteredAddress)
        {
            return PlayerAddress.Equals(RegisteredAddress);
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
                                    if (xmlNode2.Name.Equals("Bonus"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                                        InternetCafe internetCafe = new InternetCafe(int.Parse(attributes.GetNamedItem("Id").Value))
                                        {
                                            BasicExp = int.Parse(attributes.GetNamedItem("BasicExp").Value),
                                            BasicGold = int.Parse(attributes.GetNamedItem("BasicGold").Value),
                                            PremiumExp = int.Parse(attributes.GetNamedItem("PremiumExp").Value),
                                            PremiumGold = int.Parse(attributes.GetNamedItem("PremiumGold").Value)
                                        };
                                        InternetCafeXML.Cafes.Add(internetCafe);
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

        public static bool Method0(string A_1) => nameof(CouponEffectXML).Equals(A_1);
    }
}