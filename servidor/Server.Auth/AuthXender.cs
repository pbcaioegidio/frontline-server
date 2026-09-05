using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.XML;
using Server.Auth.Data.Sync;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace Server.Auth
{
    public class AuthXender
    {
        public static AuthSync Sync { get; set; }
        public static AuthManager Client { get; set; }
        public static ConcurrentDictionary<int, AuthClient> SocketSessions { get; set; }
        public static ConcurrentDictionary<string, DateTime> SocketConnections { get; set; }

        public static bool GetPlugin(string Host, int Port)
        {
            try
            {
                SocketSessions = new ConcurrentDictionary<int, AuthClient>();
                SocketConnections = new ConcurrentDictionary<string, DateTime>();
                IPEndPoint EP = SynchronizeXML.GetServer(Port).Connection;
                Sync = new AuthSync(EP);
                Client = new AuthManager(0, Host, ConfigLoader.IsUseProxy ? ConfigLoader.PROXY_PORT[0] : ConfigLoader.DEFAULT_PORT[0]);
                Sync.Start();
                Client.Start();
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }
    }
}