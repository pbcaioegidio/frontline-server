using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Logging;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Auth.Data.Models;
using Server.Auth.Data.XML;
using Server.Auth.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_GET_CHANNELLIST_REQ : AuthClientPacket
    {
        private int ServerId;

        public override void Read()
        {
            ServerId = ReadD();
        }

        public override void Run()
        {
            try
            {
                Account Player = Client.GetAccount();
                if (Player == null)
                {
                    return;
                }

                string clientKey = $"{Client.GetIPAddress()}_{Player.PlayerId}";

                // Validar ServerId
                if (ServerId <= 0)
                {
                    CLogger.Print($"Invalid ServerId: {ServerId}, ignoring request", LoggerType.Warning);
                    return;
                }

                if (ComDiv.GetDuration(Player.LastChannelList) >= 1)
                {
                    List<ChannelModel> Channels = ChannelsXML.GetChannels(ServerId);

                    if (Channels.Count == 11)
                    {
                        Client.SendPacket(new PROTOCOL_BASE_GET_CHANNELLIST_ACK(SChannelXML.GetServer(ServerId), Channels));
                    }
                    else
                    {
                        CLogger.Print($"Expected 11 channels but found {Channels.Count} for server {ServerId}", LoggerType.Warning);
                    }

                    Player.LastChannelList = DateTimeUtil.Now();
                }
                else
                {
                    CLogger.Print("Channel list request too frequent, ignoring", LoggerType.Info);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}