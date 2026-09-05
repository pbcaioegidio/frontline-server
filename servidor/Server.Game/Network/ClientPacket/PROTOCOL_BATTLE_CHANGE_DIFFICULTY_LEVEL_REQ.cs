// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BATTLE_CHANGE_DIFFICULTY_LEVEL_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_CHANGE_DIFFICULTY_LEVEL_REQ : GameClientPacket
    {
        public override void Read()
        {
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null || room.State != RoomState.BATTLE || room.IngameAiLevel >= (byte)10)
                    return;
                SlotModel slot = room.GetSlot(player.SlotId);
                if (slot == null || slot.State != SlotState.BATTLE)
                    return;
                if (room.IngameAiLevel <= (byte)9)
                    ++room.IngameAiLevel;
                using (PROTOCOL_BATTLE_CHANGE_DIFFICULTY_LEVEL_ACK Packet = new PROTOCOL_BATTLE_CHANGE_DIFFICULTY_LEVEL_ACK(room))
                    room.SendPacketToPlayers(Packet, SlotState.READY, 1);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_CHANGE_DIFFICULTY_LEVEL_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}