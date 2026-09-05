using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Server.Network
{
    public class LAUNCHER_PACKET_ACK
    {
        public ushort Opcode { get; private set; }
        public byte[] Payload { get; private set; }
        public LAUNCHER_PACKET_ACK(ushort opcode, byte[] payload)
        {
            Opcode = opcode;
            Payload = payload ?? new byte[0];
        }
    }
}