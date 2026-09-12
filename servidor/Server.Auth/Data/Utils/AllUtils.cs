using Microsoft.Extensions.Logging;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Managers;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Auth.Data.Managers;
using Server.Auth.Data.Models;
using Server.Auth.Network;
using Server.Auth.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Auth.Data.Utils
{
    public static class AllUtils
    {
        public static void ValidateAuthLevel(Account Player)
        {
            if (Enum.IsDefined(typeof(AccessLevel), (object)Player.Access))
                return;
            AccessLevel accessLevel = Player.AuthLevel();
            if (!ComDiv.UpdateDB("accounts", "access_level", (object)(int)accessLevel, "player_id", (object)Player.PlayerId))
                return;
            Player.Access = accessLevel;
        }

        public static void LoadPlayerInventory(Account Player)
        {
            List<ItemsModel> items = DaoManagerSQL.GetPlayerInventoryItems(Player.PlayerId);
            if (items == null)
                throw new InvalidOperationException($"Falha ao ler o inventario do PlayerId={Player.PlayerId} (erro de banco acima).");

            lock (Player.Inventory.Items)
                Player.Inventory.Items.AddRange((IEnumerable<ItemsModel>)items);
        }

        public static void LoadPlayerMissions(Account Player)
        {
            PlayerMissions playerMissionsDb = DaoManagerSQL.GetPlayerMissionsDB(Player.PlayerId);
            if (playerMissionsDb == null)
            {
                if (DaoManagerSQL.CreatePlayerMissionsDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Missions!", LoggerType.Warning);
            }
            else
                Player.Mission = playerMissionsDb;
        }

        public static void ValidatePlayerInventoryStatus(Account Player)
        {
            Player.Inventory.LoadBasicItems();
            // Arma Especial 2 / Smoke 2: liberar de graca (mesmo padrao do passe desativado).
            Player.Effects = InventoryUnlocks.EnsureFreeExtraSlots(Player.PlayerId, Player.Effects, Player.Inventory);
            if (Player.Rank >= 46)
                Player.Inventory.LoadGeneralBeret();
            if (Player.IsGM())
                Player.Inventory.LoadHatForGM();
            if (!string.IsNullOrEmpty(Player.Nickname))
                AllUtils.StaticMethod2(Player);
            string A_1;
            if (AllUtils.StaticMethod0(Player, out A_1))
            {
                List<ItemsModel> pcCafeRewards = TemplatePackXML.GetPCCafeRewards(Player.CafePC);
                lock (Player.Inventory.Items)
                    Player.Inventory.Items.AddRange((IEnumerable<ItemsModel>)pcCafeRewards);
                foreach (ItemsModel itemsModel in pcCafeRewards)
                {
                    if (ComDiv.GetIdStatics(itemsModel.Id, 1) == 6 && Player.Character.GetCharacter(itemsModel.Id) == null)
                        AllUtils.CreateCharacter(Player, itemsModel);
                    if (ComDiv.GetIdStatics(itemsModel.Id, 1) == 16 /*0x10*/)
                    {
                        CouponFlag couponEffect = CouponEffectXML.GetCouponEffect(itemsModel.Id);
                        if (couponEffect != null && couponEffect.EffectFlag > (CouponEffects)0 && !Player.Effects.HasFlag((Enum)couponEffect.EffectFlag))
                        {
                            Player.Effects |= couponEffect.EffectFlag;
                            DaoManagerSQL.UpdateCouponEffect(Player.PlayerId, Player.Effects);
                        }
                    }
                }
            }
            else
            {
                foreach (ItemsModel pcCafeReward in TemplatePackXML.GetPCCafeRewards(Player.CafePC))
                {
                    if (ComDiv.GetIdStatics(pcCafeReward.Id, 1) == 6 && Player.Character.GetCharacter(pcCafeReward.Id) != null)
                        AllUtils.StaticMethod1(Player, pcCafeReward.Id);
                    if (ComDiv.GetIdStatics(pcCafeReward.Id, 1) == 16 /*0x10*/)
                    {
                        CouponFlag couponEffect = CouponEffectXML.GetCouponEffect(pcCafeReward.Id);
                        if (couponEffect != null && couponEffect.EffectFlag > (CouponEffects)0 && Player.Effects.HasFlag((Enum)couponEffect.EffectFlag))
                        {
                            Player.Effects -= couponEffect.EffectFlag;
                            DaoManagerSQL.UpdateCouponEffect(Player.PlayerId, Player.Effects);
                        }
                    }
                }
                if (Player.CafePC <= CafeEnum.None || !ComDiv.UpdateDB("accounts", "pc_cafe", (object)0, "player_id", (object)Player.PlayerId))
                    return;
                Player.CafePC = CafeEnum.None;
                if (!string.IsNullOrEmpty(A_1) && ComDiv.DeleteDB("player_vip", "owner_id", (object)Player.PlayerId))
                    CLogger.Print($"VIP for UID: {Player.PlayerId} Nick: {Player.Nickname} Deleted Due To {A_1}", LoggerType.Info);
                CLogger.Print($"Player PC Cafe was resetted by default into '{Player.CafePC}'; (UID: {Player.PlayerId} Nick: {Player.Nickname})", LoggerType.Info);
            }
        }

        private static bool StaticMethod0(Account A_0, out string A_1)
        {
            if (A_0.IsGM())
            {
                A_1 = "GM Special Access";
                return true;
            }
            PlayerVip playerVip = DaoManagerSQL.GetPlayerVIP(A_0.PlayerId);
            if (playerVip != null)
            {
                if (playerVip.Expirate >= uint.Parse(DateTimeUtil.Now("yyMMddHHmm")))
                {
                    string str = $"{A_0.CafePC}";
                    if (!playerVip.Benefit.Equals(str) && ComDiv.UpdateDB("player_vip", "last_benefit", (object)str, "owner_id", (object)A_0.PlayerId))
                        playerVip.Benefit = str;
                    A_1 = "Valid Access";
                    return true;
                }
                A_1 = "The Time Has Expired!";
                return false;
            }
            A_1 = "Database Not Found!";
            return false;
        }

        public static void LoadPlayerEquipments(Account Player)
        {
            PlayerEquipment playerEquipmentsDb = DaoManagerSQL.GetPlayerEquipmentsDB(Player.PlayerId);
            if (playerEquipmentsDb == null)
            {
                if (DaoManagerSQL.CreatePlayerEquipmentsDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Equipment!", LoggerType.Warning);
            }
            else
                Player.Equipment = playerEquipmentsDb;
        }

        public static void LoadPlayerCharacters(Account Player)
        {
            List<CharacterModel> playerCharactersDb = DaoManagerSQL.GetPlayerCharactersDB(Player.PlayerId);
            if (playerCharactersDb.Count <= 0)
                return;
            Player.Character.Characters = playerCharactersDb;
        }

        public static void LoadPlayerStatistic(Account Player)
        {
            StatisticTotal playerStatBasicDb = DaoManagerSQL.GetPlayerStatBasicDB(Player.PlayerId);
            if (playerStatBasicDb == null)
            {
                if (!DaoManagerSQL.CreatePlayerStatBasicDB(Player.PlayerId))
                    CLogger.Print("There was an error when creating Player Basic Statistic!", LoggerType.Warning);
            }
            else
                Player.Statistic.Basic = playerStatBasicDb;
            StatisticSeason playerStatSeasonDb = DaoManagerSQL.GetPlayerStatSeasonDB(Player.PlayerId);
            if (playerStatSeasonDb == null)
            {
                if (!DaoManagerSQL.CreatePlayerStatSeasonDB(Player.PlayerId))
                    CLogger.Print("There was an error when creating Player Season Statistic!", LoggerType.Warning);
            }
            else
                Player.Statistic.Season = playerStatSeasonDb;
            StatisticClan playerStatClanDb = DaoManagerSQL.GetPlayerStatClanDB(Player.PlayerId);
            if (playerStatClanDb == null)
            {
                if (!DaoManagerSQL.CreatePlayerStatClanDB(Player.PlayerId))
                    CLogger.Print("There was an error when creating Player Clan Statistic!", LoggerType.Warning);
            }
            else
                Player.Statistic.Clan = playerStatClanDb;
            StatisticDaily playerStatDailiesDb = DaoManagerSQL.GetPlayerStatDailiesDB(Player.PlayerId);
            if (playerStatDailiesDb != null)
                Player.Statistic.Daily = playerStatDailiesDb;
            else if (!DaoManagerSQL.CreatePlayerStatDailiesDB(Player.PlayerId))
                CLogger.Print("There was an error when creating Player Daily Statistic!", LoggerType.Warning);
            StatisticWeapon playerStatWeaponsDb = DaoManagerSQL.GetPlayerStatWeaponsDB(Player.PlayerId);
            if (playerStatWeaponsDb == null)
            {
                if (!DaoManagerSQL.CreatePlayerStatWeaponsDB(Player.PlayerId))
                    CLogger.Print("There was an error when creating Player Weapon Statistic!", LoggerType.Warning);
            }
            else
                Player.Statistic.Weapon = playerStatWeaponsDb;
            StatisticAcemode playerStatAcemodesDb = DaoManagerSQL.GetPlayerStatAcemodesDB(Player.PlayerId);
            if (playerStatAcemodesDb != null)
                Player.Statistic.Acemode = playerStatAcemodesDb;
            else if (!DaoManagerSQL.CreatePlayerStatAcemodesDB(Player.PlayerId))
                CLogger.Print("There was an error when creating Player Acemode Statistic!", LoggerType.Warning);
            StatisticMercenary playerStatMercenaryDb = DaoManagerSQL.GetPlayerStatMercenaryDB(Player.PlayerId);
            if (playerStatMercenaryDb == null)
            {
                if (!DaoManagerSQL.CreatePlayerStatMercenaryDB(Player.PlayerId))
                    CLogger.Print("There was an error when creating Player Mercenary Statistic!", LoggerType.Warning);
            }
            else
                Player.Statistic.Mercenary = playerStatMercenaryDb;
            StatisticBattlecup playerStatBattlecupDb = DaoManagerSQL.GetPlayerStatBattlecupDB(Player.PlayerId);
            if (playerStatBattlecupDb != null)
            {
                Player.Statistic.Battlecup = playerStatBattlecupDb;
            }
            else
            {
                if (DaoManagerSQL.CreatePlayerStatBattlecupsDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Battlecup Statistic!", LoggerType.Warning);
            }
        }

        public static void LoadPlayerTitles(Account Player)
        {
            PlayerTitles playerTitlesDb = DaoManagerSQL.GetPlayerTitlesDB(Player.PlayerId);
            if (playerTitlesDb != null)
            {
                Player.Title = playerTitlesDb;
            }
            else
            {
                if (DaoManagerSQL.CreatePlayerTitlesDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Title!", LoggerType.Warning);
            }
        }

        public static void LoadPlayerBonus(Account Player)
        {
            PlayerBonus playerBonusDb = DaoManagerSQL.GetPlayerBonusDB(Player.PlayerId);
            if (playerBonusDb != null)
            {
                Player.Bonus = playerBonusDb;
            }
            else
            {
                if (DaoManagerSQL.CreatePlayerBonusDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Bonus!", LoggerType.Warning);
            }
        }

        public static void LoadPlayerFriend(Account Player, bool LoadFulLDatabase)
        {
            List<FriendModel> playerFriendsDb = DaoManagerSQL.GetPlayerFriendsDB(Player.PlayerId);
            if (playerFriendsDb.Count <= 0)
                return;
            Player.Friend.Friends = playerFriendsDb;
            if (!LoadFulLDatabase)
                return;
            AccountManager.GetFriendlyAccounts(Player.Friend);
        }

        public static void LoadPlayerEvent(Account Player)
        {
            PlayerEvent playerEventDb = DaoManagerSQL.GetPlayerEventDB(Player.PlayerId);
            if (playerEventDb == null)
            {
                if (DaoManagerSQL.CreatePlayerEventDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Event!", LoggerType.Warning);
            }
            else
                Player.Event = playerEventDb;
        }

        public static void LoadPlayerConfig(Account Player)
        {
            PlayerConfig playerConfigDb = DaoManagerSQL.GetPlayerConfigDB(Player.PlayerId);
            if (playerConfigDb != null)
            {
                Player.Config = playerConfigDb;
            }
            else
            {
                if (DaoManagerSQL.CreatePlayerConfigDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Config!", LoggerType.Warning);
            }
        }

        public static void LoadPlayerQuickstarts(Account Player)
        {
            List<QuickstartModel> playerQuickstartsDb = DaoManagerSQL.GetPlayerQuickstartsDB(Player.PlayerId);
            if (playerQuickstartsDb.Count > 0)
            {
                Player.Quickstart.Quickjoins = playerQuickstartsDb;
            }
            else
            {
                if (DaoManagerSQL.CreatePlayerQuickstartsDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Quickstarts!", LoggerType.Warning);
            }
        }

        public static void LoadPlayerReport(Account Player)
        {
            PlayerReport playerReportDb = DaoManagerSQL.GetPlayerReportDB(Player.PlayerId);
            if (playerReportDb != null)
            {
                Player.Report = playerReportDb;
            }
            else
            {
                if (DaoManagerSQL.CreatePlayerReportDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Report!", LoggerType.Warning);
            }
        }

        public static void LoadPlayerBattlepass(Account Player)
        {
            PlayerBattlepass playerBattlepassDb = DaoManagerSQL.GetPlayerBattlepassDB(Player.PlayerId);
            if (playerBattlepassDb != null)
            {
                Player.Battlepass = playerBattlepassDb;
            }
            else
            {
                if (DaoManagerSQL.CreatePlayerBattlepassDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Battlepass!", LoggerType.Warning);
            }
        }

        public static void LoadPlayerCompetitive(Account Player)
        {
            PlayerCompetitive playerCompetitiveDb = DaoManagerSQL.GetPlayerCompetitiveDB(Player.PlayerId);
            if (playerCompetitiveDb != null)
            {
                Player.Competitive = playerCompetitiveDb;
            }
            else
            {
                if (DaoManagerSQL.CreatePlayerCompetitiveDB(Player.PlayerId))
                    return;
                CLogger.Print("There was an error when creating Player Competitive!", LoggerType.Warning);
            }
        }

        public static bool DiscountPlayerItems(Account Player)
        {
            try
            {
                bool flag = false;
                uint uint32 = Convert.ToUInt32(DateTimeUtil.Now("yyMMddHHmm"));
                List<object> objectList = new List<object>();
                int bonuses = Player.Bonus != null ? Player.Bonus.Bonuses : 0;
                int freePass = Player.Bonus != null ? Player.Bonus.FreePass : 0;
                lock (Player.Inventory.Items)
                {
                    for (int index = 0; index < Player.Inventory.Items.Count; ++index)
                    {
                        ItemsModel itemsModel = Player.Inventory.Items[index];
                        if (itemsModel.Count <= uint32 && itemsModel.Equip == ItemEquipType.Temporary)
                        {
                            if (itemsModel.Category == ItemCategory.Coupon)
                            {
                                if (Player.Bonus != null)
                                {
                                    if (!Player.Bonus.RemoveBonuses(itemsModel.Id))
                                    {
                                        if (itemsModel.Id != 1600014)
                                        {
                                            if (itemsModel.Id != 1600006)
                                            {
                                                if (itemsModel.Id != 1600009)
                                                {
                                                    if (itemsModel.Id != 1600010)
                                                    {
                                                        if (itemsModel.Id == 1600187)
                                                        {
                                                            ComDiv.UpdateDB("player_bonus", "muzzle_color", (object)0, "owner_id", (object)Player.PlayerId);
                                                            Player.Bonus.MuzzleColor = 0;
                                                        }
                                                        else if (itemsModel.Id == 1600205)
                                                        {
                                                            ComDiv.UpdateDB("player_bonus", "nick_border_color", (object)0, "owner_id", (object)Player.PlayerId);
                                                            Player.Bonus.NickBorderColor = 0;
                                                        }
                                                    }
                                                    else if (Player.Bonus.FakeNick.Length > 0)
                                                    {
                                                        ComDiv.UpdateDB("player_bonus", "fake_nick", (object)"", "owner_id", (object)Player.PlayerId);
                                                        ComDiv.UpdateDB("accounts", "nickname", (object)Player.Bonus.FakeNick, "player_id", (object)Player.PlayerId);
                                                        Player.Nickname = Player.Bonus.FakeNick;
                                                        Player.Bonus.FakeNick = "";
                                                    }
                                                }
                                                else
                                                {
                                                    ComDiv.UpdateDB("player_bonus", "fake_rank", (object)RankDomain.NoFakeRank, "owner_id", (object)Player.PlayerId);
                                                    Player.Bonus.FakeRank = RankDomain.NoFakeRank;
                                                }
                                            }
                                            else
                                            {
                                                ComDiv.UpdateDB("accounts", "nick_color", (object)0, "player_id", (object)Player.PlayerId);
                                                Player.NickColor = 0;
                                            }
                                        }
                                        else
                                        {
                                            ComDiv.UpdateDB("player_bonus", "crosshair_color", (object)4, "owner_id", (object)Player.PlayerId);
                                            Player.Bonus.CrosshairColor = 4;
                                        }
                                    }
                                    CouponFlag couponEffect = CouponEffectXML.GetCouponEffect(itemsModel.Id);
                                    if (couponEffect != null && couponEffect.EffectFlag > (CouponEffects)0 && Player.Effects.HasFlag((Enum)couponEffect.EffectFlag))
                                    {
                                        Player.Effects -= couponEffect.EffectFlag;
                                        flag = true;
                                    }
                                }
                                else
                                    continue;
                            }
                            objectList.Add((object)itemsModel.ObjectId);
                            Player.Inventory.Items.RemoveAt(index--);
                        }
                        else if (itemsModel.Count == 0U)
                        {
                            objectList.Add((object)itemsModel.ObjectId);
                            Player.Inventory.Items.RemoveAt(index--);
                        }
                    }
                }
                if (objectList.Count > 0)
                {
                    for (int index = 0; index < objectList.Count; ++index)
                    {
                        ItemsModel itemsModel = Player.Inventory.GetItem((long)objectList[index]);
                        if (itemsModel != null && itemsModel.Category == ItemCategory.Character && ComDiv.GetIdStatics(itemsModel.Id, 1) == 6)
                            AllUtils.StaticMethod1(Player, itemsModel.Id);
                    }
                    ComDiv.DeleteDB("player_items", "object_id", objectList.ToArray(), "owner_id", (object)Player.PlayerId);
                }
                objectList.Clear();
                if (Player.Bonus != null && (Player.Bonus.Bonuses != bonuses || Player.Bonus.FreePass != freePass))
                    DaoManagerSQL.UpdatePlayerBonus(Player.PlayerId, Player.Bonus.Bonuses, Player.Bonus.FreePass);
                if (Player.Effects < (CouponEffects)0)
                    Player.Effects = (CouponEffects)0;
                if (flag)
                    ComDiv.UpdateDB("accounts", "coupon_effect", (object)(long)Player.Effects, "player_id", (object)Player.PlayerId);
                int num = ComDiv.CheckEquipedItems(Player.Equipment, Player.Inventory.Items, false);
                if (num > 0)
                {
                    DBQuery Query = new DBQuery();
                    if ((num & 2) == 2)
                        ComDiv.UpdateWeapons(Player.Equipment, Query);
                    if ((num & 1) == 1)
                        ComDiv.UpdateChars(Player.Equipment, Query);
                    if ((num & 3) == 3)
                        ComDiv.UpdateItems(Player.Equipment, Query);
                    ComDiv.UpdateDB("player_equipments", "owner_id", (object)Player.PlayerId, Query.GetTables(), Query.GetValues());
                }
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        private static void StaticMethod1(Account A_0, int A_1)
        {
            CharacterModel character1 = A_0.Character.GetCharacter(A_1);
            if (character1 == null)
                return;
            int Slot = 0;
            foreach (CharacterModel character2 in A_0.Character.Characters)
            {
                if (character2.Slot != character1.Slot)
                {
                    character2.Slot = Slot;
                    DaoManagerSQL.UpdatePlayerCharacter(Slot, character2.ObjectId, A_0.PlayerId);
                    ++Slot;
                }
            }
            if (!DaoManagerSQL.DeletePlayerCharacter(character1.ObjectId, A_0.PlayerId))
                return;
            A_0.Character.RemoveCharacter(character1);
        }

        public static void CheckGameEvents(Account Player)
        {
            uint[] numArray = new uint[2]
            {
      uint.Parse(DateTimeUtil.Now("yyMMddHHmm")),
      uint.Parse(DateTimeUtil.Now("yyMMdd"))
            };
            PlayerEvent playerEvent = Player.Event;
            if (playerEvent != null)
            {
                EventQuestModel runningEvent1 = EventQuestXML.GetRunningEvent();
                if (runningEvent1 != null)
                {
                    uint lastQuestDate = playerEvent.LastQuestDate;
                    int lastQuestFinish = playerEvent.LastQuestFinish;
                    if (playerEvent.LastQuestDate < runningEvent1.BeginDate)
                    {
                        playerEvent.LastQuestDate = 0U;
                        playerEvent.LastQuestFinish = 0;
                    }
                    if (playerEvent.LastQuestFinish == 0)
                    {
                        if (!Player.Mission.OwnsCardSet(13))
                        {
                            int slot = Player.Mission.FindFreeSlot();
                            if (slot >= 0)
                                Player.Mission[slot].CardSetId = 13;
                        }
                        if (playerEvent.LastQuestDate == 0U)
                            playerEvent.LastQuestDate = numArray[0];
                    }
                    if ((int)playerEvent.LastQuestDate != (int)lastQuestDate || playerEvent.LastQuestFinish != lastQuestFinish)
                        EventQuestXML.ResetPlayerEvent(Player.PlayerId, playerEvent);
                }
                EventLoginModel runningEvent2 = EventLoginXML.GetRunningEvent();
                if (runningEvent2 != null)
                {
                    if (playerEvent.LastLoginDate < runningEvent2.BeginDate)
                    {
                        playerEvent.LastLoginDate = 0U;
                        ComDiv.UpdateDB("player_events", "last_login_date", (object)0, "owner_id", (object)Player.PlayerId);
                    }
                    if (uint.Parse($"{DateTimeUtil.Convert($"{playerEvent.LastLoginDate}"):yyMMdd}") < numArray[1])
                    {
                        bool delivered = false;
                        bool failed = false;
                        foreach (int good1 in runningEvent2.Goods)
                        {
                            GoodsItem good2 = ShopManager.GetGood(good1);
                            if (good2 == null)
                            {
                                CLogger.Print($"[EventLogin] Good {good1} is not in the shop; login gift withheld for UID {Player.PlayerId}.", LoggerType.Warning);
                                failed = true;
                                continue;
                            }
                            if (ComDiv.TryCreateItem(new ItemsModel(good2.Item), Player.Inventory, Player.PlayerId))
                                delivered = true;
                            else
                            {
                                CLogger.Print($"[EventLogin] Failed to grant good {good1} to UID {Player.PlayerId}; login gift withheld.", LoggerType.Warning);
                                failed = true;
                            }
                        }
                        // Marcar la fecha solo cuando todo el lote entró: de lo contrario el
                        // jugador quedaría premiado sin haber recibido nada.
                        if (delivered && !failed)
                        {
                            playerEvent.LastLoginDate = numArray[0];
                            ComDiv.UpdateDB("player_events", "last_login_date", (object)(long)numArray[0], "owner_id", (object)Player.PlayerId);
                            Player.SendPacket((AuthServerPacket)new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("LoginGiftMessage")));
                        }
                    }
                }
                EventVisitModel runningEvent3 = EventVisitXML.GetRunningEvent();
                if (runningEvent3 != null && playerEvent.LastVisitDate < runningEvent3.BeginDate)
                {
                    playerEvent.LastVisitDate = 0U;
                    playerEvent.LastVisitCheckDay = 0;
                    playerEvent.LastVisitSeqType = 0;
                    playerEvent.LastVisitRewardDay = 0;
                    EventVisitXML.ResetPlayerEvent(Player.PlayerId, playerEvent);
                }
                EventXmasModel runningEvent4 = EventXmasXML.GetRunningEvent();
                if (runningEvent4 != null && playerEvent.LastXmasDate < runningEvent4.BeginDate)
                {
                    playerEvent.LastXmasDate = 0U;
                    ComDiv.UpdateDB("player_events", "last_xmas_date", (object)0, "owner_id", (object)Player.PlayerId);
                }
                EventPlaytimeModel EvPlaytime = EventPlaytimeJSON.GetRunningEvent();
                if (EvPlaytime != null)
                {
                    // Obtener fechas para comparación (sin hora/minutos)
                    uint currentDate = uint.Parse(DateTimeUtil.Now("yyMMdd") + "0000");
                    uint lastPlaytimeDay = playerEvent.LastPlaytimeDate > 0 ? uint.Parse($"{DateTimeUtil.Convert($"{playerEvent.LastPlaytimeDate}"):yyMMdd}0000") : 0;
                    uint eventBeginDate = EvPlaytime.BeginDate / 10000 * 10000; // Normalizar fecha de inicio

                    // 1. Si es un evento completamente nuevo o diferente
                    bool isNewEvent = playerEvent.LastPlaytimeDate < EvPlaytime.BeginDate;
                    bool isDifferentEvent = playerEvent.CurrentPlaytimeEventId != EvPlaytime.Id && playerEvent.CurrentPlaytimeEventId != 0;

                    if (isNewEvent || isDifferentEvent)
                    {
                        playerEvent.LastPlaytimeDate = uint.Parse(DateTimeUtil.Now("yyMMdd") + "0000");
                        playerEvent.LastPlaytimeFinish = 0;
                        playerEvent.LastPlaytimeValue = 0;
                        playerEvent.CurrentPlaytimeEventId = EvPlaytime.Id;

                        // Resetear niveles completados solo si es un evento completamente nuevo
                        if (isNewEvent)
                        {
                            playerEvent.PlaytimeCompletedLevels = "";
                        }

                        //Console.WriteLine($"[Server.Auth] Nuevo evento de tiempo detectado (ID: {EvPlaytime.Id}) - Reseteando jugador {Player.PlayerId}");
                        EventPlaytimeJSON.ResetPlayerEvent(Player.PlayerId, playerEvent);
                    }
                    // 2. Si es un nuevo día
                    else if (lastPlaytimeDay > 0 && lastPlaytimeDay < currentDate)
                    {
                        bool eventCompleted = playerEvent.LastPlaytimeFinish == EvPlaytime.Id;

                        // Usar la propiedad Period del evento para determinar el comportamiento
                        if (EvPlaytime.Period)
                        {
                            // EVENTO PERIÓDICO: Reseteo diario completo - Se puede repetir cada día
                            playerEvent.LastPlaytimeDate = uint.Parse(DateTimeUtil.Now("yyMMdd") + "0000");
                            playerEvent.LastPlaytimeValue = 0;
                            playerEvent.LastPlaytimeFinish = 0; // Resetear para permitir completar de nuevo
                            playerEvent.PlaytimeCompletedLevels = ""; // Resetear niveles completados

                            //Console.WriteLine($"[Server.Auth] Reseteo diario (evento periódico) - Evento ID: {EvPlaytime.Id}, Jugador: {Player.PlayerId}");
                            //Console.WriteLine($"[Server.Auth] Fecha anterior: {lastPlaytimeDay}, Fecha actual: {currentDate}");
                            EventPlaytimeJSON.ResetPlayerEvent(Player.PlayerId, playerEvent); // true = resetear niveles completados
                        }
                    }
                    // 3. Si nunca ha participado en este evento
                    else if (playerEvent.LastPlaytimeDate == 0)
                    {
                        playerEvent.LastPlaytimeDate = uint.Parse(DateTimeUtil.Now("yyMMdd") + "0000");
                        playerEvent.LastPlaytimeFinish = 0;
                        playerEvent.LastPlaytimeValue = 0;
                        playerEvent.CurrentPlaytimeEventId = EvPlaytime.Id;

                        //Console.WriteLine($"[Server.Auth] Primera participación en evento (ID: {EvPlaytime.Id}) para jugador {Player.PlayerId}");
                        EventPlaytimeJSON.ResetPlayerEvent(Player.PlayerId, playerEvent);
                    }
                }
            }
            ComDiv.UpdateDB("accounts", "last_login_date", (object)(long)numArray[0], "player_id", (object)Player.PlayerId);
        }

        public static void ProcessBattlepass(Account Player)
        {
            try
            {
                BattlePassSeason activeSeason = BattlePassLoader.GetActiveSeasons();
                if (Player.Battlepass != null && activeSeason != null)
                {
                    if (Player.Battlepass.BattlepassId != int.Parse(activeSeason.SeasonId))
                    {
                        if (Player.Battlepass.BattlepassId != 0)
                        {
                            // Real season change: wipe last season's progress (incl. daily points).
                            Player.Battlepass.BattlepassId = int.Parse(activeSeason.SeasonId);
                            Player.Battlepass.HavePremium = false;
                            Player.Battlepass.BattlepassNormalLevel = 0;
                            Player.Battlepass.BattlepassPremiumLevel = 0;
                            Player.Battlepass.EarnedPoints = 0;
                            Player.Battlepass.DailyPoints = 0;
                            Player.Battlepass.LastRecord = uint.Parse(DateTime.Now.ToString("yyyyMMdd"));
                            ComDiv.UpdateDB("player_battlepass", "owner_id", Player.PlayerId, new string[] { "battlepass_id", "battlepass_premium_levels", "battlepass_normal_levels", "battlepass_premium", "earned_points", "points", "last_record" }, new object[] { int.Parse(activeSeason.SeasonId), 0, 0, false, 0, 0, (int)Player.Battlepass.LastRecord });
                        }
                        else
                        {
                            // New player (id 0): adopt the active season, keep progress.
                            Player.Battlepass.BattlepassId = int.Parse(activeSeason.SeasonId);
                            ComDiv.UpdateDB("player_battlepass", "owner_id", Player.PlayerId, new string[] { "battlepass_id" }, new object[] { int.Parse(activeSeason.SeasonId) });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.ToString(), LoggerType.Error);
            }
        }

        public static long LoadCouponEffects(Account Player)
        {
            long num = 0;
            foreach ((CouponEffects, long) valueTuple in new List<(CouponEffects, long)>()
    {
      (CouponEffects.Ammo40, 1L),
      (CouponEffects.Ammo10, 2L),
      (CouponEffects.GetDroppedWeapon, 4L),
      (CouponEffects.QuickChangeWeapon, 16L /*0x10*/),
      (CouponEffects.QuickChangeReload, 128L /*0x80*/),
      (CouponEffects.Invincible, 512L /*0x0200*/),
      (CouponEffects.FullMetalJack, 2048L /*0x0800*/),
      (CouponEffects.HollowPoint, 8192L /*0x2000*/),
      (CouponEffects.HollowPointPlus, 32768L /*0x8000*/),
      (CouponEffects.C4SpeedKit, 65536L /*0x010000*/),
      (CouponEffects.ExtraGrenade, 131072L /*0x020000*/),
      (CouponEffects.ExtraThrowGrenade, 262144L /*0x040000*/),
      (CouponEffects.JackHollowPoint, 524288L /*0x080000*/),
      (CouponEffects.HP5, 1048576L /*0x100000*/),
      (CouponEffects.HP10, 2097152L /*0x200000*/),
      (CouponEffects.Defense5, 4194304L /*0x400000*/),
      (CouponEffects.Defense10, 8388608L /*0x800000*/),
      (CouponEffects.Defense20, 16777216L /*0x01000000*/),
      (CouponEffects.Defense90, 33554432L /*0x02000000*/),
      (CouponEffects.Respawn20, 67108864L /*0x04000000*/),
      (CouponEffects.Respawn30, 268435456L /*0x10000000*/),
      (CouponEffects.Respawn50, 1073741824L /*0x40000000*/),
      (CouponEffects.Respawn100, 8589934592L /*0x0200000000*/),
      (CouponEffects.Camoflage50, 34359738368L /*0x0800000000*/),
      (CouponEffects.Camoflage99, 68719476736L /*0x1000000000*/)
    })
            {
                if (Player.Effects.HasFlag((Enum)valueTuple.Item1))
                    num += valueTuple.Item2;
            }
            return num;
        }

        //public static List<ItemsModel> LimitationIndex(Account Player, List<ItemsModel> Items)
        //{
        //    int limit = 600 + Player.InventoryPlus;
        //    if (Items.Count > limit)
        //    {
        //        Items.RemoveRange(limit, Items.Count - limit);
        //    }
        //    return Items;
        //}

        public static List<ItemsModel> LimitationIndex(Account Player, List<ItemsModel> Items)
        {
            int num = 600 + Player.InventoryPlus;
            if (Items.Count > num)
            {
                int index = num / 3;
                if (Items.Count > index)
                    Items.RemoveRange(index, Items.Count - num);
            }
            return Items;
        }

        private static void StaticMethod2(Account A_0)
        {
            List<ItemsModel> items = A_0.Inventory.Items;
            lock (items)
            {
                foreach (ItemsModel itemsModel in items)
                {
                    if (ComDiv.GetIdStatics(itemsModel.Id, 1) == 6 && A_0.Character.GetCharacter(itemsModel.Id) == null)
                        AllUtils.CreateCharacter(A_0, itemsModel);
                }
            }
        }

        public static void CreateCharacter(Account Player, ItemsModel Item)
        {
            int count = Player.Character.Characters.Count;
            CharacterModel characterModel1 = new CharacterModel();
            characterModel1.ObjectId = Item.ObjectId;
            characterModel1.Id = Item.Id;
            characterModel1.Name = Item.Name;
            int num1 = count;
            int num2 = num1 + 1;
            characterModel1.Slot = num1;
            characterModel1.CreateDate = uint.Parse(DateTimeUtil.Now("yyMMddHHmm"));
            characterModel1.PlayTime = 0U;
            CharacterModel characterModel2 = characterModel1;
            Player.Character.AddCharacter(characterModel2);
            if (DaoManagerSQL.CreatePlayerCharacter(characterModel2, Player.PlayerId))
                return;
            CLogger.Print($"There is an error while cheating a character! (ID: {Item.Id}", LoggerType.Warning);
        }

        public static uint GetFeatures()
        {
            AccountFeatures features = AccountFeatures.ALL;
            if (!AuthXender.Client.Config.EnableClan)
                features -= AccountFeatures.CLAN_ONLY;
            if (!AuthXender.Client.Config.EnableTicket)
                features -= AccountFeatures.TICKET_ONLY;
            if (!AuthXender.Client.Config.EnableTags)
                features -= AccountFeatures.TAGS_ONLY;
            EventPlaytimeModel runningEvent = EventPlaytimeJSON.GetRunningEvent();
            if (!AuthXender.Client.Config.EnablePlaytime || runningEvent == null || !runningEvent.EventIsEnabled())
                features -= AccountFeatures.PLAYTIME_ONLY;
            return (uint)features;
        }

        public static uint ValidateKey(long PlayerId, int SessionId, uint Unknown)
        {
            return uint.Parse($"{(int)(Unknown % 999U):000}{(int)(PlayerId % 999L):000}{SessionId % 999:000}");
        }
    }
}