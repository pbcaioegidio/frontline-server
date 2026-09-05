// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_GET_ROOM_USER_DETAIL_INFO_REQ
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
    public class PROTOCOL_BASE_GET_ROOM_USER_DETAIL_INFO_REQ : GameClientPacket
    {
        private int slot;

        public override void Read() => slot = ReadD();

        public override void Run()
        {
            try
            {
                Account player = Client.Player;
                if (player != null)
                {
                    RoomModel room = player.Room;
                    SlotModel slotModel;

                    if (room != null && room.GetSlot(slot, out slotModel))
                    {
                        Account playerBySlot = room.GetPlayerBySlot(slotModel);
                        if (playerBySlot != null)
                        {
                            if (player.Nickname != playerBySlot.Nickname)
                            {
                                player.FindPlayer = playerBySlot.Nickname;
                            }
                            int num = int.MaxValue;
                            TeamEnum teamEnum = room.ValidateTeam(slotModel.Team, slotModel.CostumeTeam);
                            if (teamEnum == TeamEnum.FR_TEAM)
                            {
                                num = slotModel.Equipment.CharaRedId;
                            }
                            else if (teamEnum == TeamEnum.CT_TEAM)
                            {
                                num = slotModel.Equipment.CharaBlueId;
                            }
                            Client.SendPacket(new PROTOCOL_BASE_GET_USER_DETAIL_INFO_ACK(0U, playerBySlot, num));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BASE_GET_ROOM_USER_DETAIL_INFO_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}