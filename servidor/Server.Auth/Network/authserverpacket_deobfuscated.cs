using Microsoft.Win32.SafeHandles;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using System;
using System.IO;

namespace Server.Auth.Network
{
    public abstract class AuthServerPacket : BaseServerPacket, IDisposable
    {
        public AuthServerPacket()
        {
            MStream = new MemoryStream();
            BWriter = new BinaryWriter(MStream);
            Handle = new SafeFileHandle(IntPtr.Zero, true);
            Disposed = false;
            SECURITY_KEY = Bitwise.CRYPTO[0];
            HASH_CODE = Bitwise.CRYPTO[1];
            SEED_LENGTH = Bitwise.CRYPTO[2];
            NATIONS = ConfigLoader.National;
        }

        public byte[] GetBytes(string name)
        {
            try
            {
                if (Disposed || MStream == null || BWriter == null)
                {
                    CLogger.Print($"GetBytes problem at: {name}; Stream or Writer already disposed", LoggerType.Error);
                    return new byte[0];
                }

                Write();
                return MStream.ToArray();
            }
            catch (Exception ex)
            {
                CLogger.Print($"GetBytes problem at: {name}; {ex.Message}", LoggerType.Error, ex);
                return new byte[0];
            }
        }

        public byte[] GetCompleteBytes(string name)
        {
            try
            {
                byte[] data = GetBytes("AuthServerPacket.GetCompleteBytes");
                if (data.Length < 2)
                    return new byte[0];

                byte[] lengthBytes = BitConverter.GetBytes((ushort)(data.Length + 2));
                byte[] completePacket = new byte[data.Length + lengthBytes.Length];
                
                Array.Copy(lengthBytes, 0, completePacket, 0, lengthBytes.Length);
                Array.Copy(data, 0, completePacket, lengthBytes.Length, data.Length);
                
                return completePacket;
            }
            catch (Exception ex)
            {
                CLogger.Print($"GetCompleteBytes problem at: {name}; {ex.Message}", LoggerType.Error, ex);
                return new byte[0];
            }
        }

        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (Disposed)
                    return;

                MStream?.Dispose();
                BWriter?.Dispose();
                
                if (disposing)
                {
                    Handle?.Dispose();
                }
                
                Disposed = true;
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }

        public abstract void Write();
    }
}