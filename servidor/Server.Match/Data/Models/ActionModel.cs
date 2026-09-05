// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.ActionModel
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Server.Match.Data.Enums;


namespace Server.Match.Data.Models
{
    public class ActionModel
    {
        public ushort Slot { get; set; }
        public ushort Length { get; set; }
        public UdpGameEvent Flag { get; set; }
        public UdpSubHead SubHead { get; set; }
        public byte[] Data { get; set; }
    }
}