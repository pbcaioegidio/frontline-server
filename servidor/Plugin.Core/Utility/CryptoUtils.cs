// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Utility.CryptoUtils
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;

namespace Plugin.Core.Utility
{
    public class CryptoUtils
    {
        private static readonly Random Field0 = new Random();

        public static int UdpEncryptor(
          byte[] inputBuffer,
          uint inputLength,
          byte[] outputBuffer,
          uint maxOutputLength,
          byte[] key1,
          byte[] key2)
        {
            byte[] numArray1 = new byte[29];
            byte[] numArray2 = new byte[29];
            Array.Clear((Array)numArray1, 0, numArray1.Length);
            Array.Copy((Array)key1, 0, (Array)numArray1, 0, Math.Min(4, numArray1.Length));
            Array.Clear((Array)numArray2, 0, numArray2.Length);
            Array.Copy((Array)key2, 0, (Array)numArray2, 0, Math.Min(4, numArray2.Length));
            byte num1 = (byte)(CryptoUtils.Field0.Next() % 32 /*0x20*/);
            byte num2 = (byte)(CryptoUtils.Field0.Next() % 32 /*0x20*/);
            byte num3 = (byte)CryptoUtils.Field0.Next();
            byte num4 = (byte)((2 - (int)inputLength & 7) + 2);
            byte num5 = num4;
            uint num6 = (uint)num4;
            if ((uint)((int)num4 + (int)inputLength + 4) >= maxOutputLength)
                return 0;
            outputBuffer[0] = num1;
            outputBuffer[1] = num2;
            outputBuffer[2] = num3;
            outputBuffer[3] = num4;
            byte num7 = (byte)(((uint)num3 & 15U) + (uint)num1);
            byte num8 = (byte)(((uint)num3 >> 4) + (uint)num2);
            byte num9 = 0;
            uint index1 = 4;
            int num10 = 0;
            if (inputLength > 0U)
            {
                byte num11 = num7;
                uint index2 = 0;
                byte num12 = num7;
                byte num13 = num8;
                do
                {
                    byte num14 = CryptoUtils.SubFunc(numArray1, (byte)4, (uint)num11 + (uint)num10, true);
                    byte num15 = CryptoUtils.SubFunc(numArray2, (byte)4, (uint)num10 + (uint)num13, true);
                    num9 ^= (byte)((uint)num14 ^ (uint)num15);
                    num10 += (int)num9 + 8;
                    outputBuffer[(int)index2 + 4] = (byte)((uint)num9 ^ (uint)inputBuffer[(int)index2]);
                    byte num16 = (byte)((uint)num9 ^ (uint)inputBuffer[(int)index2]);
                    for (byte index3 = 0; index3 < (byte)4; index3 += (byte)2)
                        numArray1[(int)index3] ^= num16;
                    for (byte index4 = 1; index4 < (byte)4; index4 += (byte)2)
                        numArray2[(int)index4] ^= num16;
                    num11 = num12;
                    ++index2;
                }
                while (index2 < inputLength);
                index1 = inputLength + 4U;
            }
            if (num6 > 0U)
            {
                byte num17 = num7;
                byte num18 = num7;
                byte num19 = num8;
                do
                {
                    byte num20 = CryptoUtils.SubFunc(numArray1, (byte)4, (uint)num10 + (uint)num17, true);
                    byte num21 = CryptoUtils.SubFunc(numArray2, (byte)4, (uint)num10 + (uint)num19, true);
                    num9 ^= (byte)((uint)num20 ^ (uint)num21);
                    byte num22 = (byte)((uint)num5 ^ (uint)num9);
                    num10 += (int)num9 + 8;
                    outputBuffer[(int)index1] = num22;
                    for (byte index5 = 0; index5 < (byte)4; index5 += (byte)2)
                        numArray1[(int)index5] ^= num22;
                    for (byte index6 = 1; index6 < (byte)4; index6 += (byte)2)
                        numArray2[(int)index6] ^= num22;
                    num17 = num18;
                    ++index1;
                    --num6;
                }
                while (num6 > 0U);
            }
            return (int)index1;
        }

