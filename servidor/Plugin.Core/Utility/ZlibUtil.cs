using System;
using System.IO;
using System.IO.Compression;

namespace Plugin.Core.Utility
{
    public static class ZlibUtil
    {
        public static byte[] Compress(byte[] data)
        {
            if (data == null)
                data = new byte[0];

            byte[] deflated;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DeflateStream ds = new DeflateStream(ms, CompressionLevel.Optimal, true))
                {
                    ds.Write(data, 0, data.Length);
                }
                deflated = ms.ToArray();
            }

            byte[] outBuf = new byte[2 + deflated.Length + 4];
            outBuf[0] = 0x78;
            outBuf[1] = 0x9C;
            Buffer.BlockCopy(deflated, 0, outBuf, 2, deflated.Length);

            uint adler = Adler32(data);
            int t = 2 + deflated.Length;
            outBuf[t + 0] = (byte)(adler >> 24);
            outBuf[t + 1] = (byte)(adler >> 16);
            outBuf[t + 2] = (byte)(adler >> 8);
            outBuf[t + 3] = (byte)adler;
            return outBuf;
        }

        private static uint Adler32(byte[] data)
        {
            const uint MOD = 65521u;
            uint a = 1u, b = 0u;
            for (int i = 0; i < data.Length; i++)
            {
                a = (a + data[i]) % MOD;
                b = (b + a) % MOD;
            }
            return (b << 16) | a;
        }
    }
}
