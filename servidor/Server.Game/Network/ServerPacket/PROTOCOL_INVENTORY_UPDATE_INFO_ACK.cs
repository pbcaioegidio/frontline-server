using Plugin.Core.Models;
using Server.Game.Data.Models;
using System.Collections.Generic;

namespace Server.Game.Network.ServerPacket
{

    public class PROTOCOL_INVENTORY_UPDATE_INFO_ACK : GameServerPacket
    {
        private readonly byte category;
        private readonly List<ItemsModel> items;

       
        public PROTOCOL_INVENTORY_UPDATE_INFO_ACK(byte category, List<ItemsModel> inventoryItems)
        {
            this.category = category;
            this.items = inventoryItems ?? new List<ItemsModel>();
        }

      
        public PROTOCOL_INVENTORY_UPDATE_INFO_ACK(byte category, Account account)
        {
            this.category = category;
            this.items = new List<ItemsModel>();
            
            if (account?.Inventory?.Items != null)
            {
               
                foreach (var item in account.Inventory.Items)
                {
                    if ((byte)item.Category == category)
                    {
                        this.items.Add(item);
                    }
                }
            }
        }

        public override void Write()
        {
            WriteH(3336); 
            WriteC(category); 
            
           
            int itemCount = items.Count > 46 ? 46 : items.Count;
            WriteC((byte)itemCount);

            for (int i = 0; i < itemCount; i++)
            {
                var item = items[i];
               
                WriteD((uint)item.ObjectId);  
                WriteD(item.Id);             
                WriteC((byte)item.Equip);    
                WriteD(item.Count);           
            }
            
            
            int bytesWritten = itemCount * 13;
            int paddingNeeded = 608 - bytesWritten;
            for (int i = 0; i < paddingNeeded; i++)
            {
                WriteC(0);
            }
        }
    }
}
