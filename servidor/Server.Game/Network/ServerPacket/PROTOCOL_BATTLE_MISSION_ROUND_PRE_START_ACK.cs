using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_MISSION_ROUND_PRE_START_ACK : GameServerPacket
    {
        private readonly RoomModel roomModel;
        private readonly List<int> list;

        public PROTOCOL_BATTLE_MISSION_ROUND_PRE_START_ACK(RoomModel roomModel, List<int> list)
        {
            this.roomModel = roomModel;
            this.list = list;
        }

        public override void Write()
        {
            WriteH(5151);
            WriteD(AllUtils.GetSlotsFlag(roomModel, false, false));
            WriteB(DinoData(roomModel, list));
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
            // Enviar datos de swap si está activo
            if (roomModel.Flag.HasFlag(RoomStageFlag.TEAM_SWAP))
                WriteB(SwapData(roomModel));
        }

        private byte[] DinoData(RoomModel A_1, List<int> A_2)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                if (A_1.IsBotMode())
                    syncServerPacket.WriteB(Bitwise.HexStringToByteArray("FF FF FF FF FF FF FF FF FF FF"));
                else if (!A_1.IsDinoMode())
                {
                    syncServerPacket.WriteB(new byte[10]);
                }
                else
                {
                    int num1 = A_2.Count == 1 || A_1.IsDinoMode("CC") ? (int)byte.MaxValue : A_1.TRex;
                    syncServerPacket.WriteC((byte)num1);
                    syncServerPacket.WriteC((byte)10);
                    for (int index = 0; index < A_2.Count; ++index)
                    {
                        int num2 = A_2[index];
                        if (num2 != A_1.TRex && A_1.IsDinoMode("DE") || A_1.IsDinoMode("CC"))
                            syncServerPacket.WriteC((byte)num2);
                    }
                    int num3 = 8 - A_2.Count - (num1 == (int)byte.MaxValue ? 1 : 0);
                    for (int index = 0; index < num3; ++index)
                        syncServerPacket.WriteC(byte.MaxValue);
                    syncServerPacket.WriteC(byte.MaxValue);
                }
                return syncServerPacket.ToArray();
            }
        }

        private byte[] SwapData(RoomModel roomModel)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                // Usar SwapRound directamente para determinar qué personaje usar
                bool useSwappedChars = roomModel.SwapRound;

                foreach (SlotModel slot in roomModel.Slots)
                {
                    PlayerEquipment equipment = slot.Equipment;

                    if (equipment != null)
                    {
                        // Si ya se hizo swap, intercambiar personajes
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