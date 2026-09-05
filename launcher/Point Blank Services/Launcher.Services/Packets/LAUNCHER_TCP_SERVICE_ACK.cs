using Launcher.Server.Config;
using Launcher.Services.Config;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Launcher.Server.Network
{
    public class LAUNCHER_TCP_SERVICE_ACK
    {
        private readonly ServerConfig _config;

        private TcpListener _listener;
        private bool _isRunning;

        public LAUNCHER_TCP_SERVICE_ACK(ServerConfig config)
        {
            _config = config;
        }

        public bool Start()
        {
            IPAddress ipAddress = IPAddress.Parse(_config.Host);

            _listener = new TcpListener(ipAddress, _config.Port);

            try
            {
                _listener.Start();
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                Console.WriteLine("[!] Porta ja em uso: " + _config.Host + ":" + _config.Port);
                Console.WriteLine("[!] Feche a outra instancia do Socket.exe ou altere a porta no config.ini.");
                return false;
            }

            _isRunning = true;

            Console.WriteLine("SERVER ADRESS: " + _config.Host + ":" + _config.Port);
            Console.WriteLine("LauncherVersion: " + _config.LauncherVersion);
            Console.WriteLine("ClientVersion: " + _config.ClientVersion);
            Console.WriteLine("Maintenance: " + _config.Maintenance);

            Task.Run(() => AcceptLoop());
            return true;
        }

        private async Task AcceptLoop()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();

                    LAUNCHER_CLIENT_SESSION_ACK session =
                        new LAUNCHER_CLIENT_SESSION_ACK(client, _config);

                    Task.Run(() => session.Start());
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    if (!_isRunning)
                        break;

                    Console.WriteLine("[!] SocketException: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[!] Erro no AcceptLoop: " + ex.Message);
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;

            try
            {
                _listener?.Stop();
            }
            catch
            {
                //Exceção de tratamento
            }

            Console.WriteLine("[#] Socket encerrado.");
        }
    }
}
