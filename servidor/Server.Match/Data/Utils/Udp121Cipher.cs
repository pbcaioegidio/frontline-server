using System;
using System.Security.Cryptography;

namespace Server.Match.Data.Utils
{
    // Port of the 121 client UDP match-data payload cipher (client sub_F09D67 @0xf09d67,
    // keystream tap sub_924985 @0x924985). The 121 client encrypts only the game-data
    // payload (opcodes 3/4/131/132) with this keyed feedback-XOR, keyed by two 4-byte
    // header fields (time @off2, unk2 @off13). Header + 9-byte room trailer stay plaintext.
    // Symmetric/invertible: the key state mutates by the ciphertext byte, available to both sides.
    public static class Udp121Cipher
    {
        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();

        // Reads 8 bits at bit-offset a3 from the (circular) key buffer.
        private static byte Tap(byte[] buf, int length, uint a3)
        {
            int v4 = (int)((a3 >> 3) % (uint)length);
            int v5 = (int)(a3 & 7);
            int v6 = (v4 + 1) % length;
            if (v5 != 0)
                return (byte)((buf[v6] >> (8 - v5)) + ((buf[v4] & (0xFF >> v5)) << v5));
            return buf[v4];
        }

        // Quick signature check on a decrypted (un-rotated) blob:
        // [iv1 < 32][iv2 < 32][mix][padlen in 2..9][ciphertext...][padding...].
        public static bool LooksLikeBlob(byte[] data, int offset, int length)
        {
            if (data == null || length < 4 || offset + 4 > data.Length)
                return false;
            return data[offset] < 32 && data[offset + 1] < 32
                && data[offset + 3] >= 2 && data[offset + 3] <= 9;
        }

        // Decrypts the blob in-place region [offset, offset+length) and returns the plaintext
        // (ctlen = length - 4 - padlen bytes). key1 = header time field, key2 = header unk2 field.
        public static byte[] Decrypt(byte[] blob, int offset, int length, byte[] key1, byte[] key2)
        {
            if (blob == null || length < 4 || offset + length > blob.Length)
                return null;

            byte iv1 = blob[offset];
            byte iv2 = blob[offset + 1];
            byte mix = blob[offset + 2];
            byte padlen = blob[offset + 3];
            int ctlen = length - 4 - padlen;
            if (ctlen < 0)
                return null;

            byte[] k1 = (byte[])key1.Clone();
            byte[] k2 = (byte[])key2.Clone();
            int a6 = k1.Length;
            int a8 = k2.Length;

            byte idx1 = (byte)((mix & 0xF) + iv1);
            byte idx2 = (byte)((mix >> 4) + iv2);
            byte fb = 0;
            uint jit = 0;

            byte[] plain = new byte[ctlen];
            for (int i = 0; i < ctlen; i++)
            {
                byte ks1 = Tap(k1, a6, (uint)idx1 + jit);
                byte ks2 = Tap(k2, a8, (uint)idx2 + jit);
                fb = (byte)(fb ^ ks1 ^ ks2);
                byte c = blob[offset + 4 + i];
                plain[i] = (byte)(fb ^ c);
                jit += (uint)(fb + 8);
                for (int j = 0; j < a6; j += 2) k1[j] ^= c;
                for (int j = 1; j < a8; j += 2) k2[j] ^= c;
            }
            return plain;
        }
        public static byte[] EncryptServerToClient(byte[] plain, byte[] key1, out uint key2)
        {
            byte[] random = new byte[7];
            lock (Random)
                Random.GetBytes(random);

            key2 = BitConverter.ToUInt32(random, 0);
            byte[] k1 = (byte[])key1.Clone();
            byte[] k2 = new byte[4];
            Array.Copy(random, k2, 4);

            byte iv1 = (byte)(random[4] & 31);
            byte iv2 = (byte)(random[5] & 31);
            byte mix = random[6];
            byte padLength = (byte)(((2 - plain.Length) & 7) + 2);
            byte[] blob = new byte[4 + plain.Length + padLength];
            blob[0] = iv1;
            blob[1] = iv2;
            blob[2] = mix;
            blob[3] = padLength;

            byte index1 = (byte)(iv1 + (mix >> 4));
            byte index2 = (byte)(iv2 + (mix & 15));
            byte feedback = 0;
            uint jitter = 0;

            for (int i = 0; i < plain.Length + padLength; i++)
            {
                feedback = (byte)(feedback ^ Tap(k1, 4, (uint)index1 + jitter) ^ Tap(k2, 4, (uint)index2 + jitter));
                byte value = i < plain.Length ? plain[i] : padLength;
                byte cipher = (byte)(feedback ^ value);
                blob[4 + i] = cipher;
                jitter += (uint)(feedback + 14);

                for (int j = 0; j < 4; j += 2)
                    k1[j] ^= cipher;
                for (int j = 1; j < 4; j += 2)
                    k2[j] ^= cipher;
            }

            return blob;
        }

    }
}
