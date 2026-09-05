// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_ROOM_INFO_ENTER_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_INFO_ENTER_REQ : GameClientPacket
    {
        public override void Read()
        {
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || ComDiv.GetDuration(player.LastProfileEnter) < 1.0)
                    return;
                RoomModel room = player.Room;
                if (room != null)
                {
                    room.ChangeSlotState(player.SlotId, SlotState.INFO, false);
                    room.StopCountDown(player.SlotId);
                    room.UpdateSlotsInfo();
                }
                this.Client.SendPacket(new PROTOCOL_ROOM_INFO_ENTER_ACK());
                player.LastProfileEnter = DateTimeUtil.Now();
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}