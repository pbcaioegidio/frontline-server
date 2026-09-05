// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ClientPacket.PROTOCOL_BASE_LOGOUT_REQ
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
    public class PROTOCOL_BASE_LOGOUT_REQ : AuthClientPacket
    {
        public override void Read()
        {
        }

        
        public override void Run()
        {
            try
            {
                this.Client.SendPacket((AuthServerPacket)new PROTOCOL_BASE_LOGOUT_ACK());
                this.Client.Close(5000, true);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_LOGOUT_REQ: " + ex.Message, LoggerType.Error, ex);
                this.Client.Close(0, true);
            }
        }
    }
}