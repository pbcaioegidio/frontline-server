// Decompiled with JetBrains decompiler
// Type: Server.Match.MatchXender
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.XML;
using Server.Match.Data.Sync;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;


namespace Server.Match
{
    public class MatchXender
    {
        public static MatchSync Sync { get; set; }

        public static MatchManager Client { get; set; }

        public static ConcurrentDictionary<string, int> SpamConnections { get; set; }

        public static ConcurrentDictionary<IPEndPoint, Socket> UdpClients { get; set; }

        public static bool GetPlugin(string Host, int Port)
        {
            try
            {
                MatchXender.SpamConnections = new ConcurrentDictionary<string, int>();
                MatchXender.UdpClients = new ConcurrentDictionary<IPEndPoint, Socket>();
                MatchXender.Sync = new MatchSync(SynchronizeXML.GetServer(Port).Connection);
                MatchXender.Client = new MatchManager(Host, Port);
                MatchXender.Sync.Start();
                MatchXender.Client.Start();
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }
    }
}