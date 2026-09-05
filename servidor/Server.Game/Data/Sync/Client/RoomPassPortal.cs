using System;
using System.Runtime.CompilerServices;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Data.XML;
using Server.Game.Network.ServerPacket;

namespace Server.Game.Data.Sync.Client
{
    // Token: 0x02000200 RID: 512
    public static class RoomPassPortal
    {
        // Token: 0x0600060D RID: 1549 RVA: 0x00032EA8 File Offset: 0x000310A8

        public static void Load(SyncClientPacket C)
        {
            int id = (int)C.ReadH();
            int id2 = (int)C.ReadH();
            int serverId = (int)C.ReadH();
            int slotIdx = (int)C.ReadC();
            C.ReadC();
            if (C.ToArray().Length > 10)
            {
                CLogger.Print(string.Format("Invalid Portal (Length > 10): {0}", C.ToArray().Length), LoggerType.Warning, null);
            }
            ChannelModel channel = ChannelsXML.GetChannel(serverId, id2);
            if (channel != null)
            {
                RoomModel room = channel.GetRoom(id);
                if (room != null && !room.RoundTime.IsTimer() && room.State == RoomState.BATTLE && room.IsDinoMode("DE"))
                {
                    SlotModel slot = room.GetSlot(slotIdx);
                    if (slot != null && slot.State == SlotState.BATTLE)
                    {
                        slot.PassSequence++;
                        if (slot.Team == TeamEnum.FR_TEAM)
                        {
                            room.FRDino += 5;
                        }
                        else
                        {
                            room.CTDino += 5;
                        }
                        RoomPassPortal.StaticMethod0(room, slot);
                        using (PROTOCOL_BATTLE_MISSION_TOUCHDOWN_ACK protocol_BATTLE_MISSION_TOUCHDOWN_ACK = new PROTOCOL_BATTLE_MISSION_TOUCHDOWN_ACK(room, slot))
                        {
                            using (PROTOCOL_BATTLE_MISSION_TOUCHDOWN_COUNT_ACK protocol_BATTLE_MISSION_TOUCHDOWN_COUNT_ACK = new PROTOCOL_BATTLE_MISSION_TOUCHDOWN_COUNT_ACK(room))
                            {
                                room.SendPacketToPlayers(protocol_BATTLE_MISSION_TOUCHDOWN_ACK, protocol_BATTLE_MISSION_TOUCHDOWN_COUNT_ACK, SlotState.BATTLE, 0);
                            }
                        }
                    }
                }
                return;
            }
        }

        // Token: 0x0600060E RID: 1550 RVA: 0x00033004 File Offset: 0x00031204
        private static void StaticMethod0(RoomModel A_0, SlotModel A_1)
        {
            MissionType missionType = MissionType.NA;
            if (A_1.PassSequence == 1)
            {
                missionType = MissionType.TOUCHDOWN;
            }
            else if (A_1.PassSequence != 2)
            {
                if (A_1.PassSequence == 3)
                {
                    missionType = MissionType.TOUCHDOWN_HATTRICK;
                }
                else if (A_1.PassSequence >= 4)
                {
                    missionType = MissionType.TOUCHDOWN_GAME_MAKER;
                }
            }
            else
            {
                missionType = MissionType.TOUCHDOWN_ACE_ATTACKER;
            }
            if (missionType != MissionType.NA)
            {
                AllUtils.CompleteMission(A_0, A_1, missionType, 0);
            }
        }
    }
}
