// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_ROOM_LOADING_START_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_LOADING_START_REQ : GameClientPacket
    {
        private string Field0;

        public override void Read() => this.Field0 = this.ReadS((int)this.ReadC());

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                SlotModel Slot;
                if (room != null && room.IsPreparing() && room.GetSlot(player.SlotId, out Slot) && Slot.State == SlotState.LOAD)
                {
                    Slot.PreLoadDate = DateTimeUtil.Now();
                    room.StartCounter(0, player, Slot);
                    room.ChangeSlotState(Slot, SlotState.RENDEZVOUS, true);
                    room.MapName = this.Field0;
                    if (Slot.Id == room.Leader)
                    {
                        room.UdpServer = SynchronizeXML.GetServer(ConfigLoader.DEFAULT_PORT[2]);
                        room.State = RoomState.RENDEZVOUS;
                        room.UpdateRoomInfo();
                    }
                }
                this.Client.SendPacket(new PROTOCOL_ROOM_LOADING_START_ACK(0U));
                if (room != null)
                {
                    for (int i = 0; i <= 17; i++)
                    {
                        SlotModel other = room.GetSlot(i);
                        if (other == null || other.Id == player.SlotId || other.Equipment == null)
                            continue;
                        Account otherPlayer;
                        if (!room.GetPlayerBySlot(other, out otherPlayer) || otherPlayer == null)
                            continue;
                        this.Client.SendPacket(new PROTOCOL_ROOM_GET_USER_EQUIPMENT_ACK(0U, room, other));
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_ROOM_LOADING_START_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}