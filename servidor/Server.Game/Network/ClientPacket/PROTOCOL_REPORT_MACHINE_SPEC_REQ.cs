using Plugin.Core;
using Plugin.Core.Logging;

namespace Server.Game.Network.ClientPacket
{
    // Client -> Server, opcode 5291 (0x14AB). Periodic machine-spec / version telemetry
    // heartbeat (client emits it via internal event 0x8729, gated by a ~20s accumulator).
    // Fire-and-forget: no ACK is expected. 149-byte body = 35-byte hardware block + 114-byte
    // session block. We parse the useful fields, log them, and return. Real in-battle state
    // is relayed P2P/UDP through the Match server, NOT here.
    public class PROTOCOL_REPORT_MACHINE_SPEC_REQ : GameClientPacket
    {
        private const int BodyLength = 149;

        private int _cpuVendorCaps;
        private byte _cpuCores;
        private ushort _cpuMhz;
        private ushort _ramMb;
        private byte _osVersion;
        private byte _osBuild;
        private ushort _screenW;
        private ushort _screenH;
        private int _clientVersion;
        private ushort _regionFlag;
        private bool _parsed;

        public override void Read()
        {
            if (_raw == null || _raw.Length < 4 + BodyLength)
            {
                return;
            }

            // Block A (35B) - hardware / OS fingerprint
            _cpuVendorCaps = ReadD();   // Intel 0x10000 / AMD 0x20000 << 8 | caps
            _cpuCores = ReadC();        // SYSTEM_INFO.dwNumberOfProcessors
            _cpuMhz = ReadUH();         // registry ~MHz
            _ramMb = ReadUH();          // GlobalMemoryStatusEx ullTotalPhys >> 20
            ReadD(); ReadD(); ReadD(); ReadD(); // 4x engine ids
            ReadUH();                   // engine value
            _osVersion = ReadC();       // major*10 + minor
            ReadC();                    // flag
            _osBuild = ReadC();         // OS build / service pack
            _screenW = ReadUH();        // GetDeviceCaps HORZRES
            _screenH = ReadUH();        // GetDeviceCaps VERTRES
            ReadC();                    // misc

            // Block B (114B) - version + session
            ReadB(14);                  // session preamble
            _clientVersion = ReadD();   // hardcoded build stamp (e.g. 121000)
            _regionFlag = ReadUH();     // 9 or 11
            ReadB(94);                  // remaining session identifiers

            _parsed = true;
        }

        public override void Run()
        {
            if (!_parsed)
            {
                return;
            }

            CLogger.Event(
                LogCat.System,
                new
                {
                    ver = _clientVersion,
                    region = _regionFlag,
                    cores = _cpuCores,
                    mhz = _cpuMhz,
                    ramMb = _ramMb,
                    res = _screenW + "x" + _screenH,
                    os = _osVersion,
                    build = _osBuild,
                    vendor = "0x" + _cpuVendorCaps.ToString("X")
                },
                "machine_spec",
                LogLevel.Debug);
        }
    }
}
