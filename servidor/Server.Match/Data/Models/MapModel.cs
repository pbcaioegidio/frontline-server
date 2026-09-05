// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.MapModel
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using System.Collections.Generic;


namespace Server.Match.Data.Models
{
    public class MapModel
    {
        public int Id { get; set; }

        public List<ObjectModel> Objects { get; set; }

        public List<BombPosition> Bombs { get; set; }

        public BombPosition GetBomb(int BombId)
        {
            try
            {
                return this.Bombs[BombId];
            }
            catch
            {
                return (BombPosition)null;
            }
        }
    }
}