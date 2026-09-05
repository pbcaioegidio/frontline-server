namespace Plugin.Core.Utility
{
    public static class RankDomain
    {
        public const int NoFakeRank = 255;

        public const int Fallback = 50;

        public static bool IsValid(int rank)
        {
            return (rank >= 0 && rank <= 56)
                || (rank >= 58 && rank <= 60)
                || (rank >= 97 && rank <= 99)
                || (rank >= 101 && rank <= 112);
        }

        public static bool IsStaff(int rank) => rank == 98 || rank == 99;

        public static int Sanitize(int rank) => IsValid(rank) ? rank : Fallback;

        public static int Resolve(int realRank, int fakeRank)
        {
            return Sanitize(fakeRank != NoFakeRank ? fakeRank : realRank);
        }
    }
}
