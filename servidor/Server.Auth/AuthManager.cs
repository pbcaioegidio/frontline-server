using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.JSON;
using Plugin.Core.Logging;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Auth.Data.Models;
using Server.Auth.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Server.Auth
{
    public class AuthManager
    {
        private readonly string Host;
        private readonly int Port;
        public readonly int ServerId;
        public ServerConfig Config;
        public Socket MainSocket;
        public bool ServerIsClosed;

        public AuthManager(int ServerId, string Host, int Port)
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
                Bitwise.WarmupRsaPool(Bitwise.CRYPTO[2], 32);
                MainSocket.Listen(ConfigLoader.BackLog);
                CLogger.Print($"Endereco Auth {Host}:{Port}", LoggerType.Info);
                Thread OnDuty = new Thread(ReadCallBack)
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

        private void ReadCallBack()
        {
            CLogger.SetThreadKind(ServerKind.Auth);
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
            CLogger.SetThreadKind(ServerKind.Auth);
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
                if ((DateTimeUtil.Now() - CLogger.LastAuthException).Minutes >= 1)
                {
                    CLogger.Print($"Accept Callback Date: {DateTimeUtil.Now()}; Exception: {ex.Message}", LoggerType.Error);
                    CLogger.LastAuthException = DateTimeUtil.Now();
                }
            }
            SessionCallback(Handler);
        }

        private void SessionCallback(Socket Handler)
        {
            try
            {
                Thread.Sleep(5);
                ReadCallBack();
                if (Handler != null)
                {
                    AddSession(new AuthClient(ServerId, Handler));
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        public void AddSession(AuthClient Client)
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
                    CLogger.Print($"[FL GUARD] MaxConnectionPerIp ({ConfigLoader.MaxConnectionPerIp}) no Auth. IP: {ip}", LoggerType.Warning);
                    Plugin.Core.Security.SecurityDao.LogEvent(Plugin.Core.Security.SecurityDao.SourceAuth, 0, "", "", "login_denied", "MAX_CONN_IP",
                        "Conexões demais do mesmo IP (Auth)", "{\"ip\":\"" + ip + "\"}", 2, "FG-141");
                    Client.Close(500, true);
                    return;
                }
                if (!IsConnectionThrottled(ip, Connect))
                {
                    for (int SessionId = 1; SessionId < 100000; SessionId++)
                    {
                        if (!AuthXender.SocketSessions.ContainsKey(SessionId) && AuthXender.SocketSessions.TryAdd(SessionId, Client))
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
                    if ((Connect - CLogger.LatchAuthSession).Minutes >= 1)
                    {
                        CLogger.Print($"This connection is blocked for {ConfigLoader.ConnectionThrottleSeconds} seconds. IP: {ip}; Date:{Connect}", LoggerType.Warning);
                        CLogger.LatchAuthSession = Connect;
                    }
                    Client.Close(500, true);
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        private static bool IsMaxConnectionsReached(string address)
        {
            if (ConfigLoader.MaxConnectionPerIp <= 0 || ComDiv.IsLoopbackAddress(address))
                return false;
            int count = 0;
            foreach (AuthClient c in AuthXender.SocketSessions.Values)
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

        private static bool IsConnectionThrottled(string Address, DateTime Connect)
        {
            if (ConfigLoader.ConnectionThrottleSeconds <= 0 || ComDiv.IsLoopbackAddress(Address))
            {
                return false;
            }
            DateTime Last;
            if (!AuthXender.SocketConnections.TryGetValue(Address, out Last))
            {
                return !AuthXender.SocketConnections.TryAdd(Address, Connect);
            }
            if ((Connect - Last).TotalSeconds < ConfigLoader.ConnectionThrottleSeconds)
            {
                return true;
            }
            return !AuthXender.SocketConnections.TryUpdate(Address, Connect, Last);
        }

        public bool RemoveSession(AuthClient Client)
        {
            try
            {
                if (Client == null || Client.SessionId == 0)
                {
                    return false;
                }
                if (AuthXender.SocketSessions.ContainsKey(Client.SessionId) && AuthXender.SocketSessions.TryGetValue(Client.SessionId, out Client))
                {
                    return AuthXender.SocketSessions.TryRemove(Client.SessionId, out Client);
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
                if (AuthXender.SocketConnections.ContainsKey(Address) && AuthXender.SocketConnections.TryGetValue(Address, out DateTime Date))
                {
                    return AuthXender.SocketConnections.TryRemove(Address, out Date);
                }
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
            return false;
        }

        public int SendPacketToAllClients(AuthServerPacket Packet)
        {
            int Count = 0;
            if (AuthXender.SocketSessions.Count == 0)
            {
                return Count;
            }
            byte[] Data = Packet.GetCompleteBytes("AuthManager.SendPacketToAllClients");
            foreach (AuthClient Client in AuthXender.SocketSessions.Values)
            {
                Account Player = Client.Player;
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
            if (AuthXender.SocketSessions.Count == 0)
            {
                return null;
            }
            foreach (AuthClient client in AuthXender.SocketSessions.Values)
            {
                Account player = client.Player;
                if (player != null && player.PlayerId == accountId)
                {
                    return player;
                }
            }
            return null;
        }
    }
}
