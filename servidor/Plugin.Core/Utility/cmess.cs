using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class CMessCipher
    {

        public const int MODE_AUTH = 1;
        public const int MODE_GAME = 2;

        private static byte BitsAt(byte[] reg, int bitPos)
        {
            int byteIdx = (bitPos >> 3) & 7;
            int bitOff = bitPos & 7;

            if (bitOff != 0)
            {
                byte lo = (byte)((reg[byteIdx] & (0xFF >> bitOff)) << bitOff);
                byte hi = (byte)(reg[(byteIdx + 1) & 7] >> (8 - bitOff));
                return (byte)(lo + hi);
            }

            return reg[byteIdx];
        }

        private static void UpdateState(byte[] k0, byte[] k1, byte cipherByte)
        {
            k0[0] ^= cipherByte;
            k0[2] ^= cipherByte;
            k0[4] ^= cipherByte;
            k0[6] ^= cipherByte;

            k1[1] ^= cipherByte;
            k1[3] ^= cipherByte;
            k1[5] ^= cipherByte;
            k1[7] ^= cipherByte;
        }

        private static void DeriveInitialState(
            int r0, int r1, byte r2, int mode,
            out int initPos0, out int initPos1, out int step)
        {
            int r2adj;
            switch (mode)
            {
                case MODE_AUTH:
                    initPos0 = (r2 >> 4) + r0;
                    r2adj = r2 & 0x0F;
                    step = 8;
                    break;
                case MODE_GAME:
                    initPos0 = (r2 & 0x0F) + r0;
                    r2adj = r2 >> 4;
                    step = 14;
                    break;
                default:
                    initPos0 = r2 + r0;
                    r2adj = r2;
                    step = 12;
                    break;
            }
            initPos1 = r2adj + r1;
        }

        private static byte NextKeystreamByte(
            byte[] k0, byte[] k1,
            int initPos0, int initPos1,
            ref int v18, ref byte runXOR,
            int step)
        {
            byte s0 = BitsAt(k0, v18 + initPos0);
            byte s1 = BitsAt(k1, v18 + initPos1);
            runXOR ^= (byte)(s0 ^ s1);
            v18 = runXOR + v18 + step;
            return runXOR;
        }

        public static byte[] Encrypt(byte[] key, byte[] src, int mode, Random rng = null)
        {
            if (key == null || key.Length < 16) throw new ArgumentException("key must be 16 bytes");
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (rng == null) rng = new Random();

            int dataLen = src.Length;


            byte[] k0 = new byte[8];
            byte[] k1 = new byte[8];
            Buffer.BlockCopy(key, 0, k0, 0, 8);
            Buffer.BlockCopy(key, 8, k1, 0, 8);

            int r0 = rng.Next() % 64;
            int r1 = rng.Next() % 64;
            byte r2 = (byte)(rng.Next() & 0xFF);
            int padLen = ((2 - dataLen) & 7) + 2;

            DeriveInitialState(r0, r1, r2, mode,
                out int initPos0, out int initPos1, out int step);

            int v18 = 0;
            byte runXOR = 0;
            int outLen = 4 + dataLen + padLen;
            byte[] dst = new byte[outLen];

            dst[0] = (byte)r0;
            dst[1] = (byte)r1;
            dst[2] = r2;
            dst[3] = (byte)padLen;

            int outIdx = 4;

            for (int i = 0; i < dataLen; i++)
            {
                byte ks = NextKeystreamByte(k0, k1, initPos0, initPos1,
                                             ref v18, ref runXOR, step);
                byte cipher = (byte)(ks ^ src[i]);
                dst[outIdx++] = cipher;
                UpdateState(k0, k1, cipher);
            }

            for (int i = 0; i < padLen; i++)
            {
                byte ks = NextKeystreamByte(k0, k1, initPos0, initPos1,
                                                  ref v18, ref runXOR, step);
                byte padByte = (byte)(padLen ^ ks);
                dst[outIdx++] = padByte;
                UpdateState(k0, k1, padByte);
            }

            return dst;
        }

        public static (byte[] plaintext, bool ok) Decrypt(byte[] key, byte[] ciphertext, int mode)
        {
            if (key == null || key.Length < 16) throw new ArgumentException("key must be 16 bytes");
            if (ciphertext == null || ciphertext.Length < 4) return (null, false);

            byte[] k0 = new byte[8];
            byte[] k1 = new byte[8];
            Buffer.BlockCopy(key, 0, k0, 0, 8);
            Buffer.BlockCopy(key, 8, k1, 0, 8);


            int r0 = ciphertext[0];
            int r1 = ciphertext[1];
            byte r2 = ciphertext[2];
            int padLen = ciphertext[3];


            if (padLen < 2 || padLen > 9) return (null, false);

            int encDataLen = ciphertext.Length - 4;
            if (encDataLen < padLen) return (null, false);
            int dataLen = encDataLen - padLen;

            DeriveInitialState(r0, r1, r2, mode,
                out int initPos0, out int initPos1, out int step);

            int v18 = 0;
            byte runXOR = 0;
            byte[] plain = new byte[dataLen];
            int inIdx = 4;


            for (int i = 0; i < dataLen; i++)
            {
                byte ks = NextKeystreamByte(k0, k1, initPos0, initPos1,
                                                 ref v18, ref runXOR, step);
                byte cipher = ciphertext[inIdx + i];
                plain[i] = (byte)(ks ^ cipher);
                UpdateState(k0, k1, cipher);
            }

            inIdx += dataLen;

            byte[] padDecrypted = new byte[padLen];
            for (int i = 0; i < padLen; i++)
            {
                byte ks = NextKeystreamByte(k0, k1, initPos0, initPos1,
                                                      ref v18, ref runXOR, step);
                byte cipher = ciphertext[inIdx + i];
                padDecrypted[i] = (byte)(ks ^ cipher);
                UpdateState(k0, k1, cipher);
            }

            bool ok = (padDecrypted[0] == (byte)padLen) &&
                      (padDecrypted[1] == (byte)padLen);

            return (plain, ok);
        }
    }


    public static class PacketFraming
    {
        public const ushort ENCRYPTED_FLAG = 0x8000;


        public static bool IsEncrypted(ushort headerWord) => (headerWord & ENCRYPTED_FLAG) != 0;

        public static int PayloadLength(ushort headerWord) => headerWord & 0x7FFF;

        public static ushort MakeEncryptedHeader(int cmessLen) => (ushort)(cmessLen | ENCRYPTED_FLAG);

        public static ushort MakePlainHeader(int payloadLen) => (ushort)(payloadLen & 0x7FFF);


        public static int WireLength(ushort headerWord)
        {
            int pl = PayloadLength(headerWord);
            return IsEncrypted(headerWord) ? 3 + pl : 5 + pl; // fix TCP reassembly: PB122 adds 1 encrypted or 3 plain trailer bytes
        }

        public static byte[] BuildEncryptedFrame(byte[] cmessOutput)
        {
            ushort hw = MakeEncryptedHeader(cmessOutput.Length);
            byte[] frame = new byte[3 + cmessOutput.Length];
            frame[0] = (byte)(hw & 0xFF);
            frame[1] = (byte)(hw >> 8);
            Buffer.BlockCopy(cmessOutput, 0, frame, 2, cmessOutput.Length);
            return frame;
        }

        public static byte[] BuildPlainFrame(ushort opcode, byte[] data)
        {
            int payloadLen = 2 + (data?.Length ?? 0);
            ushort hw = MakePlainHeader(payloadLen);
            byte[] frame = new byte[5 + payloadLen];
            frame[0] = (byte)(hw & 0xFF);
            frame[1] = (byte)(hw >> 8);
            frame[2] = (byte)(opcode & 0xFF);
            frame[3] = (byte)(opcode >> 8);
            if (data != null && data.Length > 0)
                Buffer.BlockCopy(data, 0, frame, 4, data.Length);
            return frame;
        }

        public static (bool isEncrypted, int payloadLen) ParseHeader(byte b0, byte b1)
        {
            ushort hw = (ushort)(b0 | (b1 << 8));
            return (IsEncrypted(hw), PayloadLength(hw));
        }
    }

    public class SessionCrypto
    {
        private readonly byte[] _key = new byte[16];
        private readonly int _mode;
        private readonly Random _rng;

        public byte[] Key => (byte[])_key.Clone();


        public SessionCrypto(int mode = CMessCipher.MODE_AUTH)
        {
            _mode = mode;
            _rng = new Random();
        }

        public void SetKey(byte[] key)
        {
            if (key == null || key.Length < 16)
                throw new ArgumentException("CMess session key must be 16 bytes");
            Buffer.BlockCopy(key, 0, _key, 0, 16);
        }


        public byte[] GenerateKey()
        {
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(_key);
            return (byte[])_key.Clone();
        }

        public byte[] EncryptPacket(byte[] payload)
        {
            byte[] cmess = CMessCipher.Encrypt(_key, payload, _mode, _rng);
            return PacketFraming.BuildEncryptedFrame(cmess);
        }


        public (byte[] plaintext, bool ok) DecryptPacket(byte[] cmessPayload)
        {
            return CMessCipher.Decrypt(_key, cmessPayload, _mode);
        }

        public (byte[] payload, bool ok) ProcessIncoming(byte[] frame)
        {
            if (frame == null || frame.Length < 2) return (null, false);

            var (isEnc, payloadLen) = PacketFraming.ParseHeader(frame[0], frame[1]);

            if (frame.Length < PacketFraming.WireLength((ushort)(frame[0] | (frame[1] << 8)))) return (null, false);

            if (!isEnc)
            {

                byte[] plain = new byte[payloadLen];
                Buffer.BlockCopy(frame, 2, plain, 0, payloadLen);
                return (plain, true);
            }

            byte[] cmess = new byte[payloadLen];
            Buffer.BlockCopy(frame, 2, cmess, 0, payloadLen);
            return DecryptPacket(cmess);
        }
    }
}
