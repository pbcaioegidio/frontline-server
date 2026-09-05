using Plugin.Core.Models;
using Plugin.Core.Network;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using System;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_JOIN_ACK : GameServerPacket
    {
        private readonly uint Error;
        private readonly RoomModel roomModel;
        private readonly int SlotId;

        public PROTOCOL_ROOM_JOIN_ACK(uint error, Account account)
        {
            Error = error;
            if (account != null)
            {
                SlotId = account.SlotId;
                roomModel = account.GetRoom();
            }
        }

        // 121 client reads 3586 as an S2MO node list (sub_EBC28A). Wire order = reverse of the
        // prepend push order: status, teams[], slots[], KillCam, balB, balA, slotCount, leader,
        // countdown, randmaps[], optblk(175), basic(71), substatus.
        public override void Write()
        {
            WriteH((short)3586);
            WriteH((short)0);
            WriteD(Error); // leading status DWORD (vf3 reads first; <0 = abort)
            if (Error != 0U)
                return;
            lock (roomModel.Slots)
            {
                WriteB(Method0(roomModel)); // node748 teams[]: count + Team per slot
                WriteB(Method1(roomModel)); // node106 slots[]: count + 142B record per slot

                WriteC((byte)roomModel.KillCam);                      // node102
                WriteC((byte)0);                                      // node98  PVP balance B (value TBD)
                WriteC((byte)0);                                      // node94  PVP balance A (value TBD)
                WriteC((byte)roomModel.GetSlotCount());               // node90  SLOT COUNT (client asserts 1..18)
                WriteC((byte)roomModel.Leader);                       // node86  main slot idx
                WriteC((byte)roomModel.CountdownTime.GetTimeLeft());  // node82  countdown

                WriteC((byte)4);                                      // node78 randmaps[]: count
                WriteB(new byte[4]);                                  // node78 randmaps[] entries (value TBD)

                // node31 optblk(175) then node10 basic(71) — identical bytes to PROTOCOL_ROOM_CREATE_ACK,
                // but emitted in the opposite (optblk-first) order the join node list expects.
                byte[] blob = BuildRoomBlob(roomModel); // [0..71)=basic, [71..246)=optblk
                byte[] basic = new byte[71];
                byte[] optblk = new byte[blob.Length - 71];
                Array.Copy(blob, 0, basic, 0, 71);
                Array.Copy(blob, 71, optblk, 0, optblk.Length);
                WriteB(optblk);
                WriteB(basic);

                WriteC((byte)SlotId); // node6 = MySlotIdx do joiner (client le em SetMySlotIdx)
            }
        }

        // teams[] (node748, S2MOValue<uchar,18>): 1-byte count then one Team byte per slot.
        private byte[] Method0(RoomModel A_1)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)A_1.Slots.Length);
                foreach (SlotModel slot in A_1.Slots)
                    syncServerPacket.WriteC((byte)slot.Team);
                return syncServerPacket.ToArray();
            }
        }

        // slots[] (node106): 1-byte count then a 142-byte record per slot.
        private byte[] Method1(RoomModel room)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)room.Slots.Length);
                foreach (SlotModel slot in room.Slots)
                {
                    syncServerPacket.WriteC(slot.GetClientState()); // rec+0
                    Account playerBySlot = room.GetPlayerBySlot(slot);
                    if (playerBySlot != null)
                    {
                        ClanModel clan = ClanManager.GetClan(playerBySlot.ClanId);
                        syncServerPacket.WriteC((byte)playerBySlot.GetRank());          // rec+1
                        syncServerPacket.WriteD(clan.Id);                               // rec+2
                        syncServerPacket.WriteD(playerBySlot.ClanAccess);               // rec+6
                        syncServerPacket.WriteC((byte)clan.Rank);                       // rec+10
                        syncServerPacket.WriteD(clan.Logo);                             // rec+11
                        syncServerPacket.WriteC((byte)playerBySlot.CafePC);             // rec+15
                        syncServerPacket.WriteC((byte)playerBySlot.TourneyLevel());     // rec+16
                        syncServerPacket.WriteQ((long)playerBySlot.Effects);            // rec+17
                        syncServerPacket.WriteC((byte)clan.Effect);                     // rec+25
                        syncServerPacket.WriteC((byte)0);                               // rec+26
                        syncServerPacket.WriteC((byte)NATIONS);                         // rec+27
                        syncServerPacket.WriteC((byte)0);                               // rec+28
                        syncServerPacket.WriteD(playerBySlot.Equipment.NameCardId);     // rec+29
                        syncServerPacket.WriteC((byte)playerBySlot.Bonus.NickBorderColor); // rec+33
                        syncServerPacket.WriteC((byte)playerBySlot.AuthLevel());        // rec+34
                        syncServerPacket.WriteC((byte)0);                               // rec+35 pad (client 3595 handler proves clan name starts at +36)
                        syncServerPacket.WriteU(clan.Name, 34);                         // rec+36..69
                        syncServerPacket.WriteC((byte)playerBySlot.SlotId);             // rec+70
                        syncServerPacket.WriteU(playerBySlot.Nickname, 66);             // rec+71
                        syncServerPacket.WriteC((byte)playerBySlot.NickColor);          // rec+137
                        syncServerPacket.WriteC((byte)playerBySlot.Bonus.MuzzleColor);  // rec+138 (u16 lo)
                        syncServerPacket.WriteC((byte)0);                               // rec+139 (u16 hi)
                        syncServerPacket.WriteC(byte.MaxValue);                         // rec+140
                        syncServerPacket.WriteC(byte.MaxValue);                         // rec+141
                    }
                    else
                    {
                        syncServerPacket.WriteB(new byte[10]);   // rec+1..10
                        syncServerPacket.WriteD(uint.MaxValue);  // rec+11..14 (no player)
                        syncServerPacket.WriteB(new byte[55]);   // rec+15..69
                        syncServerPacket.WriteC((byte)slot.Id);  // rec+70
                        syncServerPacket.WriteB(new byte[66]);   // rec+71..136 nick
                        syncServerPacket.WriteC((byte)0);        // rec+137
                        syncServerPacket.WriteC((byte)0);        // rec+138
                        syncServerPacket.WriteC((byte)0);        // rec+139
                        syncServerPacket.WriteC(byte.MaxValue);  // rec+140
                        syncServerPacket.WriteC(byte.MaxValue);  // rec+141
                    }
                }
                return syncServerPacket.ToArray();
            }
        }

        // Builds [basic(71)][optblk(175)] exactly as PROTOCOL_ROOM_CREATE_ACK emits them (246 bytes).
        private byte[] BuildRoomBlob(RoomModel r)
        {
            using (SyncServerPacket p = new SyncServerPacket())
            {
                // basic (71)
                p.WriteD(r.RoomId);
                p.WriteU(r.Name, 46);
                p.WriteC((byte)r.MapId);
                p.WriteC((byte)r.Rule);
                p.WriteC((byte)r.Stage);
                p.WriteC((byte)r.RoomType);
                p.WriteC((byte)r.State);
                p.WriteC((byte)r.GetCountPlayers());
                p.WriteC((byte)r.GetSlotCount());
                p.WriteC((byte)r.Ping);
                p.WriteH((ushort)r.WeaponsFlag);
                p.WriteD(r.GetFlag());
                p.WriteH((short)0);
                p.WriteD(r.NewInt);
                // optblk (175)
                p.WriteH((short)0);
                p.WriteU(r.LeaderName, 66);
                p.WriteD(r.KillTime);
                p.WriteC(r.Limit);
                p.WriteC(r.WatchRuleFlag);
                p.WriteH((ushort)r.BalanceType);
                p.WriteB(r.RandomMaps);
                p.WriteC((byte)5);
                p.WriteB(r.LeaderAddr);
                p.WriteC(r.KillCam);
                p.WriteH((short)0);
                p.WriteB(new byte[68]);
                return p.ToArray();
            }
        }
    }
}
