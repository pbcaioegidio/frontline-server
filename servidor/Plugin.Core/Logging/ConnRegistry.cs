using System.Collections.Concurrent;
using System.Threading;

namespace Plugin.Core.Logging
{
    public static class ConnRegistry
    {
        private static readonly ConcurrentDictionary<string, int> Map = new ConcurrentDictionary<string, int>();
        private static int _next;

        public static int IdFor(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return 0;
            return Map.GetOrAdd(endpoint, _ => Interlocked.Increment(ref _next));
        }

        public static void Release(string endpoint)
        {
            if (!string.IsNullOrEmpty(endpoint)) Map.TryRemove(endpoint, out _);
        }
    }
}
