// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.RoomPing
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Data.XML;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Runtime.CompilerServices;

namespace Server.Game.Data.Sync.Client
{
    public static class RoomPing
    {
        public static void Load(SyncClientPacket C)
        {
            int id = (int)C.ReadH();
            int num1 = (int)C.ReadH();
            int ServerId = (int)C.ReadH();
            int SlotIdx = (int)C.ReadC();
            int num2 = (int)C.ReadC();
            int num3 = (int)C.ReadUH();
            if (C.ToArray().Length > 12)
                CLogger.Print($"Invalid Ping (Length > 12): {C.ToArray().Length}", LoggerType.Warning);
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
            Account playerBySlot = room.GetPlayerBySlot(slot);
            if (room.IsBotMode() || playerBySlot == null)
                return;
            slot.Latency = num3;
            slot.Ping = num2;
            if (slot.Latency >= ConfigLoader.MaxLatency)
                ++slot.FailLatencyTimes;
            else
                slot.FailLatencyTimes = 0;
            if (ConfigLoader.IsDebugPing && ComDiv.GetDuration(playerBySlot.LastPingDebug) >= (double)ConfigLoader.PingUpdateTimeSeconds)
            {
                playerBySlot.LastPingDebug = DateTimeUtil.Now();
                playerBySlot.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK("Server", 0, 5, false, $"{num3}ms ({num2} bar)"));
            }
            if (slot.FailLatencyTimes < ConfigLoader.MaxRepeatLatency)
            {
                AllUtils.RoomPingSync(room);
            }
            else
            {
                CLogger.Print($"Player: '{playerBySlot.Nickname}' (Id: {slot.PlayerId}) kicked due to high latency. ({slot.Latency}/{ConfigLoader.MaxLatency}ms)", LoggerType.Warning);
                playerBySlot.Connection.Close(500, true);
            }
        }
    }
}