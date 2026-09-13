using Npgsql;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Managers
{
    /// <summary>
    /// Gestor de la tienda del juego - maneja items, precios, reparaciones y efectos
    /// </summary>
    public static class ShopManager
    {
        #region Public Static Collections

        public static List<ItemsRepair> ItemRepairs = new List<ItemsRepair>();
        public static List<GoodsItem> ShopAllList = new List<GoodsItem>();
        public static List<GoodsItem> ShopBuyableList = new List<GoodsItem>();
        public static SortedList<int, GoodsItem> ShopUniqueList = new SortedList<int, GoodsItem>();

        // Goods realmente empacotados para o cliente (pos filtro de Visibility, pos corte
        // de MAX_BUY_INFO_PER_ITEM por item e pos truncagem em MAX_SHOP_GOODS). GetGood()
        // varre ShopAllList e enxerga variantes que nunca chegam ao cliente; quem monta
        // pacote com good_id tem que consultar este conjunto, nao ShopAllList.
        private static HashSet<int> PackedGoodsIds = new HashSet<int>();

        public static List<ShopData> ShopDataMt1 = new List<ShopData>();
        public static List<ShopData> ShopDataMt2 = new List<ShopData>();
        public static List<ShopData> ShopDataGoods = new List<ShopData>();
        public static List<ShopData> ShopDataItems = new List<ShopData>();
        public static List<ShopData> ShopDataItemRepairs = new List<ShopData>();

        public static List<ItemsLimited> ItemLimited = new List<ItemsLimited>();

        public static byte[] ShopTagData;

        // Distinct non-zero shop_tag values loaded from system_shop / system_shop_effects.
        // Used exclusively to build PROTOCOL_SHOP_TAG_INFO_ACK — never hardcoded.
        public static List<byte> ShopTagList = new List<byte>();
        private static readonly HashSet<byte> LoadedShopTags = new HashSet<byte>();

        // 121: zlib-compressed 210-byte SHOP_GOODS_EXPANSION records (PACKED_GOODSLIST opcode 1037)
        public static byte[] PackedGoodsBuffer;
        public static byte[] PackedItemsBuffer;
        public static byte[] PackedRepairsBuffer;
        public static int PackedGoodsCount;
        public static int PackedItemsCount;
        public static int PackedRepairsCount;
        public static byte[] PackedMatching1Buffer;
        public static byte[] PackedMatching2Buffer;
        public static int PackedMatching1Count;
        public static int PackedMatching2Count;
        public static int CatalogVersion;

        // Flash sale (client 1120): goodId = itemId * 100 + variantCode, max 3 slots.
        // Source of truth is system_shop_limited; edit those rows to change the sale.
        public static int[] FlashSaleGoodIds = new int[0];
        public static uint FlashSaleStartDate;
        public static int FlashSaleEndMinute;

        #endregion

        #region Public Static Counters

        public static int TotalGoods;
        public static int TotalItems;
        public static int TotalMatching1;
        public static int TotalMatching2;
        public static int TotalRepairs;
        public static int Set4p;

        #endregion

        #region Utility Methods

        /// <summary>
        /// Divide una lista en sublistas de tamaño específico
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int limit)
        {
            return list
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / limit)
                .Select(group => group.Select(x => x.item));
        }

        #endregion

        #region Main Load Method

        /// <summary>
        /// Carga todos los datos de la tienda desde la base de datos
        /// </summary>
        /// <param name="Type">1 = carga completa, 2 = solo items tipo 16</param>

        public static void Load(int Type)
        {
            // Arma Especial 2 depende do client (ItemGroup.dat); mantém os goods
            // invisíveis para não deixar cards sem PEF na loja.
            if (Type == 1)
                InventoryUnlocks.ResetThrow2ShopCatalog();

            LoadRepairableItems(Type);
            LoadShopItems(Type);
            LoadShopEffects(Type);
            LoadShopSets(Type);
            LoadLimitedItems(); 
            LoadFlashSale();

            // Always rebuild tag cache from DB-loaded shop_tag values, including Load(2).
            BuildShopTagData();

            if (Type != 1)
                return;

            try
            {
                BuildMatchingAndGoodsData(0);
                BuildMatchingData2(1);
                BuildUniqueItemsData();
                BuildRepairItemsData();
                BuildPackedGoodsData();
                BuildPackedCatalogData();
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }

            CLogger.Print($"Plugin carregado: {ShopBuyableList.Count} itens compraveis", LoggerType.Info);
            CLogger.Print($"Plugin carregado: {ItemRepairs.Count} itens reparaveis", LoggerType.Info);
            CLogger.Print($"Plugin carregado: {ItemLimited.Count} itens limitados", LoggerType.Info);
        }

        #endregion

        #region Database Loading Methods

        /// <summary>
        /// Carga items de la tienda desde system_shop
        /// </summary>

        private static void LoadShopItems(int loadType)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM system_shop";
                    cmd.CommandType = CommandType.Text;

                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

                    while (reader.Read())
                    {
                        int itemId = int.Parse($"{reader["item_id"]}");

                        // Parsear listas de cantidades, precios en cash y gold
                        string[] countList = ParseCommaSeparatedValue($"{reader["item_count_list"]}");
                        string[] cashPriceList = ParseCommaSeparatedValue($"{reader["price_cash_list"]}");
                        string[] goldPriceList = ParseCommaSeparatedValue($"{reader["price_gold_list"]}");
                        string[] variantCodeList = ParseCommaSeparatedValue($"{reader["variant_code_list"]}");

                        // Debug logging for specific problem items
                        if (itemId == 103274 || itemId == 104286 || itemId == 104288 || itemId == 105167 || itemId == 301146 || itemId == 800323 || itemId == 103918)
                        {
                            //CLogger.Print($"[DEBUG-ITEM] Processing {itemId}: count='{reader["item_count_list"]}' cash='{reader["price_cash_list"]}' gold='{reader["price_gold_list"]}'", LoggerType.Debug);
                            //CLogger.Print($"[DEBUG-ITEM] Parsed lengths for {itemId}: count={countList.Length}, cash={cashPriceList.Length}, gold={goldPriceList.Length}", LoggerType.Debug);
                        }

                        // Auto-pad shorter price lists with 0s to match count list length
                        int expectedLength = countList.Length;
                        if (cashPriceList.Length < expectedLength)
                        {
                            string[] paddedCash = new string[expectedLength];
                            for (int i = 0; i < expectedLength; i++)
                                paddedCash[i] = i < cashPriceList.Length ? cashPriceList[i] : "0";
                            cashPriceList = paddedCash;
                        }
                        if (goldPriceList.Length < expectedLength)
                        {
                            string[] paddedGold = new string[expectedLength];
                            for (int i = 0; i < expectedLength; i++)
                                paddedGold[i] = i < goldPriceList.Length ? goldPriceList[i] : "0";
                            goldPriceList = paddedGold;
                        }

                        // The 121 client renders at most MAX_BUY_INFO_PER_ITEM duration slots per
                        // item. Junk variants with no price only exist to pad the list; drop them
                        // when the item has any real (priced) variant, and cap the rest below, so a
                        // single item never overflows the client's per-tab buy-info array (crash).
                        bool hasPricedVariant = false;
                        for (int p = 0; p < expectedLength; p++)
                        {
                            int.TryParse(p < cashPriceList.Length ? cashPriceList[p] : "0", out int pc);
                            int.TryParse(p < goldPriceList.Length ? goldPriceList[p] : "0", out int pg);
                            if (pc > 0 || pg > 0) { hasPricedVariant = true; break; }
                        }
                        int buyableEmitted = 0;

                        int variantIndex = 0;
                        foreach (string countStr in countList)
                        {
                            variantIndex++;

                            if (!uint.TryParse(countStr, out uint itemCount))
                            {
                                CLogger.Print($"Loading goods with count != UInt ({itemId})", LoggerType.Warning);
                                continue;
                            }

                            if (!int.TryParse(cashPriceList[variantIndex - 1], out int cashPrice))
                            {
                                CLogger.Print($"Loading goods with cash != Int ({itemId})", LoggerType.Warning);
                                continue;
                            }

                            if (!int.TryParse(goldPriceList[variantIndex - 1], out int goldPrice))
                            {
                                CLogger.Print($"Loading goods with gold != Int ({itemId})", LoggerType.Warning);
                                continue;
                            }

                            int itemCategory = ComDiv.GetIdStatics(itemId, 1);
                            string itemName = $"{reader["item_name"]}";

                            // Generar ID único del good (item + variante)
                            bool isSpecialCategory = itemCategory == 22 || itemCategory == 26 ||
                                                     itemCategory == 36 || itemCategory == 37 || itemCategory == 40;
                            // Faithful port: use the real Shop.dat period-code suffix (04=1d,06=3d,08=7d,12=30d...)
                            // so the server GoodsItem.Id matches exactly what the client sends. Fall back to the
                            // legacy sequential/special scheme when variant_code_list is absent (un-migrated rows).
                            string variantSuffix;
                            if (variantCodeList.Length >= variantIndex &&
                                int.TryParse(variantCodeList[variantIndex - 1], out int variantCode))
                                variantSuffix = $"{variantCode:D2}";
                            else
                                variantSuffix = isSpecialCategory ? "00" : $"{variantIndex:D2}";
                            int goodId = int.Parse($"{itemId}{variantSuffix}");

                            GoodsItem good = new GoodsItem()
                            {
                                Id = goodId,
                                PriceGold = goldPrice,
                                PriceCash = cashPrice
                            };

                            // Set Visibility FIRST - needed to skip discount for limited items
                            bool isItemVisible = bool.Parse($"{reader["item_visible"]}");
                            good.Visibility = isItemVisible ? 0 : 4;

                            // Aplicar descuento si existe - BUT NOT for limited items (Visibility=4)
                            int discountPercent = int.Parse($"{reader["discount_percent"]}");
                            
                            // Skip discount for limited items (item_visible=false)
                            if (good.Visibility == 4)
                            {
                                discountPercent = 0; // No discount for limited items
                            }
                            
                            if (discountPercent > 0 && good.PriceCash > 0)
                            {
                                good.StarCash = good.PriceCash * 255; // Precio original
                                good.PriceCash = ComDiv.Percentage(good.PriceCash, discountPercent);
                            }
                            if (discountPercent > 0 && good.PriceGold > 0)
                            {
                                good.StarGold = good.PriceGold * 255;
                                good.PriceGold = ComDiv.Percentage(good.PriceGold, discountPercent);
                            }

                            // shop_tag from system_shop is the only source for the item tag.
                            // Do not override with Sale on discount — that invents tags not present in DB.
                            int shopTag = int.Parse($"{reader["shop_tag"]}");
                            good.Tag = (ItemTag)shopTag;
                            RegisterLoadedShopTag(shopTag);
                            good.Title = int.Parse($"{reader["title_requi"]}");
                            good.AuthType = int.Parse($"{reader["item_consume"]}");
                            good.BuyType2 = good.AuthType == 2 ? 1 : (IsRepairableItem(itemId) ? 2 : 1);
                            good.BuyType3 = good.AuthType == 1 ? 2 : 1;

                            good.Item.SetItemId(itemId);
                            good.Item.Name = good.AuthType == 1
                                ? $"{itemName} ({itemCount} qty)"
                                : (good.AuthType == 2 ? $"{itemName} ({itemCount / 3600U} hours)" : itemName);
                            good.Item.Count = itemCount;

                            // Debug logging for problem items - show final prices
                            if (itemId == 103274 || itemId == 104286 || itemId == 104288)
                            {
                                //CLogger.Print($"[DEBUG-PRICE] Item {itemId} Variant {variantIndex}: GoodId={good.Id} Cash={good.PriceCash} Gold={good.PriceGold} AuthType={good.AuthType} Visibility={good.Visibility}", LoggerType.Debug);
                            }

                            int itemType = ComDiv.GetIdStatics(good.Item.Id, 1);

                            switch (loadType)
                            {
                                case 1:
                                    ShopAllList.Add(good);
                                    if (good.Visibility != 2 && good.Visibility != 4)
                                    {
                                        bool junkVariant = good.PriceCash == 0 && good.PriceGold == 0 && hasPricedVariant;
                                        if (!junkVariant && buyableEmitted < MAX_BUY_INFO_PER_ITEM)
                                        {
                                            ShopBuyableList.Add(good);
                                            buyableEmitted++;
                                        }
                                    }
                                    if (!ShopUniqueList.ContainsKey(good.Item.Id) && good.AuthType > 0)
                                    {
                                        ShopUniqueList.Add(good.Item.Id, good);
                                        if (good.Visibility == 4)
                                        {
                                            Set4p++;
                                            //CLogger.Print($"[LIMITED-LOAD] GoodId={good.Id} ItemId={good.Item.Id} Cash={good.PriceCash} Gold={good.PriceGold} Visibility={good.Visibility}", LoggerType.Debug);
                                        }
                                    }
                                    break;
                                case 2:
                                    // Include both itemType 16 AND limited items (Visibility=4)
                                    if (itemType == 16 || good.Visibility == 4)
                                        goto case 1;
                                    break;
                            }
                        }
                    }

                    cmd.Dispose();
                    reader.Close();
                    conn.Dispose();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Carga efectos/cupones de la tienda desde system_shop_effects
        /// </summary>

        private static void LoadShopEffects(int loadType)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM system_shop_effects";
                    cmd.CommandType = CommandType.Text;

                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

                    while (reader.Read())
                    {
                        int couponId = int.Parse($"{reader["coupon_id"]}");

                        string[] dayCountList = ParseCommaSeparatedValue($"{reader["coupon_count_day_list"]}");
                        string[] cashPriceList = ParseCommaSeparatedValue($"{reader["price_cash_list"]}");
                        string[] goldPriceList = ParseCommaSeparatedValue($"{reader["price_gold_list"]}");

                        if (dayCountList.Length != cashPriceList.Length || cashPriceList.Length != goldPriceList.Length)
                        {
                            //CLogger.Print($"Loading goods with invalid counts / moneys / points sizes. ({couponId})", LoggerType.Warning);
                            continue;
                        }

                        int variantIndex = 0;
                        foreach (string dayStr in dayCountList)
                        {
                            variantIndex++;

                            if (!int.TryParse(dayStr, out int dayCount))
                            {
                                CLogger.Print($"Loading effects with count != Int ({couponId})", LoggerType.Warning);
                                continue;
                            }

                            if (!int.TryParse(cashPriceList[variantIndex - 1], out int cashPrice))
                            {
                                CLogger.Print($"Loading effects with cash != Int ({couponId})", LoggerType.Warning);
                                continue;
                            }

                            if (!int.TryParse(goldPriceList[variantIndex - 1], out int goldPrice))
                            {
                                CLogger.Print($"Loading effects with gold != Int ({couponId})", LoggerType.Warning);
                                continue;
                            }

                            // Limitar días a máximo 100
                            if (dayCount >= 100)
                                dayCount = 100;

                            // Construir ID del item basado en el coupon ID y días
                            string couponIdStr = $"{couponId}";
                            int itemId = int.Parse($"{couponIdStr.Substring(0, 2)}{dayCount:D2}{couponIdStr.Substring(4, 3)}");

                            GoodsItem good = new GoodsItem()
                            {
                                Id = int.Parse($"{couponId}{variantIndex:D2}"),
                                PriceGold = goldPrice,
                                PriceCash = cashPrice
                            };

                            int discountPercent = int.Parse($"{reader["discount_percent"]}");
                            if (discountPercent > 0 && good.PriceCash > 0)
                            {
                                good.StarCash = good.PriceCash * 255;
                                good.PriceCash = ComDiv.Percentage(good.PriceCash, discountPercent);
                            }
                            if (discountPercent > 0 && good.PriceGold > 0)
                            {
                                good.PriceGold *= 255;
                                good.PriceGold = ComDiv.Percentage(good.PriceGold, discountPercent);
                            }

                            // shop_tag from system_shop_effects is the only source for effect tags.
                            int shopTag = int.Parse($"{reader["shop_tag"]}");
                            good.Tag = (ItemTag)shopTag;
                            RegisterLoadedShopTag(shopTag);
                            good.Title = 0;
                            good.AuthType = 1;
                            good.BuyType2 = 1;
                            good.BuyType3 = 2;
                            good.Visibility = bool.Parse($"{reader["coupon_visible"]}") ? 0 : 4;

                            good.Item.SetItemId(itemId);
                            good.Item.Name = $"{reader["coupon_name"]} ({dayCount} days)";
                            good.Item.Count = 1U;

                            int itemCategory = ComDiv.GetIdStatics(good.Item.Id, 1);

                            switch (loadType)
                            {
                                case 1:
                                    ShopAllList.Add(good);
                                    if (good.Visibility != 2 && good.Visibility != 4)
                                        ShopBuyableList.Add(good);
                                    if (!ShopUniqueList.ContainsKey(good.Item.Id) && good.AuthType > 0)
                                    {
                                        ShopUniqueList.Add(good.Item.Id, good);
                                        if (good.Visibility == 4)
                                            Set4p++;
                                    }
                                    break;
                                case 2:
                                    if (itemCategory == 16)
                                        goto case 1;
                                    break;
                            }
                        }
                    }

                    cmd.Dispose();
                    reader.Close();
                    conn.Dispose();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Carga sets/paquetes de items desde system_shop_sets
        /// </summary>

        private static void LoadShopSets(int loadType)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = $"SELECT * FROM system_shop_sets WHERE visible = '{true}';";
                    cmd.CommandType = CommandType.Text;

                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

                    while (reader.Read())
                    {
                        LoadShopSetItems(
                            int.Parse($"{reader["id"]}"),
                            $"{reader["name"]}",
                            loadType
                        );
                    }

                    cmd.Dispose();
                    reader.Close();
                    conn.Dispose();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Carga items individuales de un set
        /// </summary>

        private static void LoadShopSetItems(int setId, string setName, int loadType)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = $"SELECT * FROM system_shop_sets_items WHERE set_id = '{setId}' AND set_name = '{setName}';";
                    cmd.CommandType = CommandType.Text;

                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

                    while (reader.Read())
                    {
                        int itemId = int.Parse($"{reader["id"]}");
                        string itemName = $"{reader["name"]}";
                        int consumeType = int.Parse($"{reader["consume"]}");
                        uint itemCount = uint.Parse($"{reader["count"]}");
                        int goldPrice = int.Parse($"{reader["price_gold"]}");
                        int cashPrice = int.Parse($"{reader["price_cash"]}");

                        // system_shop_sets_items has no shop_tag column — never invent Hot/New/etc.
                        GoodsItem good = new GoodsItem()
                        {
                            Id = setId,
                            PriceGold = goldPrice,
                            PriceCash = cashPrice,
                            Tag = ItemTag.None,
                            Title = 0,
                            AuthType = 0,
                            BuyType2 = 1,
                            BuyType3 = consumeType == 1 ? 2 : 1,
                            Visibility = 4
                        };

                        good.Item.SetItemId(itemId);
                        good.Item.Name = itemName;
                        good.Item.Count = itemCount;

                        int itemCategory = ComDiv.GetIdStatics(good.Item.Id, 1);

                        switch (loadType)
                        {
                            case 1:
                                ShopAllList.Add(good);
                                if (good.Visibility != 2 && good.Visibility != 4)
                                    ShopBuyableList.Add(good);
                                if (!ShopUniqueList.ContainsKey(good.Item.Id) && good.AuthType > 0)
                                {
                                    ShopUniqueList.Add(good.Item.Id, good);
                                    if (good.Visibility == 4)
                                        Set4p++;
                                }
                                break;
                            case 2:
                                if (itemCategory == 16)
                                    goto case 1;
                                break;
                        }
                    }

                    cmd.Dispose();
                    reader.Close();
                    conn.Dispose();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Carga items reparables desde system_shop_repair
        /// </summary>

        private static void LoadRepairableItems(int loadType)
        {
            try
            {
                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM system_shop_repair";
                    cmd.CommandType = CommandType.Text;

                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

                    while (reader.Read())
                    {
                        ItemsRepair repairItem = new ItemsRepair()
                        {
                            Id = int.Parse($"{reader["item_id"]}"),
                            Point = int.Parse($"{reader["price_gold"]}"),
                            Cash = int.Parse($"{reader["price_cash"]}"),
                            Quantity = uint.Parse($"{reader["quantity"]}"),
                            Enable = bool.Parse($"{reader["repairable"]}")
                        };

                        if (loadType == 1 && repairItem.Enable && repairItem.Quantity <= 100U)
                            ItemRepairs.Add(repairItem);
                    }

                    cmd.Dispose();
                    reader.Close();
                    conn.Dispose();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Carga el flash sale (cliente 1120) desde system_shop_limited.
        /// start_date/end_date son YYMMDDHHMM empaquetado, igual que el cliente.
        /// </summary>
        public static void LoadFlashSale()
        {
            FlashSaleGoodIds = new int[0];
            FlashSaleStartDate = 0;
            FlashSaleEndMinute = 0;

            try
            {
                long now = long.Parse(DateTime.Now.ToString("yyMMddHHmm"));
                List<int> goodIds = new List<int>();
                long startDate = 0;
                long endDate = 0;

                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = @"SELECT l.item_id, l.variant_index, l.start_date, l.end_date, s.variant_code_list
                                        FROM system_shop_limited l
                                        JOIN system_shop s ON s.item_id = l.item_id
                                        WHERE l.enabled = TRUE AND s.item_visible = TRUE
                                          AND l.start_date <= @now AND l.end_date >= @now
                                        ORDER BY l.id
                                        LIMIT 3";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("now", now);

                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

                    while (reader.Read())
                    {
                        int itemId = int.Parse($"{reader["item_id"]}");
                        int variantIndex = int.Parse($"{reader["variant_index"]}");
                        string[] codes = $"{reader["variant_code_list"]}".Split(',');

                        if (variantIndex < 1 || variantIndex > codes.Length)
                            variantIndex = 1;

                        int variantCode;
                        if (!int.TryParse(codes[variantIndex - 1].Trim(), out variantCode))
                            continue;

                        goodIds.Add(itemId * 100 + variantCode);

                        long rowStart = long.Parse($"{reader["start_date"]}");
                        long rowEnd = long.Parse($"{reader["end_date"]}");

                        if (startDate == 0 || rowStart < startDate)
                            startDate = rowStart;
                        if (endDate == 0 || rowEnd < endDate)
                            endDate = rowEnd;
                    }

                    cmd.Dispose();
                    reader.Close();
                    conn.Dispose();
                    conn.Close();
                }

                if (goodIds.Count == 0)
                    return;

                FlashSaleGoodIds = goodIds.ToArray();
                FlashSaleStartDate = (uint)startDate;

                // The client stores the deadline as minute-of-day and hides the card once
                // (deadline - currentMinute) <= 0, so a sale running past midnight clamps to 23:59.
                FlashSaleEndMinute = endDate / 10000L > now / 10000L
                    ? 1439
                    : (int)(endDate % 10000L / 100L * 60L + endDate % 100L);

                CLogger.Print($"Plugin carregado: {FlashSaleGoodIds.Length} Flash Sale Goods", LoggerType.Info);
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Carga items limitados desde system_shop basado en count_limited
        /// Creates ONE ItemsLimited entry per item - variants come from goods data
        /// </summary>
        public static void LoadLimitedItems()
        {
            try
            {
                ItemLimited.Clear();

                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    NpgsqlCommand cmd = conn.CreateCommand();
                    cmd.CommandText = @"SELECT * FROM system_shop WHERE count_limited IS NOT NULL AND count_limited > 0";
                    cmd.CommandType = CommandType.Text;

                    NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default);

                    while (reader.Read())
                    {
                        try
                        {
                            int itemId = int.Parse($"{reader["item_id"]}");
                            string itemName = $"{reader["item_name"]}";
                            uint countLimited = uint.Parse($"{reader["count_limited"]}");
                            long startDate = reader["start_date"] != DBNull.Value ? long.Parse($"{reader["start_date"]}") : 0;
                            long endDate = reader["end_date"] != DBNull.Value ? long.Parse($"{reader["end_date"]}") : 0;
                            int saleType = reader["sale_type"] != DBNull.Value ? int.Parse($"{reader["sale_type"]}") : 2;

                            string itemCountList = $"{reader["item_count_list"]}";
                            string priceCashList = $"{reader["price_cash_list"]}";
                            string priceGoldList = $"{reader["price_gold_list"]}";

                            int itemCategory = ComDiv.GetIdStatics(itemId, 1);
                            bool isSpecialCategory = itemCategory == 22 || itemCategory == 26 ||
                                                     itemCategory == 36 || itemCategory == 37 || itemCategory == 40;
                            
                            // Use first variant GoodId (01)
                            int goodId = int.Parse($"{itemId}{(isSpecialCategory ? "00" : "01")}");

                            // Parse first value from lists
                            string[] counts = ParseCommaSeparatedValue(itemCountList);
                            string[] cashes = ParseCommaSeparatedValue(priceCashList);
                            string[] golds = ParseCommaSeparatedValue(priceGoldList);

                            uint firstCount = 0;
                            int firstCash = 0;
                            int firstGold = 0;
                            if (counts.Length > 0) uint.TryParse(counts[0], out firstCount);
                            if (cashes.Length > 0) int.TryParse(cashes[0], out firstCash);
                            if (golds.Length > 0) int.TryParse(golds[0], out firstGold);

                            ItemsLimited limitedItem = new ItemsLimited()
                            {
                                ItemId = itemId,
                                GoodId = goodId,
                                VariantIndex = 1,
                                ItemName = itemName,
                                StartDate = startDate,
                                EndDate = endDate,
                                InitialStock = countLimited,
                                Remain = countLimited,
                                SaleType = saleType,
                                Enabled = true,
                                ItemCountList = itemCountList,
                                PriceCashList = priceCashList,
                                PriceGoldList = priceGoldList,
                                ItemCount = firstCount,
                                CashPrice = firstCash,
                                GoldPrice = firstGold
                            };

                            ItemLimited.Add(limitedItem);
                            //CLogger.Print($"[LIMITED] Loaded: {itemId} GoodId={goodId} Cash={firstCash} Gold={firstGold}", LoggerType.Debug);
                        }
                        catch (Exception rowEx)
                        {
                            CLogger.Print($"LoadLimitedItems row error: {rowEx.Message}", LoggerType.Warning);
                        }
                    }

                    cmd.Dispose();
                    reader.Close();
                    conn.Dispose();
                    conn.Close();
                }

               // CLogger.Print($"[LIMITED] Loaded {ItemLimited.Count} itens limitados", LoggerType.Info);
            }
            catch (Exception ex)
            {
                CLogger.Print($"LoadLimitedItems error: {ex.Message}", LoggerType.Error, ex);
            }
        }

        #endregion

        #region Data Building Methods

        /// <summary>
        /// Construye datos de matching y goods para enviar al cliente
        /// </summary>
        private static void BuildMatchingAndGoodsData(int pcCafeFilter)
        {
            List<GoodsItem> matchingList = new List<GoodsItem>();
            List<GoodsItem> goodsList = new List<GoodsItem>();

            lock (ShopAllList)
            {
                HashSet<int> pricedItems = BuildPricedItemSet();
                foreach (GoodsItem item in ShopAllList)
                {
                    if (item.Item.Count == 0U)
                        continue;

                    // Filtro de matching
                    if (IsMatchingEligible(item, pricedItems) &&
                        (item.Tag != ItemTag.PcCafe || pcCafeFilter != 0) &&
                        (item.Tag == ItemTag.PcCafe && pcCafeFilter > 0 || item.Visibility != 2))
                        matchingList.Add(item);

                    // Filtro de goods
                    if (item.Visibility < 2 || item.Visibility == 4)
                        goodsList.Add(item);
                }
            }

            TotalMatching1 = matchingList.Count;
            TotalGoods = goodsList.Count;

            // Serializar matching en paquetes de 500 items
            int matchingPages = (int)Math.Ceiling((double)matchingList.Count / 500.0);
            for (int page = 0; page < matchingPages; page++)
            {
                int itemsWritten = 0;
                byte[] buffer = SerializeMatchingItems(500, page, ref itemsWritten, matchingList);

                ShopDataMt1.Add(new ShopData()
                {
                    Buffer = buffer,
                    ItemsCount = itemsWritten,
                    Offset = page * 500
                });
            }

            // Serializar goods en paquetes de 50 items
            int goodsPages = (int)Math.Ceiling((double)goodsList.Count / 50.0);
            for (int page = 0; page < goodsPages; page++)
            {
                int itemsWritten = 0;
                byte[] buffer = SerializeGoodsItems(50, page, ref itemsWritten, goodsList);

                ShopDataGoods.Add(new ShopData()
                {
                    Buffer = buffer,
                    ItemsCount = itemsWritten,
                    Offset = page * 50
                });
            }
        }

        /// <summary>
        /// Construye datos de matching 2
        /// </summary>
        private static void BuildMatchingData2(int pcCafeFilter)
        {
            List<GoodsItem> matchingList = new List<GoodsItem>();

            lock (ShopAllList)
            {
                HashSet<int> pricedItems = BuildPricedItemSet();
                foreach (GoodsItem item in ShopAllList)
                {
                    if (item.Item.Count != 0U &&
                        IsMatchingEligible(item, pricedItems) &&
                        (item.Tag != ItemTag.PcCafe || pcCafeFilter != 0) &&
                        (item.Tag == ItemTag.PcCafe && pcCafeFilter > 0 || item.Visibility != 2))
                        matchingList.Add(item);
                }
            }

            TotalMatching2 = matchingList.Count;

            int pages = (int)Math.Ceiling((double)matchingList.Count / 500.0);
            for (int page = 0; page < pages; page++)
            {
                int itemsWritten = 0;
                byte[] buffer = SerializeMatchingItems(500, page, ref itemsWritten, matchingList);

                ShopDataMt2.Add(new ShopData()
                {
                    Buffer = buffer,
                    ItemsCount = itemsWritten,
                    Offset = page * 500
                });
            }
        }

        // The 121 client caps PACKED_MATCHING at MAX_SHOP_MATCHING records; anything past
        // that is silently dropped client-side. Keep the matching list under the cap by
        // pruning entries the packed goods list (ShopBuyableList) already excludes:
        // invisible goods (item_visible=false -> Visibility 4) and zero-priced junk
        // variants of items that have at least one priced variant.
        private static HashSet<int> BuildPricedItemSet()
        {
            HashSet<int> priced = new HashSet<int>();
            foreach (GoodsItem item in ShopAllList)
            {
                if (item.PriceCash > 0 || item.PriceGold > 0)
                    priced.Add(item.Item.Id);
            }
            return priced;
        }

        private static bool IsMatchingEligible(GoodsItem item, HashSet<int> pricedItems)
        {
            if (item.Visibility == 4)
                return false;
            return item.PriceCash > 0 || item.PriceGold > 0 || !pricedItems.Contains(item.Item.Id);
        }

        /// <summary>
        /// Construye datos de items únicos
        /// </summary>
        private static void BuildUniqueItemsData()
        {
            List<GoodsItem> uniqueList = new List<GoodsItem>();

            lock (ShopUniqueList)
            {
                foreach (GoodsItem item in ShopUniqueList.Values)
                {
                    if (item.Visibility != 1 && item.Visibility != 3)
                        uniqueList.Add(item);
                }
            }

            TotalItems = uniqueList.Count;

            int pages = (int)Math.Ceiling((double)uniqueList.Count / 800.0);
            for (int page = 0; page < pages; page++)
            {
                int itemsWritten = 0;
                byte[] buffer = SerializeUniqueItems(800, page, ref itemsWritten, uniqueList);

                ShopDataItems.Add(new ShopData()
                {
                    Buffer = buffer,
                    ItemsCount = itemsWritten,
                    Offset = page * 800
                });
            }
        }

        /// <summary>
        /// Construye datos de items reparables
        /// </summary>
        private static void BuildRepairItemsData()
        {
            List<ItemsRepair> repairList = new List<ItemsRepair>();

            lock (ItemRepairs)
            {
                foreach (ItemsRepair item in ItemRepairs)
                    repairList.Add(item);
            }

            TotalRepairs = repairList.Count;

            int pages = (int)Math.Ceiling((double)repairList.Count / 100.0);
            for (int page = 0; page < pages; page++)
            {
                int itemsWritten = 0;
                byte[] buffer = SerializeRepairItems(100, page, ref itemsWritten, repairList);

                ShopDataItemRepairs.Add(new ShopData()
                {
                    Buffer = buffer,
                    ItemsCount = itemsWritten,
                    Offset = page * 100
                });
            }
        }

        /// <summary>
        /// Rebuilds the shop-tag cache exclusively from shop_tag values loaded
        /// out of system_shop and system_shop_effects (via RegisterLoadedShopTag).
        /// </summary>
        private static void BuildShopTagData()
        {
            ShopTagList = LoadedShopTags.OrderBy(t => t).ToList();

            using (SyncServerPacket packet = new SyncServerPacket())
            {
                packet.WriteC((byte)ShopTagList.Count);
                foreach (byte tag in ShopTagList)
                    packet.WriteC(tag);
                ShopTagData = packet.ToArray();
            }

            CLogger.Print($"Plugin carregado: {ShopTagList.Count} Shop Tags from DB (shop_tag)", LoggerType.Info);
        }

        /// <summary>
        /// Records a non-zero shop_tag seen while loading system_shop / system_shop_effects.
        /// </summary>
        private static void RegisterLoadedShopTag(int shopTag)
        {
            if (shopTag <= 0 || shopTag > byte.MaxValue)
                return;
            LoadedShopTags.Add((byte)shopTag);
        }

        // 121 client (CShop::FillGoodsFromServer / BuildShopGoodsList) expects fixed 210-byte
        // SHOP_GOODS_EXPANSION records, zlib-compressed, sent via PACKED_GOODSLIST (opcode 1037).
        // Record layout (verified against client Shop.dat + BuildShopGoodsList):
        //   +0  u32 GoodsID
        //   +4  u8  flag0          (always 1)
        //   +5  u8  flag1          (visibility: 1 normal / 4 limited)
        //   +6  12 option entries x 17 bytes:
        //         +0 u32 gold | +4 u32 cash | +8 u32 d2 | +12 u32 period | +16 u8 code
        //
        // SaleType / shop_tag ribbon is option0.code at record+22 (NOT classic +18).
        // Evidence from live UI vs wire: variant suffix goodId%100 written into that byte
        // painted NEW(1)/HOT(2)/EVENTO(3) ribbons even when DB shop_tag was 0. Duration text
        // ("30dias") comes from the matching-list count, so option0.code is free for Tag.
        //   (slot order OBSERVED: goods 3600001 priced 10000 in price_cash_list rendered
        //    as "10.000 Gold" in the client confirm dialog and "0 CASH" on the card;
        //    client reads rec+6 / rec+10 in CShop__UpdateItemPrices @0xcd870d)
        public const int GOODS_RECORD_SIZE = 210;
        public const int GOODS_SALETYPE_OFFSET = 22;
        // 121 client renders at most 4 duration/buy-info slots per item; emitting more
        // overflows ShopBottomUIs::ApplyRadioUIByCurrPriceTab (idxInTab >= 4 -> assert + AV).
        public const int MAX_BUY_INFO_PER_ITEM = 4;
        public const int ITEM_RECORD_SIZE = 11;
        public const int REPAIR_RECORD_SIZE = 16;
        public const int MATCHING_RECORD_SIZE = 16;
        private const int MAX_SHOP_GOODS = 20000;
        private const int MAX_SHOP_ITEMS = 16000;
        private const int MAX_SHOP_REPAIRS = 16000;
        private const int MAX_SHOP_MATCHING = 30000;

        private static void BuildPackedGoodsData()
        {
            List<GoodsItem> list;
            lock (ShopBuyableList)
            {
                list = new List<GoodsItem>(ShopBuyableList);
            }
            // NAO forcar goods de evento (item_visible=false) no packed catalog:
            // o client recebe o GoodsID mas sem ShopItem/SHOP_ITEM_BASE e crasha
            // (Please Wait / 0xC0000005) ao abrir a presença. Recompensas de visita
            // precisam usar goods ja compraveis (IsPackedGood=true de verdade).
            if (list.Count > MAX_SHOP_GOODS)
                list = list.GetRange(0, MAX_SHOP_GOODS);

            byte[] raw = new byte[list.Count * GOODS_RECORD_SIZE];
            for (int i = 0; i < list.Count; i++)
                WriteGoodsRecord210(list[i], raw, i * GOODS_RECORD_SIZE);

            HashSet<int> packedIds = new HashSet<int>();
            foreach (GoodsItem good in list)
                packedIds.Add(good.Id);
            PackedGoodsIds = packedIds;

            PackedGoodsCount = list.Count;
            PackedGoodsBuffer = ZlibUtil.Compress(raw);

            CLogger.Print($"Plugin carregado: packed goods {list.Count} recs ({raw.Length}B raw -> {PackedGoodsBuffer.Length}B zlib)", LoggerType.Info);
        }

        /// <summary>
        /// True cuando el cliente recibe ese good en su catalogo (Shop.dat). Un good que
        /// existe en ShopAllList pero no aqui hace que el cliente resuelva FindGoods a NULL.
        /// </summary>
        public static bool IsPackedGood(int GoodId)
        {
            return GoodId != 0 && PackedGoodsIds.Contains(GoodId);
        }

        private static void BuildPackedCatalogData()
        {
            PackedItemsCount = Math.Min(TotalItems, MAX_SHOP_ITEMS);
            PackedRepairsCount = Math.Min(TotalRepairs, MAX_SHOP_REPAIRS);
            PackedMatching1Count = Math.Min(TotalMatching1, MAX_SHOP_MATCHING);
            PackedMatching2Count = Math.Min(TotalMatching2, MAX_SHOP_MATCHING);

            if (TotalItems > MAX_SHOP_ITEMS)
                CLogger.Print($"packed items CLAMPED {TotalItems} -> {MAX_SHOP_ITEMS} recs (client cap), tail entries dropped", LoggerType.Warning);
            if (TotalRepairs > MAX_SHOP_REPAIRS)
                CLogger.Print($"packed repairs CLAMPED {TotalRepairs} -> {MAX_SHOP_REPAIRS} recs (client cap), tail entries dropped", LoggerType.Warning);
            if (TotalMatching1 > MAX_SHOP_MATCHING)
                CLogger.Print($"packed matching1 CLAMPED {TotalMatching1} -> {MAX_SHOP_MATCHING} recs (client cap), tail entries dropped", LoggerType.Warning);
            if (TotalMatching2 > MAX_SHOP_MATCHING)
                CLogger.Print($"packed matching2 CLAMPED {TotalMatching2} -> {MAX_SHOP_MATCHING} recs (client cap), tail entries dropped", LoggerType.Warning);

            byte[] rawItems = BuildPackedRowsFromChunks(ShopDataItems, PackedItemsCount, ITEM_RECORD_SIZE);
            byte[] rawRepairs = BuildPackedRowsFromChunks(ShopDataItemRepairs, PackedRepairsCount, REPAIR_RECORD_SIZE);
            byte[] raw1 = BuildPackedRowsFromChunks(ShopDataMt1, PackedMatching1Count, MATCHING_RECORD_SIZE);
            byte[] raw2 = BuildPackedRowsFromChunks(ShopDataMt2, PackedMatching2Count, MATCHING_RECORD_SIZE);

            PackedItemsBuffer = ZlibUtil.Compress(rawItems);
            PackedRepairsBuffer = ZlibUtil.Compress(rawRepairs);
            PackedMatching1Buffer = ZlibUtil.Compress(raw1);
            PackedMatching2Buffer = ZlibUtil.Compress(raw2);
            CatalogVersion = ComputeCatalogVersion();

            CLogger.Print($"Plugin carregado: packed items {PackedItemsCount} recs ({rawItems.Length}B raw -> {PackedItemsBuffer.Length}B zlib)", LoggerType.Info);
            CLogger.Print($"Plugin carregado: packed repairs {PackedRepairsCount} recs ({rawRepairs.Length}B raw -> {PackedRepairsBuffer.Length}B zlib)", LoggerType.Info);
            CLogger.Print($"Plugin carregado: packed matching {PackedMatching1Count}/{PackedMatching2Count} recs ({raw1.Length}/{raw2.Length}B raw -> {PackedMatching1Buffer.Length}/{PackedMatching2Buffer.Length}B zlib)", LoggerType.Info);
        }

        private static int ComputeCatalogVersion()
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (byte[] buf in new[] { PackedItemsBuffer, PackedGoodsBuffer, PackedMatching1Buffer })
                {
                    if (buf == null)
                        continue;
                    foreach (byte b in buf)
                        hash = (hash ^ b) * 16777619;
                }
                int version = (int)hash;
                return version == 0 ? 1 : version;
            }
        }

        private static byte[] BuildPackedRowsFromChunks(List<ShopData> chunks, int recordCount, int recordSize)
        {
            byte[] raw = new byte[recordCount * recordSize];
            foreach (ShopData chunk in chunks)
            {
                if (chunk.Buffer == null)
                    continue;

                int dstOffset = chunk.Offset * recordSize;
                if (dstOffset >= raw.Length)
                    continue;

                int bytes = Math.Min(chunk.Buffer.Length, raw.Length - dstOffset);
                Buffer.BlockCopy(chunk.Buffer, 0, raw, dstOffset, bytes);
            }
            return raw;
        }

        private static void WriteGoodsRecord210(GoodsItem g, byte[] buf, int off)
        {
            WriteIntLE(buf, off + 0, g.Id);
            buf[off + 4] = 1;
            buf[off + 5] = g.Visibility == 4 ? (byte)4 : (byte)1;

            WriteGoodsOption(buf, off, 0, g.PriceGold, g.PriceCash, 0);

            // Per-item ribbon from system_shop.shop_tag / system_shop_effects.shop_tag.
            // Written at option0.code (+22). Never put goodId%100 here — that painted
            // fake NEW/HOT/EVENTO ribbons from the variant suffix (01/02/03).
            buf[off + GOODS_SALETYPE_OFFSET] = (byte)g.Tag;

            // The flash-sale panel ignores option 0 and reads two fixed options instead
            // (client sub_E13255 reads good+112 / good+129):
            //   option 6 = promo price (shown large), option 7 = full price (shown struck out)
            // sub_CF9537 derives the badge from (opt7 - opt6) * 100 / opt7 over the *gold*
            // field, so cash-only goods need gold mirrored here or the ratio is 0/0.
            if (Array.IndexOf(FlashSaleGoodIds, g.Id) < 0)
                return;

            int fullGold = g.StarGold > 0 ? g.StarGold / 255 : g.PriceGold;
            int fullCash = g.StarCash > 0 ? g.StarCash / 255 : g.PriceCash;

            int promoRatio = g.PriceGold > 0 ? g.PriceGold : g.PriceCash;
            int fullRatio = fullGold > 0 ? fullGold : fullCash;

            WriteGoodsOption(buf, off, 6, promoRatio, g.PriceCash, 0);
            WriteGoodsOption(buf, off, 7, fullRatio, fullCash, 0);

            // Re-apply Tag after flash-sale options (they do not touch +22, keep invariant).
            buf[off + GOODS_SALETYPE_OFFSET] = (byte)g.Tag;
        }

        // One buy-info option: 17-byte stride starting at record+6.
        // option0.code (+22) is SaleType/shop_tag — callers overwrite via GOODS_SALETYPE_OFFSET.
        // Do not write goodId%100 into code; variant identity is already in GoodsID.
        private static void WriteGoodsOption(byte[] buf, int off, int option, int gold, int cash, int code)
        {
            int o = off + 6 + option * 17;
            WriteIntLE(buf, o + 0, gold);
            WriteIntLE(buf, o + 4, cash);
            WriteIntLE(buf, o + 8, 0);
            WriteIntLE(buf, o + 12, 0);
            buf[o + 16] = (byte)code;
        }

        private static void WriteIntLE(byte[] buf, int off, int val)
        {
            buf[off + 0] = (byte)val;
            buf[off + 1] = (byte)(val >> 8);
            buf[off + 2] = (byte)(val >> 16);
            buf[off + 3] = (byte)(val >> 24);
        }

        #endregion

        #region Serialization Methods

        /// <summary>
        /// Serializa items únicos para enviar al cliente
        /// </summary>
        private static byte[] SerializeUniqueItems(int itemsPerPage, int pageIndex, ref int itemsWritten, List<GoodsItem> itemList)
        {
            itemsWritten = 0;
            using (SyncServerPacket packet = new SyncServerPacket())
            {
                for (int i = pageIndex * itemsPerPage; i < itemList.Count; i++)
                {
                    WriteItemDataShort(itemList[i], packet);
                    if (++itemsWritten == itemsPerPage)
                        break;
                }
                return packet.ToArray();
            }
        }

        /// <summary>
        /// Serializa goods items
        /// </summary>
        private static byte[] SerializeGoodsItems(int itemsPerPage, int pageIndex, ref int itemsWritten, List<GoodsItem> itemList)
        {
            itemsWritten = 0;
            using (SyncServerPacket packet = new SyncServerPacket())
            {
                for (int i = pageIndex * itemsPerPage; i < itemList.Count; i++)
                {
                    GoodsItem item = itemList[i];
                    // Debug log for limited items
                    if (item.Visibility == 4)
                    {
                       // CLogger.Print($"[GOODS-SERIALIZE] GoodId={item.Id} ItemId={item.Item.Id} Cash={item.PriceCash} Gold={item.PriceGold} Visibility={item.Visibility}", LoggerType.Debug);
                    }
                    WriteGoodsItemData(item, packet);
                    if (++itemsWritten == itemsPerPage)
                        break;
                }
                return packet.ToArray();
            }
        }

        /// <summary>
        /// Serializa items de reparación
        /// </summary>
        private static byte[] SerializeRepairItems(int itemsPerPage, int pageIndex, ref int itemsWritten, List<ItemsRepair> repairList)
        {
            itemsWritten = 0;
            using (SyncServerPacket packet = new SyncServerPacket())
            {
                for (int i = pageIndex * itemsPerPage; i < repairList.Count; i++)
                {
                    WriteRepairItemData(repairList[i], packet);
                    if (++itemsWritten == itemsPerPage)
                        break;
                }
                return packet.ToArray();
            }
        }

        /// <summary>
        /// Serializa matching items
        /// </summary>
        private static byte[] SerializeMatchingItems(int itemsPerPage, int pageIndex, ref int itemsWritten, List<GoodsItem> itemList)
        {
            itemsWritten = 0;
            using (SyncServerPacket packet = new SyncServerPacket())
            {
                for (int i = pageIndex * itemsPerPage; i < itemList.Count; i++)
                {
                    WriteMatchingItemData(itemList[i], packet);
                    if (++itemsWritten == itemsPerPage)
                        break;
                }
                return packet.ToArray();
            }
        }

        #endregion

        #region Packet Writing Methods

        /// <summary>
        /// Escribe datos cortos de item (para lista única)
        /// </summary>
        private static void WriteItemDataShort(GoodsItem item, SyncServerPacket packet)
        {
            packet.WriteD(item.Item.Id);
            packet.WriteC((byte)item.AuthType);
            packet.WriteC((byte)item.BuyType2);
            packet.WriteC((byte)item.BuyType3);
            packet.WriteC((byte)item.Title);
            packet.WriteC(item.Title != 0 ? (byte)2 : (byte)0);
            packet.WriteH((short)0);
        }

        /// <summary>
        /// Escribe datos completos de good
        /// </summary>
        private static void WriteGoodsItemData(GoodsItem item, SyncServerPacket packet)
        {
            packet.WriteD(item.Id);
            packet.WriteC((byte)1);
            packet.WriteC(item.Visibility == 4 ? (byte)4 : (byte)1);
            packet.WriteD(item.PriceGold);
            packet.WriteD(item.PriceCash);
            packet.WriteD(0);
            packet.WriteC((byte)item.Tag);
            packet.WriteC((byte)0);
            packet.WriteC((byte)0);
            packet.WriteC((byte)0);
            packet.WriteD(item.StarCash > 0 ? item.StarCash : (item.StarGold > 0 ? item.StarGold : 0));
            packet.WriteD(0);
            packet.WriteD(0);
            packet.WriteD(0);
            packet.WriteB(new byte[98]); // Padding
        }

        /// <summary>
        /// Escribe datos de item reparable
        /// </summary>
        private static void WriteRepairItemData(ItemsRepair item, SyncServerPacket packet)
        {
            packet.WriteD(item.Id);
            packet.WriteD((int)((double)item.Point / (double)item.Quantity));
            packet.WriteD((int)((double)item.Cash / (double)item.Quantity));
            packet.WriteD(item.Quantity);
        }

        /// <summary>
        /// Escribe datos de matching item
        /// </summary>
        private static void WriteMatchingItemData(GoodsItem item, SyncServerPacket packet)
        {
            packet.WriteD(item.Id);
            packet.WriteD(item.Item.Id);
            packet.WriteD(item.Item.Count);
            packet.WriteD(0);
        }

        #endregion

        #region Public Utility Methods

        /// <summary>
        /// Reinicia todas las listas y contadores
        /// </summary>
        public static void Reset()
        {
            Set4p = 0;
            ShopAllList.Clear();
            ShopBuyableList.Clear();
            ShopUniqueList.Clear();
            ShopDataMt1.Clear();
            ShopDataMt2.Clear();
            ShopDataGoods.Clear();
            ShopDataItems.Clear();
            ShopDataItemRepairs.Clear();
            ItemRepairs.Clear();
            ItemLimited.Clear();
            LoadedShopTags.Clear();
            ShopTagList.Clear();
            ShopTagData = null;
            PackedGoodsIds.Clear();
            FlashSaleGoodIds = new int[0];
            FlashSaleStartDate = 0;
            FlashSaleEndMinute = 0;
            CatalogVersion = 0;
            TotalGoods = 0;
            TotalItems = 0;
            TotalMatching1 = 0;
            TotalMatching2 = 0;
            TotalRepairs = 0;
            PackedGoodsBuffer = null;
            PackedItemsBuffer = null;
            PackedRepairsBuffer = null;
            PackedMatching1Buffer = null;
            PackedMatching2Buffer = null;
            PackedGoodsCount = 0;
            PackedItemsCount = 0;
            PackedRepairsCount = 0;
            PackedMatching1Count = 0;
            PackedMatching2Count = 0;
        }

        /// <summary>
        /// Verifica si un item es reparable
        /// </summary>
        public static bool IsRepairableItem(int ItemId) => GetRepairItem(ItemId) != null;

        /// <summary>
        /// Obtiene un item reparable por ID
        /// </summary>
        public static ItemsRepair GetRepairItem(int ItemId)
        {
            if (ItemId == 0)
                return null;

            lock (ItemRepairs)
            {
                foreach (ItemsRepair repair in ItemRepairs)
                {
                    if (repair.Id == ItemId)
                        return repair;
                }
            }
            return null;
        }

        /// <summary>
        /// Busca items bloqueados por texto
        /// </summary>
        public static bool IsBlocked(string Text, List<int> Items)
        {
            lock (ShopUniqueList)
            {
                foreach (GoodsItem item in ShopUniqueList.Values)
                {
                    if (!Items.Contains(item.Item.Id) && item.Item.Name.Contains(Text))
                        Items.Add(item.Item.Id);
                }
            }
            return false;
        }

        /// <summary>
        /// Obtiene un good por ID
        /// </summary>
        public static GoodsItem GetGood(int GoodId)
        {
            if (GoodId == 0)
                return null;

            lock (ShopAllList)
            {
                foreach (GoodsItem item in ShopAllList)
                {
                    if (item.Id == GoodId)
                        return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Obtiene un good por Item ID
        /// </summary>
        public static GoodsItem GetItemId(int ItemId)
        {
            if (ItemId == 0)
                return null;

            lock (ShopAllList)
            {
                foreach (GoodsItem item in ShopAllList)
                {
                    if (item.Item.Id == ItemId)
                        return item;
                }
            }
            return null;
        }

        private static bool TryApplyCartPrice(
            CartGoods cartItem,
            GoodsItem shopItem,
            ref int GoldPrice,
            ref int CashPrice,
            ref int TagsPrice)
        {
            // Honor the client's preferred currency (BuyType 1=Cash, 2=Gold) when that currency
            // actually has a price (dual-priced goods). The 121 client does NOT reliably map its
            // BuyType to currency for every flow (e.g. char-create sends BuyType=2 for a cash-only
            // char), so for single-currency goods we charge whichever currency the item is priced
            // in. Without this, cash-only goods bought with BuyType=2 were rejected as "not found".
            if (cartItem.BuyType == 2 && shopItem.PriceGold > 0)
            {
                GoldPrice += shopItem.PriceGold;
                return true;
            }

            if (cartItem.BuyType == 1 && shopItem.PriceCash > 0)
            {
                CashPrice += shopItem.PriceCash;
                return true;
            }

            // Fallback: charge the currency the good is actually priced in.
            if (shopItem.PriceCash > 0)
            {
                CashPrice += shopItem.PriceCash;
                return true;
            }

            if (shopItem.PriceGold > 0)
            {
                GoldPrice += shopItem.PriceGold;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Obtiene goods del carrito de compras y calcula precios totales
        /// </summary>
        public static List<GoodsItem> GetGoods(
            List<CartGoods> ShopCart,
            out int GoldPrice,
            out int CashPrice,
            out int TagsPrice)
        {
            GoldPrice = 0;
            CashPrice = 0;
            TagsPrice = 0;

            List<GoodsItem> purchasedGoods = new List<GoodsItem>();

            if (ShopCart.Count == 0)
                return purchasedGoods;

            // Track which cart items have been found
            HashSet<int> foundGoodIds = new HashSet<int>();

            // First search in ShopBuyableList (normal visible items)
            lock (ShopBuyableList)
            {
                foreach (GoodsItem shopItem in ShopBuyableList)
                {
                    foreach (CartGoods cartItem in ShopCart)
                    {
                        if (cartItem.GoodId == shopItem.Id && !foundGoodIds.Contains(cartItem.GoodId))
                        {
                            if (!TryApplyCartPrice(cartItem, shopItem, ref GoldPrice, ref CashPrice, ref TagsPrice))
                                return new List<GoodsItem>();

                            purchasedGoods.Add(shopItem);
                            foundGoodIds.Add(cartItem.GoodId);
                        }
                    }
                }
            }

            // Fallback: Search in ShopAllList for items not found (e.g., limited items with visibility=4)
            if (foundGoodIds.Count < ShopCart.Count)
            {
                lock (ShopAllList)
                {
                    foreach (GoodsItem shopItem in ShopAllList)
                    {
                        foreach (CartGoods cartItem in ShopCart)
                        {
                            if (cartItem.GoodId == shopItem.Id && !foundGoodIds.Contains(cartItem.GoodId))
                            {
                                if (!TryApplyCartPrice(cartItem, shopItem, ref GoldPrice, ref CashPrice, ref TagsPrice))
                                    return new List<GoodsItem>();

                                purchasedGoods.Add(shopItem);
                                foundGoodIds.Add(cartItem.GoodId);
                            }
                        }
                    }
                }
            }

            if (foundGoodIds.Count != ShopCart.Count)
                return new List<GoodsItem>();

            return purchasedGoods;
        }

        /// <summary>
        /// Checks if a good is a limited sale item
        /// </summary>
        public static bool IsLimitedItem(int goodId)
        {
            if (goodId == 0)
                return false;

            lock (ItemLimited)
            {
                CLogger.Print($"[LIMITED DEBUG] Checking GoodId={goodId}, ItemLimited count={ItemLimited.Count}", LoggerType.Debug);
                foreach (ItemsLimited item in ItemLimited)
                {
                    CLogger.Print($"[LIMITED DEBUG] Comparing with item.GoodId={item.GoodId}", LoggerType.Debug);
                    if (item.GoodId == goodId)
                    {
                        CLogger.Print($"[LIMITED DEBUG] MATCH FOUND! GoodId={goodId}", LoggerType.Debug);
                        return true;
                    }
                }
            }
            CLogger.Print($"[LIMITED DEBUG] No match found for GoodId={goodId}", LoggerType.Debug);
            return false;
        }

        /// <summary>
        /// Updates the stock of a limited item after purchase
        /// </summary>
        public static bool UpdateLimitedItemStock(int goodId, uint quantity = 1)
        {
            if (goodId == 0)
                return false;

            lock (ItemLimited)
            {
                foreach (ItemsLimited item in ItemLimited)
                {
                    if (item.GoodId == goodId)
                    {
                        if (item.Remain >= quantity)
                        {
                            item.Remain -= quantity;
                            
                            // Update database count_limited
                            try
                            {
                                using (NpgsqlConnection conn = ConnectionSQL.GetInstance().Conn())
                                {
                                    conn.Open();
                                    NpgsqlCommand cmd = conn.CreateCommand();
                                    cmd.CommandType = System.Data.CommandType.Text;
                                    cmd.Parameters.AddWithValue("@itemId", item.ItemId);
                                    cmd.Parameters.AddWithValue("@newCount", (int)item.Remain);
                                    cmd.CommandText = "UPDATE system_shop SET count_limited = @newCount WHERE item_id = @itemId";
                                    cmd.ExecuteNonQuery();
                                    cmd.Dispose();
                                    conn.Dispose();
                                    conn.Close();
                                }
                                CLogger.Print($"[LIMITED] Updated stock for ItemId={item.ItemId}, Remain={item.Remain}", LoggerType.Debug);
                            }
                            catch (Exception ex)
                            {
                                CLogger.Print($"[LIMITED] Failed to update database: {ex.Message}", LoggerType.Error, ex);
                            }
                            
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a limited item by GoodId
        /// </summary>
        public static ItemsLimited GetLimitedItem(int goodId)
        {
            if (goodId == 0)
                return null;

            lock (ItemLimited)
            {
                foreach (ItemsLimited item in ItemLimited)
                {
                    if (item.GoodId == goodId)
                        return item;
                }
            }
            return null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Parsea valores separados por comas
        /// </summary>
        private static string[] ParseCommaSeparatedValue(string value)
        {
            if (!value.Contains(","))
                return new string[] { value };
            else
                return value.Split(',');
        }

        #endregion
    }
}
