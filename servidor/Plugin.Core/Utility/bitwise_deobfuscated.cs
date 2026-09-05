using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using Plugin.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Plugin.Core.Utility
{
    public static class Bitwise
    {
        private static readonly string NEWLINE = string.Format("\n");
        private static readonly string STRLINE = string.Format("");
        private static readonly char[] BYTE_TO_CHAR = new char[256];
        private static readonly string[] BYTE_TO_HEX = new string[256];
        private static readonly string[] HEX_PADDING = new string[16];
        private static readonly string[] BYTE_PADDING = new string[16];
        public static readonly int[] CRYPTO = new int[3] { 29890, 32759, 1360 };

        static Bitwise()
        {
            // Initialize hex conversion tables for single digits (0-9)
            for (int i = 0; i < 10; i++)
            {
                StringBuilder builder = new StringBuilder(3);
                builder.Append(" 0");
                builder.Append(i);
                BYTE_TO_HEX[i] = builder.ToString().ToUpper();
            }

            // Initialize hex conversion tables for A-F
            for (int i = 10; i < 16; i++)
            {
                StringBuilder builder = new StringBuilder(3);
                builder.Append(" 0");
                builder.Append((char)(97 + i - 10)); // 'a' + offset
                BYTE_TO_HEX[i] = builder.ToString().ToUpper();
            }

            // Initialize hex conversion tables for values 16-255
            for (int i = 16; i < BYTE_TO_HEX.Length; i++)
            {
                StringBuilder builder = new StringBuilder(3);
                builder.Append(' ');
                builder.Append(i.ToString("X"));
                BYTE_TO_HEX[i] = builder.ToString().ToUpper();
            }

            // Initialize hex padding strings
            for (int i = 0; i < HEX_PADDING.Length; i++)
            {
                int paddingCount = HEX_PADDING.Length - i;
                StringBuilder builder = new StringBuilder(paddingCount * 3);
                for (int j = 0; j < paddingCount; j++)
                    builder.Append("   ");
                HEX_PADDING[i] = builder.ToString().ToUpper();
            }

            // Initialize byte padding strings
            for (int i = 0; i < BYTE_PADDING.Length; i++)
            {
                int capacity = BYTE_PADDING.Length - i;
                StringBuilder builder = new StringBuilder(capacity);
                for (int j = 0; j < capacity; j++)
                    builder.Append(' ');
                BYTE_PADDING[i] = builder.ToString().ToUpper();
            }

            // Initialize byte to character conversion table
            for (int i = 0; i < BYTE_TO_CHAR.Length; i++)
                BYTE_TO_CHAR[i] = i <= 31 || i >= 127 ? '.' : (char)i;
        }

        public static byte[] Decrypt(byte[] data, int shift)
        {
            byte[] decryptedData = new byte[data.Length];
            Array.Copy(data, 0, decryptedData, 0, decryptedData.Length);
            
            byte lastByte = decryptedData[decryptedData.Length - 1];
            for (int i = decryptedData.Length - 1; i > 0; i--)
                decryptedData[i] = (byte)(((decryptedData[i - 1] & 255) << (8 - shift)) | ((decryptedData[i] & 255) >> shift));
            
            decryptedData[0] = (byte)((lastByte << (8 - shift)) | ((decryptedData[0] & 255) >> shift));
            return decryptedData;
        }

        public static byte[] Encrypt(byte[] data, int shift)
        {
            byte[] encryptedData = new byte[data.Length];
            Array.Copy(data, 0, encryptedData, 0, encryptedData.Length);
            
            byte firstByte = encryptedData[0];
            for (int i = 0; i < encryptedData.Length - 1; i++)
                encryptedData[i] = (byte)(((encryptedData[i + 1] & 255) >> (8 - shift)) | ((encryptedData[i] & 255) << shift));
            
            encryptedData[encryptedData.Length - 1] = (byte)((firstByte >> (8 - shift)) | ((encryptedData[encryptedData.Length - 1] & 255) << shift));
            return encryptedData;
        }

        public static string ToHexData(string eventName, byte[] bufferData)
        {
            int length = bufferData.Length;
            int readerIndex = 0;
            int totalLength = bufferData.Length;
            
            StringBuilder hexDump = new StringBuilder((length / 16 + (length % 15 == 0 ? 0 : 1) + 4) * 80 + eventName.Length + 16);
            hexDump.Append(STRLINE + "+--------+-------------------------------------------------+----------------+");
            hexDump.Append($"{NEWLINE}[!] {eventName}; Length: [{bufferData.Length} Bytes] </>");
            hexDump.Append(NEWLINE + "         +-------------------------------------------------+");
            hexDump.Append(NEWLINE + "         |  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F |");
            hexDump.Append(NEWLINE + "+--------+-------------------------------------------------+----------------+");

            int index;
            for (index = readerIndex; index < totalLength; index++)
            {
                int relativeIndex = index - readerIndex;
                int columnIndex = relativeIndex & 15;
                
                if (columnIndex == 0)
                {
                    hexDump.Append(NEWLINE);
                    hexDump.Append(((long)relativeIndex & uint.MaxValue | 0x100000000L).ToString("X"));
                    hexDump[hexDump.Length - 9] = '|';
                    hexDump.Append('|');
                }
                
                hexDump.Append(BYTE_TO_HEX[bufferData[index]]);
                
                if (columnIndex == 15)
                {
                    hexDump.Append(" |");
                    for (int charIndex = index - 15; charIndex <= index; charIndex++)
                        hexDump.Append(BYTE_TO_CHAR[bufferData[charIndex]]);
                    hexDump.Append('|');
                }
            }

            if ((index - readerIndex & 15) != 0)
            {
                int remainder = length & 15;
                hexDump.Append(HEX_PADDING[remainder]);
                hexDump.Append(" |");
                for (int charIndex = index - remainder; charIndex < index; charIndex++)
                    hexDump.Append(BYTE_TO_CHAR[bufferData[charIndex]]);
                hexDump.Append(BYTE_PADDING[remainder]);
                hexDump.Append('|');
            }
            
            hexDump.Append(NEWLINE + "+--------+-------------------------------------------------+----------------+");
            return hexDump.ToString();
        }

        public static string HexArrayToString(byte[] buffer, int length)
        {
            string result = "";
            try
            {
                result = Encoding.Unicode.GetString(buffer, 0, length);
                int nullIndex = result.IndexOf(char.MinValue);
                if (nullIndex != -1)
                    result = result.Substring(0, nullIndex);
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return result;
        }

        public static byte[] HexStringToByteArray(string hexString)
        {
            string cleanHex = hexString.Replace(":", "").Replace("-", "").Replace(" ", "");
            byte[] byteArray = new byte[cleanHex.Length / 2];
            for (int i = 0; i < cleanHex.Length; i += 2)
                byteArray[i / 2] = (byte)(HexCharToByte(cleanHex.ElementAt(i)) << 4 | HexCharToByte(cleanHex.ElementAt(i + 1)));
            return byteArray;
        }

        private static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (int value in bytes)
                builder.Append(value.ToString("x2"));
            return builder.ToString();
        }

        private static byte[] ComputeHash(string input)
        {
            using (SHA256 hash = SHA256.Create())
                return hash.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        private static int HexCharToByte(char hexChar)
        {
            if (hexChar >= '0' && hexChar <= '9')
                return hexChar - '0';
            if (hexChar >= 'A' && hexChar <= 'F')
                return hexChar - 'A' + 10;
            if (hexChar >= 'a' && hexChar <= 'f')
                return hexChar - 'a' + 10;
            return 0;
        }

        public static string ToByteString(byte[] result)
        {
            string byteData = "";
            string hexString = BitConverter.ToString(result);
            char[] separators = new char[5] { '-', ',', '.', ':', '\t' };
            foreach (string byteValue in hexString.Split(separators))
                byteData = $"{byteData} {byteValue}";
            return byteData;
        }

        public static string GenerateRandomPassword(string allowedChars, int length, string salt)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                char[] passwordChars = new char[length];
                for (int i = 0; i < length; i++)
                    passwordChars[i] = allowedChars[randomBytes[i] % allowedChars.Length];
                return HashString(new string(passwordChars), salt, length);
            }
        }

        public static string HashString(string text, string salt, int length = 32)
        {
            using (HMACMD5 hmac = new HMACMD5(Encoding.UTF8.GetBytes(salt)))
                return ByteArrayToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(text))).Substring(0, length);
        }

        public static List<byte[]> GenerateRSAKeyPair(int sessionId, int securityKey, int seedLength)
        {
            List<byte[]> rsaKeys = new List<byte[]>();
            RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
            generator.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), seedLength));
            RsaKeyParameters publicKey = (RsaKeyParameters)generator.GenerateKeyPair().Public;
            rsaKeys.Add(publicKey.Modulus.ToByteArrayUnsigned());
            rsaKeys.Add(publicKey.Exponent.ToByteArrayUnsigned());
            
            byte[] seedBytes = BitConverter.GetBytes(sessionId + securityKey);
            Array.Copy(seedBytes, 0, rsaKeys[0], 0, Math.Min(seedBytes.Length, rsaKeys[0].Length));
            return rsaKeys;
        }

        public static ushort GenerateRandomUShort()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[2];
                rng.GetBytes(bytes);
                return BitConverter.ToUInt16(bytes, 0);
            }
        }

        public static uint GenerateRandomUInt()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        public static string ReadFile(string path)
        {
            string result = "";
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fileStream = new FileInfo(path).Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                {
                    result = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", string.Empty);
                    fileStream.Close();
                }
            }
            return result;
        }

        public static byte[] ArrayRandomize(byte[] source)
        {
            if (source == null || source.Length < 2)
                return source;

            byte[] randomizedArray = new byte[source.Length];
            Array.Copy(source, randomizedArray, source.Length);
            Random random = new Random();
            
            for (int i = randomizedArray.Length - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1);
                byte temp = randomizedArray[i];
                randomizedArray[i] = randomizedArray[randomIndex];
                randomizedArray[randomIndex] = temp;
            }
            return randomizedArray;
        }

        public static int BytesToInt(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            return byte4 << 24 | byte3 << 16 | byte2 << 8 | byte1;
        }

        public static bool ProcessPacket(byte[] packetData, int bytesToSkip, int bytesToKeepAtEnd, ushort[] opcodes)
        {
            ushort currentOpcode = BitConverter.ToUInt16(packetData, 2);
            if (opcodes.FirstOrDefault(opcode => opcode == currentOpcode) != 0)
                return false;

            int dataLength = packetData.Length - bytesToSkip - bytesToKeepAtEnd;
            if (dataLength < 0)
                dataLength = 0;

            int requiredLength = bytesToSkip + bytesToKeepAtEnd;
            if (packetData.Length >= requiredLength)
            {
                byte[] dataToEncrypt = packetData.Skip(bytesToSkip).Take(dataLength).ToArray();
                byte[] encryptedData = CryptoSyncer.StaticMethod0(dataToEncrypt);
                
                if (encryptedData.Length != dataLength)
                {
                    CLogger.Print("Encrypted data length mismatch! Encryption function changed data size.", LoggerType.Warning);
                    return false;
                }
                
                Array.Copy(encryptedData, 0, packetData, bytesToSkip, encryptedData.Length);
                return true;
            }
            
            CLogger.Print("PacketData is too short to apply encryption logic.", LoggerType.Warning);
            return false;
        }
    }
}