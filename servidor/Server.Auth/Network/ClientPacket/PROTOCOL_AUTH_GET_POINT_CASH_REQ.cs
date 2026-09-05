// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ClientPacket.PROTOCOL_AUTH_GET_POINT_CASH_REQ
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Auth.Data.Models;
using Server.Auth.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Auth.Network.ClientPacket
{
    public class PROTOCOL_AUTH_GET_POINT_CASH_REQ : AuthClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Account player = Client.Player;
                if (player == null)
                    return;
                Client.SendPacket((AuthServerPacket)new PROTOCOL_AUTH_GET_POINT_CASH_ACK(0U, player));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_AUTH_GET_POINT_CASH_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}