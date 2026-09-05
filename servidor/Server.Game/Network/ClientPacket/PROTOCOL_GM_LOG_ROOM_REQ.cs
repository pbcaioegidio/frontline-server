// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_GM_LOG_ROOM_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_GM_LOG_ROOM_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read() => this.Field0 = (int)this.ReadC();

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || !player.IsGM())
                    return;
                RoomModel room = player.Room;
                Account Player;
                if (room == null || !room.GetPlayerBySlot(this.Field0, out Player))
                    return;
                this.Client.SendPacket(new PROTOCOL_GM_LOG_ROOM_ACK(0U, Player));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_GM_LOG_ROOM_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}