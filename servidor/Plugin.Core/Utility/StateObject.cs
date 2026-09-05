using System.Net;
using System.Net.Sockets;

namespace Plugin.Core.Utility
{
    // AGREGAR ESTA CLASE AL FINAL DEL ARCHIVO MatchManager.cs O EN UN ARCHIVO SEPARADO

    public class StateObject
    {
        public Socket WorkSocket = null;
        public IPEndPoint LocalPoint = null;
        public EndPoint RemoteEP;

        // CORREGIDO: Buffer con inicialización automática
        public const int BufferSize = 8192; // Tamaño del buffer para UDP

        public const int UdpBufferSize = 8192; // Alias para compatibilidad

        private byte[] _buffer;

        public byte[] Buffer
        {
            get
            {
                if (_buffer == null)
                    _buffer = new byte[BufferSize];
                return _buffer;
            }
            set { _buffer = value; }
        }

        private byte[] _udpBuffer;

        public byte[] UdpBuffer
        {
            get
            {
                if (_udpBuffer == null)
                    _udpBuffer = new byte[UdpBufferSize];
                return _udpBuffer;
            }
            set { _udpBuffer = value; }
        }
    }
}