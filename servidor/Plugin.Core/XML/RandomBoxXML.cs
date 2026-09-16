using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;

public class RandomBoxXML
{
    public static SortedList<int, RandomBoxModel> RBoxes = new SortedList<int, RandomBoxModel>();
    public static byte[] PackedRandomBoxBuffer;
    public static int PackedRandomBoxCount;

    public const int RANDOMBOX_RECORD_SIZE = 24928;
    private const int MAX_RANDOMBOX_RECORDS = 500;
    private const int RANDOMBOX_REWARD_STRIDE = 244;
    private const int MAX_RANDOMBOX_REWARDS = 100;

    private const string FallbackDirectory = "Data/RBoxes";

    public static void Load()
    {
        SortedList<int, RandomBoxModel> Rows = DaoManagerSQL.GetRandomBoxes();
        int Count;
        lock (RBoxes)
        {
            RBoxes.Clear();
            if (Rows != null && Rows.Count > 0)
            {
                foreach (KeyValuePair<int, RandomBoxModel> Entry in Rows)
                    RBoxes.Add(Entry.Key, Entry.Value);
            }
            else
            {
                CLogger.Print($"system_random_boxes unreachable or empty, falling back to {FallbackDirectory}", LoggerType.Error);
                ParseDirectory(FallbackDirectory);
            }
            Count = RBoxes.Count;
            BuildPackedRandomBoxData();
        }
        CLogger.Print($"Plugin carregado: {Count} caixas aleatorias", LoggerType.Info);
    }

    public static void Reload()
    {
        Load();
    }

    private static void BuildPackedRandomBoxData()
    {
        // So envia ao client caixas visiveis na loja. Odds de caixas ocultas
        // (skins sem PEF / Point Up oculto) crasham o preview (Please Wait).
        HashSet<int> visibleBoxIds = new HashSet<int>();
        lock (ShopManager.ShopAllList)
        {
            foreach (GoodsItem good in ShopManager.ShopAllList)
            {
                if (good != null && good.Visibility != 4 && RBoxes.ContainsKey(good.Item.Id))
                    visibleBoxIds.Add(good.Item.Id);
            }
        }

        List<KeyValuePair<int, RandomBoxModel>> boxes = new List<KeyValuePair<int, RandomBoxModel>>();
        foreach (KeyValuePair<int, RandomBoxModel> entry in RBoxes)
        {
            if (entry.Value == null)
                continue;
            if (visibleBoxIds.Count > 0 && !visibleBoxIds.Contains(entry.Key))
                continue;

            boxes.Add(entry);
            if (boxes.Count == MAX_RANDOMBOX_RECORDS)
                break;
        }

        PackedRandomBoxCount = boxes.Count;
        if (PackedRandomBoxCount == 0)
        {
            PackedRandomBoxBuffer = null;
            return;
        }

        byte[] raw = new byte[PackedRandomBoxCount * RANDOMBOX_RECORD_SIZE];
        for (int i = 0; i < boxes.Count; i++)
        {
            int off = i * RANDOMBOX_RECORD_SIZE;
            int boxId = boxes[i].Key;
            WriteRandomBoxRecord(boxId, boxes[i].Value, raw, off);
        }

        PackedRandomBoxBuffer = ZlibUtil.Compress(raw);
        CLogger.Print(
            $"Plugin carregado: packed random boxes {PackedRandomBoxCount} recs ({raw.Length}B raw -> {PackedRandomBoxBuffer.Length}B zlib)",
            LoggerType.Info);
    }

    private static void WriteRandomBoxRecord(int boxId, RandomBoxModel box, byte[] raw, int off)
    {
        raw[off + 0] = 3;
        WriteIntLE(raw, off + 4, boxId);

        // So inclui rewards que o client consegue resolver (IsPackedGood).
        // Good dangling no RANDOMBOX_LIST → FindGoods NULL → Please Wait / AV no clique.
        List<RandomBoxItem> packedRewards = new List<RandomBoxItem>();
        if (box.Items != null)
        {
            foreach (RandomBoxItem item in box.Items)
            {
                if (item == null || item.GoodsId == 0)
                    continue;
                if (!ShopManager.IsPackedGood(item.GoodsId))
                {
                    CLogger.Print(
                        $"randombox {boxId}: skip good {item.GoodsId} (nao packed)",
                        LoggerType.Warning);
                    continue;
                }
                packedRewards.Add(item);
                if (packedRewards.Count == MAX_RANDOMBOX_REWARDS)
                    break;
            }
        }

        int rewardCount = packedRewards.Count;
        raw[off + 524] = (byte)rewardCount;

        for (int i = 0; i < rewardCount; i++)
        {
            RandomBoxItem item = packedRewards[i];
            int rewardOff = off + (i * RANDOMBOX_REWARD_STRIDE);
            raw[rewardOff + 529] = 1;
            WriteIntLE(raw, rewardOff + 532, item.GoodsId);
        }
    }

