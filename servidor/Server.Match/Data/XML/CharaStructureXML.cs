// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.XML.CharaStructureXML
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Match.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;


namespace Server.Match.Data.XML
{
    public class CharaStructureXML
    {
        public static List<CharaModel> Charas = new List<CharaModel>();

        
        public static void Load()
        {
            string str = "Data/Match/CharaHealth.xml";
            if (File.Exists(str))
                CharaStructureXML.StaticMethod0(str);
            else
                CLogger.Print("File not found: " + str, LoggerType.Warning);
        }

        public static void Reload()
        {
            CharaStructureXML.Charas.Clear();
            CharaStructureXML.Load();
        }

        public static int GetCharaHP(int CharaId)
        {
            foreach (CharaModel chara in CharaStructureXML.Charas)
            {
                if (chara.Id == CharaId)
                    return chara.HP;
            }
            return 100;
        }

        
        private static void StaticMethod0(string A_0)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (FileStream inStream = new FileStream(A_0, FileMode.Open, FileAccess.Read, FileShare.Read)) // fix linux deploy: runtime Data is mounted read-only
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
                                    if (xmlNode2.Name.Equals("Chara"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                                        CharaModel charaModel = new CharaModel()
                                        {
                                            Id = int.Parse(attributes.GetNamedItem("Id").Value),
                                            HP = int.Parse(attributes.GetNamedItem("HP").Value)
                                        };
                                        CharaStructureXML.Charas.Add(charaModel);
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
