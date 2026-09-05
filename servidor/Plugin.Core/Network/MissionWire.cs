using Plugin.Core.Models;
using Plugin.Core.Utility;

namespace Plugin.Core.Network
{
    public static class MissionWire
    {
        // The single definition of the 89-byte QUESTING_INFO block:
        //   [activeSlot:u8][4x currentCard:u8][4x 20B flags][4x cardSetId:u8]
        // consumed by MCardMgr__ParseAndSetQuestCompletion (dump122 @0xc58f0a).
        public static byte[] BuildQuestingInfo(PlayerMissions mission)
        {
            using (SyncServerPacket packet = new SyncServerPacket(89L))
            {
                packet.WriteC((byte)mission.ActualMission);
                for (int i = 0; i < PlayerMissions.SlotCount; i++)
                    packet.WriteC((byte)mission[i].CurrentCard);
                for (int i = 0; i < PlayerMissions.SlotCount; i++)
                    packet.WriteB(ComDiv.GetMissionCardFlags(mission[i].CardSetId, mission[i].Progress));
                for (int i = 0; i < PlayerMissions.SlotCount; i++)
                    packet.WriteC((byte)mission[i].CardSetId);
                return packet.ToArray();
            }
        }
    }
}
