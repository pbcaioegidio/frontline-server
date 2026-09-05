using Launcher.PointBlank.Models;
using Launcher.PointBlank.Network;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.PointBlank.Services
{
    public class LauncherConnection : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private LAUNCHER_TCP_CLIENT_REQ _client;

        public LauncherConnection(string host, int port)
        {
            _host = host;
            _port = port;
        }
        public async Task<LauncherConnectionResult> CheckConnectionAsync()
        {
            try
            {
                _client = new LAUNCHER_TCP_CLIENT_REQ();
                await _client.ConnectAsync(_host, _port);
                await _client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_CONNECT_REQ);

                LAUNCHER_PACKET_REQ response = await _client.ReceiveAsync();

                if (response.Opcode == (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_CONNECT_ACK)
                {
                    LauncherConnectionResult connectResponse =
                        _client.GetPayloadAsJson<LauncherConnectionResult>(response);

                    if (connectResponse == null)
                        return Fail("Resposta inválida do servidor.");

                    return new LauncherConnectionResult
                    {
                        Success = connectResponse.Success,
                        Message = connectResponse.Message,
                        LauncherVersion = connectResponse.LauncherVersion,
                        ClientVersion = connectResponse.ClientVersion,
                        Maintenance = connectResponse.Maintenance,
                        MaintenanceMessage = connectResponse.MaintenanceMessage,
                        ManifestUrl = connectResponse.ManifestUrl
                    };
                }

                if (response.Opcode == (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_CONNECT_FAIL)
                {
                    Infos fail = _client.GetPayloadAsJson<Infos>(response);

                    if (fail == null)
                        return Fail("Servidor recusou a conexão.");

                    return new LauncherConnectionResult
                    {
                        Success = false,
                        Message = fail.Message,
                        LauncherVersion = fail.LauncherVersion,
                        ClientVersion = fail.ClientVersion,
                        Maintenance = fail.Maintenance,
                        MaintenanceMessage = fail.MaintenanceMessage
                    };
                }

                return Fail("Resposta desconhecida do servidor. Opcode: " + response.Opcode);
            }
            catch (Exception)
            {
                _client?.Dispose();
                _client = null;
                return Fail("Não foi possível conectar ao servidor");
            }
        }

        public async Task<UpdateManifest> GetManifestAsync()
        {
            await _client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_MANIFEST_REQ);

            LAUNCHER_PACKET_REQ response = await _client.ReceiveAsync();

            if (response.Opcode == (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_MANIFEST_FAIL)
                throw new Exception("Servidor não encontrou o manifest: " + _client.GetPayloadAsString(response));

            if (response.Opcode != (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_MANIFEST_ACK)
                throw new Exception("Opcode inesperado na resposta do manifest: " + response.Opcode);

            return _client.GetPayloadAsJson<UpdateManifest>(response)
                ?? new UpdateManifest();
        }

        public async Task DownloadFileAsync(string remotePath, string localPath, Action<long, long> onProgress)
        {
            string json = JsonConvert.SerializeObject(new { path = remotePath });
            await _client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_FILE_REQ, Encoding.UTF8.GetBytes(json));

            LAUNCHER_PACKET_REQ infoPacket = await _client.ReceiveAsync();

            if (infoPacket.Opcode == (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_FILE_FAIL)
                throw new Exception("Servidor não encontrou o arquivo: " + remotePath);

            if (infoPacket.Opcode != (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_FILE_INFO_ACK)
                throw new Exception("Opcode inesperado no início do download: " + infoPacket.Opcode);

            string infoJson = _client.GetPayloadAsString(infoPacket);
            long totalBytes = (long)Newtonsoft.Json.Linq.JObject.Parse(infoJson)["size"];

            Directory.CreateDirectory(Path.GetDirectoryName(localPath));

            long received = 0;

            using (FileStream fs = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                while (true)
                {
                    LAUNCHER_PACKET_REQ packet = await _client.ReceiveAsync();

                    if (packet.Opcode == (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_FILE_END_ACK)
                        break;

                    if (packet.Opcode != (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_FILE_DATA_ACK)
                        throw new Exception("Opcode inesperado durante download: " + packet.Opcode);

                    await fs.WriteAsync(packet.Payload, 0, packet.Payload.Length);
                    received += packet.Payload.Length;

                    onProgress?.Invoke(received, totalBytes);
                }
            }
        }

        public async Task<LoginResult> SendLoginAsync(string json)
        {
            try
            {
                await _client.SendAsync(LAUNCHER_OPCODE_REQ.LAUNCHER_LOGIN_REQ, Encoding.UTF8.GetBytes(json));

                LAUNCHER_PACKET_REQ response = await _client.ReceiveAsync();

                if (response.Opcode != (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_LOGIN_ACK &&
                    response.Opcode != (ushort)LAUNCHER_OPCODE_REQ.LAUNCHER_LOGIN_FAIL)
                    return new LoginResult { Success = false, Message = "Resposta inesperada do servidor. Opcode: " + response.Opcode };

                LoginResult result = _client.GetPayloadAsJson<LoginResult>(response);

                if (result == null)
                    return new LoginResult { Success = false, Message = "Resposta inválida do servidor." };

                return result;
            }
            catch (Exception ex)
            {
                return new LoginResult { Success = false, Message = "Erro ao comunicar com o servidor.\n" + ex.Message };
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }

        private static LauncherConnectionResult Fail(string message)
        {
            return new LauncherConnectionResult { Success = false, Message = message };
        }
    }
}
