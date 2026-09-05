using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;

using Server.Auth.Data.Managers;
using Server.Auth.Data.Models;
using Server.Auth.Network.ServerPacket;


namespace Server.Auth.Data.Sync.Client
{
    public class FriendInfo
    {
        public static void Load(SyncClientPacket C)
        {
            int num1 = (int)C.ReadC();
            int num2 = (int)C.ReadC();
            long id1 = C.ReadQ();
            long id2 = C.ReadQ();
            Account account1 = AccountManager.GetAccount(id1, true);
            if (account1 == null)
                return;
            Account account2 = AccountManager.GetAccount(id2, true);
            if (account2 == null)
                return;
            FriendState friendState = num2 == 1 ? FriendState.Online : FriendState.Offline;
            if (num1 == 0)
            {
                int index = -1;
                FriendModel friend = account2.Friend.GetFriend(account1.PlayerId, out index);
                if (index == -1 || friend == null)
                    return;
                account2.SendPacket(new PROTOCOL_AUTH_FRIEND_INFO_CHANGE_ACK(FriendChangeState.Update, friend, friendState, index));
            }
            else
                account2.SendPacket(new PROTOCOL_CS_MEMBER_INFO_CHANGE_ACK(account1, friendState));
        }
    }
}