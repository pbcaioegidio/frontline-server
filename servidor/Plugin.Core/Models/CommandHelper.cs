// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.CommandHelper
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class CommandHelper
    {
        public string Tag { get; set; }

        public int AllWeapons { get; set; }

        public int AssaultRifle { get; set; }

        public int SubMachineGun { get; set; }

        public int SniperRifle { get; set; }

        public int ShotGun { get; set; }

        public int MachineGun { get; set; }

        public int Secondary { get; set; }

        public int Melee { get; set; }

        public int Knuckle { get; set; }

        public int RPG7 { get; set; }

        public int Minutes05 { get; set; }

        public int Minutes10 { get; set; }

        public int Minutes15 { get; set; }

        public int Minutes20 { get; set; }

        public int Minutes25 { get; set; }

        public int Minutes30 { get; set; }

        public CommandHelper(string A_1) => this.Tag = A_1;
    }
}