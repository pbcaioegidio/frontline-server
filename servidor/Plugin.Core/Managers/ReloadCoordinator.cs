using System.Collections.Generic;

namespace Plugin.Core.Managers
{
    /// <summary>
    /// A supervisor reload is broadcast to every sync endpoint, but Auth, the game channels and
    /// Match can all live in one process and share the same static managers. Without a gate each
    /// endpoint would run the same Reset/Load over the same collections at the same time, which
    /// duplicates entries instead of replacing them. Services still do their own per-instance
    /// work; only the shared reload is claimed, by the broadcast id that fanned it out.
    /// </summary>
    public static class ReloadCoordinator
    {
        private static readonly object Gate = new object();
        private static readonly Dictionary<int, uint> Applied = new Dictionary<int, uint>();

        /// <summary>
        /// True for the first caller of a given broadcast, false for every later one in this
        /// process. A zero id means the sender did not tag the broadcast, so nothing is gated.
        /// </summary>
        public static bool ClaimShared(int subCmd, uint broadcastId)
        {
            if (broadcastId == 0)
                return true;

            lock (Gate)
            {
                uint last;
                if (Applied.TryGetValue(subCmd, out last) && last == broadcastId)
                    return false;

                Applied[subCmd] = broadcastId;
                return true;
            }
        }
    }
}
