// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.FriendSync
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;


namespace Server.Game.Data.Sync.Client
{
    public class FriendSync
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
            else if (num1 == 1)
            {
                account.Friend.GetFriend(id2);
            }
            else
            {
                if (num1 != 2)
                    return;
                account.Friend.RemoveFriend(id2);
            }
        }
    }
}