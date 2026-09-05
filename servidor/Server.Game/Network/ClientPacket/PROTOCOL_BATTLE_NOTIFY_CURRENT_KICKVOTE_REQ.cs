using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_NOTIFY_CURRENT_KICKVOTE_REQ : GameClientPacket
    {
        private byte Field0;

        public override void Read() => this.Field0 = this.ReadC();

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                SlotModel Slot;
                if (room == null || room.State != RoomState.BATTLE || !room.VoteTime.IsTimer() || room.VoteKick == null || !room.GetSlot(player.SlotId, out Slot) || Slot.State != SlotState.BATTLE)
                    return;
                VoteKickModel voteKick = room.VoteKick;
                if (!voteKick.Votes.Contains(player.SlotId))
                {
                    lock (voteKick.Votes)
                        voteKick.Votes.Add(Slot.Id);
                    if (this.Field0 == (byte)0)
                    {
                        ++voteKick.Accept;
                        if (Slot.Team != (TeamEnum)(voteKick.VictimIdx % 2))
                            ++voteKick.Enemies;
                        else
                            ++voteKick.Allies;
                    }
                    else
                        ++voteKick.Denie;
                    if (voteKick.Votes.Count >= voteKick.GetInGamePlayers())
                    {
                        room.VoteTime.StopJob();
                        AllUtils.VotekickResult(room);
                    }
                    else
                    {
                        using (PROTOCOL_BATTLE_NOTIFY_CURRENT_KICKVOTE_ACK Packet = new PROTOCOL_BATTLE_NOTIFY_CURRENT_KICKVOTE_ACK(voteKick))
                            room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
                    }
                }
                else
                    this.Client.SendPacket(new PROTOCOL_BATTLE_VOTE_KICKVOTE_ACK(0x80001211U));
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_NOTIFY_CURRENT_KICKVOTE_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
