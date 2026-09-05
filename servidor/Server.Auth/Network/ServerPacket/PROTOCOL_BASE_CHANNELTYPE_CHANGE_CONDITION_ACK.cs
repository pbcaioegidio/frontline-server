using Plugin.Core.Managers;
using Plugin.Core.Models;

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_CHANNELTYPE_CHANGE_CONDITION_ACK : AuthServerPacket
    {
        private const int Disabled = 0;
        private const int Enabled = 1;

        public override void Write()
        {
            WriteH((short)2490);

            int FirstType = ChannelTypeConditionManager.FirstChampionshipChannelType;
            int LastType = FirstType + ChannelTypeConditionManager.ChampionshipChannelTypeCount;

            for (int ChannelType = FirstType; ChannelType < LastType; ChannelType++)
            {
                ChannelTypeConditionRow Condition = ChannelTypeConditionManager.Get(ChannelType);

                WriteD(Condition.MinValue);
                WriteD(Condition.MaxValue);
                WriteD(Condition.Enabled ? Enabled : Disabled);
            }

            WriteC(0);
        }
    }
}
