// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ClientPacket.PROTOCOL_MATCH_CLAN_SEASON_REQ
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
    public class PROTOCOL_MATCH_CLAN_SEASON_REQ : AuthClientPacket
    {
        public override void Read()
        {
        }

        
        public override void Run()
        {
            try
            {
                this.Client.SendPacket((AuthServerPacket)new PROTOCOL_MATCH_CLAN_SEASON_ACK(0));
                this.Client.SendPacket((AuthServerPacket)new PROTOCOL_SERVER_MESSAGE_USER_EVENT_START_ACK());
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_MATCH_CLAN_SEASON_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
