using Microsoft.Win32.SafeHandles;
using Plugin.Core.Enums;
using Plugin.Core.SharpDX;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Plugin.Core.Network
{
    public class SyncClientPacket : IDisposable
    {
        protected MemoryStream MStream;
        protected BinaryReader BReader;
        protected SafeHandle Handle;
        protected bool Disposed;

        public SyncClientPacket(byte[] Buffer)
        {
            if (Buffer == null)
            {
                Buffer = new byte[0];
            }

            MStream = new MemoryStream(Buffer, 0, Buffer.Length);
            BReader = new BinaryReader(MStream);
            Handle = new SafeFileHandle(IntPtr.Zero, true);
            Disposed = false;
        }

        public byte[] ToArray() => MStream.ToArray();

        public void SetMStream(MemoryStream MStream) => this.MStream = MStream;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposed)
            {
                return;
            }

            try
            {
                MStream?.Dispose();
                BReader?.Dispose();
                if (Disposing)
                {
                    Handle?.Dispose();
                }
            }
            catch
            {
                // Ignorar errores de dispose
            }

            Disposed = true;
        }

        // Métodos de utilidad para verificar disponibilidad de datos
        public int GetRemainingBytes()
        {
            try
            {
                if (MStream != null && MStream.CanSeek)
                {
                    return (int)(MStream.Length - MStream.Position);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public long GetPosition()
        {
            try
            {
                if (MStream != null && MStream.CanSeek)
                {
                    return MStream.Position;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public bool CanRead(int bytesToRead)
        {
            return GetRemainingBytes() >= bytesToRead;
        }

        // Métodos de lectura con validaciones
        public byte[] ReadB(int Length)
        {
            if (Length < 0)
            {
                return new byte[0];
            }

            if (Length == 0)
            {
                return new byte[0];
            }

            if (!CanRead(Length))
            {
                return new byte[0];
            }

            try
            {
                return BReader.ReadBytes(Length);
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadB: Error reading {Length} bytes: {ex.Message}", LoggerType.Error);
                return new byte[0];
            }
        }

        public byte ReadC()
        {
            if (!CanRead(1))
            {
                return 0;
            }

            try
            {
                return BReader.ReadByte();
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadC: Error reading byte: {ex.Message}", LoggerType.Error);
                return 0;
            }
        }

        public short ReadH()
        {
            if (!CanRead(2))
            {
                return 0;
            }

            try
            {
                return BReader.ReadInt16();
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadH: Error reading short: {ex.Message}", LoggerType.Error);
                return 0;
            }
        }

        public ushort ReadUH()
        {
            if (!CanRead(2))
            {
                return 0;
            }

            try
            {
                return BReader.ReadUInt16();
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadUH: Error reading ushort: {ex.Message}", LoggerType.Error);
                return 0;
            }
        }

        public int ReadD()
        {
            if (!CanRead(4))
            {
                return 0;
            }

            try
            {
                return BReader.ReadInt32();
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadD: Error reading int: {ex.Message}", LoggerType.Error);
                return 0;
            }
        }

        public uint ReadUD()
        {
            if (!CanRead(4))
            {
                return 0;
            }

            try
            {
                return BReader.ReadUInt32();
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadUD: Error reading uint: {ex.Message}", LoggerType.Error);
                return 0;
            }
        }

        public float ReadT()
        {
            if (!CanRead(4))
            {
                return 0f;
            }

            try
            {
                return BReader.ReadSingle();
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadT: Error reading float: {ex.Message}", LoggerType.Error);
                return 0f;
            }
        }

        public double ReadF()
        {
            if (!CanRead(8))
            {
                return 0.0;
            }

            try
            {
                return BReader.ReadDouble();
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadF: Error reading double: {ex.Message}", LoggerType.Error);
                return 0.0;
            }
        }

        public long ReadQ()
        {
            if (!CanRead(8))
            {
                return 0L;
            }

            try
            {
                return BReader.ReadInt64();
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadQ: Error reading long: {ex.Message}", LoggerType.Error);
                return 0L;
            }
        }

        public ulong ReadUQ()
        {
            if (!CanRead(8))
            {
                return 0UL;
            }

            try
            {
                return BReader.ReadUInt64();
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadUQ: Error reading ulong: {ex.Message}", LoggerType.Error);
                return 0UL;
            }
        }

        public string ReadN(int Length, string CodePage)
        {
            string Text = "";
            try
            {
                byte[] data = ReadB(Length);
                if (data.Length > 0)
                {
                    Text = Encoding.GetEncoding(CodePage).GetString(data);
                    int Value = Text.IndexOf(char.MinValue);
                    if (Value != -1)
                    {
                        Text = Text.Substring(0, Value);
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadN: Error reading string: {ex.Message}", LoggerType.Error);
            }
            return Text;
        }

        public string ReadS(int Length)
        {
            string Text = "";
            try
            {
                byte[] data = ReadB(Length);
                if (data.Length > 0)
                {
                    Text = Encoding.UTF8.GetString(data);
                    int Value = Text.IndexOf(char.MinValue);
                    if (Value != -1)
                    {
                        Text = Text.Substring(0, Value);
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadS: Error reading UTF8 string: {ex.Message}", LoggerType.Error);
            }
            return Text;
        }

        public string ReadU(int Length)
        {
            string Text = "";
            try
            {
                byte[] data = ReadB(Length);
                if (data.Length > 0)
                {
                    Text = Encoding.Unicode.GetString(data);
                    int Value = Text.IndexOf(char.MinValue);
                    if (Value != -1)
                    {
                        Text = Text.Substring(0, Value);
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadU: Error reading Unicode string: {ex.Message}", LoggerType.Error);
            }
            return Text;
        }

        public byte ReadC(out bool Exception)
        {
            try
            {
                if (!CanRead(1))
                {
                    Exception = true;
                    return 0;
                }

                byte Result = BReader.ReadByte();
                Exception = false;
                return Result;
            }
            catch
            {
                Exception = true;
                return 0;
            }
        }

        public ushort ReadUH(out bool Exception)
        {
            try
            {
                if (!CanRead(2))
                {
                    Exception = true;
                    return 0;
                }

                ushort Result = BReader.ReadUInt16();
                Exception = false;
                return Result;
            }
            catch
            {
                Exception = true;
                return 0;
            }
        }

        public void Advance(int Bytes)
        {
            try
            {
                if (Bytes < 0)
                {
                    return;
                }

                if (!CanRead(Bytes))
                {
                    return;
                }

                long NewPosition = BReader.BaseStream.Position + Bytes;
                if (NewPosition > BReader.BaseStream.Length)
                {
                    throw new Exception("Offset has exceeded the buffer value.");
                }

                BReader.BaseStream.Position = NewPosition;
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.Advance: Error advancing {Bytes} bytes: {ex.Message}", LoggerType.Error);
                throw;
            }
        }

        public Half3 ReadUHV()
        {
            if (!CanRead(6)) // 3 ushorts = 6 bytes
            {
                return new Half3(0, 0, 0);
            }

            try
            {
                return new Half3(ReadUH(), ReadUH(), ReadUH());
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadUHV: Error reading Half3: {ex.Message}", LoggerType.Error);
                return new Half3(0, 0, 0);
            }
        }

        public Half3 ReadTV()
        {
            if (!CanRead(12)) // 3 floats = 12 bytes
            {
                return new Half3(0, 0, 0);
            }

            try
            {
                return new Half3(ReadT(), ReadT(), ReadT());
            }
            catch (Exception ex)
            {
                CLogger.Print($"SyncClientPacket.ReadTV: Error reading Half3: {ex.Message}", LoggerType.Error);
                return new Half3(0, 0, 0);
            }
        }
    }
}