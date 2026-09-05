using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Auth.Data.Sync.Client;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.Auth.Data.Sync
{
    public class AuthSync
    {
        protected Socket ClientSocket;
        private bool isClosed;

        public AuthSync(IPEndPoint endpoint)
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ClientSocket.Bind(endpoint);
            ClientSocket.IOControl(-1744830452, new byte[1] { Convert.ToByte(false) }, null);
        }

        public bool Start()
        {
            try
            {
                IPEndPoint localEndPoint = ClientSocket.LocalEndPoint as IPEndPoint;
                CLogger.Print($"Auth Sync Address {localEndPoint.Address}:{localEndPoint.Port}", LoggerType.Info);
                ThreadPool.QueueUserWorkItem(state => StartReceive());
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        private void StartReceive()
        {
            if (isClosed)
                return;

            try
            {
                StateObject state = new StateObject()
                {
                    WorkSocket = ClientSocket,
                    RemoteEP = new IPEndPoint(IPAddress.Any, 8000)
                };
                ClientSocket.BeginReceiveFrom(state.UdpBuffer, 0, 8096, SocketFlags.None, ref state.RemoteEP, new AsyncCallback(ReceiveCallback), state);
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print("AuthSync socket disposed during StartReceive.", LoggerType.Warning);
                Close();
            }
            catch (Exception ex)
            {
                CLogger.Print("Error in StartReceive: " + ex.Message, LoggerType.Error, ex);
                Close();
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            if (isClosed || AuthXender.Client == null || AuthXender.Client.ServerIsClosed)
                return;

            StateObject asyncState = result.AsyncState as StateObject;
            try
            {
                int bytesReceived = ClientSocket.EndReceiveFrom(result, ref asyncState.RemoteEP);
                if (bytesReceived <= 0)
                    return;

                byte[] receivedData = new byte[bytesReceived];
                Array.Copy(asyncState.UdpBuffer, 0, receivedData, 0, bytesReceived);
                ThreadPool.QueueUserWorkItem(state => ProcessPacket(receivedData));
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print("AuthSync socket disposed during ReceiveCallback.", LoggerType.Warning);
                Close();
            }
            catch (Exception ex)
            {
                CLogger.Print("Error in ReceiveCallback: " + ex.Message, LoggerType.Error, ex);
            }
            finally
            {
                StartReceive();
            }
        }

        private void ProcessPacket(byte[] data)
        {
            try
            {
                SyncClientPacket packet = new SyncClientPacket(data);
                short opcode = packet.ReadH();
                switch (opcode)
                {
                    case 11:
                        FriendInfo.Load(packet);
                        break;
                    case 13:
                        AccountInfo.Load(packet);
                        break;
                    case 15:
                        ServerCache.Load(packet);
                        break;
                    case 16:
                        ClanSync.Load(packet);
                        break;
                    case 17:
                        FriendSync.Load(packet);
                        break;
                    case 19:
                        PlayerSync.Load(packet);
                        break;
                    case 20:
                        ServerWarning.LoadGMWarning(packet);
                        break;
                    case 22:
                        ServerWarning.LoadShopRestart(packet);
                        break;
                    case 23:
                        ServerWarning.LoadServerUpdate(packet);
                        break;
                    case 24:
                        ServerWarning.LoadShutdown(packet);
                        break;
                    case 31:
                        EventInfo.LoadEventInfo(packet);
                        break;
                    case 32:
                        ReloadConfig.Load(packet);
                        break;
                    case 33:
                        ChannelCache.Load(packet);
                        break;
                    case 34:
                        ReloadPermn.Load(packet);
                        break;
                    case 7171:
                        ServerMessage.Load(packet);
                        break;
                    default:
                        CLogger.Print(Bitwise.ToHexData($"Auth - Opcode Not Found: [{opcode}]", packet.ToArray()), LoggerType.Opcode);
                        break;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public void SendPacket(byte[] data, IPEndPoint address)
        {
            if (isClosed)
                return;

            try
            {
                ClientSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, address, new AsyncCallback(SendCallback), ClientSocket);
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print($"AuthSync socket disposed during SendPacket to {address}.", LoggerType.Warning);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error sending UDP packet to {address}: {ex.Message}", LoggerType.Error, ex);
            }
        }

        private void SendCallback(IAsyncResult result)
        {
            try
            {
                if (!(result.AsyncState is Socket socket) || !socket.Connected)
                    return;
                socket.EndSend(result);
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print("AuthSync socket disposed during SendCallback.", LoggerType.Warning);
            }
            catch (Exception ex)
            {
                CLogger.Print("Error in SendCallback: " + ex.Message, LoggerType.Error, ex);
            }
        }

        public void Close()
        {
            if (isClosed)
                return;

            isClosed = true;
            try
            {
                if (ClientSocket == null)
                    return;

                ClientSocket.Close();
                ClientSocket.Dispose();
                ClientSocket = null;
            }
            catch (Exception ex)
            {
                CLogger.Print("Error closing AuthSync: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}