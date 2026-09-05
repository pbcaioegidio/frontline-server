// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_LOBBY_ENTER_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_LOBBY_ENTER_REQ : GameClientPacket
    {
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
                player.LastLobbyEnter = DateTimeUtil.Now();
                if (player.ChannelId >= 0)
                    player.GetChannel()?.AddPlayer(player.Session);
                RoomModel room = player.Room;
                if (room != null)
                {
                    if (player.SlotId >= 0 && room.State >= RoomState.LOADING && room.Slots[player.SlotId].State >= SlotState.LOAD)
                    {
                        this.Client.SendPacket(new PROTOCOL_LOBBY_ENTER_ACK(0U));
                        player.LastLobbyEnter = DateTimeUtil.Now();
                        return;
                    }
                    room.RemovePlayer(player, false);
                }
                AllUtils.SyncPlayerToFriends(player, false);
                AllUtils.SyncPlayerToClanMembers(player);
                AllUtils.GetXmasReward(player);
                this.Client.SendPacket(new PROTOCOL_LOBBY_ENTER_ACK(0U));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_LOBBY_ENTER_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}