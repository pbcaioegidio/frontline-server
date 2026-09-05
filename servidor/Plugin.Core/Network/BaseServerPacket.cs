// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Network.BaseServerPacket
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Logging;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Plugin.Core.Network
{
    public abstract class BaseServerPacket
    {
        protected MemoryStream MStream;
        protected BinaryWriter BWriter;
        protected SafeHandle Handle;
        protected bool Disposed;
        protected int SECURITY_KEY;
        protected int HASH_CODE;
        protected int SEED_LENGTH;
        protected NationsEnum NATIONS;

        // ---- packet schema trace (pb-dev oracle). Built only when LogConfig.SchemaTraceEnabled. ----
        private List<FieldTrace> _trace;
        private int _traceDepth;
        private int _traceIdx;

        public IReadOnlyList<FieldTrace> Schema => _trace;

        private long BeginField()
        {
            _traceDepth++;
            return this.MStream != null ? this.MStream.Position : 0L;
        }

        // Records only the outermost Write* call (WriteS/U/N nest WriteB → counted once).
        private void EndField(string tag, string name, long off, object val)
        {
            _traceDepth--;
            if (_traceDepth != 0 || !LogConfig.SchemaTraceEnabled) return;
            if (_trace == null) _trace = new List<FieldTrace>(32);
            long now = this.MStream != null ? this.MStream.Position : off;
            _trace.Add(new FieldTrace { I = _traceIdx++, T = tag, Off = off, Len = (int)(now - off), Val = val, Name = name });
        }

        protected internal void WriteB(byte[] Value, int Offset, int Length, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value, Offset, Length);
            EndField("B", Name, o, "bytes[" + Length + "]");
        }

        protected internal void WriteB(byte[] Value, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value);
            EndField("B", Name, o, Value != null ? "bytes[" + Value.Length + "]" : "null");
        }

        protected internal void WriteC(byte Value, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value); EndField("C", Name, o, (long)Value);
        }

        protected internal void WriteH(ushort Value, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value); EndField("H", Name, o, (long)Value);
        }

        protected internal void WriteH(short Value, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value); EndField("H", Name, o, (long)Value);
        }

        protected internal void WriteD(uint Value, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value); EndField("D", Name, o, (long)Value);
        }

        protected internal void WriteD(int Value, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value); EndField("D", Name, o, (long)Value);
        }

        protected internal void WriteT(float Value, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value); EndField("T", Name, o, (double)Value);
        }

        protected internal void WriteF(double Value, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value); EndField("F", Name, o, Value);
        }

        protected internal void WriteQ(ulong Value, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value); EndField("Q", Name, o, (long)Value);
        }

        protected internal void WriteQ(long Value, string Name = null)
        {
            long o = BeginField(); this.BWriter.Write(Value); EndField("Q", Name, o, Value);
        }

        protected internal void WriteN(string Text, int Count, string CodePage, string Name = null)
        {
            long o = BeginField();
            if (Text != null)
            {
                this.WriteB(Encoding.GetEncoding(CodePage).GetBytes(Text));
                this.WriteB(new byte[Count - Text.Length]);
            }
            EndField("N", Name, o, Text);
        }

        protected internal void WriteS(string Text, int Count, string Name = null)
        {
            long o = BeginField();
            if (Text != null)
            {
                this.WriteB(Encoding.UTF8.GetBytes(Text));
                this.WriteB(new byte[Count - Text.Length]);
            }
            EndField("S", Name, o, Text);
        }

        protected internal void WriteU(string Text, int Count, string Name = null)
        {
            long o = BeginField();
            if (Text != null)
            {
                this.WriteB(Encoding.Unicode.GetBytes(Text));
                this.WriteB(new byte[Count - Text.Length * 2]);
            }
            EndField("U", Name, o, Text);
        }
    }
}