    private static void WriteIntLE(byte[] raw, int off, int value)
    {
        raw[off + 0] = (byte)value;
        raw[off + 1] = (byte)(value >> 8);
        raw[off + 2] = (byte)(value >> 16);
        raw[off + 3] = (byte)(value >> 24);
    }

    public static bool ContainsBox(int Id)
    {
        lock (RBoxes)
            return RBoxes.ContainsKey(Id);
    }

    public static RandomBoxModel GetBox(int Id)
    {
        lock (RBoxes)
        {
            RandomBoxModel Box;
            return RBoxes.TryGetValue(Id, out Box) ? Box : null;
        }
    }

    private static void ParseDirectory(string DirectoryPath)
    {
        DirectoryInfo Directory = new DirectoryInfo(Path.Combine(System.IO.Directory.GetCurrentDirectory(), DirectoryPath.Replace('/', Path.DirectorySeparatorChar)));
        if (!Directory.Exists)
        {
            CLogger.Print("Directory not found: " + DirectoryPath, LoggerType.Warning);
            return;
        }

        foreach (FileInfo File in Directory.GetFiles("*.xml"))
        {
            try
            {
                ParseFile(File.FullName, int.Parse(Path.GetFileNameWithoutExtension(File.Name)));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }

    private static void ParseFile(string FilePath, int BoxId)
    {
        XmlDocument Document = new XmlDocument();
        using (FileStream Stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            if (Stream.Length == 0L)
            {
                CLogger.Print("File is empty: " + FilePath, LoggerType.Warning);
                return;
            }

            try
            {
                Document.Load(Stream);
                for (XmlNode ListNode = Document.FirstChild; ListNode != null; ListNode = ListNode.NextSibling)
                {
                    if (!ListNode.Name.Equals("List", StringComparison.OrdinalIgnoreCase))
                        continue;

                    for (XmlNode ItemNode = ListNode.FirstChild; ItemNode != null; ItemNode = ItemNode.NextSibling)
                    {
                        if (!ItemNode.Name.Equals("Item", StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (RBoxes.ContainsKey(BoxId))
                            continue;

                        RandomBoxModel Box = new RandomBoxModel
                        {
                            ItemsCount = int.Parse(ItemNode.Attributes.GetNamedItem("Count").Value),
                            Items = new List<RandomBoxItem>()
                        };
                        ParseRewards(ItemNode, Box);
                        Box.SetTopPercent();
                        RBoxes.Add(BoxId, Box);
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }

    private static void ParseRewards(XmlNode ItemNode, RandomBoxModel Box)
    {
        for (XmlNode RewardsNode = ItemNode.FirstChild; RewardsNode != null; RewardsNode = RewardsNode.NextSibling)
        {
            if (!RewardsNode.Name.Equals("Rewards", StringComparison.OrdinalIgnoreCase))
                continue;

            for (XmlNode GoodNode = RewardsNode.FirstChild; GoodNode != null; GoodNode = GoodNode.NextSibling)
            {
                if (!GoodNode.Name.Equals("Good", StringComparison.OrdinalIgnoreCase))
                    continue;

                XmlNamedNodeMap Attributes = GoodNode.Attributes;
                Box.Items.Add(new RandomBoxItem
                {
                    Index = int.Parse(Attributes.GetNamedItem("Index").Value),
                    GoodsId = int.Parse(Attributes.GetNamedItem("Id").Value),
                    Percent = int.Parse(Attributes.GetNamedItem("Percent").Value),
                    Special = bool.Parse(Attributes.GetNamedItem("Special").Value)
                });
            }
        }
    }
}
