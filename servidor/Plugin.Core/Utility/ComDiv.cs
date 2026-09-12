using Npgsql;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.RAW;
using Plugin.Core.SQL;
using Plugin.Core.XML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Net;

namespace Plugin.Core.Utility
{
    public static class ComDiv
    {
        public static int CheckEquipedItems(PlayerEquipment Equip, List<ItemsModel> Inventory, bool BattleRules)
        {
            int num = 0;
            bool flag1 = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            bool flag6 = false;
            bool flag7 = false;
            bool flag8 = false;
            bool flag9 = false;
            bool flag10 = false;
            bool flag11 = false;
            bool flag12 = false;
            bool flag13 = false;
            bool flag14 = false;
            bool flag15 = false;
            bool flag16 = false;
            bool flag17 = false;
            bool flag18 = false;
            bool flag19 = false;
            bool flag20 = false;
            if (Equip.WeaponPrimary == 103004)
                flag1 = true;
            if (BattleRules)
            {
                if (!flag1 && (Equip.WeaponPrimary == 105025 || Equip.WeaponPrimary == 106007))
                    flag1 = true;
                if (!flag3 && Equip.WeaponMelee == 323001)
                    flag3 = true;
            }
            if (Equip.PartHead == 0)
                flag8 = true;
            if (Equip.PartFace == 0)
                flag9 = true;
            if (Equip.PartJacket == 0)
                flag10 = true;
            if (Equip.PartPocket == 0)
                flag11 = true;
            if (Equip.PartGlove == 0)
                flag12 = true;
            if (Equip.PartBelt == 0)
                flag13 = true;
            if (Equip.PartHolster == 0)
                flag14 = true;
            if (Equip.PartSkin == 0)
                flag15 = true;
            if (Equip.BeretItem == 0)
                flag16 = true;
            if (Equip.DinoItem == 0)
                flag17 = true;
            if (Equip.AccessoryId == 0)
                flag18 = true;
            if (Equip.SprayId == 0)
                flag19 = true;
            if (Equip.NameCardId == 0)
                flag20 = true;
            lock (Inventory)
            {
                foreach (ItemsModel itemsModel in Inventory)
                {
                    if (itemsModel.Count > 0U)
                    {
                        if (itemsModel.Id == Equip.WeaponPrimary)
                            flag1 = true;
                        else if (itemsModel.Id != Equip.WeaponSecondary)
                        {
                            if (itemsModel.Id == Equip.WeaponMelee)
                                flag3 = true;
                            else if (itemsModel.Id != Equip.WeaponExplosive)
                            {
                                if (itemsModel.Id != Equip.WeaponSpecial)
                                {
                                    if (itemsModel.Id == Equip.CharaRedId)
                                        flag6 = true;
                                    else if (itemsModel.Id != Equip.CharaBlueId)
                                    {
                                        if (itemsModel.Id == Equip.PartHead)
                                            flag8 = true;
                                        else if (itemsModel.Id == Equip.PartFace)
                                            flag9 = true;
                                        else if (itemsModel.Id == Equip.PartJacket)
                                            flag10 = true;
                                        else if (itemsModel.Id != Equip.PartPocket)
                                        {
                                            if (itemsModel.Id == Equip.PartGlove)
                                                flag12 = true;
                                            else if (itemsModel.Id == Equip.PartBelt)
                                                flag13 = true;
                                            else if (itemsModel.Id != Equip.PartHolster)
                                            {
                                                if (itemsModel.Id == Equip.PartSkin)
                                                    flag15 = true;
                                                else if (itemsModel.Id == Equip.BeretItem)
                                                    flag16 = true;
                                                else if (itemsModel.Id == Equip.DinoItem)
                                                    flag17 = true;
                                                else if (itemsModel.Id == Equip.AccessoryId)
                                                    flag18 = true;
                                                else if (itemsModel.Id == Equip.SprayId)
                                                    flag19 = true;
                                                else if (itemsModel.Id == Equip.NameCardId)
                                                    flag20 = true;
                                            }
                                            else
                                                flag14 = true;
                                        }
                                        else
                                            flag11 = true;
                                    }
                                    else
                                        flag7 = true;
                                }
                                else
                                    flag5 = true;
                            }
                            else
                                flag4 = true;
                        }
                        else
                            flag2 = true;
                        if (flag1 & flag2 & flag3 & flag4 & flag5 & flag6 & flag7 & flag8 & flag9 & flag10 & flag11 & flag12 & flag13 & flag14 & flag15 & flag16 & flag17 & flag18 & flag19 & flag20)
                            break;
                    }
                }
            }
            if (!flag1 || !flag2 || !flag3 || !flag4 || !flag5)
                num += 2;
            if (!flag6 || !flag7 || !flag8 || !flag9 || !flag10 || !flag11 || !flag12 || !flag13 || !flag14 || !flag15 || !flag16 || !flag17)
                ++num;
            if (!flag18 || !flag19 || !flag20)
                num += 3;
            if (!flag1)
                Equip.WeaponPrimary = 103004;
            if (!flag2)
                Equip.WeaponSecondary = 202022;
            if (!flag3)
                Equip.WeaponMelee = 301012;
            if (!flag4)
                Equip.WeaponExplosive = 407056;
            if (!flag5)
                Equip.WeaponSpecial = 508002;
            if (Equip.WeaponSpecial2 != 0)
            {
                bool foundSpecial2 = false;
                lock (Inventory)
                {
                    foreach (ItemsModel itemsModel in Inventory)
                    {
                        if (itemsModel.Count > 0U && itemsModel.Id == Equip.WeaponSpecial2)
                        {
                            foundSpecial2 = true;
                            break;
                        }
                    }
                }
                if (!foundSpecial2)
                    Equip.WeaponSpecial2 = 0;
            }
            if (!flag6)
                Equip.CharaRedId = 632656;
            if (!flag7)
                Equip.CharaBlueId = 664657;
            if (!flag8)
                Equip.PartHead = 0;
            if (!flag9)
                Equip.PartFace = 0;
            if (!flag10)
                Equip.PartJacket = 0;
            if (!flag11)
                Equip.PartPocket = 0;
            if (!flag12)
                Equip.PartGlove = 0;
            if (!flag13)
                Equip.PartBelt = 0;
            if (!flag14)
                Equip.PartHolster = 0;
            if (!flag15)
                Equip.PartSkin = 0;
            if (!flag16)
                Equip.BeretItem = 0;
            if (!flag17)
                Equip.DinoItem = 1500511;
            if (!flag18)
                Equip.AccessoryId = 0;
            if (!flag19)
                Equip.SprayId = 0;
            if (!flag20)
                Equip.NameCardId = 0;
            return num;
        }

