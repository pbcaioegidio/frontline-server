using System;
using System.Threading;

namespace Plugin.Core.Security
{
    /// <summary>
    /// Rate-limit simples por conexão (janela deslizante de 1s).
    /// </summary>
    public sealed class PacketRateLimiter
    {
        private readonly int _maxPerSecond;
        private long _windowTicks;
        private int _count;

        public PacketRateLimiter(int maxPerSecond = 150)
        {
            _maxPerSecond = maxPerSecond > 0 ? maxPerSecond : 150;
            _windowTicks = DateTime.UtcNow.Ticks;
        }

        public bool TryAllow()
        {
            long now = DateTime.UtcNow.Ticks;
            long window = Interlocked.Read(ref _windowTicks);
            if (now - window >= TimeSpan.TicksPerSecond)
            {
                Interlocked.Exchange(ref _windowTicks, now);
                Interlocked.Exchange(ref _count, 1);
                return true;
            }

            int n = Interlocked.Increment(ref _count);
            return n <= _maxPerSecond;
        }
    }
}
