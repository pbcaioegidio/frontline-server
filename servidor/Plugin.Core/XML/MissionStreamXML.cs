using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Plugin.Core.XML
{
    public class MissionStreamEntry
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RewardId { get; set; }
        public int RewardCount { get; set; }
        public byte[] ObjectivesData { get; set; }
    }

    /// <summary>
    /// Monta o payload de PROTOCOL_BASE_MISSION_CARD_INFO_STREAM (2520/2521).
    ///
    /// Layout confirmado no cliente 122 (MissionCardRawDataProcessor__ParseMissionCardInfoStreamAck
    /// @0xEC3BA4, decoders MissionCard_DecodeObjRecord23 @0xC56F79 e MissionCard_DecodeRewardChunk52
    /// @0xC56E84, re-serializado por MissionCardRawDataProcessor__LoadFromStream @0xCADC19):
    ///
    ///   pacote: [u16 total][u8 count] { missao } * count [u32][u32]
    ///   missao: [u8 id][u8 nameLen][nome UTF-16][u8 descLen][desc UTF-16][920 tasks][572 rewards]
    ///   task  (23B): u16 type (so 1/2/3), u8 @2 @3 @4 @5 @6, u32 @7 @11 @15 @19 (DESALINHADOS)
    ///   reward(52B): u32 d0, u32 d1 (exp), u32 d2 (medals), depois 5 pares (u32 goodId, u32 count)
    ///
    /// O bloco de rewards e 11 x 52: registros 0..9 sao as cartas, o registro 10 e a recompensa de
    /// missao completa (d1 = FinalExp, par 0 = RewardId/RewardCount). d0 ainda nao tem nome
    /// confirmado; e exposto como Field0.
    /// </summary>
    public static class MissionStreamXML
    {
        private const int TaskRecordSize = 23;
        private const int CardsPerMission = 10;
        private const int TasksPerCard = 4;
        private const int TasksSectionSize = CardsPerMission * TasksPerCard * TaskRecordSize;

        private const int RewardRecordSize = 52;
        private const int RewardItemSlots = 5;
        private const int RewardRecordCount = CardsPerMission + 1;
        private const int RewardSectionSize = RewardRecordCount * RewardRecordSize;

        private const int ObjectivesDataSize = TasksSectionSize + RewardSectionSize;

        // O .hex guarda o bloco de rewards sem o ultimo registro completo: os 12 primeiros bytes do
        // registro 10 estao no arquivo e RewardId/RewardCount vem do cabecalho.
        private const int HexBlobSize = TasksSectionSize + RewardSectionSize - 40;

        private static readonly List<MissionStreamEntry> Missions = new List<MissionStreamEntry>();
        private static bool Loaded;

        public static int CardCount => CardsPerMission;
        public static int TaskCount => TasksPerCard;
        public static int RewardRecords => RewardRecordCount;
        public static int RewardSlots => RewardItemSlots;

        public static byte[] CreateObjectivesBuffer()
        {
            return new byte[ObjectivesDataSize];
        }

        public static void SetTask(byte[] buffer, int cardIdx, int taskIdx, int reqType, int taskType,
            int limitCount, int weaponSeries, int flag6, uint mapId, uint extra1, uint extra2, uint extra3)
        {
            if (buffer == null || cardIdx < 0 || cardIdx >= CardsPerMission || taskIdx < 0 || taskIdx >= TasksPerCard)
                return;

            int offset = (cardIdx * TasksPerCard + taskIdx) * TaskRecordSize;
            Buffer.BlockCopy(BitConverter.GetBytes((ushort)reqType), 0, buffer, offset, 2);
            buffer[offset + 2] = (byte)(taskType & 0xFF);
            buffer[offset + 3] = (byte)((taskType >> 8) & 0xFF);
            buffer[offset + 4] = (byte)limitCount;
            buffer[offset + 5] = (byte)weaponSeries;
            buffer[offset + 6] = (byte)flag6;
            Buffer.BlockCopy(BitConverter.GetBytes(mapId), 0, buffer, offset + 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(extra1), 0, buffer, offset + 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(extra2), 0, buffer, offset + 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(extra3), 0, buffer, offset + 19, 4);
        }

        public static void SetReward(byte[] buffer, int recordIdx, uint[] values)
        {
            if (buffer == null || values == null || recordIdx < 0 || recordIdx >= RewardRecordCount)
                return;

            int offset = TasksSectionSize + recordIdx * RewardRecordSize;
            int count = Math.Min(values.Length, 3 + RewardItemSlots * 2);
            for (int i = 0; i < count; i++)
                Buffer.BlockCopy(BitConverter.GetBytes(values[i]), 0, buffer, offset + i * 4, 4);
        }

        public static IReadOnlyList<MissionStreamEntry> GetAllMissions()
        {
            lock (Missions)
            {
                EnsureLoaded();
                return new List<MissionStreamEntry>(Missions);
            }
        }

        public static void Reload()
        {
            lock (Missions)
            {
                Loaded = false;
                EnsureLoaded();
            }
        }

        public static void Load()
        {
            Reload();
        }

        private static void EnsureLoaded()
        {
            if (Loaded)
                return;

            if (LoadFromDatabase())
            {
                Loaded = true;
                CLogger.Print($"Plugin carregado: {Missions.Count} streams de missao", LoggerType.Info);
                return;
            }

            CLogger.Print("Mission Streams: sem dados no banco, falling back para Data/Missions", LoggerType.Warning);
            LoadFromHex("Data/Missions/Basic");

            string questActionsPath = "Data/Missions/QuestActions.xml";
            if (File.Exists(questActionsPath))
            {
                try
                {
                    ApplyXmlOverrides(questActionsPath, "Data/Missions/MissionRewards.xml");
                }
                catch (Exception ex)
                {
                    CLogger.Print($"Quest Actions XML error: {ex.Message}", LoggerType.Error, ex);
                }
            }

            string cardsPath = "Data/Missions/MissionCards.xml";
            if (File.Exists(cardsPath))
            {
                try
                {
                    ApplyXmlOverrides(cardsPath, "Data/Missions/MissionRewards.xml", hexOnlyMissing: false, respectUseHexFlag: true);
                }
                catch (Exception ex)
                {
                    CLogger.Print($"Mission Stream XML error: {ex.Message}", LoggerType.Error, ex);
                }
            }

            Loaded = true;
            CLogger.Print($"Plugin carregado: {Missions.Count} streams de missao", LoggerType.Info);
        }

        private static void ApplyXmlOverrides(string cardsPath, string rewardsPath, bool hexOnlyMissing = false, bool respectUseHexFlag = false)
        {
            var rewardsByMission = LoadRewards(rewardsPath);

            XmlDocument doc = new XmlDocument();
            using (var stream = new FileStream(cardsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                doc.Load(stream);

            XmlNode root = doc.DocumentElement;
            if (root == null)
                return;

            for (XmlNode missionNode = root.FirstChild; missionNode != null; missionNode = missionNode.NextSibling)
            {
                if (!missionNode.Name.Equals("Mission"))
                    continue;

                XmlAttributeCollection attrs = missionNode.Attributes;
                if (attrs?["Id"] == null)
                    continue;

                int missionId = int.Parse(attrs["Id"].Value);

                MissionStreamEntry previous = Missions.Find(m => m.Id == missionId);
                if (hexOnlyMissing && previous != null)
                    continue;

                if (respectUseHexFlag && previous != null)
                {
                    string useHex = attrs["UseHex"]?.Value;
                    if (useHex == null || useHex.Equals("true", StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                string name = attrs["Name"]?.Value ?? $"Mission {missionId}";
                string description = attrs["Description"]?.Value ?? string.Empty;
                MissionRewardInfo rewardInfo = rewardsByMission.ContainsKey(missionId)
                    ? rewardsByMission[missionId]
                    : new MissionRewardInfo();

                if (previous != null)
                {
                    if (rewardInfo.RewardId == 0)
                        rewardInfo.RewardId = previous.RewardId;
                    if (rewardInfo.RewardCount == 0)
                        rewardInfo.RewardCount = previous.RewardCount;
                }

                if (attrs["RewardId"] != null)
                    rewardInfo.RewardId = int.Parse(attrs["RewardId"].Value);
                if (attrs["RewardCount"] != null)
                    rewardInfo.RewardCount = int.Parse(attrs["RewardCount"].Value);

                if (rewardInfo.HasUnmappedAttrs)
                {
                    CLogger.Print(
                        $"MissionRewards mission {missionId}: Gold/TimeLimit nao tem slot no registro de 52B do cliente, ignorados",
                        LoggerType.Warning);
                }

                byte[] objectives = BuildObjectivesFromXml(missionNode, rewardInfo, previous?.ObjectivesData);

                MissionStreamEntry entry = new MissionStreamEntry
                {
                    Id = missionId,
                    Name = name,
                    Description = description,
                    RewardId = rewardInfo.RewardId,
                    RewardCount = rewardInfo.RewardCount,
                    ObjectivesData = objectives
                };

                int existingIdx = Missions.FindIndex(m => m.Id == missionId);
                if (existingIdx >= 0)
                    Missions[existingIdx] = entry;
                else
                    Missions.Add(entry);
            }

            Missions.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        private static byte[] BuildObjectivesFromXml(XmlNode missionNode, MissionRewardInfo rewardInfo, byte[] hexFallback = null)
        {
            using (SyncServerPacket packet = new SyncServerPacket())
            {
                var cards = new List<XmlNode>();
                foreach (XmlNode child in missionNode.ChildNodes)
                {
                    if (child.Name.Equals("Cards"))
                    {
                        foreach (XmlNode cardNode in child.ChildNodes)
                        {
                            if (cardNode.Name.Equals("Card"))
                                cards.Add(cardNode);
                        }
                    }
                }

                cards.Sort((a, b) =>
                {
                    int idA = int.Parse(a.Attributes?["Id"]?.Value ?? "0");
                    int idB = int.Parse(b.Attributes?["Id"]?.Value ?? "0");
                    return idA.CompareTo(idB);
                });

                for (int cardIdx = 0; cardIdx < CardsPerMission; cardIdx++)
                {
                    XmlNode cardNode = cardIdx < cards.Count ? cards[cardIdx] : null;
                    var tasks = new List<XmlNode>();

                    if (cardNode != null)
                    {
                        foreach (XmlNode taskParent in cardNode.ChildNodes)
                        {
                            if (taskParent.Name.Equals("Tasks"))
                            {
                                foreach (XmlNode taskNode in taskParent.ChildNodes)
                                {
                                    if (taskNode.Name.Equals("Task"))
                                        tasks.Add(taskNode);
                                }
                            }
                        }
                        tasks.Sort((a, b) =>
                        {
                            int idA = int.Parse(a.Attributes?["Id"]?.Value ?? "0");
                            int idB = int.Parse(b.Attributes?["Id"]?.Value ?? "0");
                            return idA.CompareTo(idB);
                        });
                    }

                    for (int taskIdx = 0; taskIdx < TasksPerCard; taskIdx++)
                    {
                        XmlNode taskNode = taskIdx < tasks.Count ? tasks[taskIdx] : null;
                        WriteTaskRecord(packet, taskNode);
                    }
                }

                for (int cardIdx = 0; cardIdx < CardsPerMission; cardIdx++)
                {
                    RewardRecord record = rewardInfo.GetCardReward(cardIdx);
                    RewardRecord fallback = ReadRewardRecord(hexFallback, cardIdx);
                    WriteRewardRecord(packet, Merge(record, fallback));
                }

                RewardRecord final = Merge(rewardInfo.Final, ReadRewardRecord(hexFallback, CardsPerMission));
                if (rewardInfo.RewardId != 0)
                {
                    final.Items[0].GoodId = rewardInfo.RewardId;
                    final.Items[0].Count = rewardInfo.RewardCount;
                }
                WriteRewardRecord(packet, final);

                return packet.ToArray();
            }
        }

        private static RewardRecord Merge(RewardRecord primary, RewardRecord fallback)
        {
            if (fallback == null)
                return primary;

            if (primary.Field0 == 0) primary.Field0 = fallback.Field0;
            if (primary.Exp == 0) primary.Exp = fallback.Exp;
            if (primary.Medals == 0) primary.Medals = fallback.Medals;

            for (int i = 0; i < RewardItemSlots; i++)
            {
                if (primary.Items[i].GoodId == 0)
                {
                    primary.Items[i].GoodId = fallback.Items[i].GoodId;
                    primary.Items[i].Count = fallback.Items[i].Count;
                }
            }
            return primary;
        }

        private static void WriteRewardRecord(SyncServerPacket packet, RewardRecord record)
        {
            packet.WriteD(record.Field0);
            packet.WriteD(record.Exp);
            packet.WriteD(record.Medals);
            for (int i = 0; i < RewardItemSlots; i++)
            {
                packet.WriteD(record.Items[i].GoodId);
                packet.WriteD(record.Items[i].Count);
            }
        }

        private static RewardRecord ReadRewardRecord(byte[] objectives, int recordIdx)
        {
            if (objectives == null || recordIdx < 0 || recordIdx >= RewardRecordCount)
                return null;

            int offset = TasksSectionSize + recordIdx * RewardRecordSize;
            if (offset + RewardRecordSize > objectives.Length)
                return null;

            RewardRecord record = new RewardRecord
            {
                Field0 = BitConverter.ToInt32(objectives, offset),
                Exp = BitConverter.ToInt32(objectives, offset + 4),
                Medals = BitConverter.ToInt32(objectives, offset + 8)
            };

            for (int i = 0; i < RewardItemSlots; i++)
            {
                record.Items[i].GoodId = BitConverter.ToInt32(objectives, offset + 12 + i * 8);
                record.Items[i].Count = BitConverter.ToInt32(objectives, offset + 16 + i * 8);
            }
            return record;
        }

        private static void WriteTaskRecord(SyncServerPacket packet, XmlNode taskNode)
        {
            ushort reqType = 2;
            ushort taskType = 15;
            byte limit = 1;
            byte weaponSeries = 0;
            uint mapId = 0;

            if (taskNode?.Attributes != null)
            {
                if (taskNode.Attributes["ReqType"] != null)
                    reqType = ushort.Parse(taskNode.Attributes["ReqType"].Value);
                if (taskNode.Attributes["TaskType"] != null)
                    taskType = ushort.Parse(taskNode.Attributes["TaskType"].Value);
                if (taskNode.Attributes["LimitCount"] != null)
                    limit = byte.Parse(taskNode.Attributes["LimitCount"].Value);
                if (taskNode.Attributes["WeaponSeries"] != null)
                    weaponSeries = byte.Parse(taskNode.Attributes["WeaponSeries"].Value);
                if (taskNode.Attributes["MapId"] != null)
                    mapId = uint.Parse(taskNode.Attributes["MapId"].Value);
            }

            packet.WriteH(reqType);
            packet.WriteC((byte)(taskType & 0xFF));
            packet.WriteC((byte)(taskType >> 8));
            packet.WriteC(limit);
            packet.WriteC(weaponSeries);
            packet.WriteC(0);
            packet.WriteD((int)mapId);
            packet.WriteB(new byte[12]);
        }

        private static Dictionary<int, MissionRewardInfo> LoadRewards(string rewardsPath)
        {
            var result = new Dictionary<int, MissionRewardInfo>();
            if (!File.Exists(rewardsPath))
                return result;

            XmlDocument doc = new XmlDocument();
            using (var stream = new FileStream(rewardsPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                doc.Load(stream);

            XmlNode root = doc.DocumentElement;
            if (root == null)
                return result;

            for (XmlNode missionNode = root.FirstChild; missionNode != null; missionNode = missionNode.NextSibling)
            {
                if (!missionNode.Name.Equals("Mission"))
                    continue;

                XmlAttributeCollection attrs = missionNode.Attributes;
                if (attrs?["Id"] == null)
                    continue;

                int missionId = int.Parse(attrs["Id"].Value);
                var info = new MissionRewardInfo
                {
                    RewardId = ParseIntAttr(attrs, "GoodId", 0),
                    RewardCount = ParseIntAttr(attrs, "RewardCount", 0)
                };

                info.Final.Field0 = ParseIntAttr(attrs, "Field0", 0);
                info.Final.Exp = ParseIntAttr(attrs, "FinalExp", 0);
                info.Final.Medals = ParseIntAttr(attrs, "MasterMedal", 0);

                if (attrs["RewardId"] != null)
                    info.RewardId = int.Parse(attrs["RewardId"].Value);

                bool warnedUnmapped = false;
                foreach (XmlNode child in missionNode.ChildNodes)
                {
                    if (!child.Name.Equals("Rewards"))
                        continue;

                    foreach (XmlNode rewardNode in child.ChildNodes)
                    {
                        if (!rewardNode.Name.Equals("Reward"))
                            continue;

                        XmlAttributeCollection rAttrs = rewardNode.Attributes;
                        if (rAttrs?["CardId"] == null)
                            continue;

                        if (!warnedUnmapped && (rAttrs["Gold"] != null || rAttrs["TimeLimit"] != null))
                        {
                            info.HasUnmappedAttrs = true;
                            warnedUnmapped = true;
                        }

                        int cardId = int.Parse(rAttrs["CardId"].Value);
                        RewardRecord record = new RewardRecord
                        {
                            Field0 = ParseIntAttr(rAttrs, "Field0", 0),
                            Exp = ParseIntAttr(rAttrs, "Exp", 0),
                            Medals = ParseIntAttr(rAttrs, "Medals", 0)
                        };

                        int slot = 0;
                        int legacyGoodId = ParseIntAttr(rAttrs, "GoodId", 0);
                        if (legacyGoodId != 0)
                        {
                            record.Items[slot].GoodId = legacyGoodId;
                            record.Items[slot].Count = ParseIntAttr(rAttrs, "Count", 1);
                            slot++;
                        }

                        foreach (XmlNode itemNode in rewardNode.ChildNodes)
                        {
                            if (!itemNode.Name.Equals("Item") || slot >= RewardItemSlots)
                                continue;

                            record.Items[slot].GoodId = ParseIntAttr(itemNode.Attributes, "GoodId", 0);
                            record.Items[slot].Count = ParseIntAttr(itemNode.Attributes, "Count", 1);
                            slot++;
                        }

                        info.CardRewards[cardId] = record;
                    }
                }

                result[missionId] = info;
            }

            return result;
        }

        private static int ParseIntAttr(XmlAttributeCollection attrs, string name, int defaultValue)
        {
            return attrs?[name] != null ? int.Parse(attrs[name].Value) : defaultValue;
        }

        private static bool LoadFromDatabase()
        {
            try
            {
                List<MissionStreamEntry> entries = DaoManagerSQL.GetMissionStreams();
                if (entries == null || entries.Count == 0)
                    return false;

                Missions.Clear();
                Missions.AddRange(entries);
                Missions.Sort((a, b) => a.Id.CompareTo(b.Id));
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Mission Streams SQL error: {ex.Message}", LoggerType.Error, ex);
                return false;
            }
        }

        private static void LoadFromHex(string directoryPath)
        {
            Missions.Clear();
            if (!Directory.Exists(directoryPath))
            {
                CLogger.Print($"Directory not found: {directoryPath}", LoggerType.Warning);
                return;
            }

            foreach (string filePath in Directory.GetFiles(directoryPath, "*.hex"))
            {
                try
                {
                    MissionStreamEntry entry = LoadHexFile(filePath);
                    if (entry != null)
                        Missions.Add(entry);
                }
                catch (Exception ex)
                {
                    CLogger.Print($"Error loading mission hex {filePath}: {ex.Message}", LoggerType.Error, ex);
                }
            }

            Missions.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        private static MissionStreamEntry LoadHexFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 6)
                return null;

            var entry = new MissionStreamEntry
            {
                Id = int.Parse(lines[0].Split('=')[1].Trim()),
                Name = lines[1].Split('=')[1].Trim(),
                Description = lines[2].Split('=')[1].Trim(),
                RewardId = int.Parse(lines[3].Split('=')[1].Trim()),
                RewardCount = int.Parse(lines[4].Split('=')[1].Trim())
            };

            string hexBlob = string.Join(" ", lines.Skip(5));
            string[] hexTokens = hexBlob.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] blob = hexTokens.Select(h => Convert.ToByte(h, 16)).ToArray();

            if (blob.Length != HexBlobSize)
            {
                CLogger.Print(
                    $"Mission hex {Path.GetFileName(filePath)}: {blob.Length}B, esperado {HexBlobSize}B",
                    LoggerType.Warning);
            }

            // Completa o registro 10: o .hex traz d0/d1/d2 e o par 0 vem do cabecalho.
            byte[] objectives = new byte[ObjectivesDataSize];
            Buffer.BlockCopy(blob, 0, objectives, 0, Math.Min(blob.Length, ObjectivesDataSize));
            if (blob.Length <= ObjectivesDataSize - 8)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(entry.RewardId), 0, objectives, blob.Length, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(entry.RewardCount), 0, objectives, blob.Length + 4, 4);
            }

            entry.ObjectivesData = objectives;
            return entry;
        }

        public static byte[] BuildPayload(MissionStreamEntry mission)
        {
            using (SyncServerPacket packet = new SyncServerPacket())
            {
                packet.WriteC((byte)mission.Id);
                packet.WriteC((byte)mission.Name.Length);
                packet.WriteN(mission.Name, mission.Name.Length, "UTF-16LE");
                packet.WriteC((byte)mission.Description.Length);
                packet.WriteN(mission.Description, mission.Description.Length, "UTF-16LE");
                packet.WriteB(mission.ObjectivesData);
                return packet.ToArray();
            }
        }

        private class MissionRewardInfo
        {
            public int RewardId { get; set; }
            public int RewardCount { get; set; }
            public bool HasUnmappedAttrs { get; set; }
            public RewardRecord Final { get; } = new RewardRecord();
            public Dictionary<int, RewardRecord> CardRewards { get; } = new Dictionary<int, RewardRecord>();

            public RewardRecord GetCardReward(int cardId)
            {
                return CardRewards.ContainsKey(cardId)
                    ? CardRewards[cardId]
                    : new RewardRecord();
            }
        }

        private class RewardItem
        {
            public int GoodId { get; set; }
            public int Count { get; set; }
        }

        private class RewardRecord
        {
            public int Field0 { get; set; }
            public int Exp { get; set; }
            public int Medals { get; set; }
            public RewardItem[] Items { get; } = Enumerable.Range(0, RewardItemSlots).Select(_ => new RewardItem()).ToArray();
        }
    }
}
