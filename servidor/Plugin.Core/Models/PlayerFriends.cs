// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerFriends
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Collections.Generic;

namespace Plugin.Core.Models
{
    public class PlayerFriends
    {
        public bool MemoryCleaned;

        public List<FriendModel> Friends { get; set; }

        public PlayerFriends() => this.Friends = new List<FriendModel>();

        public void CleanList()
        {
            lock (this.Friends)
            {
                foreach (FriendModel friend in this.Friends)
                    friend.Info = (PlayerInfo)null;
            }
            this.MemoryCleaned = true;
        }

        public void AddFriend(FriendModel friend)
        {
            lock (this.Friends)
                this.Friends.Add(friend);
        }

        public bool RemoveFriend(FriendModel friend)
        {
            lock (this.Friends)
                return this.Friends.Remove(friend);
        }

        public void RemoveFriend(int index)
        {
            lock (this.Friends)
                this.Friends.RemoveAt(index);
        }

        public void RemoveFriend(long id)
        {
            lock (this.Friends)
            {
                for (int index = 0; index < this.Friends.Count; ++index)
                {
                    if (this.Friends[index].PlayerId == id)
                    {
                        this.Friends.RemoveAt(index);
                        break;
                    }
                }
            }
        }

        public int GetFriendIdx(long id)
        {
            lock (this.Friends)
            {
                for (int index = 0; index < this.Friends.Count; ++index)
                {
                    if (this.Friends[index].PlayerId == id)
                        return index;
                }
            }
            return -1;
        }

        public FriendModel GetFriend(int idx)
        {
            lock (this.Friends)
            {
                try
                {
                    return this.Friends[idx];
                }
                catch
                {
                    return (FriendModel)null;
                }
            }
        }

        public FriendModel GetFriend(long id)
        {
            lock (this.Friends)
            {
                for (int index = 0; index < this.Friends.Count; ++index)
                {
                    FriendModel friend = this.Friends[index];
                    if (friend.PlayerId == id)
                        return friend;
                }
            }
            return (FriendModel)null;
        }

        public FriendModel GetFriend(long id, out int index)
        {
            lock (this.Friends)
            {
                for (int index1 = 0; index1 < this.Friends.Count; ++index1)
                {
                    FriendModel friend = this.Friends[index1];
                    if (friend.PlayerId == id)
                    {
                        index = index1;
                        return friend;
                    }
                }
            }
            index = -1;
            return (FriendModel)null;
        }
    }
}