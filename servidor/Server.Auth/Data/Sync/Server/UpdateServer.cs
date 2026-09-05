// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Sync.Server.UpdateServer
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using System;
using System.Net;

namespace Server.Auth.Data.Sync.Server
{
    public class UpdateServer
    {
        private static DateTime Field0;

        public static void RefreshSChannel(int ServerId)
        {
            try
            {
                if (ComDiv.GetDuration(UpdateServer.Field0) < (double)ConfigLoader.UpdateIntervalPlayersServer)
                    return;
                UpdateServer.Field0 = DateTimeUtil.Now();
                int count = AuthXender.SocketSessions.Count;
                foreach (SChannelModel server in SChannelXML.Servers)
                {
                    if (server.Id != ServerId)
                    {
                        IPEndPoint connection = SynchronizeXML.GetServer((int)server.Port).Connection;
                        using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                        {
                            syncServerPacket.WriteH((short)15);
                            syncServerPacket.WriteD(ServerId);
                            syncServerPacket.WriteD(count);
                            AuthXender.Sync.SendPacket(syncServerPacket.ToArray(), connection);
                        }
                    }
                    else
                        server.LastPlayers = count;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}