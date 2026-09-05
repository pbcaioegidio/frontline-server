using System.IO;

namespace Launcher.Server.Network
{
    public static class LAUNCHER_PACKET_WRITER_ACK
    {
        public static byte[] Build(LAUNCHER_OPCODE_ACK opcode, byte[] payload = null)
        {
            return Build((ushort)opcode, payload);
        }
        public static byte[] Build(ushort opcode, byte[] payload = null)
        {
            if (payload == null)
                payload = new byte[0];
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                int packetLength = 2 + payload.Length;
                writer.Write(packetLength);
                writer.Write(opcode);
                if (payload.Length > 0)
                    writer.Write(payload);

                return memoryStream.ToArray();
            }
        }
    }
}