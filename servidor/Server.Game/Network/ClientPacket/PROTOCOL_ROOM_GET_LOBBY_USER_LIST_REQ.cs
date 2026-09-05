// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_ROOM_GET_LOBBY_USER_LIST_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_GET_LOBBY_USER_LIST_REQ : GameClientPacket
    {
        private int Field0;
        private uint Field1;
        private int Field2;

        public override void Read()
        {
            this.Field0 = this.ReadD();
            this.Field2 = this.ReadD();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room != null && this.Field0 > 0 && this.Field0 <= 8 && ComDiv.GetDuration(player.LastRoomInvitePlayers) >= 2.0)
                {
                    ChannelModel channel = player.GetChannel();
                    if (channel != null)
                    {
                        player.LastRoomInvitePlayers = DateTimeUtil.Now();
                        using (PROTOCOL_SERVER_MESSAGE_INVITED_ACK messageInvitedAck = new PROTOCOL_SERVER_MESSAGE_INVITED_ACK(player, room))
                        {
                            byte[] completeBytes = messageInvitedAck.GetCompleteBytes("PROTOCOL_ROOM_INVITE_LOBBY_USER_LIST_REQ");
                            for (int index = 0; index < this.Field0; ++index)
                                AccountManager.GetAccount(channel.GetPlayer(this.Field2).PlayerId, true)?.SendCompletePacket(completeBytes, messageInvitedAck.GetType().Name);
                        }
                    }
                    else
                        this.Field1 = 2147483648U /*0x80000000*/;
                }
                this.Client.SendPacket(new PROTOCOL_ROOM_GET_LOBBY_USER_LIST_ACK(player.GetChannel()?.GetWaitPlayers()));
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}