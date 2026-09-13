using System;
using System.Text;
using System.Text.Json;

namespace Plugin.Core.Logging
{
    public static class LogEventJson
    {
        public static string ToLine(LogEvent e)
        {
            var sb = new StringBuilder(256);
            using (var stream = new System.IO.MemoryStream())
            {
                using (var w = new Utf8JsonWriter(stream))
                {
                    w.WriteStartObject();
                    // Horário local do processo (TZ=America/Sao_Paulo no compose).
                    w.WriteString("ts", e.TsUtc.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));
                    w.WriteNumber("seq", e.Seq);
                    w.WriteString("lvl", e.Level.ToString().ToLowerInvariant());
                    w.WriteString("cat", e.Cat.ToString().ToLowerInvariant());
                    if (e.Srv.HasValue) w.WriteString("srv", e.Srv.Value.ToString().ToLowerInvariant());
                    if (e.Conn.HasValue) w.WriteNumber("conn", e.Conn.Value);
                    if (!string.IsNullOrEmpty(e.Ip)) w.WriteString("ip", e.Ip);
                    if (e.Dir.HasValue) w.WriteString("dir", e.Dir.Value == Direction.In ? "in" : "out");
                    if (e.Op.HasValue) { w.WriteNumber("op", e.Op.Value); w.WriteString("opHex", "0x" + e.Op.Value.ToString("X")); }
                    if (!string.IsNullOrEmpty(e.Pkt)) w.WriteString("pkt", e.Pkt);
                    if (e.Len.HasValue) w.WriteNumber("len", e.Len.Value);
                    if (e.Fields != null && e.Fields.Count > 0)
                    {
                        w.WriteStartObject("fields");
                        foreach (var kv in e.Fields) WriteValue(w, kv.Key, kv.Value);
                        w.WriteEndObject();
                    }
                    if (e.Schema != null && e.Schema.Count > 0)
                    {
                        w.WriteStartArray("schema");
                        foreach (var ft in e.Schema)
                        {
                            w.WriteStartObject();
                            w.WriteNumber("i", ft.I);
                            w.WriteString("t", ft.T);
                            w.WriteNumber("off", ft.Off);
                            w.WriteNumber("len", ft.Len);
                            if (!string.IsNullOrEmpty(ft.Name)) w.WriteString("name", ft.Name);
                            WriteValue(w, "val", ft.Val);
                            w.WriteEndObject();
                        }
                        w.WriteEndArray();
                    }
                    if (!string.IsNullOrEmpty(e.Hex)) w.WriteString("hex", e.Hex);
                    if (!string.IsNullOrEmpty(e.ErrType))
                    {
                        w.WriteStartObject("err");
                        w.WriteString("type", e.ErrType);
                        if (!string.IsNullOrEmpty(e.ErrAt)) w.WriteString("at", e.ErrAt);
                        if (!string.IsNullOrEmpty(e.ErrMessage)) w.WriteString("message", e.ErrMessage);
                        w.WriteEndObject();
                    }
                    else if (!string.IsNullOrEmpty(e.ErrMessage))
                    {
                        w.WriteString("msg", e.ErrMessage);
                    }
                    w.WriteEndObject();
                }
                sb.Append(Encoding.UTF8.GetString(stream.ToArray()));
            }
            return sb.ToString();
        }

        private static void WriteValue(Utf8JsonWriter w, string key, object v)
        {
            switch (v)
            {
                case null: w.WriteNull(key); break;
                case bool b: w.WriteBoolean(key, b); break;
                case int i: w.WriteNumber(key, i); break;
                case long l: w.WriteNumber(key, l); break;
                case double d: w.WriteNumber(key, d); break;
                default: w.WriteString(key, v.ToString()); break;
            }
        }
    }
}
