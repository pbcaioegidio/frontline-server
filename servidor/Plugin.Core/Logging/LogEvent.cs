using System;
using System.Collections.Generic;
using System.Threading;

namespace Plugin.Core.Logging
{
    public enum LogLevel { Debug, Info, Warn, Error, Hack }
    public enum LogCat { Packet, Net, Login, Nick, Enter, Shop, Lobby, System, Parse, Opcode, Hack }
    public enum Direction { In, Out }
    public enum ServerKind { Auth, Game, Match }

    public sealed class LogEvent
    {
        private static long _seq;
        public static long NextSeq() => Interlocked.Increment(ref _seq);

        public long Seq { get; set; }
        public DateTime TsUtc { get; set; } = DateTime.UtcNow;
        public LogLevel Level { get; set; } = LogLevel.Info;
        public LogCat Cat { get; set; } = LogCat.System;
        public ServerKind? Srv { get; set; }
        public int? Conn { get; set; }
        public string Ip { get; set; }
        public Direction? Dir { get; set; }
        public int? Op { get; set; }
        public string Pkt { get; set; }
        public int? Len { get; set; }
        public IReadOnlyDictionary<string, object> Fields { get; set; }
        public IReadOnlyList<FieldTrace> Schema { get; set; }
        public string Hex { get; set; }
        public string ErrType { get; set; }
        public string ErrAt { get; set; }
        public string ErrMessage { get; set; }
    }
}
