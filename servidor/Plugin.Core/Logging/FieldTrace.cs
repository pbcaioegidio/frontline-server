namespace Plugin.Core.Logging
{
    // One serialized field, captured in wire order by BaseServerPacket.Write* helpers.
    // Off is payload-relative (opcode lives at offset 0). Emitted as the JSONL "schema" array.
    public sealed class FieldTrace
    {
        public int I;        // write index (order)
        public string T;     // type tag: C H D Q T F B S U N
        public long Off;     // byte offset within the payload
        public int Len;      // bytes written
        public object Val;   // value (long for integers, double for floats, preview string for bytes/text)
        public string Name;  // optional field-name tag passed to the Write* call
    }
}
