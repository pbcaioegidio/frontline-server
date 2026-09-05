//using Plugin.Core;
//using Plugin.Core.Enums;
//using Plugin.Core.Models;
//using Plugin.Core.XML;
//using Server.Auth.Data.Models;
//using Server.Auth.Data.Utils;
//using Server.Auth.Network.ServerPacket;
//using System;
//using System.Collections.Generic;

//namespace Server.Auth.Network.ClientPacket
//{
//    public class PROTOCOL_BASE_GET_INVEN_INFO_REQ : AuthClientPacket
//    {
//        public override void Read()
//        {
//        }

//        public override void Run()
//        {
//            try
//            {
//                Account player = this.Client.Player;
//                if (player == null)
//                    return;
//                List<ItemsModel> limitedItems = AllUtils.LimitationIndex(player, player.Inventory.Items);

//                int count = TemplatePackXML.Basics.Count;
//                if (TemplatePackXML.GetPCCafe(player.CafePC) != null)
//                    count += TemplatePackXML.GetPCCafeRewards(player.CafePC).Count;

//                const int CHUNK_SIZE = 100;
//                int totalItems = limitedItems.Count;

//                for (int i = 0; i < totalItems; i += CHUNK_SIZE)
//                {
//                    int remaining = Math.Min(CHUNK_SIZE, totalItems - i);
//                    List<ItemsModel> chunk = limitedItems.GetRange(i, remaining);
//                    this.Client.SendPacket(new PROTOCOL_BASE_GET_INVEN_INFO_ACK(0U, chunk, count, totalItems));
//                }
//            }
//            catch (Exception ex)
//            {
//                CLogger.Print("PROTOCOL_BASE_GET_INVEN_INFO_REQ: " + ex.Message, LoggerType.Error, ex);
//            }
//        }
//    }
//}

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Logging;
using Plugin.Core.Models;
using Plugin.Core.XML;
using Server.Auth.Data.Models;
using Server.Auth.Data.Utils;
using Server.Auth.Network.ServerPacket;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_GET_INVEN_INFO_REQ : AuthClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Account player = Client.Player;
                if (player == null)
                    return;
                List<ItemsModel> A_2 = AllUtils.LimitationIndex(player, player.Inventory.Items);
                if (A_2.Count <= 0)
                    return;
                int count = TemplatePackXML.Basics.Count;
                if (TemplatePackXML.GetPCCafe(player.CafePC) != null)
                    count += TemplatePackXML.GetPCCafeRewards(player.CafePC).Count;
                Client.SendPacket((AuthServerPacket)new PROTOCOL_BASE_GET_INVEN_INFO_ACK(0U, A_2, count));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_GET_INVEN_INFO_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}