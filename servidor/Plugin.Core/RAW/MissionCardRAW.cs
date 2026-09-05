using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Plugin.Core.RAW
{
    public class MissionCardRAW
    {
        private static List<MissionItemAward> Items = new List<MissionItemAward>();
        private static List<MissionCardModel> List = new List<MissionCardModel>();
        private static List<MissionCardAwards> Awards = new List<MissionCardAwards>();
        private static void Load(string FileName, int Type)
        {
            string Path = "Data/Missions/" + FileName + ".mqf";
            if (File.Exists(Path))
            {
                Parse(Path, FileName, Type);
            }
            else
            {
                CLogger.Print($"File not found: {Path}", LoggerType.Warning);
            }
        }
        public static void LoadBasicCards(int Type)
        {
            Load("TutorialCard_Russia", Type);
            Load("Dino_Tutorial", Type);
            Load("Human_Tutorial", Type);
            Load("AssaultCard", Type);
            Load("BackUpCard", Type);
            Load("InfiltrationCard", Type);
            Load("SpecialCard", Type);
            Load("DefconCard", Type);
            Load("Commissioned_o", Type);
            Load("Company_o", Type);
            Load("Field_o", Type);
            Load("EventCard", Type);
            Load("Dino_Basic", Type);
            Load("Human_Basic", Type);
            Load("Dino_Intensify", Type);
            Load("Human_Intensify", Type);
            CLogger.Print($"Plugin carregado: {List.Count} Mission Card List", LoggerType.Info);
            if (Type == 1)
            {
                CLogger.Print($"Plugin carregado: {Awards.Count} Mission Card Awards", LoggerType.Info);
            }
            else if (Type == 2)
            {
                CLogger.Print($"Plugin carregado: {Items.Count} Mission Reward Items", LoggerType.Info);
            }
        }
        private static int ConvertStringToInt(string MissionName)
        {
            int MissionId = 0;
            switch (MissionName)
            {
                case "TutorialCard_Russia": MissionId = 1; break;
                case "Dino_Tutorial": MissionId = 2; break;
                case "Human_Tutorial": MissionId = 3; break;
                case "AssaultCard": MissionId = 5; break;
                case "BackUpCard": MissionId = 6; break;
                case "InfiltrationCard": MissionId = 7; break;
                case "SpecialCard": MissionId = 8; break;
                case "DefconCard": MissionId = 9; break;
                case "Commissioned_o": MissionId = 10; break;
                case "Company_o": MissionId = 11; break;
                case "Field_o": MissionId = 12; break;
                case "EventCard": MissionId = 13; break;
                case "Dino_Basic": MissionId = 14; break;
                case "Human_Basic": MissionId = 15; break;
                case "Dino_Intensify": MissionId = 16; break;
                case "Human_Intensify": MissionId = 17; break;
            }
            return MissionId;
        }
        public static List<ItemsModel> GetMissionAwards(int MissionId)
        {
            List<ItemsModel> Result = new List<ItemsModel>();
            lock (Items)
            {
                foreach (MissionItemAward Mission in Items)
                {
                    if (Mission.MissionId == MissionId)
                    {
                        Result.Add(Mission.Item);
                    }
                }
            }
            return Result;
        }
        public static List<MissionCardModel> GetCards(int MissionId, int CardBasicId)
        {
            List<MissionCardModel> Result = new List<MissionCardModel>();
            lock (List)
            {
                foreach (MissionCardModel Card in List)
                {
                    if (Card.MissionId == MissionId && (CardBasicId >= 0 && Card.CardBasicId == CardBasicId || CardBasicId == -1))
                    {
                        Result.Add(Card);
                    }
                }
            }
            return Result;
        }
        public static List<MissionCardModel> GetCards(List<MissionCardModel> Cards, int CardBasicId)
        {
            if (CardBasicId == -1)
            {
                return Cards;
            }
            List<MissionCardModel> Result = new List<MissionCardModel>();
            foreach (MissionCardModel Card in List)
            {
                if (CardBasicId >= 0 && Card.CardBasicId == CardBasicId || CardBasicId == -1)
                {
                    Result.Add(Card);
                }
            }
            return Result;
        }
        public static List<MissionCardModel> GetCards(int MissionId)
        {
            List<MissionCardModel> Result = new List<MissionCardModel>();
            lock (List)
            {
                foreach (MissionCardModel Card in List)
                {
                    if (Card.MissionId == MissionId)
                    {
                        Result.Add(Card);
                    }
                }
            }
            return Result;
        }
        private static void Parse(string Path, string MissionName, int TypeLoad)
        {
            int MissionId = ConvertStringToInt(MissionName);
            if (MissionId == 0)
            {
                CLogger.Print($"Invalid: {MissionName}", LoggerType.Warning);
            }
            byte[] Buffer;
            try
            {
                Buffer = File.ReadAllBytes(Path);
            }
            catch
            {
                Buffer = new byte[0];
            }
            if (Buffer.Length == 0)
            {
                return;
            }
            try
            {
                SyncClientPacket C = new SyncClientPacket(Buffer);
                C.ReadS(4); //QF | Ext name?
                int QuestType = C.ReadD();
                C.ReadB(16);
                int Value1 = 0, Value2 = 0;
                for (int i = 0; i < 40; i++)
                {
                    int MissionBId = Value2++, CardBId = Value1;
                    if (Value2 == 4)
                    {
                        Value2 = 0;
                        Value1++;
                    }
                    int ReqType = C.ReadUH();
                    int Type = C.ReadC();
                    int MapId = C.ReadC();
                    int LimitCount = C.ReadC();
                    ClassType WeaponClass = (ClassType)C.ReadC();
                    int WeaponId = C.ReadUH();
                    MissionCardModel MissionCard = new MissionCardModel(CardBId, MissionBId)
                    {
                        MapId = MapId,
                        WeaponReq = WeaponClass,
                        WeaponReqId = WeaponId,
                        MissionType = (MissionType)Type,
                        MissionLimit = LimitCount,
                        MissionId = MissionId
                    };
                    List.Add(MissionCard);
                    if (QuestType == 1)
                    {
                        C.ReadB(24);
                    }
                }
                int Value = (QuestType == 2 ? 5 : 1);
                for (int CardIdx = 0; CardIdx < 10; CardIdx++)
                {
                    int Gold = C.ReadD();
                    int Exp = C.ReadD();
                    int Medals = C.ReadD();
                    for (int i = 0; i < Value; i++)
                    {
                        int Unk = C.ReadD();
                        int ItemEquip = C.ReadD();
                        int ItemId = C.ReadD();
                        int ItemCount = C.ReadD();
                    }
                    if (TypeLoad == 1)
                    {
                        MissionCardAwards CardAwards = new MissionCardAwards()
                        {
                            Id = MissionId,
                            Card = CardIdx,
                            Exp = (QuestType == 1 ? (Exp * 10) : Exp),
                            Gold = Gold
                        };
                        GetCardMedalInfo(CardAwards, Medals);
                        if (!CardAwards.Unusable())
                        {
                            Awards.Add(CardAwards);
                        }
                    }
                }
                if (QuestType == 2)
                {
                    int GoldResult = C.ReadD();
                    C.ReadB(8);
                    for (int i = 0; i < 5; i++)
                    {
                        int Unk = C.ReadD();
                        int ItemEquip = C.ReadD(); //1 Durable | 2 Temporary
                        int ItemId = C.ReadD();
                        uint ItemCount = C.ReadUD();
                        if (Unk > 0 && TypeLoad == 1)
                        {
                            MissionItemAward ItemAward = new MissionItemAward()
                            {
                                MissionId = MissionId,
                                Item = new ItemsModel(ItemId, "Mission Item", ItemEquipType.Durable, ItemCount)
                            };
                            Items.Add(ItemAward);
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                CLogger.Print($"File error: {Path}; {ex.Message}", LoggerType.Error, ex);
            }
        }
        private static void GetCardMedalInfo(MissionCardAwards card, int medalId)
        {
            if (medalId == 0)
            {
                return;
            }
            if (medalId >= 1 && medalId <= 50) //v >= 1 && v <= 50
            {
                card.Ribbon++;
            }
            else if (medalId >= 51 && medalId <= 100) //v >= 51 && v <= 100
            {
                card.Ensign++;
            }
            else if (medalId >= 101 && medalId <= 116) //v >= 101 && v <= 116
            {
                card.Medal++;
            }
            //v >= 117 && v <= 239
        }
        public static MissionCardAwards GetAward(int mission, int cartao)
        {
            foreach (MissionCardAwards card in Awards)
            {
                if (card.Id == mission && card.Card == cartao)
                {
                    return card;
                }
            }
            return null;
        }
    }
}
