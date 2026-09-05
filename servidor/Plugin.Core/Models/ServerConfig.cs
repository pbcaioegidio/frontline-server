// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.ServerConfig
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;

namespace Plugin.Core.Models
{
    public class ServerConfig
    {
        public int ConfigId { get; set; }

        public int ChannelAnnounceColor { get; set; }

        public int ChatAnnounceColor { get; set; }

        public bool OnlyGM { get; set; }

        public bool AccessUFL { get; set; }

        public bool Missions { get; set; }

        public bool GiftSystem { get; set; }

        public bool EnableClan { get; set; }

        public bool EnableTicket { get; set; }

        public bool EnablePlaytime { get; set; }

        public bool EnableTags { get; set; }

        public bool EnableBlood { get; set; }

        public bool OfficialBannerEnabled { get; set; }

        public string UserFileList { get; set; }

        public string ClientVersion { get; set; }

        public string ExitURL { get; set; }

        public string ShopURL { get; set; }

        public string OfficialSteam { get; set; }

        public string OfficialBanner { get; set; }

        public string OfficialAddress { get; set; }

        public string ChannelAnnounce { get; set; }

        public string ChatAnnounce { get; set; }

        public ShowroomView Showroom { get; set; }

        public ServerConfig()
        {
            this.UserFileList = "";
            this.ClientVersion = "";
            this.ExitURL = "";
            this.ShopURL = "";
            this.OfficialSteam = "";
            this.OfficialBanner = "";
            this.OfficialAddress = "";
            this.ChannelAnnounce = "";
            this.ChatAnnounce = "";
            this.Showroom = ShowroomView.S_Default;
        }
    }
}