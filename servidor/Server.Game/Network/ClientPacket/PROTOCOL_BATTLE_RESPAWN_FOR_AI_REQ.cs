// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BATTLE_RESPAWN_FOR_AI_REQ
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
    public class PROTOCOL_BATTLE_RESPAWN_FOR_AI_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read() => this.Field0 = this.ReadD();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null || room.State != RoomState.BATTLE || player.SlotId != room.Leader)
                    return;
                SlotModel slot = room.GetSlot(this.Field0);
                if (slot != null)
                {
                    slot.AiLevel = (int)room.IngameAiLevel;
                    slot.DeathState = DeadEnum.Alive;
                    ++room.SpawnsCount;
                }
                using (PROTOCOL_BATTLE_RESPAWN_FOR_AI_ACK Packet = new PROTOCOL_BATTLE_RESPAWN_FOR_AI_ACK(this.Field0))
                    room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_RESPAWN_FOR_AI_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}