using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Launcher.PointBlank.Services
{
    /// <summary>
    /// Sobe o Socket local em segundo plano (sem CMD) quando o IP é localhost.
    /// Em servidor remoto o Socket já deve estar rodando no host — não sobe nada aqui.
    /// </summary>
    public static class SocketBootstrap
    {
        private const int DefaultPort = 9000;
        private const int WaitMs = 8000;
        private static Process _ownedProcess;
        private static readonly object _lock = new object();

        public static async Task EnsureRunningAsync(string startupPath, string host, int port)
        {
            if (port <= 0)
                port = DefaultPort;

            if (IsPortOpen(host, port))
                return;

            // Só sobe Socket local se o launcher aponta para esta máquina
            if (!IsLocalHost(host))
                throw new InvalidOperationException(
                    "Não foi possível conectar ao serviço do launcher em " + host + ":" + port +
                    ". No servidor, deixe o Socket.exe rodando.");

            string socketExe = FindSocketExe(startupPath);
            if (string.IsNullOrEmpty(socketExe))
                throw new FileNotFoundException(
                    "Socket.exe não encontrado. Coloque em FLService\\Socket.exe ao lado do FLLauncher.");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = socketExe,
                WorkingDirectory = Path.GetDirectoryName(socketExe) ?? startupPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            lock (_lock)
            {
                if (_ownedProcess != null && !_ownedProcess.HasExited)
                    return;

                _ownedProcess = Process.Start(psi);
            }

            int elapsed = 0;
            while (elapsed < WaitMs)
            {
                await Task.Delay(250);
                elapsed += 250;
                if (IsPortOpen(host, port))
                    return;
            }

            throw new TimeoutException("Socket iniciou em segundo plano, mas a porta " + port + " não respondeu.");
        }

        /// <summary>Encerra só o Socket que este launcher iniciou (não mata serviço de servidor).</summary>
        public static void StopOwned()
        {
            lock (_lock)
            {
                try
                {
                    if (_ownedProcess != null && !_ownedProcess.HasExited)
                    {
                        _ownedProcess.Kill(entireProcessTree: true);
                        _ownedProcess.WaitForExit(3000);
                    }
                }
                catch
                {
                    // ignore
                }
                finally
                {
                    _ownedProcess?.Dispose();
                    _ownedProcess = null;
                }
            }
        }

        private static bool IsLocalHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return true;
            string h = host.Trim().ToLowerInvariant();
            return h == "127.0.0.1" || h == "localhost" || h == "::1" || h == "0.0.0.0";
        }

        private static string FindSocketExe(string startupPath)
        {
            string[] candidates =
            {
                Path.Combine(startupPath, "Socket.exe"),
                Path.Combine(startupPath, "FLService", "Socket.exe"),
                Path.Combine(startupPath, "PBService", "Socket.exe"),
                Path.Combine(startupPath, "Socket", "Socket.exe"),
                Path.GetFullPath(Path.Combine(startupPath, "..", "launcher", "runtime-service", "Socket.exe"))
            };

            foreach (string path in candidates)
            {
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        private static bool IsPortOpen(string host, int port)
        {
            try
            {
                string connectHost = IsLocalHost(host) ? "127.0.0.1" : host;
                using (TcpClient client = new TcpClient())
                {
                    IAsyncResult ar = client.BeginConnect(connectHost, port, null, null);
                    bool ok = ar.AsyncWaitHandle.WaitOne(400);
                    if (!ok)
                        return false;
                    client.EndConnect(ar);
                    return client.Connected;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
