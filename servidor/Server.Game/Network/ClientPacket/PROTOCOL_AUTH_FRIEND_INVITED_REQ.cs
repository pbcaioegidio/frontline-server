// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_FRIEND_INVITED_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_FRIEND_INVITED_REQ : GameClientPacket
    {
        private string Field0;

        public override void Read() => this.Field0 = this.ReadU(66);

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                if (player.Nickname.Length != 0 && !(player.Nickname == this.Field0) && ComDiv.GetDuration(player.LastFriendInvite) >= 1.0)
                {
                    if (player.Friend.Friends.Count >= 50)
                    {
                        this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147487800U));
                    }
                    else
                    {
                        Account account = AccountManager.GetAccount(this.Field0, 1, 287);
                        if (account != null)
                        {
                            if (player.Friend.GetFriendIdx(account.PlayerId) != -1)
                                this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147487809U));
                            else if (account.Friend.Friends.Count < 50)
                            {
                                int num = AllUtils.AddFriend(account, player, 2);
                                if (AllUtils.AddFriend(player, account, num == 1 ? 0 : 1) != -1 && num != -1)
                                {
                                    int index1;
                                    FriendModel friend1 = account.Friend.GetFriend(player.PlayerId, out index1);
                                    if (friend1 != null)
                                        account.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(num == 0 ? FriendChangeState.Insert : FriendChangeState.Update, friend1, index1), false);
                                    int index2;
                                    FriendModel friend2 = player.Friend.GetFriend(account.PlayerId, out index2);
                                    if (friend2 != null)
                                        this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(FriendChangeState.Insert, friend2, index2));
                                    player.LastFriendInvite = DateTimeUtil.Now();
                                }
                                else
                                    this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147487801U));
                            }
                            else
                                this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147487800U));
                        }
                        else
                            this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147487810U));
                    }
                }
                else
                    this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INVITED_ACK(2147487799U));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_FRIEND_INVITED_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}