        public static void UpdateWeapons(PlayerEquipment Source, PlayerEquipment Equip, DBQuery Query)
        {
            if (Equip.WeaponPrimary != Source.WeaponPrimary)
                Query.AddQuery("weapon_primary", (object)Source.WeaponPrimary);
            if (Equip.WeaponSecondary != Source.WeaponSecondary)
                Query.AddQuery("weapon_secondary", (object)Source.WeaponSecondary);
            if (Equip.WeaponMelee != Source.WeaponMelee)
                Query.AddQuery("weapon_melee", (object)Source.WeaponMelee);
            if (Equip.WeaponExplosive != Source.WeaponExplosive)
                Query.AddQuery("weapon_explosive", (object)Source.WeaponExplosive);
            if (Equip.WeaponSpecial == Source.WeaponSpecial && Equip.WeaponSpecial2 == Source.WeaponSpecial2)
                return;
            if (Equip.WeaponSpecial != Source.WeaponSpecial)
                Query.AddQuery("weapon_special", (object)Source.WeaponSpecial);
            if (Equip.WeaponSpecial2 != Source.WeaponSpecial2)
                Query.AddQuery("weapon_special_2", (object)Source.WeaponSpecial2);
        }

        public static void UpdateChars(PlayerEquipment Source, PlayerEquipment Equip, DBQuery Query)
        {
            if (Equip.CharaRedId != Source.CharaRedId)
                Query.AddQuery("chara_red_side", (object)Source.CharaRedId);
            if (Equip.CharaBlueId != Source.CharaBlueId)
                Query.AddQuery("chara_blue_side", (object)Source.CharaBlueId);
            if (Equip.PartHead != Source.PartHead)
                Query.AddQuery("part_head", (object)Source.PartHead);
            if (Equip.PartFace != Source.PartFace)
                Query.AddQuery("part_face", (object)Source.PartFace);
            if (Equip.PartJacket != Source.PartJacket)
                Query.AddQuery("part_jacket", (object)Source.PartJacket);
            if (Equip.PartPocket != Source.PartPocket)
                Query.AddQuery("part_pocket", (object)Source.PartPocket);
            if (Equip.PartPocket != Source.PartPocket)
                Query.AddQuery("part_glove", (object)Source.PartPocket);
            if (Equip.PartBelt != Source.PartBelt)
                Query.AddQuery("part_belt", (object)Source.PartBelt);
            if (Equip.PartHolster != Source.PartHolster)
                Query.AddQuery("part_holster", (object)Source.PartHolster);
            if (Equip.PartSkin != Source.PartSkin)
                Query.AddQuery("part_skin", (object)Source.PartSkin);
            if (Equip.BeretItem != Source.BeretItem)
                Query.AddQuery("beret_item_part", (object)Source.BeretItem);
            if (Equip.DinoItem == Source.DinoItem)
                return;
            Query.AddQuery("dino_item_chara", (object)Source.DinoItem);
        }

        public static void UpdateCharSlots(PlayerEquipment Source, PlayerEquipment Equip, DBQuery Query)
        {
            if (Equip.CharaRedId != Source.CharaRedId)
                Query.AddQuery("chara_red_side", (object)Source.CharaRedId);
            if (Equip.CharaBlueId != Source.CharaBlueId)
                Query.AddQuery("chara_blue_side", (object)Source.CharaBlueId);
            if (Equip.DinoItem == Source.DinoItem)
                return;
            Query.AddQuery("dino_item_chara", (object)Source.DinoItem);
        }

        public static void UpdateItems(PlayerEquipment Source, PlayerEquipment Equip, DBQuery Query)
        {
            if (Equip.AccessoryId != Source.AccessoryId)
                Query.AddQuery("accesory_id", (object)Source.AccessoryId);
            if (Equip.SprayId != Source.SprayId)
                Query.AddQuery("spray_id", (object)Source.SprayId);
            if (Equip.NameCardId == Source.NameCardId)
                return;
            Query.AddQuery("namecard_id", (object)Source.NameCardId);
        }

        public static void UpdateWeapons(PlayerEquipment Equip, DBQuery Query)
        {
            Query.AddQuery("weapon_primary", (object)Equip.WeaponPrimary);
            Query.AddQuery("weapon_secondary", (object)Equip.WeaponSecondary);
            Query.AddQuery("weapon_melee", (object)Equip.WeaponMelee);
            Query.AddQuery("weapon_explosive", (object)Equip.WeaponExplosive);
            Query.AddQuery("weapon_special", (object)Equip.WeaponSpecial);
            Query.AddQuery("weapon_special_2", (object)Equip.WeaponSpecial2);
        }

        public static void UpdateChars(PlayerEquipment Equip, DBQuery Query)
        {
            Query.AddQuery("chara_red_side", (object)Equip.CharaRedId);
            Query.AddQuery("chara_blue_side", (object)Equip.CharaBlueId);
            Query.AddQuery("part_head", (object)Equip.PartHead);
            Query.AddQuery("part_face", (object)Equip.PartFace);
            Query.AddQuery("part_jacket", (object)Equip.PartJacket);
            Query.AddQuery("part_pocket", (object)Equip.PartPocket);
            Query.AddQuery("part_glove", (object)Equip.PartGlove);
            Query.AddQuery("part_belt", (object)Equip.PartBelt);
            Query.AddQuery("part_holster", (object)Equip.PartHolster);
            Query.AddQuery("part_skin", (object)Equip.PartSkin);
            Query.AddQuery("beret_item_part", (object)Equip.BeretItem);
            Query.AddQuery("dino_item_chara", (object)Equip.DinoItem);
        }

