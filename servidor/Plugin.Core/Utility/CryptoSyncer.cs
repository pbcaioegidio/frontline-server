// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Utility.CryptoSyncer
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Plugin.Core.Utility
{
    public static class CryptoSyncer
    {

        //private static readonly byte[] EncryptionKey = new byte[] 
        //{ 
        //    0x4A, 0x35, 0x64, 0x62,
        //    0x39, 0x68, 0x45, 0x4F,
        //    0x39, 0x74, 0x54, 0x39,
        //    0x64, 0x69, 0x32, 0x78 
        //};
        //private static readonly byte[] InitialCounterValue = new byte[] 
        //{ 
        //    0x4F, 0x77, 0x55, 0x4D,
        //    0x37, 0x35, 0x56, 0x77,
        //    0x30, 0x76, 0x6C, 0x72,
        //    0x4D, 0x4E, 0x6C, 0x6D 
        //};

        private static readonly byte[] EncryptionKey = new byte[]
        {
            0x1F, 0x8A, 0xE9, 0xC0,
            0x47, 0xB3, 0x2D, 0x7F,
            0x9C, 0x1D, 0x6B, 0x0E,
            0x5A, 0x44, 0x89, 0x3C
        };

        private static readonly byte[] InitialCounterValue = new byte[]
        {
            0xB2, 0x9D, 0x7C, 0x5E,
            0xF6, 0x1A, 0x40, 0x3D,
            0x81, 0x0C, 0x9E, 0xD5,
            0xB7, 0x6F, 0x2A, 0x49
        };

        //private static readonly byte[] EncryptionKey = Bitwise.HexStringToByteArray("1F 8A E9 C0 47 B3 2D 7F 9C 1D 6B 0E 5A 44 89 3C");
        //private static readonly byte[] InitialCounterValue = Bitwise.HexStringToByteArray("B2 9D 7C 5E F6 1A 40 3D 81 0C 9E D5 B7 6F 2A 49");

        public static byte[] ProcessData_AES_CTR(byte[] inputData)
        {

            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;
                aes.Key = CryptoSyncer.EncryptionKey;

                using (ICryptoTransform keystreamGenerator = aes.CreateEncryptor())
                {
                    byte[] currentCounter = (byte[])CryptoSyncer.InitialCounterValue.Clone();
                    byte[] outputBuffer = new byte[inputData.Length];

                    for (int blockStart = 0; blockStart < inputData.Length; blockStart += 16)
                    {
                        byte[] keystreamBlock = keystreamGenerator.TransformFinalBlock(currentCounter, 0, 16);

                        int remainingBytes = Math.Min(16, inputData.Length - blockStart);

                        for (int byteOffset = 0; byteOffset < remainingBytes; ++byteOffset)
                            outputBuffer[blockStart + byteOffset] = (byte)((uint)inputData[blockStart + byteOffset] ^ (uint)keystreamBlock[byteOffset]);

                        int counterIndex = 15;
                        while (counterIndex >= 0 && ++currentCounter[counterIndex] == (byte)0)
                            --counterIndex;
                    }
                    return outputBuffer;
                }
            }
        }
        //public static byte[] StaticMethod0(byte[] A_0)
        //{
        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Mode = CipherMode.ECB;
        //        aes.Padding = PaddingMode.None;
        //        aes.Key = CryptoSyncer.Field0;
        //        using (ICryptoTransform encryptor = aes.CreateEncryptor())
        //        {
        //            byte[] inputBuffer = (byte[])CryptoSyncer.Field1.Clone();
        //            byte[] numArray1 = new byte[A_0.Length];
        //            for (int index1 = 0; index1 < A_0.Length; index1 += 16 /*0x10*/)
        //            {
        //                byte[] numArray2 = encryptor.TransformFinalBlock(inputBuffer, 0, 16 /*0x10*/);
        //                int num = Math.Min(16 /*0x10*/, A_0.Length - index1);
        //                for (int index2 = 0; index2 < num; ++index2)
        //                    numArray1[index1 + index2] = (byte)((uint)A_0[index1 + index2] ^ (uint)numArray2[index2]);
        //                int index3 = 15;
        //                while (index3 >= 0 && ++inputBuffer[index3] == (byte)0)
        //                    --index3;
        //            }
        //            return numArray1;
        //        }
        //    }
        //}
    }
}