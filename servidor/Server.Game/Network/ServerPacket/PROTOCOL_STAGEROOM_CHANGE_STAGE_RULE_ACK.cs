using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_STAGEROOM_CHANGE_STAGE_RULE_ACK : GameServerPacket
    {
        private readonly MatchStageRule Rule;

        public PROTOCOL_STAGEROOM_CHANGE_STAGE_RULE_ACK(MatchStageRule rule)
        {
            this.Rule = rule;
        }

        public override void Write()
        {
            this.WriteH((short)7956);
            this.WriteMatchStageRule(this.Rule);
        }
    }
}
