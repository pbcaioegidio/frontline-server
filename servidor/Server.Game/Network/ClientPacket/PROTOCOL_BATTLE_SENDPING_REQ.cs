// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BATTLE_SENDPING_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_SENDPING_REQ : GameClientPacket
    {
        private byte[] Field0;

        public override void Read() => this.Field0 = this.ReadB(16 /*0x10*/);

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                SlotModel Slot;
                if (room == null || !room.GetSlot(player.SlotId, out Slot))
                    return;
                int num = 0;
                if (Slot == null || Slot.State < SlotState.BATTLE_READY)
                    return;
                if (room.State == RoomState.BATTLE)
                    room.Ping = (int)this.Field0[room.Leader];
                using (PROTOCOL_BATTLE_SENDPING_ACK battleSendpingAck = new PROTOCOL_BATTLE_SENDPING_ACK(this.Field0))
                {
                    List<Account> allPlayers = room.GetAllPlayers(SlotState.READY, 1);
                    if (allPlayers.Count == 0)
                        return;
                    byte[] completeBytes = battleSendpingAck.GetCompleteBytes(this.GetType().Name);
                    foreach (Account account in allPlayers)
                    {
                        SlotModel slot = room.GetSlot(account.SlotId);
                        if (slot != null && slot.State >= SlotState.BATTLE_READY)
                            account.SendCompletePacket(completeBytes, battleSendpingAck.GetType().Name);
                        else
                            ++num;
                    }
                }
                if (num != 0)
                    return;
                room.SpawnReadyPlayers();
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_SENDPING_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}