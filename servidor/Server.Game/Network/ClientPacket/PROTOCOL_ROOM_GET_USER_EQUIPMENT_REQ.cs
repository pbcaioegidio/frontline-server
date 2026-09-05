// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_ROOM_GET_USER_EQUIPMENT_REQ
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
    public class PROTOCOL_ROOM_GET_USER_EQUIPMENT_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read() => this.Field0 = (int)this.ReadC();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                SlotModel Slot;
                if (room == null || !room.GetSlot(this.Field0, out Slot) || player.SlotId == this.Field0)
                    return;
                this.Client.SendPacket(new PROTOCOL_ROOM_GET_USER_EQUIPMENT_ACK(0U, room, Slot));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_ROOM_GET_USER_EQUIPMENT_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}