// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerInventory
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.XML;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    public class PlayerInventory
    {
        public List<ItemsModel> Items { get; set; }

        public PlayerInventory() => this.Items = new List<ItemsModel>();

        public ItemsModel GetItem(int Id)
        {
            lock (this.Items)
            {
                foreach (ItemsModel itemsModel in this.Items)
                {
                    if (itemsModel.Id == Id)
                        return itemsModel;
                }
            }
            return (ItemsModel)null;
        }

        public ItemsModel GetItem(long ObjectId)
        {
            lock (this.Items)
            {
                foreach (ItemsModel itemsModel in this.Items)
                {
                    if (itemsModel.ObjectId == ObjectId)
                        return itemsModel;
                }
            }
            return (ItemsModel)null;
        }

        public List<ItemsModel> GetItemsByType(ItemCategory Type)
        {
            List<ItemsModel> itemsByType = new List<ItemsModel>();
            lock (this.Items)
            {
                foreach (ItemsModel itemsModel in this.Items)
                {
                    if (itemsModel.Category == Type || itemsModel.Id > 1600000 && itemsModel.Id < 1700000 && Type == ItemCategory.NewItem)
                        itemsByType.Add(itemsModel);
                }
            }
            return itemsByType;
        }

        public bool RemoveItem(ItemsModel Item)
        {
            lock (this.Items)
                return this.Items.Remove(Item);
        }

        public void AddItem(ItemsModel Item)
        {
            lock (this.Items)
                this.Items.Add(Item);
        }

        public void LoadBasicItems()
        {
            lock (this.Items)
                this.Items.AddRange((IEnumerable<ItemsModel>)TemplatePackXML.Basics);
        }

        
        public void LoadGeneralBeret()
        {
            lock (this.Items)
                this.Items.Add(new ItemsModel(2700008, "Beret S. General", ItemEquipType.Permanent, 1U));
        }

        
        public void LoadHatForGM()
        {
            lock (this.Items)
                this.Items.Add(new ItemsModel(700160, "MOD Hat", ItemEquipType.Permanent, 1U));
        }

        public byte[] EquipmentData(int ItemId)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                ItemsModel itemsModel = this.GetItem(ItemId);
                if (itemsModel == null)
                {
                    syncServerPacket.WriteD(0);
                    syncServerPacket.WriteD((uint)ItemId);
                }
                else
                {
                    syncServerPacket.WriteD((uint)itemsModel.ObjectId);
                    syncServerPacket.WriteD(itemsModel.Id);
                }
                return syncServerPacket.ToArray();
            }
        }

        public byte[] EquipmentDataChara(int ItemId)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                ItemsModel itemsModel = this.GetItem(ItemId);
                if (itemsModel == null)
                {
                    syncServerPacket.WriteD((uint)ItemId);
                    syncServerPacket.WriteD(0);
                }
                else
                {
                    syncServerPacket.WriteD((uint)itemsModel.Id);
                    syncServerPacket.WriteD((uint)itemsModel.ObjectId);
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}