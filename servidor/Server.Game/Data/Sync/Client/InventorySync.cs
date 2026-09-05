// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.InventorySync
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;

namespace Server.Game.Data.Sync.Client
{
    public class InventorySync
    {
        public static void Load(SyncClientPacket C)
        {
            long id = C.ReadQ();
            long ObjectId = C.ReadQ();
            int num1 = C.ReadD();
            ItemEquipType itemEquipType = (ItemEquipType)C.ReadC();
            ItemCategory itemCategory = (ItemCategory)C.ReadC();
            uint num2 = C.ReadUD();
            byte Length = C.ReadC();
            string str = C.ReadS((int)Length);
            Account account = AccountManager.GetAccount(id, true);
            if (account == null)
                return;
            ItemsModel itemsModel1 = account.Inventory.GetItem(ObjectId);
            if (itemsModel1 != null)
            {
                itemsModel1.Count = num2;
            }
            else
            {
                ItemsModel itemsModel2 = new ItemsModel()
                {
                    ObjectId = ObjectId,
                    Id = num1,
                    Equip = itemEquipType,
                    Count = num2,
                    Category = itemCategory,
                    Name = str
                };
                account.Inventory.AddItem(itemsModel2);
            }
        }

    }
}