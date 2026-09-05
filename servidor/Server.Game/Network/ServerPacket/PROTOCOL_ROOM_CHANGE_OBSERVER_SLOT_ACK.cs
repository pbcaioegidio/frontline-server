namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_CHANGE_OBSERVER_SLOT_ACK : GameServerPacket
    {
        private readonly int SLotId;

        public PROTOCOL_ROOM_CHANGE_OBSERVER_SLOT_ACK(int SLotId) => this.SLotId = SLotId;

        public override void Write()
        {
            WriteH(3651);
            WriteD(SLotId);
        }
    }
}
