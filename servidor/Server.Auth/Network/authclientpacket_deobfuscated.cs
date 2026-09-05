using Microsoft.Win32.SafeHandles;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using System;
using System.IO;

namespace Server.Auth.Network
{
    public abstract class AuthClientPacket : BaseClientPacket, IDisposable
    {
        protected AuthClient Client;

        public AuthClientPacket()
        {
            Handle = new SafeFileHandle(IntPtr.Zero, true);
            Disposed = false;
            SECURITY_KEY = Bitwise.CRYPTO[0];
            HASH_CODE = Bitwise.CRYPTO[1];
            SEED_LENGTH = Bitwise.CRYPTO[2];
            NATIONS = ConfigLoader.National;
        }

        protected internal void Makeme(AuthClient client, byte[] buffer)
        {
            this.Client = client;
            MStream = new MemoryStream(buffer, 4, buffer.Length - 4);
            BReader = new BinaryReader(MStream);
            Read();
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
                BReader?.Dispose();
                
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

        public abstract void Read();
        public abstract void Run();
    }
}