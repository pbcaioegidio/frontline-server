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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Plugin.Core.Utility
{

    public static class Bitwise
    {
        #region Private Fields - Hex Formatting

        private static readonly string NEW_LINE = string.Format("\n");
        private static readonly string EMPTY_STRING = string.Format("");
        private static readonly char[] ASCII_PRINTABLE_CHARS = new char[256];
        private static readonly string[] HEX_BYTE_STRINGS = new string[256];
        private static readonly string[] HEX_PADDING_SPACES = new string[16];
        private static readonly string[] ASCII_PADDING_SPACES = new string[16];
        #endregion

        #region Public Constants

        public static readonly int[] CRYPTO = new int[3] { 29890, 32759, 1360 };

        private static readonly System.Collections.Concurrent.ConcurrentBag<(byte[] Mod, byte[] Exp)> RsaPool =
            new System.Collections.Concurrent.ConcurrentBag<(byte[] Mod, byte[] Exp)>();
        private static readonly object RsaGenLock = new object();
        private static int _rsaPoolSeedLength;
        #endregion

        #region Static Constructor
        static Bitwise()
        {
            for (int i = 0; i < 10; i++)
            {
                StringBuilder sb = new StringBuilder(3);
                sb.Append(" 0");
                sb.Append(i);
                HEX_BYTE_STRINGS[i] = sb.ToString().ToUpper();
            }

            for (int i = 10; i < 16; i++)
            {
                StringBuilder sb = new StringBuilder(3);
                sb.Append(" 0");
                sb.Append((char)(97 + i - 10));
                HEX_BYTE_STRINGS[i] = sb.ToString().ToUpper();
            }

            for (int i = 16; i < HEX_BYTE_STRINGS.Length; i++)
            {
                StringBuilder sb = new StringBuilder(3);
                sb.Append(' ');
                sb.Append(i.ToString("X"));
                HEX_BYTE_STRINGS[i] = sb.ToString().ToUpper();
            }

            for (int i = 0; i < HEX_PADDING_SPACES.Length; i++)
            {
                int paddingCount = HEX_PADDING_SPACES.Length - i;
                StringBuilder sb = new StringBuilder(paddingCount * 3);
                for (int j = 0; j < paddingCount; j++)
                    sb.Append("   ");
                HEX_PADDING_SPACES[i] = sb.ToString().ToUpper();
            }

            for (int i = 0; i < ASCII_PADDING_SPACES.Length; i++)
            {
                int paddingCount = ASCII_PADDING_SPACES.Length - i;
                StringBuilder sb = new StringBuilder(paddingCount);
                for (int j = 0; j < paddingCount; j++)
                    sb.Append(' ');
                ASCII_PADDING_SPACES[i] = sb.ToString().ToUpper();
            }

            for (int i = 0; i < ASCII_PRINTABLE_CHARS.Length; i++)
            {
                ASCII_PRINTABLE_CHARS[i] = i <= 31 || i >= 127 ? '.' : (char)i;
            }
        }
        #endregion

        #region Encryption/Decryption Methods
        public static byte[] Decrypt(byte[] Data, int Shift)
        {
            byte[] result = new byte[Data.Length];
            Array.Copy(Data, 0, result, 0, result.Length);

            byte lastByte = result[result.Length - 1];
            for (int i = result.Length - 1; i > 0; i--)
            {
                result[i] = (byte)(
                    ((result[i - 1] & 0xFF) << (8 - Shift)) |
                    ((result[i] & 0xFF) >> Shift)
                );
            }
            result[0] = (byte)(
                (lastByte << (8 - Shift)) |
                ((result[0] & 0xFF) >> Shift)
            );

            return result;
        }

        public static byte[] Encrypt(byte[] Data, int Shift)
        {
            if (Data == null || Data.Length == 0)
            {
                return Data ?? new byte[0];
            }

            byte[] result = new byte[Data.Length];
            Array.Copy(Data, 0, result, 0, result.Length);
            byte firstByte = result[0];

            for (int i = 0; i < result.Length - 1; i++)
            {
                result[i] = (byte)(
                    ((result[i + 1] & 0xFF) >> (8 - Shift)) |
                    ((result[i] & 0xFF) << Shift)
                );
            }
            result[result.Length - 1] = (byte)(
                (firstByte >> (8 - Shift)) |
                ((result[result.Length - 1] & 0xFF) << Shift)
            );

            return result;
        }
        #endregion


        #region Hex Dump Methods
        public static string ToHexData(string EventName, byte[] BuffData)
        {
            int dataLength = BuffData.Length;
            int startOffset = 0;
            int endOffset = BuffData.Length;
            int estimatedLines = (dataLength / 16 + (dataLength % 15 == 0 ? 0 : 1) + 4);
            StringBuilder output = new StringBuilder(estimatedLines * 80 + EventName.Length + 16);
            output.Append(EMPTY_STRING + "+--------+-------------------------------------------------+----------------+");
            output.Append($"{NEW_LINE}[!] {EventName}; Length: [{BuffData.Length} Bytes] </>");
            output.Append(NEW_LINE + "         +-------------------------------------------------+");
            output.Append(NEW_LINE + "         |  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F |");
            output.Append(NEW_LINE + "+--------+-------------------------------------------------+----------------+");
            int byteIndex;
            for (byteIndex = startOffset; byteIndex < endOffset; byteIndex++)
            {
                int relativeOffset = byteIndex - startOffset;
                int columnInRow = relativeOffset & 15;
                if (columnInRow == 0)
                {
                    output.Append(NEW_LINE);
                    output.Append(((long)relativeOffset & 0xFFFFFFFF | 0x100000000L).ToString("X"));
                    output[output.Length - 9] = '|';
                    output.Append('|');
                }
                output.Append(HEX_BYTE_STRINGS[BuffData[byteIndex]]);
                if (columnInRow == 15)
                {
                    output.Append(" |");
                    for (int asciiIndex = byteIndex - 15; asciiIndex <= byteIndex; asciiIndex++)
                        output.Append(ASCII_PRINTABLE_CHARS[BuffData[asciiIndex]]);
                    output.Append('|');
                }
            }
            if ((byteIndex - startOffset & 15) != 0)
            {
                int remainingBytes = dataLength & 15;
                output.Append(HEX_PADDING_SPACES[remainingBytes]);
                output.Append(" |");

                for (int asciiIndex = byteIndex - remainingBytes; asciiIndex < byteIndex; asciiIndex++)
                    output.Append(ASCII_PRINTABLE_CHARS[BuffData[asciiIndex]]);

                output.Append(ASCII_PADDING_SPACES[remainingBytes]);
                output.Append('|');
            }


            output.Append(NEW_LINE + "+--------+-------------------------------------------------+----------------+");

            return output.ToString();
        }
        #endregion


        #region Conversion Methods
        public static string HexArrayToString(byte[] Buffer, int Length)
        {
            string result = "";
            try
            {
                result = Encoding.Unicode.GetString(Buffer, 0, Length);
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

        public static byte[] HexStringToByteArray(string HexString)
        {
            string cleanHex = HexString.Replace(":", "").Replace("-", "").Replace(" ", "");
            byte[] result = new byte[cleanHex.Length / 2];

            for (int i = 0; i < cleanHex.Length; i += 2)
            {
                result[i / 2] = (byte)(
                    HexCharToInt(cleanHex.ElementAt(i)) << 4 |
                    HexCharToInt(cleanHex.ElementAt(i + 1))
                );
            }

            return result;
        }

        private static string BytesToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static int HexCharToInt(char hexChar)
        {
            if (hexChar >= '0' && hexChar <= '9')
                return hexChar - '0';
            if (hexChar >= 'A' && hexChar <= 'F')
                return hexChar - 'A' + 10;
            if (hexChar >= 'a' && hexChar <= 'f')
                return hexChar - 'a' + 10;
            return 0;
        }

        public static string ToByteString(byte[] Result)
        {
            string output = "";
            string hexString = BitConverter.ToString(Result);
            char[] separators = new char[] { '-', ',', '.', ':', '\t' };

            foreach (string part in hexString.Split(separators))
                output = $"{output} {part}";

            return output;
        }

        #endregion


        #region Cryptographic Methods
        public static string GenerateRandomPassword(string AllowedChars, int Length, string Salt)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[Length];
                rng.GetBytes(randomBytes);

                char[] passwordChars = new char[Length];
                for (int i = 0; i < Length; i++)
                    passwordChars[i] = AllowedChars[randomBytes[i] % AllowedChars.Length];

                return HashString(new string(passwordChars), Salt, Length);
            }
        }


        public static string HashString(string Text, string Salt, int Length = 32)
        {
            using (HMACMD5 hmac = new HMACMD5(Encoding.UTF8.GetBytes(Salt)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(Text));
                return BytesToHexString(hash).Substring(0, Length);
            }
        }


        public static void WarmupRsaPool(int seedLength, int count = 32)
        {
            _rsaPoolSeedLength = seedLength;
            int made = 0;
            for (int i = 0; i < count; i++)
            {
                try
                {
                    RsaPool.Add(GenerateRawRsaKey(seedLength));
                    made++;
                }
                catch { break; }
            }
            Plugin.Core.CLogger.Print($"RSA pool aquecido: {made} chaves ({seedLength}-bit)", Plugin.Core.Enums.LoggerType.Info);
        }

        private static (byte[] Mod, byte[] Exp) GenerateRawRsaKey(int seedLength)
        {
            RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
            generator.Init(new KeyGenerationParameters(
                new SecureRandom(new CryptoApiRandomGenerator()),
                seedLength
            ));
            RsaKeyParameters publicKey = (RsaKeyParameters)generator.GenerateKeyPair().Public;
            return (publicKey.Modulus.ToByteArrayUnsigned(), publicKey.Exponent.ToByteArrayUnsigned());
        }

        public static List<byte[]> GenerateRSAKeyPair(int SessionId, int SecurityKey, int SeedLength)
        {
            List<byte[]> keyPair = new List<byte[]>();

            byte[] modulus;
            byte[] exponent;
            // Pool só se já aquecido; senão gera na hora (com lock pra não matar o ARM).
            if (RsaPool.TryTake(out var pooled))
            {
                modulus = (byte[])pooled.Mod.Clone();
                exponent = (byte[])pooled.Exp.Clone();
            }
            else
            {
                lock (RsaGenLock)
                {
                    var raw = GenerateRawRsaKey(SeedLength > 0 ? SeedLength : (_rsaPoolSeedLength > 0 ? _rsaPoolSeedLength : 1360));
                    modulus = raw.Mod;
                    exponent = raw.Exp;
                }
            }

            keyPair.Add(modulus);
            keyPair.Add(exponent);

            byte[] sessionBytes = BitConverter.GetBytes(SessionId + SecurityKey);
            Array.Copy(sessionBytes, 0, keyPair[0], 0, Math.Min(sessionBytes.Length, keyPair[0].Length));

            return keyPair;
        }

        public static string ReadFile(string Path)
        {
            string hashResult = "";

            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fileStream = new FileInfo(Path).Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                {
                    hashResult = BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", string.Empty);
                    fileStream.Close();
                }
            }

            return hashResult;
        }

        #endregion

        #region Helper Classes (Compiler Generated)

        private class OpcodeComparer
        {
            public ushort TargetOpcode;

            public OpcodeComparer(ushort targetOpcode)
            {
                TargetOpcode = targetOpcode;
            }

            public bool MatchesOpcode(ushort opcode)
            {
                return opcode == this.TargetOpcode;
            }
        }

        #endregion
    }
}