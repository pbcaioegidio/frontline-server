// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_LOBBY_LEAVE_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_LOBBY_LEAVE_REQ : GameClientPacket
    {
        private uint Field0;

        public override void Read()
        {
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ChannelModel channel = player.GetChannel();
                if (player.Room != null || player.Match != null)
                    return;
                if (channel == null || player.Session == null || !channel.RemovePlayer(player))
                    this.Field0 = 2147483648U /*0x80000000*/;
                this.Client.SendPacket(new PROTOCOL_LOBBY_LEAVE_ACK(this.Field0));
                if (this.Field0 == 0U)
                {
                    player.ResetPages();
                    player.Status.UpdateChannel(byte.MaxValue);
                    AllUtils.SyncPlayerToFriends(player, false);
                    AllUtils.SyncPlayerToClanMembers(player);
                }
                else
                    this.Client.Close(1000, true);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_LOBBY_LEAVE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}