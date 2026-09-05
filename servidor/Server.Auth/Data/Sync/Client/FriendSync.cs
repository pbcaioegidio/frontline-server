using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Auth.Data.Managers;
using Server.Auth.Data.Models;

namespace Server.Auth.Data.Sync.Client
{
    public static class FriendSync
    {
        public static void Load(SyncClientPacket C)
        {
            long id1 = C.ReadQ();
            int num1 = (int)C.ReadC();
            long id2 = C.ReadQ();
            FriendModel friend = (FriendModel)null;
            if (num1 <= 1)
            {
                int num2 = (int)C.ReadC();
                bool flag = C.ReadC() == (byte)1;
                friend = new FriendModel(id2)
                {
                    State = num2,
                    Removed = flag
                };
            }
            if (friend == null && num1 <= 1)
                return;
            Account account = AccountManager.GetAccount(id1, true);
            if (account == null)
                return;
            if (num1 <= 1)
            {
                friend.Info.Nickname = account.Nickname;
                friend.Info.Rank = account.Rank;
                friend.Info.IsOnline = account.IsOnline;
                friend.Info.Status = account.Status;
            }
            if (num1 == 0)
                account.Friend.AddFriend(friend);
            else if (num1 != 1)
            {
                if (num1 != 2)
                    return;
                account.Friend.RemoveFriend(id2);
            }
            else
                account.Friend.GetFriend(id2);
        }
    }
}