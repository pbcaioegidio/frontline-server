// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_AUTH_FRIEND_INSERT_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_AUTH_FRIEND_INSERT_REQ : GameClientPacket
    {
        private string Field0;
        private int Field1;
        private int Field2;

        public override void Read() => this.Field0 = this.ReadU(66);

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                if (player.Nickname.Length != 0 && !(player.Nickname == this.Field0))
                {
                    if (player.Friend.Friends.Count < 50)
                    {
                        Account account = AccountManager.GetAccount(this.Field0, 1, 287);
                        if (account == null)
                            this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INSERT_ACK(2147487810U));
                        else if (player.Friend.GetFriendIdx(account.PlayerId) == -1)
                        {
                            if (account.Friend.Friends.Count >= 50)
                            {
                                this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INSERT_ACK(2147487800U));
                            }
                            else
                            {
                                int num = AllUtils.AddFriend(account, player, 2);
                                if (AllUtils.AddFriend(player, account, num == 1 ? 0 : 1) != -1 && num != -1)
                                {
                                    FriendModel friend1 = account.Friend.GetFriend(player.PlayerId, out this.Field2);
                                    if (friend1 != null)
                                    {
                                        MessageModel A_1 = this.Method0(player.Nickname, account.PlayerId, this.Client.PlayerId);
                                        if (A_1 != null)
                                            account.SendPacket(new PROTOCOL_MESSENGER_NOTE_RECEIVE_ACK(A_1), false);
                                        account.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(num == 0 ? FriendChangeState.Insert : FriendChangeState.Update, friend1, this.Field2), false);
                                    }
                                    FriendModel friend2 = player.Friend.GetFriend(account.PlayerId, out this.Field1);
                                    if (friend2 == null)
                                        return;
                                    this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(FriendChangeState.Insert, friend2, this.Field1));
                                }
                                else
                                    this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INSERT_ACK(2147487801U));
                            }
                        }
                        else
                            this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INSERT_ACK(2147487809U));
                    }
                    else
                        this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INSERT_ACK(2147487800U));
                }
                else
                    this.Client.SendPacket(new PROTOCOL_AUTH_FRIEND_INSERT_ACK(2147487799U));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_FRIEND_INVITED_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        private MessageModel Method0(string A_1, long A_2, long A_3)
        {
            MessageModel Message = new MessageModel(7.0)
            {
                SenderId = A_3,
                SenderName = A_1,
                Type = NoteMessageType.Insert,
                State = NoteMessageState.Unreaded
            };
            return DaoManagerSQL.CreateMessage(A_2, Message) ? Message : (MessageModel)null;
        }
    }
}