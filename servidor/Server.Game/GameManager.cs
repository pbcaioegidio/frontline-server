using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.JSON;
using Plugin.Core.Logging;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Sync;
using Server.Game.Network;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Server.Game
{
    public class GameManager
    {
        private readonly string Host;
        private readonly int Port;
        public readonly int ServerId;
        public ServerConfig Config;
        public Socket MainSocket;
        public bool ServerIsClosed;
        public GameSync Sync;
        public readonly ConcurrentDictionary<int, GameClient> SocketSessions = new ConcurrentDictionary<int, GameClient>();
        public readonly ConcurrentDictionary<string, DateTime> SocketConnections = new ConcurrentDictionary<string, DateTime>();

        public GameManager(int ServerId, string Host, int Port)
        {
            this.Host = Host;
            this.Port = Port;
            this.ServerId = ServerId;
        }

        public bool Start()
        {
            try
            {
                Config = ServerConfigJSON.GetConfig(ConfigLoader.ConfigId);
                MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint Local = new IPEndPoint(IPAddress.Parse(Host), Port);
                MainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    MainSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted); // fix linux: IP protection levels are a Windows socket option
                MainSocket.DontFragment = false;
                MainSocket.NoDelay = true;
                MainSocket.Bind(Local);
                MainSocket.Listen(ConfigLoader.BackLog);
                CLogger.Print($"Endereco Game {Host}:{Port}", LoggerType.Info);
                Thread OnDuty = new Thread(ReadCallback)
                {
                    Priority = ThreadPriority.Highest
                };
                OnDuty.Start();
                return true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return false;
            }
        }

        private void ReadCallback()
        {
            CLogger.SetThreadKind(ServerKind.Game);
            try
            {
                MainSocket.BeginAccept(new AsyncCallback(AcceptCallback), MainSocket);
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        private void AcceptCallback(IAsyncResult Result)
        {
            CLogger.SetThreadKind(ServerKind.Game);
            if (ServerIsClosed)
            {
                return;
            }
            Socket ClientSocket = Result.AsyncState as Socket;
            Socket Handler = null;
            try
            {
                Handler = ClientSocket.EndAccept(Result);
            }
            catch (Exception ex)
            {
                if ((DateTimeUtil.Now() - CLogger.LastGameException).Minutes >= 1)
                {
                    CLogger.Print($"Accept Callback Date: {DateTimeUtil.Now()}; Exception: {ex.Message}", LoggerType.Error);
                    CLogger.LastGameException = DateTimeUtil.Now();
                }
            }
            SessionCallBack(Handler, ClientSocket);
        }

        private void SessionCallBack(Socket Handler, Socket ClientSocket)
        {
            try
            {
                Thread.Sleep(5);
                ReadCallback();
                if (Handler != null)
                {
                    AddSession(new GameClient(ServerId, Handler));
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        public void AddSession(GameClient Client)
        {
            try
            {
                if (Client == null)
                {
                    CLogger.Print("Destroyed after failed to add to list.", LoggerType.Warning);
                    return;
                }
                DateTime Connect = DateTimeUtil.Now();
                string ip = Client.GetIPAddress();
                if (IsMaxConnectionsReached(ip))
                {
                    CLogger.Print($"[FL GUARD] MaxConnectionPerIp ({ConfigLoader.MaxConnectionPerIp}) atingido. IP: {ip}", LoggerType.Warning);
                    Plugin.Core.Security.SecurityDao.LogEvent(Plugin.Core.Security.SecurityDao.SourceGame, 0, "", "", "login_denied", "MAX_CONN_IP",
                        "Conexões demais do mesmo IP", "{\"ip\":\"" + ip + "\"}", 2, "FG-141");
                    Client.Close(500, true);
                    return;
                }
                if (!IsConnectionThrottled(ip, Connect))
                {
                    for (int SessionId = 1; SessionId < 100000; SessionId++)
                    {
                        if (!SocketSessions.ContainsKey(SessionId) && SocketSessions.TryAdd(SessionId, Client))
                        {
                            Client.SessionDate = Connect;
                            Client.SessionId = SessionId;
                            Client.SessionSeed = (ushort)new Random(Connect.Millisecond).Next(SessionId, 0x7FFF);
                            Client.StartSession();
                            return;
                        }
                    }
                    CLogger.Print($"Unable to add session list. IPAddress: {ip}; Date: {Connect}", LoggerType.Warning);
                    Client.Close(500, true);
                }
                else
                {
                    if ((Connect - CLogger.LastGameSession).Minutes >= 1)
                    {
                        CLogger.Print($"This connection is blocked for {ConfigLoader.ConnectionThrottleSeconds} seconds. IP: {ip}; Date:{Connect}", LoggerType.Warning);
                        CLogger.LastGameSession = Connect;
                    }
                    Client.Close(500, true);
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        private bool IsMaxConnectionsReached(string address)
        {
            if (ConfigLoader.MaxConnectionPerIp <= 0 || ComDiv.IsLoopbackAddress(address))
                return false;
            int count = 0;
            foreach (GameClient c in SocketSessions.Values)
            {
                try
                {
                    if (c != null && string.Equals(c.GetIPAddress(), address, StringComparison.OrdinalIgnoreCase))
                        count++;
                }
                catch { }
            }
            return count >= ConfigLoader.MaxConnectionPerIp;
        }

        private bool IsConnectionThrottled(string Address, DateTime Connect)
        {
            if (ConfigLoader.ConnectionThrottleSeconds <= 0 || ComDiv.IsLoopbackAddress(Address))
            {
                return false;
            }
            DateTime Last;
            if (!SocketConnections.TryGetValue(Address, out Last))
            {
                return !SocketConnections.TryAdd(Address, Connect);
            }
            if ((Connect - Last).TotalSeconds < ConfigLoader.ConnectionThrottleSeconds)
            {
                return true;
            }
            return !SocketConnections.TryUpdate(Address, Connect, Last);
        }

        public bool RemoveSession(GameClient Client)
        {
            try
            {
                if (Client == null || Client.SessionId == 0)
                {
                    return false;
                }
                if (SocketSessions.ContainsKey(Client.SessionId) && SocketSessions.TryGetValue(Client.SessionId, out Client))
                {
                    return SocketSessions.TryRemove(Client.SessionId, out Client);
                }
                Client = null;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
            return false;
        }

        public bool RemoveConnection(string Address)
        {
            try
            {
                if (SocketConnections.ContainsKey(Address) && SocketConnections.TryGetValue(Address, out DateTime Date))
                {
                    return SocketConnections.TryRemove(Address, out Date);
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
            return false;
        }

        public int SendPacketToAllClients(GameServerPacket Packet)
        {
            int Count = 0;
            if (SocketSessions.Count == 0)
            {
                return Count;
            }
            byte[] Data = Packet.GetCompleteBytes("GameManager.SendPacketToAllClients");
            foreach (GameClient Client in SocketSessions.Values)
            {
                Account Player = Client.GetAccount();
                if (Player != null && Player.IsOnline)
                {
                    Player.SendCompletePacket(Data, Packet.GetType().Name);
                    Count++;
                }
            }
            return Count;
        }

        public Account SearchActiveClient(long accountId)
        {
            if (SocketSessions.Count == 0)
            {
                return null;
            }
            foreach (GameClient client in SocketSessions.Values)
            {
                Account player = client.Player;
                if (player != null && player.PlayerId == accountId)
                {
                    return player;
                }
            }
            return null;
        }

        public Account SearchActiveClient(uint sessionId)
        {
            if (SocketSessions.Count == 0)
            {
                return null;
            }
            foreach (GameClient client in SocketSessions.Values)
            {
                if (client.Player != null && client.SessionId == sessionId)
                {
                    return client.Player;
                }
            }
            return null;
        }

        public int KickActiveClient(double Hours)
        {
            int count = 0;
            DateTime now = DateTimeUtil.Now();
            foreach (GameClient client in SocketSessions.Values)
            {
                Account pl = client.Player;
                if (pl != null && pl.Room == null && pl.ChannelId > -1 && !pl.IsGM() && (now - pl.LastLobbyEnter).TotalHours >= Hours)
                {
                    count++;
                    pl.Close(5000);
                }
            }
            return count;
        }

        public int KickCountActiveClient(double Hours)
        {
            int count = 0;
            DateTime now = DateTimeUtil.Now();
            foreach (GameClient client in SocketSessions.Values)
            {
                Account pl = client.Player;
                if (pl != null && pl.Room == null && pl.ChannelId > -1 && !pl.IsGM() && (now - pl.LastLobbyEnter).TotalHours >= Hours)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
