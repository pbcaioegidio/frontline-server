// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_CS_CLIENT_ENTER_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.SQL;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_CLIENT_ENTER_REQ : GameClientPacket
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
                if (room != null)
                {
                    room.ChangeSlotState(player.SlotId, SlotState.CLAN, false);
                    room.StopCountDown(player.SlotId);
                    room.UpdateSlotsInfo();
                }
                int num = 0;
                ClanModel clan = ClanManager.GetClan(player.ClanId);
                if (player.ClanId == 0 && player.Nickname.Length > 0)
                    num = DaoManagerSQL.GetRequestClanId(player.PlayerId);
                this.Client.SendPacket(new PROTOCOL_CS_CLIENT_ENTER_ACK(num > 0 ? num : clan.Id, player.ClanAccess));
                this.Client.SendPacket(new PROTOCOL_CS_MEDAL_INFO_ACK());
                if (clan.Id <= 0 || num != 0)
                    return;
                this.Client.SendPacket(new PROTOCOL_CS_DETAIL_INFO_ACK(0, clan));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_CLIENT_ENTER_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}