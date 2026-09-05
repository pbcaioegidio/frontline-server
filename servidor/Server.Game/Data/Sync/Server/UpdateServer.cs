// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Server.UpdateServer
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Sync;
using Server.Game.Data.Models;
using Server.Game.Data.XML;
using System;
using System.Net;


namespace Server.Game.Data.Sync.Server
{
    public class UpdateServer
    {
        private static DateTime Field0;

        public static void RefreshSChannel(int serverId)
        {
            try
            {
                if (ComDiv.GetDuration(UpdateServer.Field0) < (double)ConfigLoader.UpdateIntervalPlayersServer)
                    return;
                UpdateServer.Field0 = DateTimeUtil.Now();
                GameSync sync = GameXender.SyncOf(serverId);
                if (sync == null)
                    return;
                int num = 0;
                foreach (ChannelModel channel in ChannelsXML.Channels)
                    num += channel.Players.Count;
                foreach (SChannelModel server in SChannelXML.Servers)
                {
                    if (server.Id != serverId)
                    {
                        IPEndPoint connection = SynchronizeXML.GetServer((int)server.Port).Connection;
                        using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                        {
                            syncServerPacket.WriteH((short)15);
                            syncServerPacket.WriteD(serverId);
                            syncServerPacket.WriteD(num);
                            sync.SendPacket(syncServerPacket.ToArray(), connection);
                        }
                    }
                    else
                        server.LastPlayers = num;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}