using System;

namespace Plugin.Core.Utility
{
    public class CryptoUtils
    {
        private static readonly Random randomGenerator = new Random();

        public static int UdpEncryptor(byte[] inputBuffer, uint inputLength, byte[] outputBuffer, uint maxOutputLength, byte[] key1, byte[] key2)
        {
            byte[] expandedKey1 = new byte[29];
            byte[] expandedKey2 = new byte[29];
            
            Array.Clear(expandedKey1, 0, expandedKey1.Length);
            Array.Copy(key1, 0, expandedKey1, 0, Math.Min(4, expandedKey1.Length));
            Array.Clear(expandedKey2, 0, expandedKey2.Length);
            Array.Copy(key2, 0, expandedKey2, 0, Math.Min(4, expandedKey2.Length));

            byte randomByte1 = (byte)(randomGenerator.Next() % 32);
            byte randomByte2 = (byte)(randomGenerator.Next() % 32);
            byte randomByte3 = (byte)randomGenerator.Next();
            byte paddingLength = (byte)((2 - (int)inputLength & 7) + 2);
            byte originalPaddingLength = paddingLength;
            uint paddingLengthUint = paddingLength;

            if ((uint)(paddingLength + inputLength + 4) >= maxOutputLength)
                return 0;

            outputBuffer[0] = randomByte1;
            outputBuffer[1] = randomByte2;
            outputBuffer[2] = randomByte3;
            outputBuffer[3] = paddingLength;

            byte keyMixer1 = (byte)(((uint)randomByte3 & 15U) + randomByte1);
            byte keyMixer2 = (byte)(((uint)randomByte3 >> 4) + randomByte2);
            byte xorAccumulator = 0;
            uint outputIndex = 4;
            int keyOffset = 0;

            if (inputLength > 0U)
            {
                byte keyIndex1 = keyMixer1;
                uint inputIndex = 0;
                byte originalKeyMixer1 = keyMixer1;
                byte originalKeyMixer2 = keyMixer2;

                do
                {
                    byte keyByte1 = SubFunc(expandedKey1, 4, (uint)keyIndex1 + keyOffset, true);
                    byte keyByte2 = SubFunc(expandedKey2, 4, (uint)keyOffset + originalKeyMixer2, true);
                    xorAccumulator ^= (byte)(keyByte1 ^ keyByte2);
                    keyOffset += xorAccumulator + 8;
                    outputBuffer[inputIndex + 4] = (byte)(xorAccumulator ^ inputBuffer[inputIndex]);
                    byte encryptedByte = (byte)(xorAccumulator ^ inputBuffer[inputIndex]);
                    
                    for (byte i = 0; i < 4; i += 2)
                        expandedKey1[i] ^= encryptedByte;
                    for (byte i = 1; i < 4; i += 2)
                        expandedKey2[i] ^= encryptedByte;
                    
                    keyIndex1 = originalKeyMixer1;
                    inputIndex++;
                }
                while (inputIndex < inputLength);
                
                outputIndex = inputLength + 4U;
            }

            if (paddingLengthUint > 0U)
            {
                byte keyIndex1 = keyMixer1;
                byte originalKeyMixer1 = keyMixer1;
                byte originalKeyMixer2 = keyMixer2;

                do
                {
                    byte keyByte1 = SubFunc(expandedKey1, 4, (uint)keyOffset + keyIndex1, true);
                    byte keyByte2 = SubFunc(expandedKey2, 4, (uint)keyOffset + originalKeyMixer2, true);
                    xorAccumulator ^= (byte)(keyByte1 ^ keyByte2);
                    byte paddingByte = (byte)(originalPaddingLength ^ xorAccumulator);
                    keyOffset += xorAccumulator + 8;
                    outputBuffer[outputIndex] = paddingByte;
                    
                    for (byte i = 0; i < 4; i += 2)
                        expandedKey1[i] ^= paddingByte;
                    for (byte i = 1; i < 4; i += 2)
                        expandedKey2[i] ^= paddingByte;
                    
                    keyIndex1 = originalKeyMixer1;
                    outputIndex++;
                    paddingLengthUint--;
                }
                while (paddingLengthUint > 0U);
            }

            return (int)outputIndex;
        }

