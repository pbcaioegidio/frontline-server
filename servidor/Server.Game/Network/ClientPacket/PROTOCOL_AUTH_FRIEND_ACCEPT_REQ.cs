// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_FRIEND_ACCEPT_REQ
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Server;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_FRIEND_ACCEPT_REQ : GameClientPacket
    {
        private int Field0;
        private uint Field1;

        public override void Read() => this.Field0 = (int)this.ReadC();

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                FriendModel friend1 = player.Friend.GetFriend(this.Field0);
                if (friend1 != null && friend1.State > 0)
                {
                    Account account = AccountManager.GetAccount(friend1.PlayerId, 287);
                    if (account == null)
                    {
                        this.Field1 = 0x80000000;
                    }
                    else
                    {
                        if (friend1.Info == null)
                            friend1.SetModel(account.PlayerId, account.Rank, account.NickColor, account.Nickname, account.IsOnline, account.Status);
                        else
                            friend1.Info.SetInfo(account.Rank, account.NickColor, account.Nickname, account.IsOnline, account.Status);
                        friend1.State = 0;
                        DaoManagerSQL.UpdatePlayerFriendState(player.PlayerId, friend1);
                        this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(FriendChangeState.Accept, (FriendModel)null, 0, this.Field0));
                        this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(FriendChangeState.Update, friend1, this.Field0));
                        int index = -1;
                        FriendModel friend2 = account.Friend.GetFriend(player.PlayerId, out index);
                        if (friend2 != null && friend2.State > 0)
                        {
                            if (friend2.Info == null)
                                friend2.SetModel(player.PlayerId, player.Rank, player.NickColor, player.Nickname, player.IsOnline, player.Status);
                            else
                                friend2.Info.SetInfo(player.Rank, player.NickColor, player.Nickname, player.IsOnline, player.Status);
                            friend2.State = 0;
                            DaoManagerSQL.UpdatePlayerFriendState(account.PlayerId, friend2);
                            SendFriendInfo.Load(account, friend2, 1);
                            account.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(FriendChangeState.Update, friend2, index), false);
                        }
                    }
                }
                else
                    this.Field1 = 2147483648U /*0x80000000*/;
                if (this.Field1 <= 0U)
                    return;
                this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_ACCEPT_ACK(this.Field1));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_FRIEND_ACCEPT_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}