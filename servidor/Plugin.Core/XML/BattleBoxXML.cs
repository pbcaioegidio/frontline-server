using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.SQL;

namespace Plugin.Core.XML
{
    public class BattleBoxXML
    {
        public static List<BattleBoxModel> BBoxes = new List<BattleBoxModel>();
        public static List<ShopData> ShopDataBattleBoxes = new List<ShopData>();
        public static int TotalBoxes;

        private const string FallbackDirectory = "Data/BBoxes";

        public static void Load()
        {
            List<BattleBoxModel> Rows = DaoManagerSQL.GetBattleBoxes();
            int Count;
            lock (BBoxes)
            {
                BBoxes.Clear();
                if (Rows != null && Rows.Count > 0)
                {
                    BBoxes.AddRange(Rows);
                }
                else
                {
                    CLogger.Print($"system_battle_boxes unreachable or empty, falling back to {FallbackDirectory}", LoggerType.Error);
                    ParseDirectory(FallbackDirectory);
                }
                Count = BBoxes.Count;
                InitializeShopData();
            }
            CLogger.Print($"Plugin carregado: {Count} Battle Boxes", LoggerType.Info);
        }

        public static void Reload()
        {
            Load();
        }

        private static void ParseDirectory(string DirectoryPath)
        {
            string Path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Data", "BBoxes");
            DirectoryInfo Info = new DirectoryInfo(Path);
            if (!Info.Exists)
            {
                CLogger.Print("Directory not found: " + DirectoryPath, LoggerType.Warning);
                return;
            }

            foreach (FileInfo File in Info.GetFiles("*.xml"))
            {
                try
                {
                    ParseFile(File.FullName, int.Parse(System.IO.Path.GetFileNameWithoutExtension(File.Name)));
                }
                catch (Exception ex)
                {
                    CLogger.Print(ex.Message, LoggerType.Error, ex);
                }
            }
        }

        private static void ParseFile(string FilePath, int CouponId)
        {
            XmlDocument Document = new XmlDocument();
            using (FileStream Stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (Stream.Length == 0)
                {
                    CLogger.Print("File is empty: " + FilePath, LoggerType.Warning);
                    return;
                }
                Document.Load(Stream);
            }

            foreach (XmlNode Node in Document.SelectNodes("//List/BattleBox"))
            {
                BattleBoxModel Box = new BattleBoxModel
                {
                    CouponId = CouponId,
                    RequireTags = int.Parse(Node.Attributes["RequireTags"].Value),
                    Items = new List<BattleBoxItem>()
                };

                foreach (XmlNode RewardNode in Node.SelectNodes("Rewards/Good"))
                {
                    Box.Items.Add(new BattleBoxItem
                    {
                        GoodsId = int.Parse(RewardNode.Attributes["Id"].Value),
                        Percent = int.Parse(RewardNode.Attributes["Percent"].Value)
                    });
                }

                Box.InitItemPercentages();
                BBoxes.Add(Box);
            }
        }

        private static void InitializeShopData()
        {
            List<ShopData> Pages = new List<ShopData>();

            List<BattleBoxModel> boxesCopy = new List<BattleBoxModel>(BBoxes);
            TotalBoxes = boxesCopy.Count;
            int pages = (int)Math.Ceiling(boxesCopy.Count / 100.0);

            for (int i = 0; i < pages; i++)
            {
                int itemsCount = 0;
                byte[] buffer = SerializeBattleBoxes(100, i, ref itemsCount, boxesCopy);

                ShopData shopData = new ShopData
                {
                    Buffer = buffer,
                    ItemsCount = itemsCount,
                    Offset = i * 100
                };

                Pages.Add(shopData);
            }

            ShopDataBattleBoxes = Pages;
        }

        private static byte[] SerializeBattleBoxes(int pageSize, int pageIndex, ref int itemsCount, List<BattleBoxModel> boxes)
        {
            itemsCount = 0;
            using (SyncServerPacket packet = new SyncServerPacket())
            {
                for (int i = pageIndex * pageSize; i < boxes.Count; i++)
                {
                    WriteBattleBoxData(boxes[i], packet);
                    itemsCount++;
                    if (itemsCount == pageSize)
                        break;
                }
                return packet.ToArray();
            }
        }

        private static void WriteBattleBoxData(BattleBoxModel box, SyncServerPacket packet)
        {
            packet.WriteD(box.CouponId);
            packet.WriteH((ushort)box.RequireTags);
            packet.WriteH(0);
            packet.WriteH(0);
            packet.WriteC(0);
        }

        public static BattleBoxModel GetBattleBox(int battleBoxId)
        {
            if (battleBoxId == 0) return null;

            lock (BBoxes)
            {
                foreach (var box in BBoxes)
                {
                    if (box.CouponId == battleBoxId)
                        return box;
                }
            }
            return null;
        }
    }
}
