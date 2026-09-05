using System;

namespace Server.Game.Data.Models
{
    public sealed class MatchStageRule
    {
        public const int StageCount = 5;
        public const int WireSize = 23;

        public byte ConfigA;
        public byte ConfigB;
        public byte ConfigC;
        public readonly uint[] StageIds;

        public MatchStageRule()
        {
            this.StageIds = new uint[StageCount];
        }

        public MatchStageRule(byte configA, byte configB, byte configC, uint[] stageIds)
        {
            this.ConfigA = configA;
            this.ConfigB = configB;
            this.ConfigC = configC;
            this.StageIds = new uint[StageCount];
            if (stageIds != null)
            {
                int n = Math.Min(stageIds.Length, StageCount);
                Array.Copy(stageIds, this.StageIds, n);
            }
        }
    }
}
