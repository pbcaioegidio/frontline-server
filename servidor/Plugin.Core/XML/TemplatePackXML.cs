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
    public class TemplatePackXML
    {
        private static readonly object SyncRoot = new object();

        public static List<ItemsModel> Basics = new List<ItemsModel>();
        public static List<ItemsModel> Awards = new List<ItemsModel>();
        public static List<PCCafeModel> Cafes = new List<PCCafeModel>();

        public static void Load()
        {
            lock (SyncRoot)
            {
                List<ItemsModel> LoadedBasics = DaoManagerSQL.GetTemplateItems("basic", ItemEquipType.Permanent, true);
                if (LoadedBasics == null || LoadedBasics.Count == 0)
                {
                    CLogger.Print("Basic Templates: sem dados no banco, falling back para Data/Temps", LoggerType.Warning);
                    LoadedBasics = LoadBasics("Data/Temps/Basic.xml");
                }

                List<ItemsModel> LoadedAwards = DaoManagerSQL.GetTemplateItems("award", ItemEquipType.Durable, false);
                if (LoadedAwards == null || LoadedAwards.Count == 0)
                {
                    CLogger.Print("Award Templates: sem dados no banco, falling back para Data/Temps", LoggerType.Warning);
                    LoadedAwards = LoadAwards("Data/Temps/Award.xml");
                }

                List<PCCafeModel> LoadedCafes = DaoManagerSQL.GetPCCafes();
                if (LoadedCafes == null || LoadedCafes.Count == 0)
                {
                    CLogger.Print("PC Cafes: sem dados no banco, falling back para Data/Temps", LoggerType.Warning);
                    LoadedCafes = LoadCafes("Data/Temps/CafePC.xml");
                }

                Basics = LoadedBasics;
                Cafes = LoadedCafes;
                Awards = LoadedAwards;

                CLogger.Print($"Plugin carregado: {LoadedBasics.Count} Basic Templates", LoggerType.Info);
                CLogger.Print($"Plugin carregado: {LoadedCafes.Count} PC Cafes", LoggerType.Info);
                CLogger.Print($"Plugin carregado: {LoadedAwards.Count} Award Templates", LoggerType.Info);
            }
        }

        public static void Reload()
        {
            Load();
        }

        public static PCCafeModel GetPCCafe(CafeEnum Type)
        {
            List<PCCafeModel> Snapshot = Cafes;
            foreach (PCCafeModel Cafe in Snapshot)
            {
                if (Cafe.Type == Type)
                    return Cafe;
            }
            return null;
        }

        public static List<ItemsModel> GetPCCafeRewards(CafeEnum Type)
        {
            PCCafeModel Cafe = GetPCCafe(Type);
            if (Cafe != null)
            {
                lock (Cafe.Rewards)
                {
                    List<ItemsModel> Rewards;
                    if (Cafe.Rewards.TryGetValue(Type, out Rewards))
                        return Rewards;
                }
            }
            return new List<ItemsModel>();
        }

        private static XmlDocument OpenDocument(string Path)
        {
            if (!File.Exists(Path))
            {
                CLogger.Print("File not found: " + Path, LoggerType.Warning);
                return null;
            }

            try
            {
                using (FileStream Stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (Stream.Length == 0L)
                    {
                        CLogger.Print("File is empty: " + Path, LoggerType.Warning);
                        return null;
                    }

                    XmlDocument Document = new XmlDocument();
                    Document.Load(Stream);
                    return Document;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return null;
            }
        }

        private static List<ItemsModel> LoadBasics(string Path)
        {
            List<ItemsModel> Result = new List<ItemsModel>();
            XmlDocument Document = OpenDocument(Path);
            if (Document == null)
                return Result;

            for (XmlNode Root = Document.FirstChild; Root != null; Root = Root.NextSibling)
            {
                if (!Root.Name.Equals("List"))
                    continue;

                for (XmlNode Node = Root.FirstChild; Node != null; Node = Node.NextSibling)
                {
                    if (!Node.Name.Equals("Item"))
                        continue;

                    XmlAttributeCollection Attributes = Node.Attributes;
                    int ItemId = int.Parse(Attributes["Id"].Value);
                    Result.Add(new ItemsModel(ItemId)
                    {
                        ObjectId = ComDiv.ValidateStockId(ItemId),
                        Name = Attributes["Name"].Value,
                        Count = 1,
                        Equip = ItemEquipType.Permanent
                    });
                }
            }
            return Result;
        }

        private static List<ItemsModel> LoadAwards(string Path)
        {
            List<ItemsModel> Result = new List<ItemsModel>();
            XmlDocument Document = OpenDocument(Path);
            if (Document == null)
                return Result;

            for (XmlNode Root = Document.FirstChild; Root != null; Root = Root.NextSibling)
            {
                if (!Root.Name.Equals("List"))
                    continue;

                for (XmlNode Node = Root.FirstChild; Node != null; Node = Node.NextSibling)
                {
                    if (!Node.Name.Equals("Item"))
                        continue;

                    XmlAttributeCollection Attributes = Node.Attributes;
                    Result.Add(new ItemsModel(int.Parse(Attributes["Id"].Value))
                    {
                        Name = Attributes["Name"].Value,
                        Count = uint.Parse(Attributes["Count"].Value),
                        Equip = ItemEquipType.Durable
                    });
                }
            }
            return Result;
        }

        private static List<PCCafeModel> LoadCafes(string Path)
        {
            List<PCCafeModel> Result = new List<PCCafeModel>();
            XmlDocument Document = OpenDocument(Path);
            if (Document == null)
                return Result;

            for (XmlNode Root = Document.FirstChild; Root != null; Root = Root.NextSibling)
            {
                if (!Root.Name.Equals("List"))
                    continue;

                for (XmlNode Node = Root.FirstChild; Node != null; Node = Node.NextSibling)
                {
                    if (!Node.Name.Equals("Cafe"))
                        continue;

                    XmlAttributeCollection Attributes = Node.Attributes;
                    PCCafeModel Cafe = new PCCafeModel(ComDiv.ParseEnum<CafeEnum>(Attributes["Type"].Value))
                    {
                        ExpUp = int.Parse(Attributes["ExpUp"].Value),
                        PointUp = int.Parse(Attributes["PointUp"].Value),
                        Rewards = new SortedList<CafeEnum, List<ItemsModel>>()
                    };

                    LoadCafeRewards(Node, Cafe);
                    Result.Add(Cafe);
                }
            }
            return Result;
        }

        private static void LoadCafeRewards(XmlNode CafeNode, PCCafeModel Cafe)
        {
            for (XmlNode Group = CafeNode.FirstChild; Group != null; Group = Group.NextSibling)
            {
                if (!Group.Name.Equals("Rewards"))
                    continue;

                for (XmlNode Node = Group.FirstChild; Node != null; Node = Node.NextSibling)
                {
                    if (!Node.Name.Equals("Item"))
                        continue;

                    XmlAttributeCollection Attributes = Node.Attributes;
                    int ItemId = int.Parse(Attributes["Id"].Value);
                    ItemsModel Item = new ItemsModel(ItemId)
                    {
                        ObjectId = ComDiv.ValidateStockId(ItemId),
                        Name = Attributes["Name"].Value,
                        Count = 1,
                        Equip = ItemEquipType.CafePC
                    };

                    if (Cafe.Rewards.ContainsKey(Cafe.Type))
                        Cafe.Rewards[Cafe.Type].Add(Item);
                    else
                        Cafe.Rewards.Add(Cafe.Type, new List<ItemsModel> { Item });
                }
            }
        }
    }
}
