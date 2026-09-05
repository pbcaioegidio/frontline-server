// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_FRIEND_DELETE_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_FRIEND_DELETE_REQ : GameClientPacket
    {
        private int Field0;
        private uint Field1;

        public override void Read() => this.Field0 = (int)this.ReadC();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || ComDiv.GetDuration(player.LastFriendDelete) < 1.0)
                    return;
                FriendModel friend1 = player.Friend.GetFriend(this.Field0);
                if (friend1 == null)
                {
                    this.Field1 = 2147483648U /*0x80000000*/;
                }
                else
                {
                    DaoManagerSQL.DeletePlayerFriend(friend1.PlayerId, player.PlayerId);
                    Account account = AccountManager.GetAccount(friend1.PlayerId, 287);
                    if (account != null)
                    {
                        int index = -1;
                        FriendModel friend2 = account.Friend.GetFriend(player.PlayerId, out index);
                        if (friend2 != null)
                        {
                            friend2.Removed = true;
                            DaoManagerSQL.UpdatePlayerFriendBlock(account.PlayerId, friend2);
                            SendFriendInfo.Load(account, friend2, 2);
                            account.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(FriendChangeState.Update, friend2, index), false);
                        }
                    }
                    player.Friend.RemoveFriend(friend1);
                    this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(FriendChangeState.Delete, (FriendModel)null, 0, this.Field0));
                }
                this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_DELETE_ACK(this.Field1));
                this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_ACK(player.Friend.Friends));
                player.LastFriendDelete = DateTimeUtil.Now();
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_FRIEND_DELETE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}