// Decompiled with JetBrains decompiler
namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_VOTE_KICKVOTE_ACK : GameServerPacket
    {
        private readonly uint Field0;

        public PROTOCOL_BATTLE_VOTE_KICKVOTE_ACK(uint A_1) => this.Field0 = A_1;

        public override void Write()
        {
            this.WriteH((short)5195);
            this.WriteD(this.Field0);
        }
    }
}