        public static int UdpDecryptor(
          byte[] inputBuffer,
          uint inputLength,
          byte[] outputBuffer,
          uint maxOutputLength,
          byte[] key1,
          byte[] key2)
        {
            byte[] numArray1 = new byte[29];
            byte[] numArray2 = new byte[29];
            byte[] numArray3 = new byte[12];
            byte[] destinationArray = new byte[5];
            Array.Clear((Array)numArray1, 0, numArray1.Length);
            Array.Copy((Array)key1, 0, (Array)numArray1, 0, Math.Min(4, numArray1.Length));
            Array.Clear((Array)numArray2, 0, numArray2.Length);
            Array.Copy((Array)key2, 0, (Array)numArray2, 0, Math.Min(4, numArray2.Length));
            byte num1 = inputBuffer[3];
            if ((int)num1 - 2 > 7)
                return -1;
            uint num2 = inputLength - (uint)num1;
            if (num2 - 4U >= maxOutputLength)
                return 0;
            uint num3 = 0;
            destinationArray[4] = (byte)0;
            byte num4 = (byte)((uint)inputBuffer[1] + ((uint)inputBuffer[2] & 15U));
            byte num5 = (byte)((uint)inputBuffer[0] + ((uint)inputBuffer[2] >> 4));
            uint index1 = 4;
            byte num6 = num5;
            Array.Copy((Array)BitConverter.GetBytes((short)num4), 0, (Array)destinationArray, 0, 4);
            byte num7 = 0;
            if (num2 > 4U)
            {
                int num8 = (int)num5;
                int num9 = (int)num4;
                byte num10 = 0;
                int num11 = num8;
                int num12 = num9;
                num3 = num2 - 4U;
                do
                {
                    byte num13 = CryptoUtils.SubFunc(numArray1, (byte)4, (uint)(num8 + BitConverter.ToInt32(destinationArray, 1)), true);
                    byte num14 = CryptoUtils.SubFunc(numArray2, (byte)4, (uint)(num12 + BitConverter.ToInt32(destinationArray, 1)), true);
                    num10 ^= (byte)((uint)num13 ^ (uint)num14);
                    Array.Copy((Array)BitConverter.GetBytes(BitConverter.ToInt32(destinationArray, 1) + (int)num10 + 14), 0, (Array)destinationArray, 1, 4);
                    byte num15 = (byte)((uint)num10 ^ (uint)inputBuffer[(int)index1]);
                    outputBuffer[(int)index1 - 4] = num15;
                    byte num16 = (byte)((uint)num10 ^ (uint)num15);
                    for (byte index2 = 0; index2 < (byte)4; index2 += (byte)2)
                        numArray1[(int)index2] ^= num16;
                    for (byte index3 = 1; index3 < (byte)4; index3 += (byte)2)
                        numArray2[(int)index3] ^= num16;
                    num8 = num11;
                    ++index1;
                }
                while (index1 < num2);
                num5 = num6;
                num7 = num10;
                num4 = destinationArray[0];
            }
            uint index4 = 0;
            if (num1 > (byte)0)
            {
                int num17 = (int)num5;
                int num18 = (int)num5;
                for (uint index5 = 0; index5 < (uint)num1; ++index5)
                {
                    byte num19 = CryptoUtils.SubFunc(numArray1, (byte)4, (uint)(num17 + BitConverter.ToInt32(destinationArray, 1)), true);
                    byte num20 = CryptoUtils.SubFunc(numArray2, (byte)4, (uint)num4 + (uint)BitConverter.ToInt32(destinationArray, 1), true);
                    num7 ^= (byte)((uint)num19 ^ (uint)num20);
                    Array.Copy((Array)BitConverter.GetBytes(BitConverter.ToInt32(destinationArray, 1) + (int)num7 + 14), 0, (Array)destinationArray, 1, 4);
                    byte num21 = (byte)((uint)num7 ^ (uint)inputBuffer[(int)num3 + 4 + (int)index5]);
                    numArray3[(int)index4] = num21;
                    byte num22 = (byte)((uint)num7 ^ (uint)num21);
                    for (byte index6 = 0; index6 < (byte)4; index6 += (byte)2)
                        numArray1[(int)index6] ^= num22;
                    for (byte index7 = 1; index7 < (byte)4; index7 += (byte)2)
                        numArray2[(int)index7] ^= num22;
                    num17 = num18;
                    ++index4;
                }
            }
            return (int)num1 == (int)numArray3[0] && (int)num1 == (int)numArray3[1] ? (int)num3 : -1;
        }

        public static byte SubFunc(byte[] buffer, byte length, uint index, bool direction)
        {
            uint index1;
            int num;
            byte index2;
            if (direction)
            {
                index1 = (index >> 3) % (uint)length;
                num = (int)index & 7;
                index2 = (byte)((index1 + 1U) % (uint)length);
            }
            else
            {
                index1 = (uint)(byte)((ulong)(((long)(8 * (int)length) - (long)index % (long)(8 * (int)length)) / 8L) % (ulong)length);
                index2 = (byte)((index1 + 1U) % (uint)length);
                num = (int)(((long)(8 * (int)length) - (long)index % (long)(8 * (int)length)) % 8L);
            }
            return num != 0 ? (byte)(((int)buffer[(int)index2] >> 8 - num) + (((int)buffer[(int)index1] & (int)(byte)((int)byte.MaxValue >> num)) << num)) : buffer[(int)index1];
        }
    }
}