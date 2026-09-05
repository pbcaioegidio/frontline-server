using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_GET_SLOTINFO_ACK : GameServerPacket
    {
        private readonly RoomModel roomModel;

        public PROTOCOL_ROOM_GET_SLOTINFO_ACK(RoomModel room)
        {
            roomModel = room.GetRoom();
            if (room != null && room.GetLeader() == null)
            {
                room.SetNewLeader(-1, SlotState.EMPTY, room.Leader, false);
            }
        }

        public override void Write()
        {
            WriteH((short)3595);
            WriteC((byte)roomModel.Leader);
            WriteB(SlotInfoData(roomModel));
            WriteB(RoomSlotData(roomModel));
        }

        private byte[] SlotInfoData(RoomModel room)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                foreach (SlotModel slot in room.Slots)
                {
                    syncServerPacket.WriteH((short)36);
                    syncServerPacket.WriteC(slot.GetClientState());
                    Account playerBySlot = room.GetPlayerBySlot(slot);
                    if (playerBySlot == null)
                    {
                        syncServerPacket.WriteB(new byte[10]);
                        syncServerPacket.WriteD(uint.MaxValue);
                        syncServerPacket.WriteB(new byte[22]);
                    }
                    else
                    {
                        ClanModel clan = ClanManager.GetClan(playerBySlot.ClanId);
                        syncServerPacket.WriteC((byte)playerBySlot.GetRank());
                        syncServerPacket.WriteD(clan.Id);
                        syncServerPacket.WriteD(playerBySlot.ClanAccess);
                        syncServerPacket.WriteC((byte)clan.Rank);
                        syncServerPacket.WriteD(clan.Logo);
                        syncServerPacket.WriteC((byte)playerBySlot.CafePC);
                        syncServerPacket.WriteC((byte)playerBySlot.TourneyLevel());
                        syncServerPacket.WriteQ((long)playerBySlot.Effects);
                        syncServerPacket.WriteC((byte)clan.Effect);
                        syncServerPacket.WriteC((byte)slot.ViewType);
                        syncServerPacket.WriteC((byte)NATIONS);
                        syncServerPacket.WriteC((byte)0);
                        syncServerPacket.WriteD(playerBySlot.Equipment.NameCardId);
                        syncServerPacket.WriteC((byte)playerBySlot.Bonus.NickBorderColor);
                        syncServerPacket.WriteC((byte)playerBySlot.CountryFlags);
                        syncServerPacket.WriteC(0);
                        syncServerPacket.WriteC((byte)(clan.Name.Length * 2));
                        syncServerPacket.WriteU(clan.Name, clan.Name.Length * 2);
                    }
                }
                return syncServerPacket.ToArray();
            }
        }

        private byte[] RoomSlotData(RoomModel room)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                foreach (SlotModel slot in room.Slots)
                {
                    syncServerPacket.WriteC((byte)room.ValidateTeam(slot.Team, slot.CostumeTeam));
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}