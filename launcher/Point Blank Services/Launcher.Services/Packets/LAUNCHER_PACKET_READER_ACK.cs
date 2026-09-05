using System;
using System.IO;
using System.Net.Sockets;

namespace Launcher.Server.Network
{
    public static class LAUNCHER_PACKET_READER_ACK
    {
        private const int MaxPacketSize = 48 * 1024 * 1024;

        public static LAUNCHER_PACKET_ACK Read(NetworkStream stream)
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

            return new LAUNCHER_PACKET_ACK(opcode, payload);
        }
        private static byte[] ReadExact(NetworkStream stream, int size)
        {
            byte[] buffer = new byte[size];
            int offset = 0;

            while (offset < size)
            {
                int bytesRead = stream.Read(buffer, offset, size - offset);

                if (bytesRead <= 0)
                    throw new IOException("Conexão finalizada.");
                offset += bytesRead;
            }
            return buffer;
        }
    }
}