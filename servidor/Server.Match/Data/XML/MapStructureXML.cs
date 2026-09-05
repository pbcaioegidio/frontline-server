// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.XML.MapStructureXML
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.SharpDX;
using Server.Match.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;


namespace Server.Match.Data.XML
{
    public class MapStructureXML
    {
        public static List<MapModel> Maps = new List<MapModel>();

        
        public static void Load()
        {
            string str = "Data/Match/MapStructure.xml";
            if (File.Exists(str))
                MapStructureXML.StaticMethod0(str);
            else
                CLogger.Print("File not found: " + str, LoggerType.Warning);
        }

        public static void Reload()
        {
            MapStructureXML.Maps.Clear();
            MapStructureXML.Load();
        }

        public static MapModel GetMapId(int MapId)
        {
            lock (MapStructureXML.Maps)
            {
                foreach (MapModel map in MapStructureXML.Maps)
                {
                    if (map.Id == MapId)
                        return map;
                }
                return (MapModel)null;
            }
        }

        public static void SetObjectives(ObjectModel obj, RoomModel room)
        {
            if (obj.UltraSync == 0)
                return;
            if (obj.UltraSync != 1 && obj.UltraSync != 3)
            {
                if (obj.UltraSync != 2 && obj.UltraSync != 4)
                    return;
                room.Bar2 = obj.Life;
                room.Default2 = room.Bar2;
            }
            else
            {
                room.Bar1 = obj.Life;
                room.Default1 = room.Bar1;
            }
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
                        for (XmlNode xmlNode = xmlDocument.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
                        {
                            if (xmlNode.Name.Equals("List"))
                            {
                                for (XmlNode A_0_1 = xmlNode.FirstChild; A_0_1 != null; A_0_1 = A_0_1.NextSibling)
                                {
                                    if (A_0_1.Name.Equals("Map"))
                                    {
                                        XmlNamedNodeMap attributes = (XmlNamedNodeMap)A_0_1.Attributes;
                                        MapModel A_1 = new MapModel()
                                        {
                                            Id = int.Parse(attributes.GetNamedItem("Id").Value),
                                            Objects = new List<ObjectModel>(),
                                            Bombs = new List<BombPosition>()
                                        };
                                        MapStructureXML.StaticMethod1(A_0_1, A_1);
                                        MapStructureXML.StaticMethod2(A_0_1, A_1);
                                        MapStructureXML.Maps.Add(A_1);
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

        
        private static void StaticMethod1(XmlNode A_0, MapModel A_1)
        {
            for (XmlNode xmlNode1 = A_0.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
            {
                if (xmlNode1.Name.Equals("BombPositions"))
                {
                    for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                    {
                        if (xmlNode2.Name.Equals("Bomb"))
                        {
                            XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                            BombPosition bombPosition = new BombPosition()
                            {
                                X = float.Parse(attributes.GetNamedItem("X").Value),
                                Y = float.Parse(attributes.GetNamedItem("Y").Value),
                                Z = float.Parse(attributes.GetNamedItem("Z").Value)
                            };
                            bombPosition.Position = new Half3(bombPosition.X, bombPosition.Y, bombPosition.Z);
                            if ((double)bombPosition.X == 0.0 && (double)bombPosition.Y == 0.0 && (double)bombPosition.Z == 0.0)
                                bombPosition.EveryWhere = true;
                            A_1.Bombs.Add(bombPosition);
                        }
                    }
                }
            }
        }

        
        private static void StaticMethod2(XmlNode A_0, MapModel A_1)
        {
            for (XmlNode xmlNode = A_0.FirstChild; xmlNode != null; xmlNode = xmlNode.NextSibling)
            {
                if (xmlNode.Name.Equals("Objects"))
                {
                    for (XmlNode A_0_1 = xmlNode.FirstChild; A_0_1 != null; A_0_1 = A_0_1.NextSibling)
                    {
                        if (A_0_1.Name.Equals("Obj"))
                        {
                            XmlNamedNodeMap attributes = (XmlNamedNodeMap)A_0_1.Attributes;
                            ObjectModel A_1_1 = new ObjectModel(bool.Parse(attributes.GetNamedItem("NeedSync").Value))
                            {
                                Id = int.Parse(attributes.GetNamedItem("Id").Value),
                                Life = int.Parse(attributes.GetNamedItem("Life").Value),
                                Animation = int.Parse(attributes.GetNamedItem("Animation").Value)
                            };
                            if (A_1_1.Life > -1)
                                A_1_1.Destroyable = true;
                            if (A_1_1.Animation > (int)byte.MaxValue)
                            {
                                if (A_1_1.Animation == 256 /*0x0100*/)
                                    A_1_1.UltraSync = 1;
                                else if (A_1_1.Animation != 257)
                                {
                                    if (A_1_1.Animation == 258)
                                        A_1_1.UltraSync = 3;
                                    else if (A_1_1.Animation == 259)
                                        A_1_1.UltraSync = 4;
                                }
                                else
                                    A_1_1.UltraSync = 2;
                                A_1_1.Animation = (int)byte.MaxValue;
                            }
                            MapStructureXML.StaticMethod3(A_0_1, A_1_1);
                            MapStructureXML.StaticMethod4(A_0_1, A_1_1);
                            A_1.Objects.Add(A_1_1);
                        }
                    }
                }
            }
        }

        
        private static void StaticMethod3(XmlNode A_0, ObjectModel A_1)
        {
            for (XmlNode xmlNode1 = A_0.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
            {
                if (xmlNode1.Name.Equals("Anims"))
                {
                    for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                    {
                        if (xmlNode2.Name.Equals("Sync"))
                        {
                            XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                            AnimModel animModel = new AnimModel()
                            {
                                Id = int.Parse(attributes.GetNamedItem("Id").Value),
                                Duration = float.Parse(attributes.GetNamedItem("Date").Value),
                                NextAnim = int.Parse(attributes.GetNamedItem("Next").Value),
                                OtherObj = int.Parse(attributes.GetNamedItem("OtherOBJ").Value),
                                OtherAnim = int.Parse(attributes.GetNamedItem("OtherANIM").Value)
                            };
                            if (animModel.Id == 0)
                                A_1.NoInstaSync = true;
                            if (animModel.Id != (int)byte.MaxValue)
                                A_1.UpdateId = 3;
                            A_1.Animations.Add(animModel);
                        }
                    }
                }
            }
        }

        
        private static void StaticMethod4(XmlNode A_0, ObjectModel A_1)
        {
            for (XmlNode xmlNode1 = A_0.FirstChild; xmlNode1 != null; xmlNode1 = xmlNode1.NextSibling)
            {
                if (xmlNode1.Name.Equals("DestroyEffects"))
                {
                    for (XmlNode xmlNode2 = xmlNode1.FirstChild; xmlNode2 != null; xmlNode2 = xmlNode2.NextSibling)
                    {
                        if (xmlNode2.Name.Equals("Effect"))
                        {
                            XmlNamedNodeMap attributes = (XmlNamedNodeMap)xmlNode2.Attributes;
                            DeffectModel deffectModel = new DeffectModel()
                            {
                                Id = int.Parse(attributes.GetNamedItem("Id").Value),
                                Life = int.Parse(attributes.GetNamedItem("Percent").Value)
                            };
                            A_1.Effects.Add(deffectModel);
                        }
                    }
                }
            }
        }
    }
}
