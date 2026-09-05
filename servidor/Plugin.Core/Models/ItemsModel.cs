using Plugin.Core.Enums;
using Plugin.Core.Utility;

namespace Plugin.Core.Models
{
    public class ItemsModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ItemCategory Category { get; set; }

        public ItemEquipType Equip { get; set; }

        public long ObjectId { get; set; }

        public uint Count { get; set; }

        public ItemsModel()
        {
        }

        public ItemsModel(int A_1) => this.SetItemId(A_1);

        public ItemsModel(int A_1, string A_2, ItemEquipType A_3, uint A_4)
        {
            this.SetItemId(A_1);
            this.Name = A_2;
            this.Equip = A_3;
            this.Count = A_4;
        }

        public ItemsModel(ItemsModel A_1)
        {
            this.Id = A_1.Id;
            this.Name = A_1.Name;
            this.Count = A_1.Count;
            this.Equip = A_1.Equip;
            this.Category = A_1.Category;
            this.ObjectId = A_1.ObjectId;
        }

        public void SetItemId(int Id)
        {
            this.Id = Id;
            this.Category = ComDiv.GetItemCategory(Id);
        }

    }
}