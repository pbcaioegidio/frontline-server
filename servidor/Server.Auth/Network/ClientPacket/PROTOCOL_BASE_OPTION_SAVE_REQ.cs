// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ClientPacket.PROTOCOL_BASE_OPTION_SAVE_REQ
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Auth.Data.Models;
using Server.Auth.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_OPTION_SAVE_REQ : AuthClientPacket
    {
        private byte[] Field0;
        private string Field1;
        private string Field2;
        private string Field3;
        private string Field4;
        private string Field5;
        private int Field6;
        private int Field7;
        private int Field8;
        private int Field9;
        private int Field10;
        private int Field11;
        private int Field12;
        private int Field13;
        private int Field14;
        private int Field15;
        private int Field16;
        private int Field17;
        private int Field18;
        private int Field19;
        private int Field20;
        private int Field21;

        public override void Read()
        {
            this.Field6 = (int)this.ReadC();
            this.Field7 = (int)this.ReadC();
            int num1 = (int)this.ReadH();
            if ((this.Field6 & 1) == 1)
            {
                this.Field8 = (int)this.ReadH();
                this.Field9 = (int)this.ReadC();
                this.Field10 = (int)this.ReadC();
                this.Field11 = this.ReadD();
                this.Field12 = (int)this.ReadC();
                this.ReadB(5);
                this.Field13 = (int)this.ReadC();
                this.Field14 = (int)this.ReadC();
                this.Field15 = (int)this.ReadH();
                this.Field16 = (int)this.ReadC();
                this.Field17 = (int)this.ReadC();
                int num2 = (int)this.ReadC();
                int num3 = (int)this.ReadC();
                this.Field18 = (int)this.ReadC();
                this.Field19 = (int)this.ReadC();
                this.Field20 = (int)this.ReadC();
                int num4 = (int)this.ReadC();
                int num5 = (int)this.ReadC();
                int num6 = (int)this.ReadC();
            }
            if ((this.Field6 & 2) == 2)
            {
                this.ReadB(5);
                this.Field0 = this.ReadB(240 /*0xF0*/);
            }
            if ((this.Field6 & 4) == 4)
            {
                this.Field1 = this.ReadU((int)this.ReadC() * 2);
                this.Field2 = this.ReadU((int)this.ReadC() * 2);
                this.Field3 = this.ReadU((int)this.ReadC() * 2);
                this.Field4 = this.ReadU((int)this.ReadC() * 2);
                this.Field5 = this.ReadU((int)this.ReadC() * 2);
            }
            if ((this.Field6 & 8) != 8)
                return;
            this.Field21 = (int)this.ReadH();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.Player;
                if (player == null || ComDiv.GetDuration(player.LastSaveConfigs) < 1.0)
                    return;
                DBQuery A_1 = new DBQuery();
                PlayerConfig config = player.Config;
                if (config != null)
                {
                    if ((this.Field6 & 1) == 1)
                        this.Method0(A_1, config);
                    if ((this.Field6 & 2) == 2 && config.KeyboardKeys != this.Field0)
                    {
                        config.KeyboardKeys = this.Field0;
                        A_1.AddQuery("keyboard_keys", (object)config.KeyboardKeys);
                    }
                    if ((this.Field6 & 4) == 4)
                        this.Method1(A_1, config);
                    if ((this.Field6 & 8) == 8 && config.Nations != this.Field21)
                    {
                        config.Nations = this.Field21;
                        A_1.AddQuery("nations", (object)config.Nations);
                    }
                    ComDiv.UpdateDB("player_configs", "owner_id", (object)player.PlayerId, A_1.GetTables(), A_1.GetValues());
                }
                this.Client.SendPacket((AuthServerPacket)new PROTOCOL_BASE_OPTION_SAVE_ACK());
                player.LastSaveConfigs = DateTimeUtil.Now();
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        
        private void Method0(DBQuery A_1, PlayerConfig A_2)
        {
            if (A_2.ShowBlood != this.Field8)
            {
                A_2.ShowBlood = this.Field8;
                A_1.AddQuery("show_blood", (object)A_2.ShowBlood);
            }
            if (A_2.Crosshair != this.Field9)
            {
                A_2.Crosshair = this.Field9;
                A_1.AddQuery("crosshair", (object)A_2.Crosshair);
            }
            if (A_2.HandPosition != this.Field10)
            {
                A_2.HandPosition = this.Field10;
                A_1.AddQuery("hand_pos", (object)A_2.HandPosition);
            }
            if (A_2.Config != this.Field11)
            {
                A_2.Config = this.Field11;
                A_1.AddQuery("configs", (object)A_2.Config);
            }
            if (A_2.AudioEnable != this.Field12)
            {
                A_2.AudioEnable = this.Field12;
                A_1.AddQuery("audio_enable", (object)A_2.AudioEnable);
            }
            if (A_2.AudioSFX != this.Field13)
            {
                A_2.AudioSFX = this.Field13;
                A_1.AddQuery("audio_sfx", (object)A_2.AudioSFX);
            }
            if (A_2.AudioBGM != this.Field14)
            {
                A_2.AudioBGM = this.Field14;
                A_1.AddQuery("audio_bgm", (object)A_2.AudioBGM);
            }
            if (A_2.PointOfView != this.Field15)
            {
                A_2.PointOfView = this.Field15;
                A_1.AddQuery("pov_size", (object)A_2.PointOfView);
            }
            if (A_2.Sensitivity != this.Field16)
            {
                A_2.Sensitivity = this.Field16;
                A_1.AddQuery("sensitivity", (object)A_2.Sensitivity);
            }
            if (A_2.InvertMouse != this.Field17)
            {
                A_2.InvertMouse = this.Field17;
                A_1.AddQuery("invert_mouse", (object)A_2.InvertMouse);
            }
            if (A_2.EnableInviteMsg != this.Field18)
            {
                A_2.EnableInviteMsg = this.Field18;
                A_1.AddQuery("enable_invite", (object)A_2.EnableInviteMsg);
            }
            if (A_2.EnableWhisperMsg != this.Field19)
            {
                A_2.EnableWhisperMsg = this.Field19;
                A_1.AddQuery("enable_whisper", (object)A_2.EnableWhisperMsg);
            }
            if (A_2.Macro == this.Field20)
                return;
            A_2.Macro = this.Field20;
            A_1.AddQuery("macro_enable", (object)A_2.Macro);
        }

        
        private void Method1(DBQuery A_1, PlayerConfig A_2)
        {
            if ((this.Field7 & 1) == 1 && A_2.Macro1 != this.Field1)
            {
                A_2.Macro1 = this.Field1;
                A_1.AddQuery("macro1", (object)A_2.Macro1);
            }
            if ((this.Field7 & 2) == 2 && A_2.Macro2 != this.Field2)
            {
                A_2.Macro2 = this.Field2;
                A_1.AddQuery("macro1", (object)A_2.Macro1);
            }
            if ((this.Field7 & 4) == 4 && A_2.Macro3 != this.Field3)
            {
                A_2.Macro3 = this.Field3;
                A_1.AddQuery("macro1", (object)A_2.Macro1);
            }
            if ((this.Field7 & 8) == 8 && A_2.Macro4 != this.Field4)
            {
                A_2.Macro4 = this.Field4;
                A_1.AddQuery("macro1", (object)A_2.Macro1);
            }
            if ((this.Field7 & 16 /*0x10*/) != 16 /*0x10*/ || !(A_2.Macro5 != this.Field5))
                return;
            A_2.Macro5 = this.Field5;
            A_1.AddQuery("macro1", (object)A_2.Macro1);
        }
    }
}