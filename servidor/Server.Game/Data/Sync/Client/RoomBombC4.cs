// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.RoomBombC4
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Data.XML;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Runtime.CompilerServices;

namespace Server.Game.Data.Sync.Client
{
    public static class RoomBombC4
    {
        public static void Load(SyncClientPacket C)
        {
            int id = (int)C.ReadH();
            int num1 = (int)C.ReadH();
            int ServerId = (int)C.ReadH();
            int num2 = (int)C.ReadC();
            int SlotIdx = (int)C.ReadC();
            byte Zone = 0;
            ushort Unk = 0;
            float X = 0.0f;
            float Y = 0.0f;
            float Z = 0.0f;
            switch (num2)
            {
                case 0:
                    Zone = C.ReadC();
                    X = C.ReadT();
                    Y = C.ReadT();
                    Z = C.ReadT();
                    Unk = C.ReadUH();
                    if (C.ToArray().Length > 25)
                    {
                        CLogger.Print($"Invalid Bomb (Length > 25): {C.ToArray().Length}", LoggerType.Warning);
                        break;
                    }
                    break;

                case 1:
                    if (C.ToArray().Length > 10)
                    {
                        CLogger.Print($"Invalid Bomb Type[1] (Length > 10): {C.ToArray().Length}", LoggerType.Warning);
                        break;
                    }
                    break;
            }
            int Id = num1;
            ChannelModel channel = ChannelsXML.GetChannel(ServerId, Id);
            if (channel == null)
                return;
            RoomModel room = channel.GetRoom(id);
            if (room == null || room.RoundTime.IsTimer() || room.State != RoomState.BATTLE)
                return;
            SlotModel slot = room.GetSlot(SlotIdx);
            if (slot == null || slot.State != SlotState.BATTLE)
                return;
            if (num2 == 0)
            {
                RoomBombC4.InstallBomb(room, slot, Zone, Unk, X, Y, Z);
            }
            else
            {
                if (num2 != 1)
                    return;
                RoomBombC4.UninstallBomb(room, slot);
            }
        }

        public static void InstallBomb(RoomModel Room, SlotModel Slot, byte Zone, ushort Unk, float X, float Y, float Z)
        { 
            if (Room.ActiveC4)
                return;
            using (PROTOCOL_BATTLE_MISSION_BOMB_INSTALL_ACK Packet = new PROTOCOL_BATTLE_MISSION_BOMB_INSTALL_ACK(Slot.Id, Zone, Unk, X, Y, Z))
            {
                Room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
            }
            if (Room.RoomType != RoomCondition.Tutorial)
            {
                Room.ActiveC4 = true;
                ++Slot.Objects;
                AllUtils.CompleteMission(Room, Slot, MissionType.C4_PLANT, 0);
                Room.StartBomb();
            }
            else
            {
                Room.ActiveC4 = true;
            }
        }

        public static void UninstallBomb(RoomModel Room, SlotModel Slot)
        {
            if (!Room.ActiveC4)
            {
                return;
            }
            using (PROTOCOL_BATTLE_MISSION_BOMB_UNINSTALL_ACK Packet = new PROTOCOL_BATTLE_MISSION_BOMB_UNINSTALL_ACK(Slot.Id))
            {
                Room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
            }
            if (Room.RoomType != RoomCondition.Tutorial)
            {
                Slot.Objects++;
                if (Room.SwapRound)
                {
                    Room.FRRounds++;
                }
                else
                {
                    Room.CTRounds++;
                }

                AllUtils.CompleteMission(Room, Slot, MissionType.C4_DEFUSE, 0);
                AllUtils.BattleEndRound(Room, (Room.SwapRound == true ? TeamEnum.FR_TEAM : TeamEnum.CT_TEAM), RoundEndType.Uninstall);
            }
            else
            {
                Room.ActiveC4 = false;
            }
        }
    }
}