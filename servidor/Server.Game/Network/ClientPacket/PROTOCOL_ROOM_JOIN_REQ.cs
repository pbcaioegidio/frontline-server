// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_ROOM_JOIN_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_JOIN_REQ : GameClientPacket
    {
        private int Field0;
        private int Field1;
        private string Field2;

        public override void Read()
        {
            this.Field0 = this.ReadD();
            this.Field2 = this.ReadS(4);
            this.Field1 = (int)this.ReadC();
            int num = (int)this.ReadC();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                ChannelModel Channel;
                if (player.Nickname.Length > 0 && player.Room == null && player.Match == null && player.GetChannel(out Channel))
                {
                    RoomModel room = Channel.GetRoom(this.Field0);
                    if (room != null && room.GetLeader() != null)
                    {
                        if (room.RoomType == RoomCondition.Tutorial)
                            this.Client.SendPacket(new PROTOCOL_ROOM_JOIN_ACK(2147487868U, (Account)null));
                        else if (room.Password.Length > 0 && this.Field2 != room.Password && player.Rank != 53 && !player.IsGM() && this.Field1 != 1)
                            this.Client.SendPacket(new PROTOCOL_ROOM_JOIN_ACK(2147487749U /*0x80001005*/, (Account)null));
                        else if (room.Limit == (byte)1 && room.State >= RoomState.COUNTDOWN && !player.IsGM())
                            this.Client.SendPacket(new PROTOCOL_ROOM_JOIN_ACK(2147487763U, (Account)null));
                        else if (room.KickedPlayersVote.Contains(player.PlayerId) && !player.IsGM())
                            this.Client.SendPacket(new PROTOCOL_ROOM_JOIN_ACK(2147487756U /*0x8000100C*/, (Account)null));
                        else if (room.KickedPlayersHost.ContainsKey(player.PlayerId) && ComDiv.GetDuration(room.KickedPlayersHost[player.PlayerId]) < (double)ConfigLoader.IntervalEnterRoomAfterKickSeconds)
                            this.Client.SendPacket(new PROTOCOL_SERVER_MESSAGE_ANNOUNCE_ACK(Translation.GetLabel("KickByHostMessage", (object)ConfigLoader.IntervalEnterRoomAfterKickSeconds, (object)(int)ComDiv.GetDuration(room.KickedPlayersHost[player.PlayerId]))));
                        else if (room.AddPlayer(player) >= 0)
                        {
                            player.ResetPages();
                            using (PROTOCOL_ROOM_GET_SLOTONEINFO_ACK Packet = new PROTOCOL_ROOM_GET_SLOTONEINFO_ACK(player))
                                room.SendPacketToPlayers(Packet, player.PlayerId);
                            if (room.Competitive)
                                AllUtils.SendCompetitiveInfo(player);
                            this.Client.SendPacket(new PROTOCOL_ROOM_JOIN_ACK(0U, player));
                        }
                        else
                            this.Client.SendPacket(new PROTOCOL_ROOM_JOIN_ACK(2147487747U /*0x80001003*/, (Account)null));
                    }
                    else
                        this.Client.SendPacket(new PROTOCOL_ROOM_JOIN_ACK(2147487748U /*0x80001004*/, (Account)null));
                }
                else
                    this.Client.SendPacket(new PROTOCOL_ROOM_JOIN_ACK(2147487748U /*0x80001004*/, (Account)null));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_LOBBY_JOIN_ROOM_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}