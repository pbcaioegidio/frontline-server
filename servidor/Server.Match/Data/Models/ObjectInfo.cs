// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.ObjectInfo
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using System;


namespace Server.Match.Data.Models
{
    public class ObjectInfo
    {
        public int Id { get; set; }

        public int Life { get; set; }

        public int DestroyState { get; set; }

        public AnimModel Animation { get; set; }

        public DateTime UseDate { get; set; }

        public ObjectModel Model { get; set; }

        public ObjectInfo(int A_1)
        {
            this.Id = A_1;
            this.Life = 100;
        }
    }
}