using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_STAGEROOM_CHANGE_MATCH_TYPE_ACK : GameServerPacket
    {
        private readonly MatchStageRule Rule;
        private readonly byte MatchType;

        public PROTOCOL_STAGEROOM_CHANGE_MATCH_TYPE_ACK(MatchStageRule rule, byte matchType)
        {
            this.Rule = rule;
            this.MatchType = matchType;
        }

        public override void Write()
        {
            this.WriteH((short)7954);
            this.WriteMatchStageRule(this.Rule);
            this.WriteC(this.MatchType);
        }
    }
}
