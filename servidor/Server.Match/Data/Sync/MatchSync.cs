using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Match.Data.Sync.Client;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Server.Match.Data.Sync
{
    /// <summary>
    /// Servidor UDP para sincronización de datos del servidor de partidas
    /// </summary>
    public class MatchSync
    {
        #region Private Fields

        protected Socket ClientSocket;
        private bool _isClosed;

        #endregion Private Fields

        #region Constructor

        public MatchSync(IPEndPoint bindEndPoint)
        {
            // Crear socket UDP
            this.ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.ClientSocket.Bind(bindEndPoint);

            // Configurar IOControl para prevenir excepciones ICMP unreachable
            // SIO_UDP_CONNRESET (0x9800000C)
            if (OperatingSystem.IsWindows())
                this.ClientSocket.IOControl(-1744830452, new byte[1] { Convert.ToByte(false) }, null); // fix linux: SIO_UDP_CONNRESET only exists on Winsock
        }

        #endregion Constructor

        #region Start/Stop

        /// <summary>
        /// Inicia el servidor de sincronización UDP
        /// </summary>

        public bool Start()
        {
            try
            {
                IPEndPoint localEndPoint = this.ClientSocket.LocalEndPoint as IPEndPoint;
                CLogger.Print($"Endereco Sync Match {localEndPoint.Address}:{localEndPoint.Port}", LoggerType.Info);

                ThreadPool.QueueUserWorkItem(_ => BeginReceiveData());

                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
                return false;
            }
        }

        /// <summary>
        /// Cierra el servidor de sincronización
        /// </summary>

        public void Close()
        {
            if (_isClosed)
                return;

            _isClosed = true;

            try
            {
                if (this.ClientSocket == null)
                    return;

                this.ClientSocket.Close();
                this.ClientSocket.Dispose();
                this.ClientSocket = null;
            }
            catch (Exception ex)
            {
                CLogger.Print("Error closing MatchSync: " + ex.Message, LoggerType.Error, ex);
            }
        }

        #endregion Start/Stop

        #region Receive Data

        /// <summary>
        /// Inicia la recepción de datos UDP
        /// </summary>

        private void BeginReceiveData()
        {
            if (_isClosed)
                return;

            try
            {
                StateObject state = new StateObject()
                {
                    WorkSocket = this.ClientSocket,
                    RemoteEP = new IPEndPoint(IPAddress.Any, 8000)
                };

                this.ClientSocket.BeginReceiveFrom(
                    state.UdpBuffer,
                    0,
                    8096,
                    SocketFlags.None,
                    ref state.RemoteEP,
                    new AsyncCallback(OnReceiveData),
                    state
                );
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print("MatchSync socket disposed during StartReceive.", LoggerType.Warning, ex);
                this.Close();
            }
            catch (Exception ex)
            {
                CLogger.Print("Error in StartReceive: " + ex.Message, LoggerType.Error, ex);
                this.Close();
            }
        }

        /// <summary>
        /// Callback cuando se reciben datos UDP
        /// </summary>

        private void OnReceiveData(IAsyncResult asyncResult)
        {
            if (_isClosed || MatchXender.Client == null || MatchXender.Client.ServerIsClosed)
                return;

            StateObject state = asyncResult.AsyncState as StateObject;

            try
            {
                int bytesReceived = this.ClientSocket.EndReceiveFrom(asyncResult, ref state.RemoteEP);

                if (bytesReceived <= 0)
                    return;

                byte[] receivedData = new byte[bytesReceived];
                Array.Copy(state.UdpBuffer, 0, receivedData, 0, bytesReceived);

                // Procesar el paquete en un thread pool
                ThreadPool.QueueUserWorkItem(_ => ProcessReceivedPacket(receivedData));
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print("MatchSync socket disposed during ReceiveCallback.", LoggerType.Warning, ex);
                this.Close();
            }
            catch (Exception ex)
            {
                CLogger.Print("Error in ReceiveCallback: " + ex.Message, LoggerType.Error, ex);
            }
            finally
            {
                // Continuar recibiendo datos
                BeginReceiveData();
            }
        }

        #endregion Receive Data

        #region Packet Processing

        /// <summary>
        /// Procesa un paquete recibido según su opcode
        /// </summary>

        private void ProcessReceivedPacket(byte[] packetData)
        {
            // VALIDACIÓN 1: Verificar que el paquete no sea nulo o vacío
            if (packetData == null || packetData.Length < 2)
            {
                CLogger.Print($"ProcessReceivedPacket: Invalid packet data (Length: {packetData?.Length ?? 0})",
                             LoggerType.Warning);
                return;
            }

            try
            {
                SyncClientPacket packet = new SyncClientPacket(packetData);
                short opcode = packet.ReadH();

                switch (opcode)
                {
                    case 1:
                        // Sincronización de respawn
                        try
                        {
                            RespawnSync.Load(packet);
                        }
                        catch (IndexOutOfRangeException ex)
                        {
                            CLogger.Print($"RespawnSync IndexOutOfRange: Packet length={packetData.Length}\n{ex.Message}",
                                         LoggerType.Error, ex);
                        }
                        catch (Exception ex)
                        {
                            CLogger.Print($"RespawnSync error: {ex.Message}", LoggerType.Error, ex);
                        }
                        break;

                    case 2:
                        // Remover jugador
                        try
                        {
                            RemovePlayerSync.Load(packet);
                        }
                        catch (Exception ex)
                        {
                            CLogger.Print($"RemovePlayerSync error: {ex.Message}", LoggerType.Error, ex);
                        }
                        break;

                    case 3:
                        // Sincronización de ronda de partida
                        try
                        {
                            MatchRoundSync.Load(packet);
                        }
                        catch (Exception ex)
                        {
                            CLogger.Print($"MatchRoundSync error: {ex.Message}", LoggerType.Error, ex);
                        }
                        break;

                    case 25:
                        try
                        {
                            SupervisorReload.Load(packet);
                        }
                        catch (Exception ex)
                        {
                            CLogger.Print($"SupervisorReload error: {ex.Message}", LoggerType.Error, ex);
                        }
                        break;

                    default:
                        CLogger.Print(
                            Bitwise.ToHexData($"Match - Opcode Not Found: [{opcode}]", packet.ToArray()),
                            LoggerType.Opcode
                        );
                        break;
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                CLogger.Print($"ProcessReceivedPacket IndexOutOfRange: Packet length={packetData.Length}\n{ex.Message}",
                             LoggerType.Error, ex);
                CLogger.Print($"Packet Hex: {BitConverter.ToString(packetData).Replace("-", " ")}",
                             LoggerType.Debug);
            }
            catch (Exception ex)
            {
                CLogger.Print($"ProcessReceivedPacket error: {ex.Message}", LoggerType.Error, ex);
            }
        }

        #endregion Packet Processing

        #region Send Data

        /// <summary>
        /// Envía un paquete UDP a una dirección específica
        /// </summary>

        public void SendPacket(byte[] data, IPEndPoint address)
        {
            if (_isClosed)
                return;

            try
            {
                this.ClientSocket.BeginSendTo(
                    data,
                    0,
                    data.Length,
                    SocketFlags.None,
                    address,
                    new AsyncCallback(OnSendComplete),
                    this.ClientSocket
                );
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print($"MatchSync socket disposed during SendPacket to {address}.", LoggerType.Warning, ex);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error sending UDP packet to {address}: {ex.Message}", LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Callback cuando se completa el envío de datos
        /// </summary>

        private void OnSendComplete(IAsyncResult asyncResult)
        {
            try
            {
                if (!(asyncResult.AsyncState is Socket socket) || !socket.Connected)
                    return;

                socket.EndSend(asyncResult);
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print("MatchSync socket disposed during SendCallback.", LoggerType.Warning, ex);
            }
            catch (Exception ex)
            {
                CLogger.Print("Error in SendCallback: " + ex.Message, LoggerType.Error, ex);
            }
        }

        #endregion Send Data
    }
}
