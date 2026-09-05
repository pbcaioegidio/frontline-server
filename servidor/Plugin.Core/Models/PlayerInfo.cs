// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerInfo
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Models
{
    public class PlayerInfo
    {
        public int Rank { get; set; }

        public int NickColor { get; set; }

        public long PlayerId { get; set; }

        public string Nickname { get; set; }

        public bool IsOnline { get; set; }

        public AccountStatus Status { get; set; }

        public uint ClanLogo { get; set; } = uint.MaxValue;

        public PlayerInfo(long A_1)
        {
            this.PlayerId = A_1;
            this.Status = new AccountStatus();
        }

        public PlayerInfo(long A_1, int A_2, int A_3, string A_4, bool A_5, AccountStatus A_6)
        {
            this.PlayerId = A_1;
            this.SetInfo(A_2, A_3, A_4, A_5, A_6);
        }

        
        public void SetOnlineStatus(bool state)
        {
            if (this.IsOnline == state || !ComDiv.UpdateDB("accounts", "online", (object)state, "player_id", (object)this.PlayerId))
                return;
            this.IsOnline = state;
        }

        public void SetInfo(
          int Rank,
          int NickColor,
          string Nickname,
          bool IsOnline,
          AccountStatus Status)
        {
            this.Rank = Rank;
            this.NickColor = NickColor;
            this.Nickname = Nickname;
            this.IsOnline = IsOnline;
            this.Status = Status;
        }
    }
}