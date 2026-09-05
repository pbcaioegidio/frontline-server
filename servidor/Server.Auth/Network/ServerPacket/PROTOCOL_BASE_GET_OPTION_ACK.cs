// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_GET_OPTION_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Models;
using Plugin.Core.Utility;
using System.Runtime.CompilerServices;


namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_GET_OPTION_ACK : AuthServerPacket
    {
        private readonly int Field0;
        private readonly PlayerConfig Field1;

        public PROTOCOL_BASE_GET_OPTION_ACK(int A_1, PlayerConfig A_2)
        {
            this.Field0 = A_1;
            this.Field1 = A_2;
        }

        
        public override void Write()
        {
            this.WriteH((short)2321);
            this.WriteH((short)0);
            this.WriteD(this.Field0);
            if (this.Field0 != 0)
                return;
            this.WriteH((ushort)this.Field1.Nations);
            this.WriteH((ushort)this.Field1.Macro5.Length);
            this.WriteN(this.Field1.Macro5, this.Field1.Macro5.Length, "UTF-16LE");
            this.WriteH((ushort)this.Field1.Macro4.Length);
            this.WriteN(this.Field1.Macro4, this.Field1.Macro4.Length, "UTF-16LE");
            this.WriteH((ushort)this.Field1.Macro3.Length);
            this.WriteN(this.Field1.Macro3, this.Field1.Macro3.Length, "UTF-16LE");
            this.WriteH((ushort)this.Field1.Macro2.Length);
            this.WriteN(this.Field1.Macro2, this.Field1.Macro2.Length, "UTF-16LE");
            this.WriteH((ushort)this.Field1.Macro1.Length);
            this.WriteN(this.Field1.Macro1, this.Field1.Macro1.Length, "UTF-16LE");
            this.WriteH((short)49);
            WriteB(Bitwise.HexStringToByteArray("DA 7F 1B 00"));
            this.WriteB(this.Field1.KeyboardKeys);
            this.WriteH((short)this.Field1.ShowBlood);
            this.WriteC((byte)this.Field1.Crosshair);
            this.WriteC((byte)this.Field1.HandPosition);
            this.WriteD(this.Field1.Config);
            this.WriteD(this.Field1.AudioEnable);
            this.WriteH((short)0);
            this.WriteC((byte)this.Field1.AudioSFX);
            this.WriteC((byte)this.Field1.AudioBGM);
            this.WriteC((byte)this.Field1.PointOfView);
            this.WriteC((byte)0);
            this.WriteC((byte)this.Field1.Sensitivity);
            this.WriteC((byte)this.Field1.InvertMouse);
            this.WriteH((short)0);
            this.WriteC((byte)this.Field1.EnableInviteMsg);
            this.WriteC((byte)this.Field1.EnableWhisperMsg);
            this.WriteD(this.Field1.Macro);
        }
    }
}