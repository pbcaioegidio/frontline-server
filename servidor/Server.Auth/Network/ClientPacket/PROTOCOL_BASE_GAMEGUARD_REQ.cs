// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ClientPacket.PROTOCOL_BASE_GAMEGUARD_REQ
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Auth.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_BASE_GAMEGUARD_REQ : AuthClientPacket
    {
        private byte[] Version;

        public override void Read()
        {
            this.ReadB(48 /*0x30*/);
            this.Version = this.ReadB(3);
        }
        public override void Run()
        {
            try
            {
                // PB121: C2S 2312 is the response to optional S2C challenge 2311.
                // The client has no S2C 2312 handler, so accepting the response is enough.
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_GAMEGUARD_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
