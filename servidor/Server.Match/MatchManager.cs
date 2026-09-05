//v3.81 - FIXED VERSION
// Correcciones principales:
// 1. Corregido el bug de socket.Connected en UDP (siempre es false)
// 2. Corregido EndSend -> EndSendTo para UDP
// 3. Agregada lógica de recuperación de socket de v3.68
// 4. Thread dedicado de alta prioridad para mayor estabilidad
// 5. Manejo mejorado de errores

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Logging;
using Plugin.Core.Utility;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Server.Match
{
    /// <summary>
    /// Administrador principal del servidor de partidas UDP
    /// Maneja la recepción y envío de paquetes UDP para las partidas
    /// </summary>
    public class MatchManager
    {
        #region Private Fields

        private readonly string _ipAddress;
        private readonly int _port;
        private StateObject _state;
        private Thread _receiveThread;

        // Constantes para recuperación de socket
        private const int MAX_RETRY_ATTEMPTS = 5;

        private const int INITIAL_RETRY_DELAY_MS = 100;
        private const int MAX_RETRY_DELAY_MS = 5000;

        #endregion Private Fields

        #region Public Fields

        public Socket MainSocket;
        public bool ServerIsClosed;

        #endregion Public Fields

        #region Constructor

        public MatchManager(string ipAddress, int port)
        {
            this._ipAddress = ipAddress;
            this._port = port;
        }

        #endregion Constructor

        #region Server Start

        /// <summary>
        /// Inicia el servidor UDP de partidas
        /// </summary>
        public bool Start()
        {
            try
            {
                // Crear socket UDP
                MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // Configurar IOControl para prevenir excepciones ICMP unreachable
                // SIO_UDP_CONNRESET (0x9800000C)
                uint IOC_IN = 0x80000000;
                uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    MainSocket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null); // fix linux: SIO_UDP_CONNRESET only exists on Winsock

                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);

                // Configurar opciones del socket
                MainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    MainSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted); // fix linux: IP protection levels are a Windows socket option
                MainSocket.DontFragment = false;
                MainSocket.Ttl = 128;

                // Buffers más grandes para mejor rendimiento
                MainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 65536);
                MainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 65536);

                // Enlazar socket
                MainSocket.Bind(localEndPoint);

                // Crear objeto de estado
                _state = new StateObject()
                {
                    WorkSocket = MainSocket,
                    RemoteEP = new IPEndPoint(IPAddress.Any, 0)
                };

                CLogger.Print($"Endereco Match {localEndPoint.Address}:{localEndPoint.Port}", LoggerType.Info);

                // Usar thread dedicado de alta prioridad (más estable que ThreadPool para este caso)
                _receiveThread = new Thread(Read)
                {
                    Priority = ThreadPriority.Highest,
                    IsBackground = true,
                    Name = "MatchManager-Receive"
                };
                _receiveThread.Start();

                return true;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Failed to start Match Manager: {ex.Message}", LoggerType.Error, ex);
                CleanupSocket();
                return false;
            }
        }

        #endregion Server Start

        #region Receive Data

        /// <summary>
        /// Método principal del thread de recepción
        /// </summary>
        private void Read()
        {
            CLogger.SetThreadKind(ServerKind.Match);
            try
            {
                if (MainSocket == null || ServerIsClosed)
                    return;

                BeginReceiveData();
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error in Read method: {ex.Message}", LoggerType.Error, ex);

                // Intentar recuperar
                if (!ServerIsClosed)
                {
                    Thread.Sleep(1000);
                    BeginReceiveData();
                }
            }
        }

        /// <summary>
        /// Inicia la operación asíncrona para recibir datos UDP
        /// </summary>
        private void BeginReceiveData()
        {
            try
            {
                if (MainSocket == null || ServerIsClosed || _state == null)
                    return;

                EndPoint remoteEP = _state.RemoteEP;
                MainSocket.BeginReceiveFrom(
                    _state.UdpBuffer,
                    0,
                    8096,
                    SocketFlags.None,
                    ref remoteEP,
                    new AsyncCallback(OnReceiveData),
                    _state
                );
            }
            catch (SocketException ex)
            {
                CLogger.Print($"Socket error in BeginReceiveData: {ex.Message}", LoggerType.Error, ex);
                RecoverSocket();
            }
            catch (ObjectDisposedException)
            {
                // Socket fue cerrado, intentar recuperar
                if (!ServerIsClosed)
                    RecoverSocket();
            }
            catch (Exception ex)
            {
                CLogger.Print($"Unexpected error in BeginReceiveData: {ex.Message}", LoggerType.Error, ex);
                if (!ServerIsClosed)
                    RecoverSocket();
            }
        }

        /// <summary>
        /// Callback cuando se reciben datos UDP
        /// </summary>
        private void OnReceiveData(IAsyncResult asyncResult)
        {
            CLogger.SetThreadKind(ServerKind.Match);
            if (ServerIsClosed || MainSocket == null)
                return;

            if (!asyncResult.IsCompleted)
                CLogger.Print("IAsyncResult is not completed.", LoggerType.Warning);

            StateObject state = asyncResult.AsyncState as StateObject;
            Socket workSocket = state?.WorkSocket;
            IPEndPoint originalRemoteEndPoint = state?.RemoteEP as IPEndPoint;

            bool needRestart = false;

            try
            {
                if (workSocket == null || !workSocket.IsBound)
                {
                    CLogger.Print("Socket is null or not bound", LoggerType.Warning);
                    needRestart = true;
                    return;
                }

                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int bytesReceived = workSocket.EndReceiveFrom(asyncResult, ref remoteEndPoint);

                if (bytesReceived <= 0)
                    return;

                byte[] receivedData = new byte[bytesReceived];
                Buffer.BlockCopy(state.UdpBuffer, 0, receivedData, 0, bytesReceived);

                // Validar tamaño del buffer (mínimo 22, máximo 8096 bytes)
                if (receivedData.Length >= 22 && receivedData.Length <= 8096)
                {
                    IPEndPoint clientEP = remoteEndPoint as IPEndPoint;
                    if (clientEP != null)
                    {
                        MatchClient client = new MatchClient(workSocket, clientEP);
                        BeginReceive(client, receivedData);
                    }
                }
                else
                {
                    // Registrar buffer inválido (throttled a 1 por minuto)
                    if ((DateTimeUtil.Now() - CLogger.GetLastMatchBuffer()).Minutes < 1)
                        return;

                    CLogger.Print($"Invalid Buffer Length: {receivedData.Length}; IP: {originalRemoteEndPoint}", LoggerType.Hack);
                    CLogger.SetLastMatchBuffer(DateTimeUtil.Now());
                }
            }
            catch (SocketException ex)
            {
                // Registrar error de socket (throttled a 1 por minuto)
                if ((DateTimeUtil.Now() - CLogger.GetLastMatchSocket()).Minutes >= 1)
                {
                    CLogger.Print($"Socket Error on Receive: {ex.SocketErrorCode} - {ex.Message}", LoggerType.Warning);
                    CLogger.SetLastMatchSocket(DateTimeUtil.Now());
                }

                if (ex.SocketErrorCode == SocketError.ConnectionReset ||
                    ex.SocketErrorCode == SocketError.ConnectionAborted ||
                    ex.SocketErrorCode == SocketError.NetworkReset ||
                    ex.SocketErrorCode == SocketError.TimedOut)
                {
                    RemoveUdpClient(originalRemoteEndPoint);
                    RemoveSpamConnection($"{originalRemoteEndPoint?.Address}");
                    needRestart = true;
                }
                else
                {
                    RecoverSocket();
                    return;
                }
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print("Socket was disposed during operation", LoggerType.Warning, ex);
                if (!ServerIsClosed)
                    RecoverSocket();
                return;
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error during OnReceiveData: {ex.Message}", LoggerType.Error, ex);
                RemoveUdpClient(originalRemoteEndPoint);
                RemoveSpamConnection($"{originalRemoteEndPoint?.Address}");
                needRestart = true;
            }
            finally
            {
                // Continuar recibiendo datos
                if (!ServerIsClosed)
                {
                    if (needRestart)
                        Thread.Sleep(50); // Pequeño delay antes de reintentar

                    BeginReceiveData();
                }
            }
        }

        /// <summary>
        /// Inicia el procesamiento de datos recibidos para un cliente
        /// </summary>
        public void BeginReceive(MatchClient client, byte[] buffer)
        {
            try
            {
                if (client == null)
                {
                    CLogger.Print("Client is null, destroyed after failed to add to list.", LoggerType.Warning);
                    return;
                }

                client.BeginReceive(buffer, DateTimeUtil.Now());
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error in BeginReceive for client: {ex.Message}", LoggerType.Error, ex);
            }
        }

        #endregion Receive Data

        #region Socket Recovery

        /// <summary>
        /// Recupera el socket después de un error
        /// </summary>
        private void RecoverSocket()
        {
            if (ServerIsClosed)
                return;

            CLogger.Print("Attempting to recover socket connection...", LoggerType.Warning);

            // Cerrar el socket viejo
            CleanupSocket();

            // Reintentar con backoff exponencial
            bool recovered = false;
            int retryDelayMs = INITIAL_RETRY_DELAY_MS;

            for (int attempt = 1; attempt <= MAX_RETRY_ATTEMPTS && !recovered && !ServerIsClosed; attempt++)
            {
                try
                {
                    CLogger.Print($"Recovery attempt {attempt} of {MAX_RETRY_ATTEMPTS}...", LoggerType.Warning);

                    // Crear nuevo socket
                    MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    // Configurar opciones
                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                    if (OperatingSystem.IsWindows())
                        MainSocket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null); // fix linux: recovered sockets must skip Winsock-only control codes
                    MainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    MainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 65536);
                    MainSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 65536);

                    // Bind al mismo endpoint
                    IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
                    MainSocket.Bind(localEndPoint);

                    // Crear nuevo objeto de estado
                    _state = new StateObject()
                    {
                        WorkSocket = MainSocket,
                        RemoteEP = new IPEndPoint(IPAddress.Any, 0)
                    };

                    // Comenzar a recibir de nuevo
                    BeginReceiveData();

                    recovered = true;
                    CLogger.Print("Socket successfully recovered", LoggerType.Info);
                }
                catch (Exception ex)
                {
                    CLogger.Print($"Recovery attempt {attempt} failed: {ex.Message}", LoggerType.Error);
                    CleanupSocket();

                    // Backoff exponencial con límite
                    if (attempt < MAX_RETRY_ATTEMPTS)
                    {
                        Thread.Sleep(Math.Min(retryDelayMs, MAX_RETRY_DELAY_MS));
                        retryDelayMs *= 2;
                    }
                }
            }

            if (!recovered && !ServerIsClosed)
            {
                CLogger.Print("Failed to recover socket after multiple attempts - manual intervention may be needed", LoggerType.Error);
            }
        }

        /// <summary>
        /// Limpia y cierra el socket
        /// </summary>
        private void CleanupSocket()
        {
            try
            {
                if (MainSocket != null)
                {
                    MainSocket.Close();
                    MainSocket.Dispose();
                    MainSocket = null;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error cleaning up socket: {ex.Message}", LoggerType.Error);
            }
        }

        #endregion Socket Recovery

        #region Client Management

        /// <summary>
        /// Remueve un cliente UDP del diccionario de clientes
        /// </summary>
        private bool RemoveUdpClient(IPEndPoint endPoint)
        {
            try
            {
                if (endPoint == null)
                    return false;

                Socket removedSocket;
                return MatchXender.UdpClients.ContainsKey(endPoint) &&
                       MatchXender.UdpClients.TryGetValue(endPoint, out removedSocket) &&
                       MatchXender.UdpClients.TryRemove(endPoint, out removedSocket);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error removing UDP client: {ex.Message}", LoggerType.Error, ex);
            }
            return false;
        }

        /// <summary>
        /// Remueve una conexión de spam del diccionario de control
        /// </summary>
        private bool RemoveSpamConnection(string ipAddress)
        {
            try
            {
                if (string.IsNullOrEmpty(ipAddress) || ipAddress.Equals("0.0.0.0"))
                    return false;

                int removedCount;
                return MatchXender.SpamConnections.ContainsKey(ipAddress) &&
                       MatchXender.SpamConnections.TryGetValue(ipAddress, out removedCount) &&
                       MatchXender.SpamConnections.TryRemove(ipAddress, out removedCount);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error removing spam connection: {ex.Message}", LoggerType.Error, ex);
            }
            return false;
        }

        #endregion Client Management

        #region Send Data

        /// <summary>
        /// Envía un paquete UDP a una dirección específica
        /// </summary>
        public void SendPacket(byte[] data, IPEndPoint address)
        {
            try
            {
                if (MainSocket == null || ServerIsClosed || data == null || address == null)
                    return;

                MainSocket.BeginSendTo(
                    data,
                    0,
                    data.Length,
                    SocketFlags.None,
                    address,
                    new AsyncCallback(OnSendComplete),
                    MainSocket
                );
            }
            catch (SocketException ex)
            {
                CLogger.Print($"Socket error sending packet to {address}: {ex.Message}", LoggerType.Error, ex);
            }
            catch (ObjectDisposedException ex)
            {
                CLogger.Print($"Socket was disposed while sending to {address}", LoggerType.Warning, ex);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Failed to send package to {address}: {ex.Message}", LoggerType.Error, ex);
            }
        }

        /// <summary>
        /// Callback cuando se completa el envío de datos
        /// CORREGIDO: UDP no usa socket.Connected y debe usar EndSendTo
        /// </summary>
        private void OnSendComplete(IAsyncResult asyncResult)
        {
            try
            {
                if (!(asyncResult.AsyncState is Socket socket))
                    return;

                // CORREGIDO: UDP sockets no tienen Connected, siempre es false
                // Removida la verificación de socket.Connected que causaba el bug

                // CORREGIDO: UDP usa EndSendTo, no EndSend
                socket.EndSendTo(asyncResult);
            }
            catch (SocketException ex)
            {
                // Log throttled para evitar spam en logs
                if ((DateTimeUtil.Now() - CLogger.GetLastMatchSocket()).Minutes >= 1)
                {
                    CLogger.Print($"Socket Error on Send: {ex.SocketErrorCode}", LoggerType.Warning);
                    CLogger.SetLastMatchSocket(DateTimeUtil.Now());
                }
            }
            catch (ObjectDisposedException)
            {
                // Socket cerrado durante el envío - esto es esperado durante shutdown
                if (!ServerIsClosed)
                    CLogger.Print("Socket was closed while sending.", LoggerType.Warning);
            }
            catch (Exception ex)
            {
                CLogger.Print($"Error during OnSendComplete: {ex.Message}", LoggerType.Error, ex);
            }
        }

        #endregion Send Data

        #region Shutdown

        /// <summary>
        /// Cierra el servidor de manera ordenada
        /// </summary>
        public void Shutdown()
        {
            if (ServerIsClosed)
                return;

            ServerIsClosed = true;

            CLogger.Print("Shutting down Match Manager...", LoggerType.Info);

            // Limpiar socket
            CleanupSocket();

            // Esperar a que el thread termine
            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                if (!_receiveThread.Join(5000))
                {
                    CLogger.Print("Receive thread did not terminate gracefully", LoggerType.Warning);
                }
            }

            CLogger.Print("Match Manager shutdown complete", LoggerType.Info);
        }

        #endregion Shutdown
    }
}