        public static void UpdateItems(PlayerEquipment Equip, DBQuery Query)
        {
            Query.AddQuery("accesory_id", (object)Equip.AccessoryId);
            Query.AddQuery("spray_id", (object)Equip.SprayId);
            Query.AddQuery("namecard_id", (object)Equip.NameCardId);
        }

        public static bool TryCreateItem(ItemsModel Model, PlayerInventory Inventory, long OwnerId)
        {
            try
            {
                ItemsModel itemsModel = Inventory.GetItem(Model.Id);
                if (itemsModel == null)
                {
                    if (!DaoManagerSQL.CreatePlayerInventoryItem(Model, OwnerId))
                        return false;
                    Inventory.AddItem(Model);
                }
                else
                {
                    Model.ObjectId = itemsModel.ObjectId;
                    if (itemsModel.Equip == ItemEquipType.Durable)
                    {
                        if (ShopManager.IsRepairableItem(Model.Id))
                        {
                            Model.Count = 100U;
                            ComDiv.UpdateDB("player_items", "count", (object)(long)Model.Count, "owner_id", (object)OwnerId, "id", (object)Model.Id);
                        }
                        else
                        {
                            Model.Count += itemsModel.Count;
                            ComDiv.UpdateDB("player_items", "count", (object)(long)Model.Count, "owner_id", (object)OwnerId, "id", (object)Model.Id);
                        }
                    }
                    else if (itemsModel.Equip == ItemEquipType.Temporary)
                    {
                        DateTime exact = DateTime.ParseExact(itemsModel.Count.ToString(), "yyMMddHHmm", (IFormatProvider)CultureInfo.InvariantCulture);
                        if (Model.Category == ItemCategory.Coupon)
                        {
                            TimeSpan timeSpan = DateTime.ParseExact(Model.Count.ToString(), "yyMMddHHmm", (IFormatProvider)CultureInfo.InvariantCulture) - DateTimeUtil.Now();
                            Model.Equip = ItemEquipType.Temporary;
                            Model.Count = Convert.ToUInt32(exact.AddDays(timeSpan.TotalDays).ToString("yyMMddHHmm"));
                        }
                        else
                        {
                            Model.Equip = ItemEquipType.Temporary;
                            Model.Count = Convert.ToUInt32(exact.AddSeconds((double)Model.Count).ToString("yyMMddHHmm"));
                        }
                        ComDiv.UpdateDB("player_items", "count", (object)(long)Model.Count, "owner_id", (object)OwnerId, "id", (object)Model.Id);
                    }
                    itemsModel.Equip = Model.Equip;
                    itemsModel.Count = Model.Count;
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static ItemCategory GetItemCategory(int ItemId)
        {
            int idStatics1 = ComDiv.GetIdStatics(ItemId, 1);
            int idStatics2 = ComDiv.GetIdStatics(ItemId, 4);
            if (idStatics1 >= 1 && idStatics1 <= 5)
                return ItemCategory.Weapon;
            if (idStatics1 >= 6 && idStatics1 <= 14 || idStatics1 == 27 || idStatics2 >= 7 && idStatics2 <= 14)
                return ItemCategory.Character;
            // 42/91/92/99 are decoration, the same family as the 36-40 already listed here
            if (idStatics1 >= 16 /*0x10*/ && idStatics1 <= 20 || idStatics1 == 22 || idStatics1 == 26 || idStatics1 >= 28 && idStatics1 <= 29 || idStatics1 >= 36 && idStatics1 <= 40 || idStatics1 == 42 || idStatics1 == 91 || idStatics1 == 92 || idStatics1 == 99)
                return ItemCategory.Coupon;
            if (idStatics1 == 15 || idStatics1 >= 30 && idStatics1 <= 35)
                return ItemCategory.NewItem;
            // 41xxxxx = emotes/PET (client domain Emotion). The enum member existed but nothing
            // ever returned it, so owned emotes were invisible to every GetItemsByType lookup.
            if (idStatics1 == 41)
                return ItemCategory.Emoticon;
            // BR 122: famílias 50/53 (sets/cosméticos da shop) — tratar como Coupon
            if (idStatics1 == 50 || idStatics1 == 53)
                return ItemCategory.Coupon;
            CLogger.Print($"Categoria invalida [{idStatics1}]: {ItemId}", LoggerType.Warning);
            return ItemCategory.None;
        }

        public static uint ValidateStockId(int ItemId)
        {
            int idStatics = ComDiv.GetIdStatics(ItemId, 4);
            int ItemId1;
            switch (idStatics)
            {
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    ItemId1 = idStatics;
                    break;

                default:
                    ItemId1 = ItemId;
                    break;
            }
            return ComDiv.GenStockId(ItemId1);
        }

        public static int GetIdStatics(int Id, int Type)
        {
            switch (Type)
            {
                case 1:
                    return Id / 100000;

                case 2:
                    return Id % 100000 / 1000;

                case 3:
                    return Id % 1000;

                case 4:
                    return Id % 10000000 / 100000;

                case 5:
                    return Id / 1000;

                default:
                    return 0;
            }
        }

        public static double GetDuration(DateTime Date) => (DateTimeUtil.Now() - Date).TotalSeconds;

        public static byte[] AddressBytes(string Host) => IPAddress.Parse(Host).GetAddressBytes();

        public static int CreateItemId(int ItemClass, int ClassType, int Number)
        {
            return ItemClass * 100000 + ClassType * 1000 + Number;
        }

        public static int Percentage(int Total, int Percent) => Total * Percent / 100;

        public static float Percentage(float Total, int Percent)
        {
            return (float)((double)Total * (double)Percent / 100.0);
        }

        public static char[] SubArray(this char[] Input, int StartIndex, int Length)
        {
            List<char> charList = new List<char>();
            for (int index = StartIndex; index < Length; ++index)
                charList.Add(Input[index]);
            return charList.ToArray();
        }

        public static bool IsLoopbackAddress(string Address)
        {
            IPAddress Parsed;
            return !string.IsNullOrEmpty(Address) && IPAddress.TryParse(Address, out Parsed) && IPAddress.IsLoopback(Parsed);
        }

        public static bool UpdateDB(string TABEL, string[] COLUMNS, params object[] VALUES)
        {
            if (COLUMNS.Length != 0 && VALUES.Length != 0 && COLUMNS.Length != VALUES.Length)
            {
                CLogger.Print($"[Update Database] Wrong values: {string.Join(",", COLUMNS)}/{string.Join(",", VALUES)}", LoggerType.Warning);
                return false;
            }
            if (COLUMNS.Length != 0)
            {
                if (VALUES.Length != 0)
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                            {
                                ((DbConnection)npgsqlConnection).Open();
                                ((DbCommand)command).CommandType = CommandType.Text;
                                List<string> stringList = new List<string>();
                                for (int index = 0; index < VALUES.Length; ++index)
                                {
                                    object obj = VALUES[index];
                                    string str1 = COLUMNS[index];
                                    string str2 = "@Value" + index.ToString();
                                    command.Parameters.AddWithValue(str2, obj);
                                    stringList.Add($"{str1}={str2}");
                                }
                                string str = string.Join(",", stringList.ToArray());
                                ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {str}";
                                ((DbCommand)command).ExecuteNonQuery();
                                ((Component)command).Dispose();
                                ((DbConnection)npgsqlConnection).Close();
                            }
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print("[AllUtils.UpdateDB1] " + ex.Message, LoggerType.Error, ex);
                        return false;
                    }
                }
            }
            return false;
        }

        public static bool UpdateDB(
          string TABEL,
          string Req1,
          object ValueReq1,
          string[] COLUMNS,
          params object[] VALUES)
        {
            if (COLUMNS.Length != 0 && VALUES.Length != 0 && COLUMNS.Length != VALUES.Length)
            {
                CLogger.Print($"[Update Database] Wrong values: {string.Join(",", COLUMNS)}/{string.Join(",", VALUES)}", LoggerType.Warning);
                return false;
            }
            if (COLUMNS.Length != 0)
            {
                if (VALUES.Length != 0)
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                            {
                                ((DbConnection)npgsqlConnection).Open();
                                ((DbCommand)command).CommandType = CommandType.Text;
                                List<string> stringList = new List<string>();
                                for (int index = 0; index < VALUES.Length; ++index)
                                {
                                    object obj = VALUES[index];
                                    string str1 = COLUMNS[index];
                                    string str2 = "@Value" + index.ToString();

                                    // ✅ ป้องกัน error จาก UInt32, UInt64 fixed from mithg
                                    if (obj is uint uintVal)
                                        obj = (int)uintVal;
                                    else if (obj is ulong ulongVal)
                                        obj = (long)ulongVal;

                                    command.Parameters.AddWithValue(str2, obj);
                                    stringList.Add($"{str1}={str2}");
                                }
                                string str = string.Join(",", stringList.ToArray());
                                command.Parameters.AddWithValue("@Req1", ValueReq1);
                                ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {str} WHERE {Req1}=@Req1";
                                ((DbCommand)command).ExecuteNonQuery();
                                ((Component)command).Dispose();
                                ((DbConnection)npgsqlConnection).Close();
                            }
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print("[AllUtils.UpdateDB2] " + ex.Message, LoggerType.Error, ex);
                        return false;
                    }
                }
            }
            return false;
        }

