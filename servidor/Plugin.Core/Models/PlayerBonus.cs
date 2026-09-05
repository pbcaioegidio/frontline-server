// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerBonus
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Utility;

namespace Plugin.Core.Models
{
    public class PlayerBonus
    {
        public long OwnerId { get; set; }

        public int Bonuses { get; set; }

        public int CrosshairColor { get; set; }

        public int MuzzleColor { get; set; }

        public int FreePass { get; set; }

        public int FakeRank { get; set; }

        public int NickBorderColor { get; set; }

        public string FakeNick { get; set; }

        public PlayerBonus()
        {
            this.CrosshairColor = 4;
            this.FakeRank = RankDomain.NoFakeRank;
            this.FakeNick = "";
        }

        public bool RemoveBonuses(int ItemId)
        {
            int bonuses = this.Bonuses;
            int freePass = this.FreePass;
            switch (ItemId)
            {
                case 1600001:
                    this.Method0(1);
                    break;
                case 1600002:
                    this.Method0(2);
                    break;
                case 1600003:
                    this.Method0(4);
                    break;
                case 1600004:
                    this.Method0(32 /*0x20*/);
                    break;
                case 1600011:
                case 1600162:
                    this.Method2(128 /*0x80*/);
                    break;
                case 1600012:
                    this.Method0(256 /*0x100*/);
                    break;
                case 1600037:
                    this.Method0(8);
                    break;
                case 1600038:
                    this.Method0(128 /*0x80*/);
                    break;
                case 1600119:
                    this.Method0(64 /*0x40*/);
                    break;
                case 1600201:
                    this.Method0(512 /*0x0200*/);
                    break;
                case 1600202:
                    this.Method0(1024 /*0x0400*/);
                    break;
                case 1600203:
                    this.Method0(2048 /*0x0800*/);
                    break;
                case 1600204:
                    this.Method0(4096 /*0x1000*/);
                    break;
            }
            return this.Bonuses != bonuses || this.FreePass != freePass;
        }

        public bool AddBonuses(int ItemId)
        {
            int bonuses = this.Bonuses;
            int freePass = this.FreePass;
            switch (ItemId)
            {
                case 1600001:
                    this.Method1(1);
                    break;
                case 1600002:
                    this.Method1(2);
                    break;
                case 1600003:
                    this.Method1(4);
                    break;
                case 1600004:
                    this.Method1(32 /*0x20*/);
                    break;
                case 1600011:
                case 1600162:
                    this.Method3(128 /*0x80*/);
                    break;
                case 1600012:
                    this.Method1(256 /*0x100*/);
                    break;
                case 1600037:
                    this.Method1(8);
                    break;
                case 1600038:
                    this.Method1(128 /*0x80*/);
                    break;
                case 1600119:
                    this.Method1(64 /*0x40*/);
                    break;
                case 1600201:
                    this.Method1(512 /*0x0200*/);
                    break;
                case 1600202:
                    this.Method1(1024 /*0x0400*/);
                    break;
                case 1600203:
                    this.Method1(2048 /*0x0800*/);
                    break;
                case 1600204:
                    this.Method1(4096 /*0x1000*/);
                    break;
            }
            return this.Bonuses != bonuses || this.FreePass != freePass;
        }

        private void Method0(int A_1) => this.Bonuses &= ~A_1;

        private void Method1(int A_1) => this.Bonuses |= A_1;

        private void Method2(int A_1) => this.FreePass &= ~A_1;

        private void Method3(int A_1) => this.FreePass |= A_1;
    }
}