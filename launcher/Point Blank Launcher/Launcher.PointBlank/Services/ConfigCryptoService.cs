using System;
using System.IO;
using System.Text;

namespace Launcher.PointBlank.Services
{
    public static class ConfigCryptoService
    {
        private const byte InitialFeedback = 0xA7;
        private const int RotateBits = 3;
        private static readonly byte[] CryptoKey = Encoding.ASCII.GetBytes("PointBlank.Config.Security");

        public static string DecryptFile(string path)
        {
            byte[] encryptedBytes = File.ReadAllBytes(path);
            byte[] plainBytes = Decrypt(encryptedBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }

        public static void EncryptFile(string path, string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText ?? string.Empty);
            File.WriteAllBytes(path, Encrypt(plainBytes));
        }

        private static byte[] Decrypt(byte[] encryptedBytes)
        {
            byte[] plainBytes = new byte[encryptedBytes.Length];
            byte feedback = InitialFeedback;

            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                byte encrypted = encryptedBytes[i];
                byte key = CryptoKey[i % CryptoKey.Length];
                byte positionMask = unchecked((byte)((i * 37) + 0x5A));
                byte mask = unchecked((byte)(key + positionMask + feedback));

                plainBytes[i] = unchecked((byte)(RotateRight(encrypted, RotateBits) ^ mask));
                feedback = encrypted;
            }

            return plainBytes;
        }

        private static byte[] Encrypt(byte[] plainBytes)
        {
            byte[] encryptedBytes = new byte[plainBytes.Length];
            byte feedback = InitialFeedback;

            for (int i = 0; i < plainBytes.Length; i++)
            {
                byte key = CryptoKey[i % CryptoKey.Length];
                byte positionMask = unchecked((byte)((i * 37) + 0x5A));
                byte mask = unchecked((byte)(key + positionMask + feedback));
                byte encrypted = RotateLeft(unchecked((byte)(plainBytes[i] ^ mask)), RotateBits);
                encryptedBytes[i] = encrypted;
                feedback = encrypted;
            }

            return encryptedBytes;
        }

        private static byte RotateRight(byte value, int bits)
        {
            return unchecked((byte)((value >> bits) | (value << (8 - bits))));
        }

        private static byte RotateLeft(byte value, int bits)
        {
            return unchecked((byte)((value << bits) | (value >> (8 - bits))));
        }
    }
}
