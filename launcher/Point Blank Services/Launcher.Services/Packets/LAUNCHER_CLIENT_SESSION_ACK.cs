using Launcher.Server.Config;
using Launcher.Services.Config;
using Launcher.Services.Models;
using Launcher.Services.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Launcher.Server.Network
{
    public class LAUNCHER_CLIENT_SESSION_ACK
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly ServerConfig _config;

        public LAUNCHER_CLIENT_SESSION_ACK(TcpClient client, ServerConfig config)
        {
            _client = client;
            _stream = client.GetStream();
            _config = config;
        }
        public void Start()
        {
            try
            {
                Console.WriteLine("CLIENT ADRESS: " + _client.Client.RemoteEndPoint);

                while (_client.Connected)
                {
                    LAUNCHER_PACKET_ACK packet = LAUNCHER_PACKET_READER_ACK.Read(_stream);

                    HandlePacket(packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Status: " + ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }
        private void HandlePacket(LAUNCHER_PACKET_ACK packet)
        {
            switch ((LAUNCHER_OPCODE_ACK)packet.Opcode)
            {
                case LAUNCHER_OPCODE_ACK.LAUNCHER_CONNECT_REQ:
                    HandleLauncherConnect();
                    break;

                case LAUNCHER_OPCODE_ACK.LAUNCHER_MANIFEST_REQ:
                    HandleManifestRequest();
                    break;

                case LAUNCHER_OPCODE_ACK.LAUNCHER_FILE_REQ:
                    HandleFileRequest(packet);
                    break;

                case LAUNCHER_OPCODE_ACK.LAUNCHER_LOGIN_REQ:
                    HandleLoginRequest(packet);
                    break;

                case LAUNCHER_OPCODE_ACK.LAUNCHER_HEARTBEAT_REQ:
                    HandleHeartbeat(packet);
                    break;

                case LAUNCHER_OPCODE_ACK.LAUNCHER_CAPTURE_POLL_REQ:
                    HandleCapturePoll(packet);
                    break;

                case LAUNCHER_OPCODE_ACK.LAUNCHER_CAPTURE_UPLOAD:
                    HandleCaptureUpload(packet);
                    break;

                default:
                    SendString(
                        LAUNCHER_OPCODE_ACK.LAUNCHER_ERROR_ACK,
                        "Opcode desconhecido: " + packet.Opcode
                    );
                    break;
            }
        }
        private void HandleLauncherConnect()
        {
            Console.WriteLine("Packet Recive: LAUNCHER_CONNECT_REQ");
            if (_config.Maintenance)
            {
                Infos response = new Infos
                {
                    Success = false,
                    Message = _config.MaintenanceMessage,
                    LauncherVersion = _config.LauncherVersion,
                    ClientVersion = _config.ClientVersion,
                    Maintenance = true,
                    MaintenanceMessage = _config.MaintenanceMessage
                };
                SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_CONNECT_FAIL, response);

                Console.WriteLine("LAUNCHER_CONNECT_FAIL : Manutenção");
                return;
            }
            Infos successResponse = new Infos
            {
                Success = true,
                Message = "Conexão estabelecida com sucesso.",
                LauncherVersion = _config.LauncherVersion,
                ClientVersion = _config.ClientVersion,
                Maintenance = false,
                MaintenanceMessage = _config.MaintenanceMessage,
                ManifestUrl = _config.ManifestUrl
            };
            SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_CONNECT_ACK, successResponse);
            Console.WriteLine("Packet Send: LAUNCHER_CONNECT_ACK");
        }
        private void SendJson(LAUNCHER_OPCODE_ACK opcode, object data)
        {
            string json = JsonConvert.SerializeObject(data);
            byte[] payload = Encoding.UTF8.GetBytes(json);

            byte[] packet = LAUNCHER_PACKET_WRITER_ACK.Build(opcode, payload);

            _stream.Write(packet, 0, packet.Length);
        }
        private void SendString(LAUNCHER_OPCODE_ACK opcode, string message)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message);

            byte[] packet = LAUNCHER_PACKET_WRITER_ACK.Build(opcode, payload);

            _stream.Write(packet, 0, packet.Length);
        }
        private void HandleManifestRequest()
        {
            Console.WriteLine("Packet Receive: LAUNCHER_MANIFEST_REQ");

            string manifestPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                _config.ManifestPath
            );

            if (!File.Exists(manifestPath))
            {
                SendString(LAUNCHER_OPCODE_ACK.LAUNCHER_MANIFEST_FAIL, "Manifest não encontrado.");
                Console.WriteLine("LAUNCHER_MANIFEST_FAIL: arquivo não encontrado em " + manifestPath);
                return;
            }

            byte[] payload = File.ReadAllBytes(manifestPath);
            byte[] packet = LAUNCHER_PACKET_WRITER_ACK.Build(LAUNCHER_OPCODE_ACK.LAUNCHER_MANIFEST_ACK, payload);
            _stream.Write(packet, 0, packet.Length);

            Console.WriteLine("Packet Send: LAUNCHER_MANIFEST_ACK");
        }
        private void HandleFileRequest(LAUNCHER_PACKET_ACK packet)
        {
            Console.WriteLine("Packet Receive: LAUNCHER_FILE_REQ");

            string json = Encoding.UTF8.GetString(packet.Payload);
            string relativePath = JObject.Parse(json)["path"].ToString();

            string fullPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                _config.ClientFilesPath,
                relativePath.TrimStart('/', '\\')
            );
            if (!File.Exists(fullPath))
            {
                SendString(LAUNCHER_OPCODE_ACK.LAUNCHER_FILE_FAIL, "Arquivo não encontrado: " + relativePath);
                Console.WriteLine("LAUNCHER_FILE_FAIL: " + relativePath);
                return;
            }
            FileInfo fileInfo = new FileInfo(fullPath);
            long fileSize = fileInfo.Length;

            byte[] infoPayload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { size = fileSize }));
            byte[] infoPacket = LAUNCHER_PACKET_WRITER_ACK.Build(LAUNCHER_OPCODE_ACK.LAUNCHER_FILE_INFO_ACK, infoPayload);
            _stream.Write(infoPacket, 0, infoPacket.Length);

            const int chunkSize = 1024 * 1024;
            byte[] buffer = new byte[chunkSize];

            using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int bytesRead;
                while ((bytesRead = fs.Read(buffer, 0, chunkSize)) > 0)
                {
                    byte[] chunkPayload = new byte[bytesRead];
                    Array.Copy(buffer, chunkPayload, bytesRead);
                    byte[] chunkPacket = LAUNCHER_PACKET_WRITER_ACK.Build(LAUNCHER_OPCODE_ACK.LAUNCHER_FILE_DATA_ACK, chunkPayload);
                    _stream.Write(chunkPacket, 0, chunkPacket.Length);
                }
            }

            byte[] endPacket = LAUNCHER_PACKET_WRITER_ACK.Build(LAUNCHER_OPCODE_ACK.LAUNCHER_FILE_END_ACK);
            _stream.Write(endPacket, 0, endPacket.Length);

            Console.WriteLine($"Packet Send: LAUNCHER_FILE_END_ACK [{relativePath}] ({fileSize} bytes)");
        }

        private void HandleLoginRequest(LAUNCHER_PACKET_ACK packet)
        {
            Console.WriteLine("Packet Receive: LAUNCHER_LOGIN_REQ");

            try
            {
                string json = Encoding.UTF8.GetString(packet.Payload);
                LoginRequest request = JsonConvert.DeserializeObject<LoginRequest>(json);

                LoginProcessor processor = new LoginProcessor(_config);
                LoginResult result = processor.Process(request, RemoteIp());

                if (result.Success)
                {
                    SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_LOGIN_ACK, result);
                    Console.WriteLine("Packet Send: LAUNCHER_LOGIN_ACK [" + request?.Username + "] token " + _config.TokenMinutes + "min");
                }
                else
                {
                    SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_LOGIN_FAIL, result);
                    Console.WriteLine("Packet Send: LAUNCHER_LOGIN_FAIL [" + request?.Username + "] " +
                        (string.IsNullOrEmpty(result.Code) ? "" : result.Code + " ") + result.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro no login: " + ex.Message);
                SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_LOGIN_FAIL, new LoginResult
                {
                    Success = false,
                    Message = "Erro interno no servidor."
                });
            }
        }

        private string RemoteIp()
        {
            try
            {
                var ep = _client.Client.RemoteEndPoint as System.Net.IPEndPoint;
                if (ep == null) return "";
                var addr = ep.Address;
                if (addr.IsIPv4MappedToIPv6) addr = addr.MapToIPv4();
                return addr.ToString();
            }
            catch
            {
                return "";
            }
        }

        private void HandleHeartbeat(LAUNCHER_PACKET_ACK packet)
        {
            try
            {
                string json = Encoding.UTF8.GetString(packet.Payload ?? new byte[0]);
                JObject req = string.IsNullOrWhiteSpace(json) ? new JObject() : JObject.Parse(json);
                long playerId = req.Value<long?>("player_id") ?? 0;
                string sessionId = req.Value<string>("session_id") ?? "";
                string fingerprint = req.Value<string>("fingerprint") ?? "";
                string status = req.Value<string>("status") ?? "ok";
                string statusReason = req.Value<string>("status_reason") ?? "";
                string modulesHash = req.Value<string>("modules_hash") ?? "";
                int blockedStreak = req.Value<int?>("capture_blocked_streak") ?? 0;

                if (playerId <= 0)
                {
                    SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_HEARTBEAT_ACK, new { ok = false, message = "player_id inválido" });
                    return;
                }

                var repo = new SecurityRepository(_config.DbHost, _config.DbPort, _config.DbName, _config.DbUser, _config.DbPassword);

                JArray suspicious = req["suspicious"] as JArray;
                if (suspicious != null && suspicious.Count > 0)
                {
                    status = "tamper";
                    statusReason = "processo suspeito: " + string.Join(",", suspicious);
                    repo.LogEvent(playerId, "", "module_injection", "PROCESS",
                        statusReason,
                        "{\"suspicious\":" + suspicious.ToString(Formatting.None) + ",\"modules_hash\":\"" + modulesHash + "\"}",
                        5, "FG-112");
                    // pede clip automático quando detecta cheat tool
                    try
                    {
                        // enfileira via SQL direto (Socket não tem SecurityDao do Plugin.Core)
                        using (var conn = new Npgsql.NpgsqlConnection(string.Format(
                            "Host={0};Port={1};Database={2};Username={3};Password={4};",
                            _config.DbHost, _config.DbPort, _config.DbName, _config.DbUser, _config.DbPassword)))
                        {
                            conn.Open();
                            using (var cmd = new Npgsql.NpgsqlCommand(@"
                                INSERT INTO capture_requests (player_id, kind, reason, requested_by)
                                SELECT @p, 'clip', @r, 'socket:process'
                                WHERE NOT EXISTS (
                                  SELECT 1 FROM capture_requests
                                   WHERE player_id = @p AND kind = 'clip' AND status = 'pending'
                                     AND created_at > now() - interval '30 seconds')", conn))
                            {
                                cmd.Parameters.AddWithValue("p", playerId);
                                cmd.Parameters.AddWithValue("r", TruncStr(statusReason, 255));
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch { }
                }

                if (blockedStreak >= 5)
                {
                    status = "suspect";
                    statusReason = "captura bloqueada (" + blockedStreak + ")";
                    repo.LogEvent(playerId, "", "capture_blocked", "CAPTURE_BLOCKED",
                        "Ring buffer sem frames / janela bloqueada",
                        "{\"streak\":" + blockedStreak + "}", 4, "FG-114");
                }

                if (string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase))
                {
                    repo.EndLiveSession(playerId, string.IsNullOrWhiteSpace(statusReason) ? "guard_stop" : statusReason);
                    SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_HEARTBEAT_ACK, new { ok = true, closed = true });
                    return;
                }

                repo.UpsertLiveSession(playerId, sessionId, fingerprint, RemoteIp(), status, statusReason, modulesHash);
                // Fallback: launchers antigos só pegam jobs no heartbeat.
                // Com poll dedicado (5004), SKIP LOCKED evita entregar o mesmo job duas vezes.
                var jobs = repo.PeekAndDeliverCaptures(playerId, 2);

                SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_HEARTBEAT_ACK, new
                {
                    ok = true,
                    captures = jobs.ConvertAll(j => new { request_id = j.Id, kind = j.Kind, reason = j.Reason })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FL GUARD] heartbeat: " + ex.Message);
                SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_HEARTBEAT_ACK, new { ok = false, message = "erro interno" });
            }
        }

        /// <summary>
        /// Poll leve: só entrega capture_requests pending. Não atualiza live_sessions
        /// (isso continua no heartbeat 15s) e não encolhe o ring buffer do Guard.
        /// </summary>
        private void HandleCapturePoll(LAUNCHER_PACKET_ACK packet)
        {
            try
            {
                string json = Encoding.UTF8.GetString(packet.Payload ?? new byte[0]);
                JObject req = string.IsNullOrWhiteSpace(json) ? new JObject() : JObject.Parse(json);
                long playerId = req.Value<long?>("player_id") ?? 0;

                if (playerId <= 0)
                {
                    SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_CAPTURE_POLL_ACK, new { ok = false, message = "player_id inválido" });
                    return;
                }

                var repo = new SecurityRepository(_config.DbHost, _config.DbPort, _config.DbName, _config.DbUser, _config.DbPassword);
                var jobs = repo.PeekAndDeliverCaptures(playerId, 2);
                SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_CAPTURE_POLL_ACK, new
                {
                    ok = true,
                    captures = jobs.ConvertAll(j => new { request_id = j.Id, kind = j.Kind, reason = j.Reason })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FL GUARD] capture poll: " + ex.Message);
                SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_CAPTURE_POLL_ACK, new { ok = false, message = "erro interno" });
            }
        }

        private static string TruncStr(string s, int max)
        {
            if (s == null) return "";
            return s.Length <= max ? s : s.Substring(0, max);
        }

        private void HandleCaptureUpload(LAUNCHER_PACKET_ACK packet)
        {
            try
            {
                string json = Encoding.UTF8.GetString(packet.Payload ?? new byte[0]);
                JObject req = JObject.Parse(json);
                long requestId = req.Value<long?>("request_id") ?? 0;
                long playerId = req.Value<long?>("player_id") ?? 0;
                string kind = (req.Value<string>("kind") ?? "screenshot").ToLowerInvariant();
                bool blocked = req.Value<bool?>("blocked") ?? false;
                string error = req.Value<string>("error") ?? "";
                string username = req.Value<string>("username") ?? "";
                string b64 = req.Value<string>("data_base64") ?? "";

                var repo = new SecurityRepository(_config.DbHost, _config.DbPort, _config.DbName, _config.DbUser, _config.DbPassword);
                string path = "";
                string mp4Path = null;

                if (!blocked && !string.IsNullOrEmpty(b64))
                {
                    byte[] data = Convert.FromBase64String(b64);
                    string root = string.IsNullOrWhiteSpace(_config.EvidenceRoot)
                        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Evidence")
                        : _config.EvidenceRoot;
                    string dir = Path.Combine(root, playerId.ToString());
                    Directory.CreateDirectory(dir);
                    string ext = kind == "clip" ? ".zip" : ".jpg";
                    string file = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "_" + requestId + "_" + kind + ext;
                    string abs = Path.Combine(dir, file);
                    File.WriteAllBytes(abs, data);
                    path = Path.Combine("Evidence", playerId.ToString(), file);
                    if (!string.IsNullOrWhiteSpace(_config.EvidenceRoot))
                        path = abs;
                    Console.WriteLine("[FL GUARD] evidência salva: " + abs + " (" + data.Length + " bytes)");

                    if (kind == "clip")
                    {
                        mp4Path = EvidenceFfmpeg.TryConvertClipZip(abs, AppDomain.CurrentDomain.BaseDirectory);
                        if (!string.IsNullOrEmpty(mp4Path))
                        {
                            Console.WriteLine("[FL GUARD] clip MP4: " + mp4Path);
                            path = mp4Path.Replace('/', Path.DirectorySeparatorChar);
                        }
                    }
                }

                repo.CompleteCapture(requestId, playerId, kind, path, blocked, error, username);
                SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_CAPTURE_ACK, new { ok = true, path, mp4 = mp4Path, blocked, request_id = requestId });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FL GUARD] capture upload: " + ex.Message);
                SendJson(LAUNCHER_OPCODE_ACK.LAUNCHER_CAPTURE_ACK, new { ok = false, message = ex.Message });
            }
        }

        private void Disconnect()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch
            {

            }
        }
    }
}