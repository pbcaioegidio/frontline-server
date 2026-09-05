using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Auth.Data.Models;
using Server.Auth.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_DAILY_RECORD_REQ : AuthClientPacket
    {
        private byte LastPlaytimeFinish = 0;
        private uint LastPlaytimeValue = 0;

        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Account player = Client.GetAccount();
                if (player != null)
                {
                    PlayerEvent playerEvent = player.Event;
                    if (playerEvent != null)
                    {
                        LastPlaytimeFinish = (byte)playerEvent.LastPlaytimeFinish;
                        LastPlaytimeValue = (uint)playerEvent.LastPlaytimeValue;
                    }
                    StatisticDaily daily = player.Statistic.Daily;
                    if (daily != null)
                    {
                        Client.SendPacket(new PROTOCOL_BASE_DAILY_RECORD_ACK(daily, LastPlaytimeFinish, LastPlaytimeValue));
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_DAILY_RECORD_REQ: ", LoggerType.Error, ex);
            }
        }
    }
}