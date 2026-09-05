using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.XML;
using System;
using System.Collections.Generic;
using System.Net;

namespace Executable.UDP.Server
{
    public static class SupervisorCommands
    {
        private static uint _broadcastId;

        public static void BroadcastReload(byte subCmd)
        {
            uint broadcastId = ++_broadcastId;

            foreach (IPEndPoint target in AllServiceEndpoints())
            {
                try
                {
                    using (SyncServerPacket S = new SyncServerPacket())
                    {
                        S.WriteH(25);
                        S.WriteC(subCmd);
                        S.WriteD(broadcastId);
                        Communication.SendPacket(S.ToArray(), target);
                    }
                }
                catch (Exception Ex)
                {
                    CLogger.Print($"[SupervisorCommands] send to {target} failed: {Ex.Message}", LoggerType.Warning, Ex);
                }
            }
            CLogger.Print($"[SupervisorCommands] Reload subCmd={subCmd} broadcast to all services.", LoggerType.Command);
        }

        private static IEnumerable<IPEndPoint> AllServiceEndpoints()
        {
            Synchronize auth = SynchronizeXML.GetServer(ConfigLoader.DEFAULT_PORT[0]);
            if (auth != null) yield return auth.Connection;

            foreach (SChannelModel server in SChannelXML.Servers)
            {
                if (server == null) continue;
                Synchronize game = SynchronizeXML.GetServer(server.Port);
                if (game != null) yield return game.Connection;
            }

            Synchronize match = SynchronizeXML.GetServer(ConfigLoader.DEFAULT_PORT[2]);
            if (match != null) yield return match.Connection;
        }
    }
}
