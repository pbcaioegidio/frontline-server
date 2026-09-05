// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_ROOM_CHANGE_PASSWD_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_CHANGE_PASSWD_REQ : GameClientPacket
    {
        private string Field0;

        public override void Read() => this.Field0 = this.ReadS(4);

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null || room.Leader != player.SlotId || !(room.Password != this.Field0))
                    return;
                room.Password = this.Field0;
                using (PROTOCOL_ROOM_CHANGE_PASSWD_ACK Packet = new PROTOCOL_ROOM_CHANGE_PASSWD_ACK(this.Field0))
                    room.SendPacketToPlayers(Packet);
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
