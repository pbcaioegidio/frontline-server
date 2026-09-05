using System;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
 
    public class ItemsLimited
    {

        public int ItemId { get; set; }
        public int GoodId { get; set; }
        public int VariantIndex { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public uint InitialStock { get; set; }
        public uint Remain { get; set; }
        public int SaleType { get; set; }
        public bool Enabled { get; set; }
        public string ItemName { get; set; }
        public uint ItemCount { get; set; }
        public int GoldPrice { get; set; }
        public int CashPrice { get; set; }
        public int DiscountPercent { get; set; }
        public string ItemCountList { get; set; }
        public string PriceCashList { get; set; }
        public string PriceGoldList { get; set; }


        public bool IsAvailable()
        {
            if (!Enabled || Remain <= 0)
                return false;

            long currentDate = GetCurrentDateAsLong();
            return currentDate >= StartDate && currentDate <= EndDate;
        }

        public bool HasStock()
        {
            return Remain > 0;
        }

        public bool DecreaseStock(uint quantity = 1)
        {
            if (Remain < quantity)
                return false;

            Remain -= quantity;
            return true;
        }

        private long GetCurrentDateAsLong()
        {
            DateTime now = DateTime.Now;
            string dateStr = now.ToString("yyMMddHHmm");
            return long.Parse(dateStr);
        }

        public override string ToString()
        {
            return $"GoodId: {GoodId}, Stock: {Remain}/{InitialStock}, Period: {StartDate}-{EndDate}, Active: {IsAvailable()}";
        }

    }
}