        public static int UdpDecryptor(byte[] inputBuffer, uint inputLength, byte[] outputBuffer, uint maxOutputLength, byte[] key1, byte[] key2)
        {
            byte[] expandedKey1 = new byte[29];
            byte[] expandedKey2 = new byte[29];
            byte[] paddingValidation = new byte[12];
            byte[] keyOffsetBuffer = new byte[5];

            Array.Clear(expandedKey1, 0, expandedKey1.Length);
            Array.Copy(key1, 0, expandedKey1, 0, Math.Min(4, expandedKey1.Length));
            Array.Clear(expandedKey2, 0, expandedKey2.Length);
            Array.Copy(key2, 0, expandedKey2, 0, Math.Min(4, expandedKey2.Length));

            byte paddingLength = inputBuffer[3];
            if (paddingLength - 2 > 7)
                return -1;

            uint dataLength = inputLength - paddingLength;
            if (dataLength - 4U >= maxOutputLength)
                return 0;

            uint validationIndex = 0;
            keyOffsetBuffer[4] = 0;
            byte keyMixer1 = (byte)((uint)inputBuffer[1] + ((uint)inputBuffer[2] & 15U));
            byte keyMixer2 = (byte)((uint)inputBuffer[0] + ((uint)inputBuffer[2] >> 4));
            uint inputIndex = 4;
            byte originalKeyMixer2 = keyMixer2;
            Array.Copy(BitConverter.GetBytes((short)keyMixer1), 0, keyOffsetBuffer, 0, 4);
            byte xorAccumulator = 0;

            if (dataLength > 4U)
            {
                int keyIndex1 = keyMixer2;
                int keyIndex2 = keyMixer1;
                byte tempXor = 0;
                int originalKeyIndex1 = keyIndex1;
                int originalKeyIndex2 = keyIndex2;
                uint actualDataLength = dataLength - 4U;

                do
                {
                    byte keyByte1 = SubFunc(expandedKey1, 4, (uint)(keyIndex1 + BitConverter.ToInt32(keyOffsetBuffer, 1)), true);
                    byte keyByte2 = SubFunc(expandedKey2, 4, (uint)(originalKeyIndex2 + BitConverter.ToInt32(keyOffsetBuffer, 1)), true);
                    tempXor ^= (byte)(keyByte1 ^ keyByte2);
                    Array.Copy(BitConverter.GetBytes(BitConverter.ToInt32(keyOffsetBuffer, 1) + tempXor + 14), 0, keyOffsetBuffer, 1, 4);
                    byte decryptedByte = (byte)(tempXor ^ inputBuffer[inputIndex]);
                    outputBuffer[inputIndex - 4] = decryptedByte;
                    byte feedbackByte = (byte)(tempXor ^ decryptedByte);
                    
                    for (byte i = 0; i < 4; i += 2)
                        expandedKey1[i] ^= feedbackByte;
                    for (byte i = 1; i < 4; i += 2)
                        expandedKey2[i] ^= feedbackByte;
                    
                    keyIndex1 = originalKeyIndex1;
                    inputIndex++;
                }
                while (inputIndex < dataLength);
                
                keyMixer2 = originalKeyMixer2;
                xorAccumulator = tempXor;
                keyMixer1 = keyOffsetBuffer[0];
            }

            uint paddingIndex = 0;
            if (paddingLength > 0)
            {
                int keyIndex1 = keyMixer2;
                int originalKeyIndex1 = keyIndex1;
                
                for (uint i = 0; i < paddingLength; i++)
                {
                    byte keyByte1 = SubFunc(expandedKey1, 4, (uint)(keyIndex1 + BitConverter.ToInt32(keyOffsetBuffer, 1)), true);
                    byte keyByte2 = SubFunc(expandedKey2, 4, (uint)keyMixer1 + BitConverter.ToInt32(keyOffsetBuffer, 1), true);
                    xorAccumulator ^= (byte)(keyByte1 ^ keyByte2);
                    Array.Copy(BitConverter.GetBytes(BitConverter.ToInt32(keyOffsetBuffer, 1) + xorAccumulator + 14), 0, keyOffsetBuffer, 1, 4);
                    byte paddingByte = (byte)(xorAccumulator ^ inputBuffer[actualDataLength + 4 + i]);
                    paddingValidation[paddingIndex] = paddingByte;
                    byte feedbackByte = (byte)(xorAccumulator ^ paddingByte);
                    
                    for (byte j = 0; j < 4; j += 2)
                        expandedKey1[j] ^= feedbackByte;
                    for (byte j = 1; j < 4; j += 2)
                        expandedKey2[j] ^= feedbackByte;
                    
                    keyIndex1 = originalKeyIndex1;
                    paddingIndex++;
                }
            }

            return paddingLength == paddingValidation[0] && paddingLength == paddingValidation[1] ? (int)actualDataLength : -1;
        }

        public static byte SubFunc(byte[] buffer, byte length, uint index, bool direction)
        {
            uint byteIndex;
            int bitShift;
            byte nextByteIndex;

            if (direction)
            {
                byteIndex = (index >> 3) % length;
                bitShift = (int)index & 7;
                nextByteIndex = (byte)((byteIndex + 1U) % length);
            }
            else
            {
                byteIndex = (uint)(byte)(((long)(8 * length) - (long)index % (long)(8 * length)) / 8L % length);
                nextByteIndex = (byte)((byteIndex + 1U) % length);
                bitShift = (int)(((long)(8 * length) - (long)index % (long)(8 * length)) % 8L);
            }

            return bitShift != 0 ? 
                (byte)((buffer[nextByteIndex] >> (8 - bitShift)) + ((buffer[byteIndex] & (byte)(255 >> bitShift)) << bitShift)) : 
                buffer[byteIndex];
        }
    }
}