// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Sync.Server.AuthLogin
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.XML;
using Server.Auth.Data.Models;
using System;
using System.Net;


namespace Server.Auth.Data.Sync.Server
{
    public class AuthLogin
    {
        public static void SendLoginKickInfo(Account Player)
        {
            try
            {
                int serverId = (int)Player.Status.ServerId;
                switch (serverId)
                {
                    case 0:
                    case (int)byte.MaxValue:
                        Player.SetOnlineStatus(false);
                        break;
                    default:
                        SChannelModel server = SChannelXML.GetServer(serverId);
                        if (server == null)
                            break;
                        IPEndPoint connection = SynchronizeXML.GetServer((int)server.Port).Connection;
                        using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                        {
                            syncServerPacket.WriteH((short)10);
                            syncServerPacket.WriteQ(Player.PlayerId);
                            AuthXender.Sync.SendPacket(syncServerPacket.ToArray(), connection);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}