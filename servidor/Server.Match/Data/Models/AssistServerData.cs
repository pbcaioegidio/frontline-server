// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Models.AssistServerData
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll


namespace Server.Match.Data.Models
{
    public class AssistServerData
    {
        public int RoomId { get; set; }

        public int Killer { get; set; }

        public int Victim { get; set; }

        public int Damage { get; set; }

        public bool IsAssist { get; set; }

        public bool IsKiller { get; set; }

        public bool VictimDead { get; set; }

        public AssistServerData()
        {
            this.Killer = -1;
            this.Victim = -1;
        }
    }
}