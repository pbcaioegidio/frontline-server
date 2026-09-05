// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_SELECT_CHANNEL_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Data.XML;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_SELECT_CHANNEL_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read()
        {
            this.ReadB(4);
            this.Field0 = (int)this.ReadH();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || player.ChannelId >= 0)
                    return;
                ChannelModel channel = ChannelsXML.GetChannel(this.Client.ServerId, this.Field0);
                if (channel != null)
                {
                    if (!AllUtils.ChannelRequirementCheck(player, channel))
                    {
                        if (channel.Players.Count < SChannelXML.GetServer(this.Client.ServerId).ChannelPlayers)
                        {
                            player.ServerId = channel.ServerId;
                            player.ChannelId = channel.Id;
                            this.Client.SendPacket(new PROTOCOL_BASE_SELECT_CHANNEL_ACK(0U, player.ServerId, player.ChannelId));
                            player.Status.UpdateServer((byte)player.ServerId);
                            player.Status.UpdateChannel((byte)player.ChannelId);
                            player.UpdateCacheInfo();
                        }
                        else
                            this.Client.SendPacket(new PROTOCOL_BASE_SELECT_CHANNEL_ACK(2147484161U /*0x80000201*/, -1, -1));
                    }
                    else
                        this.Client.SendPacket(new PROTOCOL_BASE_SELECT_CHANNEL_ACK(2147484162U /*0x80000202*/, -1, -1));
                }
                else
                    this.Client.SendPacket(new PROTOCOL_BASE_SELECT_CHANNEL_ACK(2147483648U /*0x80000000*/, -1, -1));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_SELECT_CHANNEL_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}