// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BATTLE_START_KICKVOTE_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_START_KICKVOTE_REQ : GameClientPacket
    {
        private int Field0;
        private int Field1;
        private uint Field2;

        public override void Read()
        {
            this.Field1 = (int)this.ReadC();
            this.Field0 = (int)this.ReadC();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null || room.State != RoomState.BATTLE || player.SlotId == this.Field1)
                    return;
                SlotModel slot = room.GetSlot(player.SlotId);
                if (slot == null || slot.State != SlotState.BATTLE || room.GetSlot(this.Field1).State != SlotState.BATTLE)
                    return;
                room.GetPlayingPlayers(true, out int _, out int _);
                if (player.Rank < ConfigLoader.MinRankVote && !player.IsGM())
                    this.Field2 = 0x80001205U;
                else if (!room.VoteTime.IsTimer())
                {
                    if (slot.NextVoteDate > DateTimeUtil.Now())
                        this.Field2 = 0x80001201U;
                }
                else
                    this.Field2 = 0x80001200U;
                this.Client.SendPacket(new PROTOCOL_BATTLE_SUGGEST_KICKVOTE_ACK(this.Field2));
                if (this.Field2 > 0U)
                    return;
                slot.NextVoteDate = DateTimeUtil.Now().AddMinutes(1.0);
                VoteKickModel voteKickModel = new VoteKickModel(slot.Id, this.Field1)
                {
                    Motive = this.Field0
                };
                room.VoteKick = voteKickModel;
                for (int index = 0; index < 18; ++index)
                    room.VoteKick.TotalArray[index] = room.Slots[index].State == SlotState.BATTLE;
                using (PROTOCOL_BATTLE_START_KICKVOTE_ACK Packet = new PROTOCOL_BATTLE_START_KICKVOTE_ACK(room.VoteKick))
                    room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0, player.SlotId, this.Field1);
                room.StartVote();
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_START_KICKVOTE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
