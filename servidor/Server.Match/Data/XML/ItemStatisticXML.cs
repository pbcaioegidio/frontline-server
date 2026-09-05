using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Server.Match.Data.XML
{
    public class ItemStatisticXML
    {
        private static readonly object SyncRoot = new object();

        public static List<ItemsStatistic> Stats = new List<ItemsStatistic>();

        public static void Load()
        {
            lock (SyncRoot)
            {
                List<ItemsStatistic> Loaded = DaoManagerSQL.GetItemStatistics();
                if (Loaded == null || Loaded.Count == 0)
                {
                    CLogger.Print("Item Statistics: sem dados no banco, falling back para Data/Match", LoggerType.Warning);
                    Loaded = ParseFile("Data/Match/ItemStatistics.xml");
                }

                Stats = Loaded;
                CLogger.Print($"Plugin carregado: {Loaded.Count} estatisticas de itens", LoggerType.Info);
            }
        }

        public static void Reload()
        {
            Load();
        }

        public static ItemsStatistic GetItemStats(int ItemId)
        {
            List<ItemsStatistic> Snapshot = Stats;
            foreach (ItemsStatistic Stat in Snapshot)
            {
                if (Stat.Id == ItemId)
                    return Stat;
            }
            return null;
        }

        private static List<ItemsStatistic> ParseFile(string Path)
        {
            List<ItemsStatistic> Result = new List<ItemsStatistic>();
            if (!File.Exists(Path))
            {
                CLogger.Print("File not found: " + Path, LoggerType.Warning);
                return Result;
            }

            try
            {
                XmlDocument Document = new XmlDocument();
                using (FileStream Stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (Stream.Length == 0L)
                    {
                        CLogger.Print("File is empty: " + Path, LoggerType.Warning);
                        return Result;
                    }
                    Document.Load(Stream);
                }

                for (XmlNode Root = Document.FirstChild; Root != null; Root = Root.NextSibling)
                {
                    if (!Root.Name.Equals("List"))
                        continue;

                    for (XmlNode Node = Root.FirstChild; Node != null; Node = Node.NextSibling)
                    {
                        if (!Node.Name.Equals("Statistic"))
                            continue;

                        XmlAttributeCollection Attributes = Node.Attributes;
                        Result.Add(new ItemsStatistic
                        {
                            Id = int.Parse(Attributes["Id"].Value),
                            Name = Attributes["Name"].Value,
                            BulletLoaded = int.Parse(Attributes["LoadedBullet"].Value),
                            BulletTotal = int.Parse(Attributes["TotalBullet"].Value),
                            Damage = int.Parse(Attributes["Damage"].Value),
                            FireDelay = float.Parse(Attributes["FireDelay"].Value, CultureInfo.InvariantCulture),
                            HelmetPenetrate = int.Parse(Attributes["HelmetPenetrate"].Value),
                            Range = float.Parse(Attributes["Range"].Value, CultureInfo.InvariantCulture)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return Result;
        }
    }
}
