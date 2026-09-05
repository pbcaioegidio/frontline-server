//using Plugin.Core;
//using Plugin.Core.Enums;
//using Plugin.Core.Utility;
//using Plugin.Core.XML;
//using Server.Game.Data.Models;
//using Server.Game.Data.XML;
//using Server.Game.Network.ServerPacket;
//using System;
//using System.Collections.Generic;
//using System.Numerics;


//namespace Server.Game.Network.ClientPacket
//{
//    public class PROTOCOL_BASE_GET_CHANNELLIST_REQ : GameClientPacket
//    {
//        private int ServerId;

//        public override void Read()
//        {
//            ServerId = ReadD();
//            CLogger.Print($"Read Auth PROTOCOL_BASE_GET_CHANNELLIST_REQ {ServerId}", LoggerType.Info);
//        }

//        public override void Run()
//        {
//            try
//            {
//                Account Player = Client.GetAccount();
//                if (Player == null)
//                {
//                    return;
//                }
//                if (ComDiv.GetDuration(Player.LastChannelList) >= 1)
//                {
//                    List<ChannelModel> Channels = ChannelsXML.GetChannels(ServerId);
//                    if (Channels.Count == 11)
//                    {
//                        Client.SendPacket(new PROTOCOL_BASE_GET_CHANNELLIST_ACK(SChannelXML.GetServer(ServerId), Channels));
//                        CLogger.Print("Sent Game PROTOCOL_BASE_GET_CHANNELLIST_ACK", LoggerType.Info);
//                    }
//                    Player.LastChannelList = DateTimeUtil.Now();
//                }
//            }
//            catch (Exception ex)
//            {
//                CLogger.Print(ex.Message, LoggerType.Error, ex);
//            }
//        }
//    }
//}

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.SQL;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.XML;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_GET_CHANNELLIST_REQ : GameClientPacket
    {
        private int ServerId;
        //public PROTOCOL_BASE_GET_CHANNELLIST_REQ(GameClient client, byte[] data)
        //{
        //    Makeme(client, data);
        //}
        public override void Read()
        {
            ServerId = ReadD();
        }
        public override void Run()
        {
            try
            {
                List<ChannelModel> Channels = ChannelsXML.GetChannels(ServerId);
                if (Channels.Count == 11)
                {
                    Client.SendPacket(new PROTOCOL_BASE_GET_CHANNELLIST_ACK(SChannelXML.GetServer(ServerId), Channels));
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }
    }
}