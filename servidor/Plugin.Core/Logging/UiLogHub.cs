using System;
using System.Collections.Generic;

namespace Plugin.Core.Logging
{
    public sealed class UiLogLine
    {
        public DateTime Time { get; set; }
        public LogLevel Level { get; set; }
        public LogCat Cat { get; set; }
        public string Text { get; set; }
    }

    /// <summary>
    /// Buffer circular de logs para o painel do FLMonitor (sem terminal).
    /// </summary>
    public static class UiLogHub
    {
        public const int MaxLines = 400;
        private static readonly object Sync = new object();
        private static readonly Queue<UiLogLine> Lines = new Queue<UiLogLine>();

        public static event Action<UiLogLine> LineAdded;

        public static void Push(UiLogLine line)
        {
            if (line == null || string.IsNullOrEmpty(line.Text)) return;
            lock (Sync)
            {
                Lines.Enqueue(line);
                while (Lines.Count > MaxLines) Lines.Dequeue();
            }
            try { LineAdded?.Invoke(line); }
            catch { /* UI pode ter fechado */ }
        }

        public static List<UiLogLine> Snapshot()
        {
            lock (Sync) return new List<UiLogLine>(Lines);
        }

        public static void Clear()
        {
            lock (Sync) Lines.Clear();
        }
    }
}
