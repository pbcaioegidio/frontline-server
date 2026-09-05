using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.PointBlank.Network
{
    public static class LAUNCHER_PACKET_READER_REQ
    {
        private const int MaxPacketSize = 10 * 1024 * 1024;

        public static LAUNCHER_PACKET_REQ Read(NetworkStream stream)
        {
            byte[] lengthBuffer = ReadExact(stream, 4);
            int packetLength = BitConverter.ToInt32(lengthBuffer, 0);

            if (packetLength < 2)
                throw new InvalidDataException("Pacote inválido.");

            if (packetLength > MaxPacketSize)
                throw new InvalidDataException("Pacote excedeu o tamanho máximo permitido.");

            byte[] opcodeBuffer = ReadExact(stream, 2);
            ushort opcode = BitConverter.ToUInt16(opcodeBuffer, 0);

            int payloadLength = packetLength - 2;
            byte[] payload = ReadExact(stream, payloadLength);

            return new LAUNCHER_PACKET_REQ(opcode, payload);
        }
        private static byte[] ReadExact(NetworkStream stream, int size)
        {
            byte[] buffer = new byte[size];
            int offset = 0;

            while (offset < size)
            {
                int bytesRead = stream.Read(buffer, offset, size - offset);

                if (bytesRead <= 0)
                    throw new IOException("Conexão encerrada pelo servidor.");

                offset += bytesRead;
            }

            return buffer;
        }
    }
}
