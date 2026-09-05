// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.DeathServerData
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;


namespace Server.Match.Data.Models
{
    public class DeathServerData
    {
        public CharaDeath DeathType { get; set; }

        public PlayerModel Player { get; set; }

        public int AssistSlot { get; set; }

        public DeathServerData() => this.AssistSlot = -1;
    }
}