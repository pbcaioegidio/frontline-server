using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;


namespace Plugin.Core.XML
{
    public static class CouponEffectXML
    {
        private const string FallbackPath = "Data/CouponFlags.xml";

        private static readonly List<CouponFlag> Flags = new List<CouponFlag>();

        public static void Load()
        {
            List<CouponFlag> Rows = DaoManagerSQL.GetCouponEffects();
            int Count;
            lock (Flags)
            {
                Flags.Clear();
                if (Rows != null && Rows.Count > 0)
                {
                    Flags.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_coupon_effects unreachable or empty, falling back to {FallbackPath}", LoggerType.Error);
                    if (!File.Exists(FallbackPath))
                        CLogger.Print("File not found: " + FallbackPath, LoggerType.Warning);
                    else
                        ParseFile(FallbackPath);
                }
                Count = Flags.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Coupon Effects", LoggerType.Info);
        }

        public static void Reload()
        {
            CouponEffectXML.Load();
        }

        public static CouponFlag GetCouponEffect(int id)
        {
            lock (CouponEffectXML.Flags)
            {
                for (int index = 0; index < CouponEffectXML.Flags.Count; ++index)
                {
                    CouponFlag couponEffect = CouponEffectXML.Flags[index];
                    if (couponEffect.ItemId == id)
                        return couponEffect;
                }
                return (CouponFlag)null;
            }
        }

        private static void ParseFile(string Path)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream inStream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (inStream.Length == 0L)
                {
                    CLogger.Print("File is empty: " + Path, LoggerType.Warning);
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
                                    if (xmlNode2.Name.Equals("Coupon"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                                        CouponFlag couponFlag = new CouponFlag()
                                        {
                                            ItemId = int.Parse(attributes.GetNamedItem("ItemId").Value),
                                            EffectFlag = ComDiv.ParseEnum<CouponEffects>(attributes.GetNamedItem("EffectFlag").Value)
                                        };
                                        CouponEffectXML.Flags.Add(couponFlag);
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
