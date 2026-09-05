using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Plugin.Core.Utility
{
    public static class CryptoSyncer
    {
        private static readonly byte[] AES_KEY = Bitwise.HexStringToByteArray("4A 35 64 62 39 68 45 4F 39 74 54 39 64 69 32 78");
        private static readonly byte[] AES_IV = Bitwise.HexStringToByteArray("4F 77 55 4D 37 35 56 77 30 76 6C 72 4D 4E 6C 6D");

        static CryptoSyncer()
        {
        }

        private static byte[] EncryptAESCounter(byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;
                aes.Key = AES_KEY;
                
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] counter = (byte[])AES_IV.Clone();
                    byte[] encrypted = new byte[data.Length];
                    
                    for (int i = 0; i < data.Length; i += 16)
                    {
                        byte[] keyStream = encryptor.TransformFinalBlock(counter, 0, 16);
                        int blockSize = Math.Min(16, data.Length - i);
                        
                        for (int j = 0; j < blockSize; j++)
                            encrypted[i + j] = (byte)(data[i + j] ^ keyStream[j]);
                        
                        // Increment counter (little-endian)
                        int carryIndex = 15;
                        while (carryIndex >= 0 && ++counter[carryIndex] == 0)
                            carryIndex--;
                    }
                    return encrypted;
                }
            }
        }

        public static byte[] StaticMethod0(byte[] cipher) => EncryptAESCounter(cipher);

        public static byte[] Decrypt(byte[] cipher) => EncryptAESCounter(cipher);

        public static void MD5File(string filePath)
        {
            try
            {
                using (MD5 md5 = MD5.Create())
                {
                    using (FileStream fileStream = File.OpenRead(filePath))
                        Console.WriteLine("MD5: " + BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLower());
                }
            }
            catch (Exception ex)
            {
                // Silently handle exception
            }
        }

        public static char[] HexCodes(byte[] data)
        {
            char[] hexChars = new char[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
                data[i].ToString("x2").CopyTo(0, hexChars, i * 2, 2);
            return hexChars;
        }

        public static string SHA1File(string message)
        {
            try
            {
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(message));
                    StringBuilder result = new StringBuilder();
                    for (int i = 0; i < hash.Length; i++)
                        result.Append(hash[i].ToString("x2"));
                    return result.ToString();
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static int SHA1_Int32(string message)
        {
            try
            {
                return int.Parse(SHA1File(message), NumberStyles.HexNumber);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static string GetHWID()
        {
            try
            {
                string systemInfo = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") + 
                                   Environment.GetEnvironmentVariable("COMPUTERNAME") + 
                                   Environment.UserName.Trim();
                
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(systemInfo));
                    StringBuilder hwid = new StringBuilder();
                    
                    for (int i = 0; i < hash.Length; i++)
                    {
                        hwid.Append(hash[i].ToString("x3").Substring(0, 3));
                        if (i != hash.Length - 1)
                            hwid.Append("-");
                    }
                    return hwid.ToString();
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}