using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_MISSION_ROUND_START_ACK : GameServerPacket
    {
        private readonly RoomModel roomModel;

        public PROTOCOL_BATTLE_MISSION_ROUND_START_ACK(RoomModel room) => roomModel = room;

        public override void Write()
        {
            WriteH((short)5153);
            WriteC((byte)roomModel.Rounds);
            WriteD(roomModel.GetInBattleTimeLeft());
            WriteD(AllUtils.GetSlotsFlag(roomModel, true, false));
            byte swapFlag = 0;
            if (roomModel.Flag.HasFlag(RoomStageFlag.TEAM_SWAP))
            {
                int swapPoint = roomModel.GetSwapRoundByMask();
                int currentRounds = roomModel.FRRounds + roomModel.CTRounds;

                // Solo en el momento exacto del swap y si aún no se activó
                if (currentRounds == swapPoint && !roomModel.SwapRound)
                {
                    swapFlag = 3;
                }
            }

            WriteC(swapFlag);
            WriteH((ushort)roomModel.FRRounds);
            WriteH((ushort)roomModel.CTRounds);
            WriteB(SwapData(roomModel));
        }

        private byte[] SwapData(RoomModel roomModel)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                bool useSwappedChars = roomModel.SwapRound;

                foreach (SlotModel slot in roomModel.Slots)
                {
                    PlayerEquipment equipment = slot.Equipment;

                    if (equipment != null)
                    {
                        if (slot.Team == TeamEnum.FR_TEAM)
                        {
                            syncServerPacket.WriteD(useSwappedChars == true ? equipment.CharaRedId : equipment.CharaBlueId);
                        }
                        else if (slot.Team == TeamEnum.CT_TEAM)
                        {
                            syncServerPacket.WriteD(useSwappedChars == true ? equipment.CharaBlueId : equipment.CharaRedId);
                        }
                        else
                        {
                            if (slot.Team == TeamEnum.FR_TEAM)
                            {
                                syncServerPacket.WriteD(useSwappedChars == true ? 601001 : 602002);
                            }
                            else if (slot.Team == TeamEnum.CT_TEAM)
                            {
                                syncServerPacket.WriteD(useSwappedChars == true ? 602002 : 601001);
                            }
                        }
                    }
                    else
                    {
                        // Personajes por defecto
                        if (slot.Team == TeamEnum.FR_TEAM)
                        {
                            syncServerPacket.WriteD(useSwappedChars == true ? 601001 : 602002);
                        }
                        else if (slot.Team == TeamEnum.CT_TEAM)
                        {
                            syncServerPacket.WriteD(useSwappedChars == true ? 602002 : 601001);
                        }
                    }
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}