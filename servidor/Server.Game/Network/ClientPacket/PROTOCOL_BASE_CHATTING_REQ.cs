// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BASE_CHATTING_REQ
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
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BASE_CHATTING_REQ : GameClientPacket
    {
        private string Field0;
        private ChattingType Field1;

        public override void Read()
        {
            this.Field1 = (ChattingType)this.ReadH();
            this.Field0 = this.ReadU((int)this.ReadH() * 2);
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null || string.IsNullOrEmpty(this.Field0) || this.Field0.Length > 60 || player.Nickname.Length == 0 || ComDiv.GetDuration(player.LastChatting) < 1.0)
                    return;

                if (player.IsBanned())
                {
                    Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK("Your account is banned."));
                    return;
                }
                if (player.IsMuted())
                {
                    Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK("You are muted and cannot chat."));
                    return;
                }
                RoomModel room = player.Room;
                switch (this.Field1)
                {
                    case ChattingType.All:
                    case ChattingType.Lobby:
                        if (room != null)
                        {
                            if (!AllUtils.ServerCommands(player, this.Field0))
                            {
                                SlotModel slot1 = room.Slots[player.SlotId];
                                using (PROTOCOL_ROOM_CHATTING_ACK protocolRoomChattingAck = new PROTOCOL_ROOM_CHATTING_ACK((int)this.Field1, slot1.Id, player.UseChatGM(), this.Field0))
                                {
                                    byte[] completeBytes = protocolRoomChattingAck.GetCompleteBytes("PROTOCOL_BASE_CHATTING_REQ-2");
                                    lock (room.Slots)
                                    {
                                        foreach (SlotModel slot2 in room.Slots)
                                        {
                                            Account playerBySlot = room.GetPlayerBySlot(slot2);
                                            if (playerBySlot != null && AllUtils.SlotValidMessage(slot1, slot2))
                                                playerBySlot.SendCompletePacket(completeBytes, protocolRoomChattingAck.GetType().Name);
                                        }
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                        ChannelModel channel = player.GetChannel();
                        if (channel == null)
                            return;
                        if (!AllUtils.ServerCommands(player, this.Field0))
                        {
                            using (PROTOCOL_LOBBY_CHATTING_ACK Packet = new PROTOCOL_LOBBY_CHATTING_ACK(player, this.Field0))
                            {
                                channel.SendPacketToWaitPlayers(Packet);
                                break;
                            }
                        }
                        break;
                    case ChattingType.Team:
                        if (room == null)
                            return;
                        SlotModel slot3 = room.Slots[player.SlotId];
                        int[] teamArray = room.GetTeamArray(slot3.Team);
                        using (PROTOCOL_ROOM_CHATTING_ACK protocolRoomChattingAck = new PROTOCOL_ROOM_CHATTING_ACK((int)this.Field1, slot3.Id, player.UseChatGM(), this.Field0))
                        {
                            byte[] completeBytes = protocolRoomChattingAck.GetCompleteBytes("PROTOCOL_BASE_CHATTING_REQ-1");
                            lock (room.Slots)
                            {
                                foreach (int index in teamArray)
                                {
                                    SlotModel slot4 = room.Slots[index];
                                    if (slot4 != null)
                                    {
                                        Account playerBySlot = room.GetPlayerBySlot(slot4);
                                        if (playerBySlot != null && AllUtils.SlotValidMessage(slot3, slot4))
                                            playerBySlot.SendCompletePacket(completeBytes, protocolRoomChattingAck.GetType().Name);
                                    }
                                }
                                break;
                            }
                        }
                    case ChattingType.Match:
                        // Clan-war team chat. The 122 client has no dedicated
                        // TEAM_CHATTING_REQ (6928) prototype, so it routes through
                        // the generic chat opcode; the reply is CLAN_WAR_TEAM_CHATTING_ACK
                        // (6929), whose hand-rolled reader lives at 0xEF1514.
                        if (player.Match == null)
                            return;
                        using (PROTOCOL_CLAN_WAR_TEAM_CHATTING_ACK Packet = new PROTOCOL_CLAN_WAR_TEAM_CHATTING_ACK(player.Nickname, this.Field0))
                            player.Match.SendPacketToPlayers(Packet);
                        break;
                }
                player.LastChatting = DateTimeUtil.Now();
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}