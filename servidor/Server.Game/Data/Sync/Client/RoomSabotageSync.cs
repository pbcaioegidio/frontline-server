// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.RoomSabotageSync
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
    public class RoomSabotageSync
    {
        
        public static void Load(SyncClientPacket C)
        {
            int id = (int)C.ReadH();
            int num1 = (int)C.ReadH();
            int ServerId = (int)C.ReadH();
            byte SlotId = C.ReadC();
            ushort num2 = C.ReadUH();
            ushort num3 = C.ReadUH();
            int num4 = (int)C.ReadC();
            ushort num5 = C.ReadUH();
            if (C.ToArray().Length > 16 /*0x10*/)
                CLogger.Print($"Invalid Sabotage (Length > 16): {C.ToArray().Length}", LoggerType.Warning);
            int Id = num1;
            ChannelModel channel = ChannelsXML.GetChannel(ServerId, Id);
            if (channel == null)
                return;
            RoomModel room = channel.GetRoom(id);
            SlotModel Slot;
            if (room == null || room.RoundTime.IsTimer() || room.State != RoomState.BATTLE || !room.GetSlot((int)SlotId, out Slot))
                return;
            room.Bar1 = (int)num2;
            room.Bar2 = (int)num3;
            RoomCondition roomType = room.RoomType;
            int num6 = 0;
            switch (num4)
            {
                case 1:
                    Slot.DamageBar1 += num5;
                    num6 += (int)Slot.DamageBar1 / 600;
                    break;
                case 2:
                    Slot.DamageBar2 += num5;
                    num6 += (int)Slot.DamageBar2 / 600;
                    break;
            }
            Slot.EarnedEXP = num6;
            switch (roomType)
            {
                case RoomCondition.Destroy:
                    using (PROTOCOL_BATTLE_MISSION_GENERATOR_INFO_ACK Packet = new PROTOCOL_BATTLE_MISSION_GENERATOR_INFO_ACK(room))
                        room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
                    if (room.Bar1 == 0)
                    {
                        RoomSabotageSync.EndRound(room, !room.SwapRound ? TeamEnum.CT_TEAM : TeamEnum.FR_TEAM);
                        break;
                    }
                    if (room.Bar2 != 0)
                        break;
                    RoomSabotageSync.EndRound(room, !room.SwapRound ? TeamEnum.FR_TEAM : TeamEnum.CT_TEAM);
                    break;
                case RoomCondition.Defense:
                    using (PROTOCOL_BATTLE_MISSION_DEFENCE_INFO_ACK Packet = new PROTOCOL_BATTLE_MISSION_DEFENCE_INFO_ACK(room))
                        room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
                    if (room.Bar1 != 0 || room.Bar2 != 0)
                        break;
                    RoomSabotageSync.EndRound(room, !room.SwapRound ? TeamEnum.FR_TEAM : TeamEnum.CT_TEAM);
                    break;
            }
        }

        public static void EndRound(RoomModel room, TeamEnum winner)
        {
            switch (winner)
            {
                case TeamEnum.FR_TEAM:
                    ++room.FRRounds;
                    break;
                case TeamEnum.CT_TEAM:
                    ++room.CTRounds;
                    break;
            }
            AllUtils.BattleEndRound(room, winner, RoundEndType.Normal);
        }
    }
}