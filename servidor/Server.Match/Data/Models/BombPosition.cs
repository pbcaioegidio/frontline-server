// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.BombPosition
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.SharpDX;


namespace Server.Match.Data.Models
{
    public class BombPosition
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public Half3 Position { get; set; }

        public bool EveryWhere { get; set; }
    }
}