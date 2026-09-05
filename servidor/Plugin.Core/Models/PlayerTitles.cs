// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerTitles
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class PlayerTitles
    {
        public long OwnerId { get; set; }

        public long Flags { get; set; }

        public int Equiped1 { get; set; }

        public int Equiped2 { get; set; }

        public int Equiped3 { get; set; }

        public int Slots { get; set; }

        public PlayerTitles() => this.Slots = 1;

        public long Add(long flag)
        {
            this.Flags |= flag;
            return this.Flags;
        }

        public bool Contains(long flag) => (this.Flags & flag) == flag || flag == 0L;

        public void SetEquip(int index, int value)
        {
            switch (index)
            {
                case 0:
                    this.Equiped1 = value;
                    break;
                case 1:
                    this.Equiped2 = value;
                    break;
                case 2:
                    this.Equiped3 = value;
                    break;
            }
        }

        public int GetEquip(int index)
        {
            switch (index)
            {
                case 0:
                    return this.Equiped1;
                case 1:
                    return this.Equiped2;
                case 2:
                    return this.Equiped3;
                default:
                    return 0;
            }
        }
    }
}