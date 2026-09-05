// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.FriendInfo
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;


namespace Server.Game.Data.Sync.Client
{
    public class FriendInfo
    {
        public static void Load(SyncClientPacket C)
        {
            int num1 = (int)C.ReadC();
            int num2 = (int)C.ReadC();
            long id1 = C.ReadQ();
            long id2 = C.ReadQ();
            Account account1 = AccountManager.GetAccount(id1, 31 /*0x1F*/);
            if (account1 == null)
                return;
            Account account2 = AccountManager.GetAccount(id2, true);
            if (account2 == null)
                return;
            FriendState friendState = num2 == 1 ? FriendState.Online : FriendState.Offline;
            if (num1 != 0)
            {
                account2.SendPacket(new PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(account1, friendState));
            }
            else
            {
                int index = -1;
                FriendModel friend = account2.Friend.GetFriend(account1.PlayerId, out index);
                if (index == -1 || friend == null || friend.State != 0)
                    return;
                account2.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(FriendChangeState.Update, friend, friendState, index));
            }
        }
    }
}
