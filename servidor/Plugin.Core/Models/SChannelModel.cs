// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.SChannelModel
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;

namespace Plugin.Core.Models
{
    public class SChannelModel
    {
        public int Id { get; set; }

        public int LastPlayers { get; set; }

        public int MaxPlayers { get; set; }

        public int ChannelPlayers { get; set; }

        public bool State { get; set; }

        public SChannelType Type { get; set; }

        public string Host { get; set; }

        public ushort Port { get; set; }

        public bool IsMobile { get; set; }

        public SChannelModel(string A_1, ushort A_2)
        {
            this.Host = A_1;
            this.Port = A_2;
        }
    }
}