        public static bool UpdateDB(
          string TABEL,
          string COLUMN,
          object VALUE,
          string Req1,
          object ValueReq1)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        ((DbConnection)npgsqlConnection).Open();
                        ((DbCommand)command).CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@Value", VALUE);
                        command.Parameters.AddWithValue("@Req1", ValueReq1);
                        ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {COLUMN}=@Value WHERE {Req1}=@Req1";
                        ((DbCommand)command).ExecuteNonQuery();
                        ((Component)command).Dispose();
                        ((DbConnection)npgsqlConnection).Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print("[AllUtils.UpdateDB3] " + ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool UpdateDB(
          string TABEL,
          string Req1,
          object ValueReq1,
          string Req2,
          object valueReq2,
          string[] COLUMNS,
          params object[] VALUES)
        {
            if (COLUMNS.Length != 0 && VALUES.Length != 0 && COLUMNS.Length != VALUES.Length)
            {
                CLogger.Print($"[Update Database] Wrong values: {string.Join(",", COLUMNS)}/{string.Join(",", VALUES)}", LoggerType.Warning);
                return false;
            }
            if (COLUMNS.Length != 0)
            {
                if (VALUES.Length != 0)
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                            {
                                ((DbConnection)npgsqlConnection).Open();
                                ((DbCommand)command).CommandType = CommandType.Text;
                                List<string> stringList = new List<string>();
                                for (int index = 0; index < VALUES.Length; ++index)
                                {
                                    object obj = VALUES[index];
                                    string str1 = COLUMNS[index];
                                    string str2 = "@Value" + index.ToString();
                                    command.Parameters.AddWithValue(str2, obj);
                                    stringList.Add($"{str1}={str2}");
                                }
                                string str = string.Join(",", stringList.ToArray());
                                if (Req1 != null)
                                    command.Parameters.AddWithValue("@Req1", ValueReq1);
                                if (Req2 != null)
                                    command.Parameters.AddWithValue("@Req2", valueReq2);
                                if (Req1 != null && Req2 == null)
                                    ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {str} WHERE {Req1}=@Req1";
                                else if (Req2 != null && Req1 == null)
                                    ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {str} WHERE {Req2}=@Req2";
                                else if (Req2 != null && Req1 != null)
                                    ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {str} WHERE {Req1}=@Req1 AND {Req2}=@Req2";
                                ((DbCommand)command).ExecuteNonQuery();
                                ((Component)command).Dispose();
                                ((DbConnection)npgsqlConnection).Close();
                            }
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print("[AllUtils.UpdateDB4] " + ex.Message, LoggerType.Error, ex);
                        return false;
                    }
                }
            }
            return false;
        }

        public static bool UpdateDB(
          string TABEL,
          string Req1,
          int[] ValueReq1,
          string Req2,
          object ValueReq2,
          string[] COLUMNS,
          params object[] VALUES)
        {
            if (COLUMNS.Length != 0 && VALUES.Length != 0 && COLUMNS.Length != VALUES.Length)
            {
                CLogger.Print($"[updateDB5] Wrong values: {string.Join(",", COLUMNS)}/{string.Join(",", VALUES)}", LoggerType.Warning);
                return false;
            }
            if (COLUMNS.Length != 0)
            {
                if (VALUES.Length != 0)
                {
                    try
                    {
                        using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                        {
                            using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                            {
                                ((DbConnection)npgsqlConnection).Open();
                                ((DbCommand)command).CommandType = CommandType.Text;
                                List<string> stringList = new List<string>();
                                for (int index = 0; index < VALUES.Length; ++index)
                                {
                                    object obj = VALUES[index];
                                    string str1 = COLUMNS[index];
                                    string str2 = "@Value" + index.ToString();
                                    command.Parameters.AddWithValue(str2, obj);
                                    stringList.Add($"{str1}={str2}");
                                }
                                string str = string.Join(",", stringList.ToArray());
                                if (Req1 != null)
                                    command.Parameters.AddWithValue("@Req1", (object)ValueReq1);
                                if (Req2 != null)
                                    command.Parameters.AddWithValue("@Req2", ValueReq2);
                                if (Req1 != null && Req2 == null)
                                    ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {str} WHERE {Req1} = ANY (@Req1)";
                                else if (Req2 != null && Req1 == null)
                                    ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {str} WHERE {Req2}=@Req2";
                                else if (Req2 != null && Req1 != null)
                                    ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {str} WHERE {Req1} = ANY (@Req1) AND {Req2}=@Req2";
                                ((DbCommand)command).ExecuteNonQuery();
                                ((Component)command).Dispose();
                                ((DbConnection)npgsqlConnection).Close();
                            }
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        CLogger.Print("[AllUtils.UpdateDB5] " + ex.Message, LoggerType.Error, ex);
                        return false;
                    }
                }
            }
            return false;
        }

        public static bool UpdateDB(
          string TABEL,
          string COLUMN,
          object VALUE,
          string Req1,
          object ValueReq1,
          string Req2,
          object ValueReq2)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        ((DbConnection)npgsqlConnection).Open();
                        ((DbCommand)command).CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@Value", VALUE);
                        if (Req1 != null)
                            command.Parameters.AddWithValue("@Req1", ValueReq1);
                        if (Req2 != null)
                            command.Parameters.AddWithValue("@Req2", ValueReq2);
                        if (Req1 != null && Req2 == null)
                            ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {COLUMN}=@Value WHERE {Req1}=@Req1";
                        else if (Req2 != null && Req1 == null)
                            ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {COLUMN}=@Value WHERE {Req2}=@Req2";
                        else if (Req2 != null && Req1 != null)
                            ((DbCommand)command).CommandText = $"UPDATE {TABEL} SET {COLUMN}=@Value WHERE {Req1}=@Req1 AND {Req2}=@Req2";
                        ((DbCommand)command).ExecuteNonQuery();
                        ((Component)command).Dispose();
                        ((DbConnection)npgsqlConnection).Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print("[AllUtils.UpdateDB6] " + ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool DeleteDB(string TABEL, string Req1, object ValueReq1)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        ((DbConnection)npgsqlConnection).Open();
                        ((DbCommand)command).CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@Req1", ValueReq1);
                        ((DbCommand)command).CommandText = $"DELETE FROM {TABEL} WHERE {Req1}=@Req1";
                        ((DbCommand)command).ExecuteNonQuery();
                        ((Component)command).Dispose();
                        ((Component)npgsqlConnection).Dispose();
                        ((DbConnection)npgsqlConnection).Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool DeleteDB(
          string TABEL,
          string Req1,
          object ValueReq1,
          string Req2,
          object ValueReq2)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        ((DbConnection)npgsqlConnection).Open();
                        ((DbCommand)command).CommandType = CommandType.Text;
                        if (Req1 != null)
                            command.Parameters.AddWithValue("@Req1", ValueReq1);
                        if (Req2 != null)
                            command.Parameters.AddWithValue("@Req2", ValueReq2);
                        if (Req1 != null && Req2 == null)
                            ((DbCommand)command).CommandText = $"DELETE FROM {TABEL} WHERE {Req1}=@Req1";
                        else if (Req2 != null && Req1 == null)
                            ((DbCommand)command).CommandText = $"DELETE FROM {TABEL} WHERE {Req2}=@Req2";
                        else if (Req2 != null && Req1 != null)
                            ((DbCommand)command).CommandText = $"DELETE FROM {TABEL} WHERE {Req1}=@Req1 AND {Req2}=@Req2";
                        ((DbCommand)command).ExecuteNonQuery();
                        ((Component)command).Dispose();
                        ((Component)npgsqlConnection).Dispose();
                        ((DbConnection)npgsqlConnection).Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool DeleteDB(
          string TABEL,
          string Req1,
          object[] ValueReq1,
          string Req2,
          object ValueReq2)
        {
            if (ValueReq1.Length == 0)
                return false;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        ((DbConnection)npgsqlConnection).Open();
                        ((DbCommand)command).CommandType = CommandType.Text;
                        List<string> stringList = new List<string>();
                        for (int index = 0; index < ValueReq1.Length; ++index)
                        {
                            object obj = ValueReq1[index];
                            string str = "@Value" + index.ToString();
                            command.Parameters.AddWithValue(str, obj);
                            stringList.Add(str);
                        }
                        string str1 = string.Join(",", stringList.ToArray());
                        command.Parameters.AddWithValue("@Req2", ValueReq2);
                        ((DbCommand)command).CommandText = $"DELETE FROM {TABEL} WHERE {Req1} in ({str1}) AND {Req2}=@Req2";
                        ((DbCommand)command).ExecuteNonQuery();
                        ((Component)command).Dispose();
                        ((Component)npgsqlConnection).Dispose();
                        ((DbConnection)npgsqlConnection).Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static uint GetPlayerStatus(AccountStatus status, bool isOnline)
        {
            FriendState state;
            int roomId;
            int channelId;
            int serverId;
            ComDiv.GetPlayerLocation(status, isOnline, out state, out roomId, out channelId, out serverId);
            return ComDiv.GetPlayerStatus(roomId, channelId, serverId, (int)state);
        }

        public static uint GetPlayerStatus(int roomId, int channelId, int serverId, int stateId)
        {
            int num1 = (stateId & (int)byte.MaxValue) << 28;
            int num2 = (serverId & (int)byte.MaxValue) << 20;
            int num3 = (channelId & (int)byte.MaxValue) << 12;
            int num4 = roomId & 4095 /*0x0FFF*/;
            int num5 = num2;
            return (uint)(num1 | num5 | num3 | num4);
        }

        public static ulong GetPlayerStatus(
          int clanFId,
          int roomId,
          int channelId,
          int serverId,
          int stateId)
        {
            long num1 = ((long)clanFId & (long)uint.MaxValue) << 32 /*0x20*/;
            long num2 = (long)((stateId & (int)byte.MaxValue) << 28);
            long num3 = (long)((serverId & (int)byte.MaxValue) << 20);
            long num4 = (long)((channelId & (int)byte.MaxValue) << 12);
            long num5 = (long)(roomId & 4095 /*0x0FFF*/);
            long num6 = num2;
            return (ulong)(num1 | num6 | num3 | num4 | num5);
        }

        public static ulong GetClanStatus(AccountStatus status, bool isOnline)
        {
            FriendState state;
            int roomId;
            int channelId;
            int serverId;
            int clanFId;
            ComDiv.GetPlayerLocation(status, isOnline, out state, out roomId, out channelId, out serverId, out clanFId);
            return ComDiv.GetPlayerStatus(clanFId, roomId, channelId, serverId, (int)state);
        }

        public static ulong GetClanStatus(FriendState state)
        {
            return ComDiv.GetPlayerStatus(0, 0, 0, 0, (int)state);
        }

        public static uint GetFriendStatus(FriendModel f)
        {
            PlayerInfo info = f.Info;
            if (info == null)
                return 0;
            FriendState state = FriendState.None;
            int serverId = 0;
            int channelId = 0;
            int roomId = 0;
            if (!f.Removed)
            {
                if (f.State > 0)
                    state = (FriendState)f.State;
                else
                    ComDiv.GetPlayerLocation(info.Status, info.IsOnline, out state, out roomId, out channelId, out serverId);
            }
            else
                state = FriendState.Offline;
            return ComDiv.GetPlayerStatus(roomId, channelId, serverId, (int)state);
        }

        public static uint GetFriendStatus(FriendModel f, FriendState stateN)
        {
            PlayerInfo info = f.Info;
            if (info == null)
                return 0;
            FriendState state = stateN;
            int serverId = 0;
            int channelId = 0;
            int roomId = 0;
            if (f.Removed)
                state = FriendState.Offline;
            else if (f.State > 0)
                state = (FriendState)f.State;
            else if (stateN == FriendState.None)
                ComDiv.GetPlayerLocation(info.Status, info.IsOnline, out state, out roomId, out channelId, out serverId);
            return ComDiv.GetPlayerStatus(roomId, channelId, serverId, (int)state);
        }

        public static void GetPlayerLocation(
          AccountStatus status,
          bool isOnline,
          out FriendState state,
          out int roomId,
          out int channelId,
          out int serverId)
        {
            roomId = 0;
            channelId = 0;
            serverId = 0;
            if (!isOnline)
            {
                state = FriendState.Offline;
            }
            else
            {
                if (status.RoomId != byte.MaxValue)
                {
                    roomId = (int)status.RoomId;
                    channelId = (int)status.ChannelId;
                    state = FriendState.Room;
                }
                else if (status.RoomId == byte.MaxValue && status.ChannelId != byte.MaxValue)
                {
                    channelId = (int)status.ChannelId;
                    state = FriendState.Lobby;
                }
                else
                    state = status.RoomId != byte.MaxValue || status.ChannelId != byte.MaxValue ? FriendState.Offline : FriendState.Online;
                if (status.ServerId == byte.MaxValue)
                    return;
                serverId = (int)status.ServerId;
            }
        }

        public static void GetPlayerLocation(
          AccountStatus status,
          bool isOnline,
          out FriendState state,
          out int roomId,
          out int channelId,
          out int serverId,
          out int clanFId)
        {
            roomId = 0;
            channelId = 0;
            serverId = 0;
            clanFId = 0;
            if (!isOnline)
            {
                state = FriendState.Offline;
            }
            else
            {
                if (status.RoomId != byte.MaxValue)
                {
                    roomId = (int)status.RoomId;
                    channelId = (int)status.ChannelId;
                    state = FriendState.Room;
                }
                else if ((status.ClanMatchId != byte.MaxValue || status.RoomId == byte.MaxValue) && status.ChannelId != byte.MaxValue)
                {
                    channelId = (int)status.ChannelId;
                    state = FriendState.Lobby;
                }
                else
                    state = status.RoomId != byte.MaxValue || status.ChannelId != byte.MaxValue ? FriendState.Offline : FriendState.Online;
                if (status.ServerId != byte.MaxValue)
                    serverId = (int)status.ServerId;
                if (status.ClanMatchId == byte.MaxValue)
                    return;
                clanFId = (int)status.ClanMatchId + 1;
            }
        }

        // Card-set ids the client never offers for sale (UIMainFrame::ShowQuestUI skips
        // idx < 4 and idx == 13 when building the buy list) = always available.
        private static readonly int[] FreeCardSetIds = new int[4] { 1, 2, 3, 13 };

        /// <summary>
        /// Bitmask of card sets the player can pick in the mission-card tab.
        /// Bit index = CardSetId (1..17, matching Quest.pef). Goes out as field 28 of
        /// GET_SYSTEM_INFO_ACK (2315) and lands in MCardMgr+372; the client turns it into
        /// the combo list (MCardMgr+56), so a zero mask means an empty tab.
        /// </summary>
        public static int GetOwnedCardSetMask(PlayerMissions missions)
        {
            var streamed = new HashSet<int>();
            foreach (MissionStreamEntry mission in MissionStreamXML.GetAllMissions())
                streamed.Add(mission.Id);

            int mask = 0;
            foreach (int cardSetId in FreeCardSetIds)
            {
                if (streamed.Contains(cardSetId))
                    mask |= 1 << cardSetId;
            }

            if (missions != null)
                mask |= missions.OwnedCardSetMask & MaskStreamed(streamed);

            return mask;
        }

        private static int MaskStreamed(HashSet<int> streamed)
        {
            int mask = 0;
            foreach (int id in streamed)
                if (id > 0 && id < 32)
                    mask |= 1 << id;
            return mask;
        }

        public static ushort GetMissionCardFlags(int missionId, int cardIdx, byte[] arrayList)
        {
            if (missionId == 0)
                return 0;
            int missionCardFlags = 0;
            foreach (MissionCardModel card in MissionCardRAW.GetCards(missionId, cardIdx))
            {
                if ((int)arrayList[card.ArrayIdx] >= card.MissionLimit)
                    missionCardFlags |= card.Flag;
            }
            return (ushort)missionCardFlags;
        }

        public static byte[] GetMissionCardFlags(int missionId, byte[] arrayList)
        {
            if (missionId == 0)
                return new byte[20];
            List<MissionCardModel> cards = MissionCardRAW.GetCards(missionId);
            if (cards.Count == 0)
                return new byte[20];
            using (SyncServerPacket syncServerPacket = new SyncServerPacket(20L))
            {
                int num = 0;
                for (int CardBasicId = 0; CardBasicId < 10; ++CardBasicId)
                {
                    foreach (MissionCardModel card in MissionCardRAW.GetCards(cards, CardBasicId))
                    {
                        if ((int)arrayList[card.ArrayIdx] >= card.MissionLimit)
                            num |= card.Flag;
                    }
                    syncServerPacket.WriteH((ushort)num);
                    num = 0;
                }
                return syncServerPacket.ToArray();
            }
        }

        public static int CountDB(string CommandArgument)
        {
            int num = 0;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    ((DbConnection)npgsqlConnection).Open();
                    ((DbCommand)command).CommandText = CommandArgument;
                    num = Convert.ToInt32(((DbCommand)command).ExecuteScalar());
                    ((Component)command).Dispose();
                    ((Component)npgsqlConnection).Dispose();
                    ((DbConnection)npgsqlConnection).Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[QuerySQL.CountDB] " + ex.Message, LoggerType.Error, ex);
            }
            return num;
        }

        public static bool ValidateAllPlayersAccount()
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    NpgsqlCommand command = npgsqlConnection.CreateCommand();
                    ((DbConnection)npgsqlConnection).Open();
                    ((DbCommand)command).CommandType = CommandType.Text;
                    ((DbCommand)command).CommandText = $"UPDATE accounts SET online = {(System.ValueType)false} WHERE online = {(System.ValueType)true}";
                    ((DbCommand)command).ExecuteNonQuery();
                    ((Component)command).Dispose();
                    ((DbConnection)npgsqlConnection).Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static uint GenStockId(int ItemId)
        {
            return BitConverter.ToUInt32(ComDiv.StaticMethod0(ItemId), 0);
        }

        public static bool IsStockId(long ObjectId)
        {
            return ObjectId >= 0x40000000L && ObjectId <= 0x40FFFFFFL;
        }

        private static byte[] StaticMethod0(int A_0)
        {
            byte[] bytes = BitConverter.GetBytes(A_0);
            bytes[3] = (byte)64 /*0x40*/;
            return bytes;
        }

        public static T NextOf<T>(IList<T> List, T Item)
        {
            int num = List.IndexOf(Item);
            return List[num == List.Count - 1 ? 0 : num + 1];
        }

        public static T ParseEnum<T>(string Value) => (T)System.Enum.Parse(typeof(T), Value, true);

        public static string[] SplitObjects(string Input, string Delimiter)
        {
            return Input.Split(new string[1] { Delimiter }, StringSplitOptions.None);
        }

        public static string ToTitleCase(string Text)
        {
            string titleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Text.Split(' ')[0].ToLower());
            Text = Text.Replace(Text.Split(' ')[0], titleCase);
            return Text;
        }

        private static int StaticMethod1(int A_0, int A_1)
        {
            int num;
            for (; A_1 != 0; A_1 = num)
            {
                num = A_0 % A_1;
                A_0 = A_1;
            }
            return A_0;
        }

        public static string AspectRatio(int X, int Y)
        {
            return $"{X / ComDiv.StaticMethod1(X, Y)}:{Y / ComDiv.StaticMethod1(X, Y)}";
        }

        //public static uint Verificate(byte A, byte B, byte C, byte D)
        //{
        //    byte[] source = new byte[4] { A, B, C, D };
        //    if (!((IEnumerable<byte>)source).Any<byte>())
        //        return 0;
        //    if (B >= (byte)60)
        //    {
        //        if (C >= (byte)0 && C <= (byte)1)
        //            return BitConverter.ToUInt32(source, 0);
        //        CLogger.Print($"Unknown Window State ({C})", LoggerType.Warning);
        //        return 0;
        //    }
        //    CLogger.Print($"Refresh Rate is below the minimum limit ({B})", LoggerType.Warning);
        //    return 0;
        //}
        public static uint Verificate(byte A, byte B, byte C, byte D)
        {
            byte[] source = new byte[4] { A, B, C, D };

            // Si todos son cero, usar valores por defecto
            if (A == 0 && B == 0 && C == 0 && D == 0)
            {
                source = new byte[4] { 0, 60, 0, 1 }; // Valores por defecto válidos
                return BitConverter.ToUInt32(source, 0);
            }

            // Verificación más permisiva para el refresh rate
            if (B >= 30) // Reducir de 60 a 30 Hz temporalmente
            {
                if (C >= 0 && C <= 1)
                    return BitConverter.ToUInt32(source, 0);

                return BitConverter.ToUInt32(source, 0); // Aceptar de todas formas
            }
            source[1] = 60; // Forzar 60 Hz
            return BitConverter.ToUInt32(source, 0);
        }

        public static bool CheckIfPlayerHasVIP(long OwnerId)
        {
            int count = 0;
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        ((DbConnection)npgsqlConnection).Open();
                        ((DbCommand)command).CommandType = CommandType.Text;

                        // Consulta para contar las filas que coincidan con el OwnerId
                        ((DbCommand)command).CommandText = "SELECT COUNT(*) FROM player_vip WHERE owner_id = @OwnerId";

                        // Añadir el parámetro de forma segura
                        command.Parameters.AddWithValue("@OwnerId", OwnerId);

                        // Ejecutar la consulta y obtener el resultado
                        count = Convert.ToInt32(((DbCommand)command).ExecuteScalar());

                        ((Component)command).Dispose();
                        ((DbConnection)npgsqlConnection).Close();
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[ComDiv.CheckIfPlayerHasVIP] " + ex.Message, LoggerType.Error, ex);
                // Si hay un error, asumimos que no hay VIP o que la verificación falló (false)
                return false;
            }

            // Devuelve true si el conteo es mayor a cero
            return count > 0;
        }

        public static bool UpsertPlayerVIP(long OwnerId, string RegisteredIp, string LastBenefit, long Expirate)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        ((DbConnection)npgsqlConnection).Open();
                        ((DbCommand)command).CommandType = CommandType.Text;

                        // --- 1. INTENTAR ACTUALIZAR (UPDATE) ---

                        // Asigna parámetros comunes
                        command.Parameters.AddWithValue("@OwnerId", OwnerId);
                        command.Parameters.AddWithValue("@LastBenefit", LastBenefit);
                        command.Parameters.AddWithValue("@Expirate", Expirate);

                        // Comando UPDATE: Intenta cambiar los valores para el owner_id existente.
                        ((DbCommand)command).CommandText = @"
                    UPDATE player_vip
                    SET
                        last_benefit = @LastBenefit,
                        expirate = @Expirate
                    WHERE owner_id = @OwnerId";

                        int rowsAffected = ((DbCommand)command).ExecuteNonQuery();

                        // --- 2. VERIFICAR RESULTADO Y REALIZAR INSERCIÓN SI ES NECESARIO ---

                        if (rowsAffected > 0)
                        {
                            // Si rowsAffected > 0, la actualización fue exitosa.
                            ((DbConnection)npgsqlConnection).Close();
                            return true;
                        }

                        // Si rowsAffected es 0, la fila no existía. Proceder con INSERT.

                        // Añadir parámetro exclusivo de INSERT
                        command.Parameters.AddWithValue("@RegisteredIp", RegisteredIp);

                        // Limpiar el texto del comando (o crear uno nuevo) y ejecutar INSERT
                        ((DbCommand)command).CommandText = @"
                    INSERT INTO player_vip (owner_id, registered_ip, last_benefit, expirate)
                    VALUES (@OwnerId, @RegisteredIp, @LastBenefit, @Expirate)";

                        ((DbCommand)command).ExecuteNonQuery();

                        ((DbConnection)npgsqlConnection).Close();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("[ComDiv.UpsertPlayerVIP] " + ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        public static bool InsertPlayerVIP(long OwnerId, string RegisteredIp, string LastBenefit, long Expirate)
        {
            try
            {
                using (NpgsqlConnection npgsqlConnection = ConnectionSQL.GetInstance().Conn())
                {
                    using (NpgsqlCommand command = npgsqlConnection.CreateCommand())
                    {
                        ((DbConnection)npgsqlConnection).Open();
                        ((DbCommand)command).CommandType = CommandType.Text;

                        // 1. Añadir parámetros
                        command.Parameters.AddWithValue("@owner_id", OwnerId);
                        command.Parameters.AddWithValue("@registered_ip", RegisteredIp);
                        command.Parameters.AddWithValue("@last_benefit", LastBenefit);
                        command.Parameters.AddWithValue("@expirate", Expirate);

                        // 2. Construir el comando INSERT
                        ((DbCommand)command).CommandText = @"
                    INSERT INTO player_vip (owner_id, registered_ip, last_benefit, expirate)
                    VALUES (@owner_id, @registered_ip, @last_benefit, @expirate)";

                        // 3. Ejecutar el comando
                        ((DbCommand)command).ExecuteNonQuery();

                        ((Component)command).Dispose();
                        ((DbConnection)npgsqlConnection).Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // Usa el mismo patrón de registro de errores que tu código existente
                CLogger.Print("[ComDiv.InsertPlayerVIP] " + ex.Message, LoggerType.Error, ex);
                return false;
            }
        }
    }
}