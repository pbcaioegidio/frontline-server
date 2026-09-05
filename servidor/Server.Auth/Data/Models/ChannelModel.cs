// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Models.ChannelModel
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;

namespace Server.Auth.Data.Models
{
    public class ChannelModel
    {
        public int Id { get; set; }
        public ChannelType Type { get; set; }
        public int ServerId { get; set; }
        public int TotalPlayers { get; set; }
        public int MaxRooms { get; set; }
        public int ExpBonus { get; set; }
        public int GoldBonus { get; set; }
        public int CashBonus { get; set; }
        public string Password { get; set; }
        public ChannelModel(int A_1) => this.ServerId = A_1;
    }
}
