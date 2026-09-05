// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_ROOM_TOTAL_TEAM_CHANGE_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_TOTAL_TEAM_CHANGE_REQ : GameClientPacket
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
                if (room == null || room.Leader != player.SlotId || room.State != RoomState.READY || ComDiv.GetDuration(room.LastChangeTeam) < 1.5 || room.ChangingSlots)
                    return;
                List<SlotModel> slotModelList = new List<SlotModel>();
                lock (room.Slots)
                {
                    room.ChangingSlots = true;
                    foreach (int OldSlotId in room.FR_TEAM)
                    {
                        int NewSlotId = OldSlotId + 1;
                        if (OldSlotId != room.Leader)
                        {
                            if (NewSlotId == room.Leader)
                                room.Leader = OldSlotId;
                        }
                        else
                            room.Leader = NewSlotId;
                        room.SwitchSlots(slotModelList, NewSlotId, OldSlotId, true);
                    }
                    if (slotModelList.Count > 0)
                    {
                        using (PROTOCOL_ROOM_TEAM_BALANCE_ACK roomTeamBalanceAck = new PROTOCOL_ROOM_TEAM_BALANCE_ACK(slotModelList, room.Leader, 2))
                        {
                            byte[] completeBytes = roomTeamBalanceAck.GetCompleteBytes("PROTOCOL_ROOM_CHANGE_TEAM_REQ");
                            foreach (Account allPlayer in room.GetAllPlayers())
                            {
                                allPlayer.SlotId = AllUtils.GetNewSlotId(allPlayer.SlotId);
                                allPlayer.SendCompletePacket(completeBytes, roomTeamBalanceAck.GetType().Name);
                            }
                        }
                    }
                    room.ChangingSlots = false;
                }
                room.LastChangeTeam = DateTimeUtil.Now();
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_ROOM_CHANGE_TEAM_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}