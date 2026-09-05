using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Plugin.Core.XML
{
    // MissionAwardXML: mission-level completion reward table (Data/Cards/MissionAwards.xml).
    // Loaded at startup (Unico.Execute Program.cs), reloaded via supervisor/FormConfig. GetAward(id)
    // is consumed in AllUtils.ProcessFullMissionCompletion on full mission completion, granting
    // MasterMedal + Exp + Gold.
    // Note: the XML attribute "Point" is the lobby point currency, which this server stores in
    // player.Gold (cf. PROTOCOL_SHOP_PLUS_POINT_ACK(player.Gold, ...); Account has no separate Point
    // field). The granted Gold/Exp/MasterMedal persist via the shared match-end account save (RoomModel
    // compares pre/post gold+exp and writes the delta), so the in-memory bump here is durable.
    public class MissionAwardXML
    {
        private const string FilePath = "Data/Cards/MissionAwards.xml";

        private static readonly List<MissionAwards> Awards = new List<MissionAwards>();

        public static void Load()
        {
            List<MissionAwards> Rows = DaoManagerSQL.GetMissionAwards();
            int Count;
            lock (Awards)
            {
                Awards.Clear();
                if (Rows != null && Rows.Count > 0)
                {
                    Awards.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_mission_awards unreachable or empty, falling back to {FilePath}", LoggerType.Error);
                    if (!File.Exists(FilePath))
                        CLogger.Print("File not found: " + FilePath, LoggerType.Warning);
                    else
                        ParseFile(FilePath);
                }
                Count = Awards.Count;
            }
            CLogger.Print($"Plugin carregado: {Count} Mission Awards", LoggerType.Info);
        }

        public static void Reload()
        {
            Load();
        }

        public static MissionAwards GetAward(int missionId)
        {
            lock (Awards)
            {
                foreach (MissionAwards award in Awards)
                {
                    if (award.Id == missionId)
                        return award;
                }
                return null;
            }
        }

        private static void ParseFile(string path)
        {
            var xmlDocument = new XmlDocument();
            using (var inStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (inStream.Length == 0L)
                {
                    CLogger.Print("File is empty: " + path, LoggerType.Warning);
                    return;
                }

                try
                {
                    xmlDocument.Load(inStream);
                    for (XmlNode listNode = xmlDocument.FirstChild; listNode != null; listNode = listNode.NextSibling)
                    {
                        if (!listNode.Name.Equals("List"))
                            continue;

                        for (XmlNode missionNode = listNode.FirstChild; missionNode != null; missionNode = missionNode.NextSibling)
                        {
                            if (!missionNode.Name.Equals("Mission"))
                                continue;

                            string id = Attr(missionNode, "Id");
                            string masterMedal = Attr(missionNode, "MasterMedal");
                            string exp = Attr(missionNode, "Exp");
                            string point = Attr(missionNode, "Point");

                            if (id == null || masterMedal == null || exp == null || point == null)
                            {
                                CLogger.Print("MissionAwards: skipping malformed row (missing attribute)", LoggerType.Warning);
                                continue;
                            }

                            // XML "Point" -> player.Gold (lobby point currency); see class note.
                            lock (Awards)
                                Awards.Add(new MissionAwards(int.Parse(id), int.Parse(masterMedal), int.Parse(exp), int.Parse(point)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    CLogger.Print(ex.Message, LoggerType.Error, ex);
                }
            }
        }

        private static string Attr(XmlNode node, string name)
        {
            return node.Attributes?.GetNamedItem(name)?.Value;
        }
    }
}
