// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.GoodsItem
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;

namespace Plugin.Core.Models
{
    public class GoodsItem
    {
        public int Id { get; set; }

        public int PriceGold { get; set; }

        public int PriceCash { get; set; }

        public int AuthType { get; set; }

        public int BuyType2 { get; set; }

        public int BuyType3 { get; set; }

        public int Title { get; set; }

        public int Visibility { get; set; }

        public int StarGold { get; set; }

        public int StarCash { get; set; }

        public ItemTag Tag { get; set; }

        public ItemsModel Item { get; set; }

        public GoodsItem()
        {
            this.Item = new ItemsModel()
            {
                Equip = ItemEquipType.Durable
            };
        }
    }
}