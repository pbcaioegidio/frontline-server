// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_FRIEND_INVITED_REQUEST_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_FRIEND_INVITED_REQUEST_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read() => this.Field0 = (int)this.ReadC();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || ComDiv.GetDuration(player.LastFriendInviteRoom) < 1.0)
                    return;
                Account account = this.Method0(player);
                if (account == null)
                    this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147487805U));
                else if (account.Status.ServerId == byte.MaxValue || account.Status.ServerId == (byte)0)
                    this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147495938U /*0x80003002*/));
                else if (account.MatchSlot < 0)
                {
                    int friendIdx = account.Friend.GetFriendIdx(player.PlayerId);
                    if (friendIdx == -1)
                        this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147487806U));
                    else if (account.IsOnline)
                        account.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_REQUEST_ACK(friendIdx), false);
                    else
                        this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147487807U));
                    player.LastFriendInviteRoom = DateTimeUtil.Now();
                }
                else
                    this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147495939U /*0x80003003*/));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_FRIEND_INVITED_REQUEST_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        private Account Method0(Account A_1)
        {
            FriendModel friend = A_1.Friend.GetFriend(this.Field0);
            return friend != null ? AccountManager.GetAccount(friend.PlayerId, 287) : (Account)null;
        }
    }
}