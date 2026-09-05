using Plugin.Core.Models;
using Server.Game.Data.Models;
using System.Collections.Generic;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BASE_GET_CHANNELLIST_ACK : GameServerPacket
    {
        private readonly SChannelModel SChannel;
        private readonly List<ChannelModel> Channels;

        public PROTOCOL_BASE_GET_CHANNELLIST_ACK(SChannelModel SChannel, List<ChannelModel> Channels)
        {
            this.SChannel = SChannel;
            this.Channels = Channels;
        }

        public override void Write()
        {
            this.WriteH((short)2333);
            this.WriteD(0);
            this.WriteD(1);
            this.WriteD(0);
            this.WriteH((short)0);
            this.WriteC((byte)0);
            this.WriteC((byte)this.Channels.Count);
            foreach (ChannelModel channelModel in this.Channels)
                this.WriteH((ushort)channelModel.Players.Count);
            this.WriteH((short)310);
            this.WriteH((ushort)this.SChannel.ChannelPlayers);
            this.WriteC((byte)this.Channels.Count);
            this.WriteC((byte)0);
        }
    }
}
