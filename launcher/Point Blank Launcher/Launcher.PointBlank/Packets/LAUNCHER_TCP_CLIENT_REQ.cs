using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.PointBlank.Network
{
    public class LAUNCHER_TCP_CLIENT_REQ : IDisposable
    {
        private TcpClient client;
        private NetworkStream stream;
        public bool IsConnected
        {
            get
            {
                return client != null && client.Connected;
            }
        }
        public async Task ConnectAsync(string host, int port)
        {
            client = new TcpClient();

            await client.ConnectAsync(host, port);

            stream = client.GetStream();
        }
        public async Task SendAsync(LAUNCHER_OPCODE_REQ opcode)
        {
            byte[] packet = LAUNCHER_PACKET_WRITER_REQ.Build(opcode);

            await stream.WriteAsync(packet, 0, packet.Length);
        }
        public async Task SendAsync(LAUNCHER_OPCODE_REQ opcode, string message)
        {
            byte[] payload = new byte[0];

            if (!string.IsNullOrEmpty(message))
                payload = Encoding.UTF8.GetBytes(message);

            byte[] packet = LAUNCHER_PACKET_WRITER_REQ.Build(opcode, payload);

            await stream.WriteAsync(packet, 0, packet.Length);
        }
        public async Task SendAsync(LAUNCHER_OPCODE_REQ opcode, byte[] payload)
        {
            byte[] packet = LAUNCHER_PACKET_WRITER_REQ.Build(opcode, payload);

            await stream.WriteAsync(packet, 0, packet.Length);
        }
        public Task<LAUNCHER_PACKET_REQ> ReceiveAsync()
        {
            return Task.Run(() => LAUNCHER_PACKET_READER_REQ.Read(stream));
        }
        public string GetPayloadAsString(LAUNCHER_PACKET_REQ packet)
        {
            if (packet == null || packet.Payload == null || packet.Payload.Length == 0)
                return string.Empty;

            return Encoding.UTF8.GetString(packet.Payload);
        }
        public T GetPayloadAsJson<T>(LAUNCHER_PACKET_REQ packet)
        {
            string json = GetPayloadAsString(packet);

            if (string.IsNullOrWhiteSpace(json))
                return default(T);
            return JsonConvert.DeserializeObject<T>(json);
        }
        public void Disconnect()
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }
        public void Dispose()
        {
            Disconnect();
        }
    }
}
