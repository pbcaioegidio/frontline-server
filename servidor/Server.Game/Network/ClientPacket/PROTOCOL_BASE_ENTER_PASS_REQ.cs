// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_ENTER_PASS_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Data.XML;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_ENTER_PASS_REQ : GameClientPacket
    {
        private int Field0;
        private string Field1;

        public override void Read()
        {
            this.Field0 = (int)this.ReadH();
            this.Field1 = this.ReadS(16 /*0x10*/);
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || player.ChannelId >= 0)
                    return;
                ChannelModel channel = ChannelsXML.GetChannel(this.Client.ServerId, this.Field0);
                if (channel == null)
                    return;
                if (this.Field1.Equals(channel.Password))
                    this.Client.SendPacket(new PROTOCOL_BASE_ENTER_PASS_ACK(0U));
                else
                    this.Client.SendPacket(new PROTOCOL_BASE_ENTER_PASS_ACK(2147483648U /*0x80000000*/));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}