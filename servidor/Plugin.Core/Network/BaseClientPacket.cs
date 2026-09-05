using Plugin.Core.Enums;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Plugin.Core.Network
{
    public abstract class BaseClientPacket : IDisposable
    {
        protected MemoryStream MStream;
        protected BinaryReader BReader;
        protected SafeHandle Handle;
        protected bool Disposed;
        protected int SECURITY_KEY;
        protected int HASH_CODE;
        protected int SEED_LENGTH;
        protected NationsEnum NATIONS;

        public BaseClientPacket()
        {
        }

        // MÉTODOS SIMPLES - Fallan inmediatamente si hay problema
        protected internal byte[] ReadB(int Length) => BReader.ReadBytes(Length);

        protected internal byte ReadC()
        {
            if (BReader == null)
            {
                Console.WriteLine("ReadC: BReader is null");
                return 0;
            }
            return BReader.ReadByte();
        }

        protected internal short ReadH()
        {
            if (BReader == null)
            {
                Console.WriteLine("ReadC: BReader is null");
                return 0;
            }
            return BReader.ReadInt16();
        }

        protected internal ushort ReadUH() => BReader.ReadUInt16();

        protected internal int ReadD() => BReader.ReadInt32();

        protected internal uint ReadUD() => BReader.ReadUInt32();

        protected internal float ReadT() => BReader.ReadSingle();

        protected internal long ReadQ() => BReader.ReadInt64();

        protected internal ulong ReadUQ() => BReader.ReadUInt64();

        protected internal double ReadF() => BReader.ReadDouble();

        // String methods con logging para debugging
        protected internal string ReadN(int Length, string CodePage)
        {
            try
            {
                if (BReader == null)
                {
                    Console.WriteLine("ReadN: BReader is null");
                    return "";
                }

                string text = Encoding.GetEncoding(CodePage).GetString(ReadB(Length));
                int nullIndex = text.IndexOf(char.MinValue);
                return nullIndex != -1 ? text.Substring(0, nullIndex) : text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReadN Exception: {ex.Message}");
                return "";
            }
        }

        protected internal string ReadS(int Length)
        {
            try
            {
                if (BReader == null)
                {
                    Console.WriteLine("ReadS: BReader is null");
                    return "";
                }

                string text = Encoding.UTF8.GetString(ReadB(Length));
                int nullIndex = text.IndexOf(char.MinValue);
                return nullIndex != -1 ? text.Substring(0, nullIndex) : text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReadS Exception: {ex.Message}");
                return "";
            }
        }

        protected internal string ReadU(int Length)
        {
            try
            {
                if (BReader == null)
                {
                    Console.WriteLine("ReadU: BReader is null");
                    return "";
                }

                string text = Encoding.Unicode.GetString(ReadB(Length));
                int nullIndex = text.IndexOf(char.MinValue);
                return nullIndex != -1 ? text.Substring(0, nullIndex) : text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReadU Exception: {ex.Message}");
                return "";
            }
        }

        // Dispose simple
        public virtual void Dispose()
        {
            if (!Disposed)
            {
                BReader?.Dispose();
                MStream?.Dispose();
                Handle?.Dispose();
                Disposed = true;
            }
        }

        // Métodos virtuales
        public virtual void Read()
        { }

        public virtual void Run()
        { }
    }
}