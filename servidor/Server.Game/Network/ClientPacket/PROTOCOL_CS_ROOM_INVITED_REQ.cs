// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_ROOM_INVITED_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_ROOM_INVITED_REQ : GameClientPacket
    {
        private long Field0;

        public override void Read() => this.Field0 = this.ReadQ();

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || player.ClanId == 0)
                    return;
                RoomModel room = player.Room;
                if (room == null)
                    return;
                Account account = AccountManager.GetAccount(this.Field0, 31 /*0x1F*/);
                if (account != null && account.ClanId == player.ClanId)
                    account.SendPacket(new PROTOCOL_SERVER_MESSAGE_INVITED_ACK(player, room), false);
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
