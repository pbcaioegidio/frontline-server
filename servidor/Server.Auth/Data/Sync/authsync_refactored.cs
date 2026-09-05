// Decompiled with JetBrains decompiler
// Type: Server.Auth.Data.Sync.AuthSync
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Auth.Data.Sync.Client;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

#nullable disable
namespace Server.Auth.Data.Sync
{
    public class AuthSync
    {
        protected Socket ClientSocket;
        private bool isClosed;

        public AuthSync(IPEndPoint endpoint)
        {
            this.ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.ClientSocket.Bind(endpoint);
            this.ClientSocket.IOControl(-1744830452 /*0x9800000C*/, new byte[1]
            {
                Convert.ToByte(false)
            }, null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool Start()
        {
            try
            {
                IPEndPoint localEndPoint = this.ClientSocket.LocalEndPoint as IPEndPoint;
                CLogger.Print($"Auth Sync Address {localEndPoint.Address}:{localEndPoint.Port}", LoggerType.Info);
                ThreadPool.QueueUserWorkItem(state => this.StartReceive());
                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void StartReceive()
        {
            if (this.isClosed)
                return;
            try
            {
                StateObject state = new StateObject()
                {
                    WorkSocket = this.ClientSocket,
                    RemoteEP = new IPEndPoint(IPAddress.Any, 8000)
                };
                this.ClientSocket.BeginReceiveFrom(state.UdpBuffer, 0, 8096, SocketFlags.None, ref state.RemoteEP, new AsyncCallback(this.ReceiveCallback), state);
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print("AuthSync socket disposed during StartReceive.", LoggerType.Warning);
                this.Close();
            }
            catch (Exception ex)
            {
                CLogger.Print("Error in StartReceive: " + ex.Message, LoggerType.Error, ex);
                this.Close();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ReceiveCallback(IAsyncResult result)
        {
            if (this.isClosed || AuthXender.Client == null || AuthXender.Client.ServerIsClosed)
                return;
            
            StateObject asyncState = result.AsyncState as StateObject;
            try
            {
                int bytesReceived = this.ClientSocket.EndReceiveFrom(result, ref asyncState.RemoteEP);
                if (bytesReceived <= 0)
                    return;
                
                byte[] receivedData = new byte[bytesReceived];
                Array.Copy(asyncState.UdpBuffer, 0, receivedData, 0, bytesReceived);
                ThreadPool.QueueUserWorkItem(state => this.ProcessReceivedPacket(receivedData));
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print("AuthSync socket disposed during ReceiveCallback.", LoggerType.Warning);
                this.Close();
            }
            catch (Exception ex)
            {
                CLogger.Print("Error in ReceiveCallback: " + ex.Message, LoggerType.Error, ex);
                this.Close();
            }
            finally
            {
                // Continue listening for more data
                this.StartReceive();
            }
        }

        private void ProcessReceivedPacket(byte[] packetData)
        {
            try
            {
                // Process the received packet data
                // Implementation would depend on the specific packet format
                if (packetData != null && packetData.Length > 0)
                {
                    // Handle packet processing here
                    CLogger.Print($"Received packet of {packetData.Length} bytes", LoggerType.Debug);
                    
                    // Example packet processing - would need actual implementation
                    // var packet = PacketFactory.CreateFromBytes(packetData);
                    // packet.Process();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error processing packet: {ex.Message}", LoggerType.Error, ex);
            }
        }

        public void Close()
        {
            if (this.isClosed)
                return;
                
            try
            {
                this.isClosed = true;
                this.ClientSocket?.Close();
                CLogger.Print("AuthSync connection closed", LoggerType.Info);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error closing AuthSync: {ex.Message}", LoggerType.Error, ex);
            }
        }

        public void Dispose()
        {
            Close();
            this.ClientSocket?.Dispose();
        }
    }
}