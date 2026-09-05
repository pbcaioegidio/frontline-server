// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ClientPacket.PROTOCOL_MATCH_SERVER_IDX_REQ
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
    public class PROTOCOL_MATCH_SERVER_IDX_REQ : AuthClientPacket
    {
        private short Field0;

        public override void Read()
        {
            this.Field0 = this.ReadH();
            int num = (int)this.ReadC();
        }

        
        public override void Run()
        {
            try
            {
                this.Client.SendPacket((AuthServerPacket)new PROTOCOL_MATCH_SERVER_IDX_ACK(this.Field0));
                this.Client.Close(0, false);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_MATCH_SERVER_IDX_REQ: " + ex.Message, LoggerType.Warning);
            }
        }
    }
}