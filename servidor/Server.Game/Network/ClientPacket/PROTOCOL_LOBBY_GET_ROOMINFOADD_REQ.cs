// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_LOBBY_GET_ROOMINFOADD_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_LOBBY_GET_ROOMINFOADD_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read() => this.Field0 = this.ReadD();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ChannelModel channel = player.GetChannel();
                if (channel == null)
                    return;
                RoomModel room = channel.GetRoom(this.Field0);
                if (room == null || room.GetLeader() == null)
                    return;
                this.Client.SendPacket(new PROTOCOL_LOBBY_GET_ROOMINFOADD_ACK(room));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_LOBBY_GET_ROOMINFOADD_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}