// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.PacketModel
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using System;


namespace Server.Match.Data.Models
{
    public class PacketModel
    {
        public int Opcode { get; set; }

        public int Slot { get; set; }

        public int Round { get; set; }

        public int Length { get; set; }

        public int Respawn { get; set; }

        public int RoundNumber { get; set; }

        public int AccountId { get; set; }

        public int Unk1 { get; set; }

        public int Unk2 { get; set; }

        public float Time { get; set; }

        public byte[] Data { get; set; }

        public byte[] WithEndData { get; set; }

        public byte[] WithoutEndData { get; set; }

        public DateTime ReceiveDate { get; set; }
    }
}