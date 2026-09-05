// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Models.PlayerConfig
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

namespace Plugin.Core.Models
{
    public class PlayerConfig
    {
        public long OwnerId { get; set; }

        public int Crosshair { get; set; }

        public int AudioSFX { get; set; }

        public int AudioBGM { get; set; }

        public int Sensitivity { get; set; }

        public int PointOfView { get; set; }

        public int ShowBlood { get; set; }

        public int HandPosition { get; set; }

        public int AudioEnable { get; set; }

        public int Config { get; set; }

        public int InvertMouse { get; set; }

        public int EnableInviteMsg { get; set; }

        public int EnableWhisperMsg { get; set; }

        public int Macro { get; set; }

        public int Nations { get; set; }

        public string Macro1 { get; set; }

        public string Macro2 { get; set; }

        public string Macro3 { get; set; }

        public string Macro4 { get; set; }

        public string Macro5 { get; set; }

        public byte[] KeyboardKeys { get; set; }

        public PlayerConfig()
        {
            this.AudioSFX = 100;
            this.AudioBGM = 60;
            this.Crosshair = 2;
            this.Sensitivity = 50;
            this.PointOfView = 80 /*0x50*/;
            this.ShowBlood = 11;
            this.AudioEnable = 7;
            this.Config = 55;
            this.Macro = 31 /*0x1F*/;
            this.Macro1 = "";
            this.Macro2 = "";
            this.Macro3 = "";
            this.Macro4 = "";
            this.Macro5 = "";
            this.Nations = 0;
            this.KeyboardKeys = new byte[240 /*0xF0*/];
        }
    }
}