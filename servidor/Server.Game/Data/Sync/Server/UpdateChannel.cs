// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Server.UpdateChannel
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.XML;
using Server.Game.Data.Sync;
using System;
using System.Net;

namespace Server.Game.Data.Sync.Server
{
    public class UpdateChannel
    {
        public static void RefreshChannel(int ServerId, int ChannelId, int Count)
        {
            try
            {
                GameSync sync = GameXender.SyncOf(ServerId);
                if (sync == null)
                    return;
                SChannelModel server = sync.GetServer(0);
                if (server == null)
                    return;
                IPEndPoint connection = SynchronizeXML.GetServer((int)server.Port).Connection;
                using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                {
                    syncServerPacket.WriteH((short)33);
                    syncServerPacket.WriteD(ServerId);
                    syncServerPacket.WriteD(ChannelId);
                    syncServerPacket.WriteD(Count);
                    sync.SendPacket(syncServerPacket.ToArray(), connection);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}