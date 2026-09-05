using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.PointBlank.Network
{
    public class LAUNCHER_PACKET_REQ
    {
        public ushort Opcode { get; private set; }
        public byte[] Payload { get; private set; }
        public LAUNCHER_PACKET_REQ(ushort opcode, byte[] payload)
        {
            Opcode = opcode;
            Payload = payload ?? new byte[0];
        }
    }
}
