// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.FriendModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class FriendModel
    {
        public long ObjectId { get; set; }

        public long OwnerId { get; set; }

        public long PlayerId { get; set; }

        public int State { get; set; }

        public bool Removed { get; set; }

        public PlayerInfo Info { get; set; }

        public FriendModel(long A_1)
        {
            this.PlayerId = A_1;
            this.Info = new PlayerInfo(A_1);
        }

        public FriendModel(long A_1, int A_2, int A_3, string A_4, bool A_5, AccountStatus A_6)
        {
            this.PlayerId = A_1;
            this.SetModel(A_1, A_2, A_3, A_4, A_5, A_6);
        }

        public void SetModel(
          long PlayerId,
          int Rank,
          int NickColor,
          string Nickname,
          bool IsOnline,
          AccountStatus Status)
        {
            this.Info = new PlayerInfo(PlayerId, Rank, NickColor, Nickname, IsOnline, Status);
        }
    }
}