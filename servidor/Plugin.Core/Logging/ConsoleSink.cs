using System;
using System.Collections.Generic;
using System.Text;
using Plugin.Core.Utility;
using Plugin.Core.Colorful;
using Console = Plugin.Core.Colorful.Console;

namespace Plugin.Core.Logging
{
    public sealed class ConsoleSink : ILogSink
    {
        private readonly LogLevel _min;
        private readonly bool _showPackets;
        private readonly object _lock = new object();
        private readonly List<Group> _pending = new List<Group>();

        private sealed class Group
        {
            public string Body;
            public DateTime TsFirst;
            public DateTime TsLast;
            public int Count;
            public System.Drawing.Color Color;
        }

        public ConsoleSink(LogLevel minLevel, bool showPackets)
        {
            _min = minLevel;
            _showPackets = showPackets;
        }

        public static string ShortPkt(string pkt)
        {
            if (string.IsNullOrEmpty(pkt)) return pkt;
            const string p = "PROTOCOL_";
            string s = pkt.StartsWith(p) ? pkt.Substring(p.Length) : pkt;
            int us = s.IndexOf('_');
            return us > 0 ? s.Substring(us + 1) : s;
        }

        public static string Render(LogEvent e)
        {
            string ts = e.TsUtc.ToLocalTime().ToString("HH:mm:ss");
            return "[" + ts + "] " + RenderBody(e);
        }

        public static string RenderBody(LogEvent e)
        {
            var sb = new StringBuilder();
            if (e.Cat == LogCat.Packet)
            {
                string srv = e.Srv?.ToString().ToLowerInvariant() ?? "?";
                string arrow = e.Dir == Direction.In ? "<-" : "->";
                sb.Append(srv).Append(' ').Append(arrow).Append(' ')
                  .Append(ShortPkt(e.Pkt)).Append(" op=").Append(e.Op);
                if (e.Conn.HasValue) sb.Append(" c").Append(e.Conn.Value);
            }
            else if (e.Cat == LogCat.Opcode)
            {
                sb.Append("OPCODE? op=").Append(e.Op).Append(" (0x").Append((e.Op ?? 0).ToString("X")).Append(')');
                if (e.Dir.HasValue) sb.Append(e.Dir == Direction.In ? " in" : " out");
                if (e.Len.HasValue) sb.Append(" len=").Append(e.Len.Value);
                if (e.Conn.HasValue) sb.Append(" c").Append(e.Conn.Value);
                if (!string.IsNullOrEmpty(e.ErrMessage)) sb.Append(' ').Append(e.ErrMessage);
            }
            else
            {
                sb.Append(e.Cat.ToString().ToLowerInvariant());
                if (!string.IsNullOrEmpty(e.ErrMessage)) sb.Append(' ').Append(e.ErrMessage);
                if (e.Fields != null)
                    foreach (var kv in e.Fields) sb.Append(' ').Append(kv.Key).Append('=').Append(kv.Value);
            }
            return sb.ToString();
        }

        public void Write(LogEvent e)
        {
            try
            {
                if (e.Level < _min) return;
                if (e.Cat == LogCat.Packet && !_showPackets) return;

                lock (_lock)
                {
                    if (e.Cat != LogCat.Packet)
                    {
                        FlushAll();
                        Console.WriteLine(Render(e), ColorFor(e));
                        return;
                    }

                    string body = RenderBody(e);
                    var ts = e.TsUtc.ToLocalTime();
                    var existing = _pending.Find(g => g.Body == body);
                    if (existing != null)
                    {
                        existing.Count++;
                        existing.TsLast = ts;
                        return;
                    }

                    bool anyRepeat = _pending.Exists(g => g.Count >= 2);
                    if (anyRepeat || _pending.Count >= 2) FlushAll();
                    _pending.Add(new Group { Body = body, TsFirst = ts, TsLast = ts, Count = 1, Color = ColorFor(e) });
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("[ConsoleSink failed] " + ex.Message + " | " + Render(e));
            }
        }

        private void FlushAll()
        {
            foreach (var g in _pending)
            {
                string ts = g.Count > 1
                    ? g.TsFirst.ToString("HH:mm:ss") + "-" + g.TsLast.ToString("HH:mm:ss")
                    : g.TsFirst.ToString("HH:mm:ss");
                string line = "[" + ts + "] " + g.Body + (g.Count > 1 ? " (x" + g.Count + ")" : "");
                Console.WriteLine(line, g.Color);
            }
            _pending.Clear();
        }

        private static System.Drawing.Color ColorFor(LogEvent e)
        {
            switch (e.Level)
            {
                case LogLevel.Error: return ColorUtil.Red;
                case LogLevel.Warn:  return ColorUtil.Yellow;
                case LogLevel.Hack:  return ColorUtil.Fuchsia;
                case LogLevel.Debug: return ColorUtil.LightGrey;
                default:
                    return e.Cat == LogCat.Packet ? ColorUtil.Cyan
                         : e.Cat == LogCat.Opcode ? ColorUtil.Fuchsia
                         : ColorUtil.White;
            }
        }
    }
}
