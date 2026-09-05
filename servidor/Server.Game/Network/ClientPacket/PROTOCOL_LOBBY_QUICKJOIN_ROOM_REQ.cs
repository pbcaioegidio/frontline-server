// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_LOBBY_QUICKJOIN_ROOM_REQ
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_LOBBY_QUICKJOIN_ROOM_REQ : GameClientPacket
    {
        private int Field0;
        private List<RoomModel> Field1 = new List<RoomModel>();
        private List<QuickstartModel> Field2 = new List<QuickstartModel>();
        private QuickstartModel Field3;

        public override void Read()
        {
            this.Field0 = (int)this.ReadC();
            for (int index = 0; index < 3; ++index)
                this.Field2.Add(new QuickstartModel()
                {
                    MapId = (int)this.ReadC(),
                    Rule = (int)this.ReadC(),
                    StageOptions = (int)this.ReadC(),
                    Type = (int)this.ReadC()
                });
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                player.Quickstart.Quickjoins[this.Field0] = this.Field2[this.Field0];
                ComDiv.UpdateDB("player_quickstarts", "owner_id", (object)player.PlayerId, new string[4]
                {
                    $"list{this.Field0}_map_id",
                    $"list{this.Field0}_map_rule",
                    $"list{this.Field0}_map_stage",
                    $"list{this.Field0}_map_type"
                }, (object)this.Field2[this.Field0].MapId, (object)this.Field2[this.Field0].Rule, (object)this.Field2[this.Field0].StageOptions, (object)this.Field2[this.Field0].Type);
                ChannelModel Channel;
                if (player.Nickname.Length > 0 && player.Room == null && player.Match == null && player.GetChannel(out Channel))
                {
                    lock (Channel.Rooms)
                    {
                        using (List<RoomModel>.Enumerator enumerator = Channel.Rooms.GetEnumerator())
                        {
                        label_13:
                            while (enumerator.MoveNext())
                            {
                                RoomModel current = enumerator.Current;
                                if (current.RoomType != RoomCondition.Tutorial)
                                {
                                    this.Field3 = this.Field2[this.Field0];
                                    if ((MapIdEnum)this.Field3.MapId == current.MapId && (MapRules)this.Field3.Rule == current.Rule && (StageOptions)this.Field3.StageOptions == current.Stage && (RoomCondition)this.Field3.Type == current.RoomType && current.Password.Length == 0 && current.Limit == (byte)0 && (!current.KickedPlayersVote.Contains(player.PlayerId) || player.IsGM()))
                                    {
                                        SlotModel[] slots = current.Slots;
                                        int index = 0;
                                        while (true)
                                        {
                                            if (index < slots.Length)
                                            {
                                                SlotModel slotModel = slots[index];
                                                if (slotModel.PlayerId != 0L || slotModel.State != SlotState.EMPTY)
                                                    ++index;
                                                else
                                                    break;
                                            }
                                            else
                                                goto label_13;
                                        }
                                        this.Field1.Add(current);
                                    }
                                }
                            }
                        }
                    }
                }
                if (this.Field1.Count == 0)
                {
                    this.Client.SendPacket(new PROTOCOL_LOBBY_QUICKJOIN_ROOM_ACK(2147483648U /*0x80000000*/, this.Field2, (RoomModel)null, (QuickstartModel)null));
                }
                else
                {
                    RoomModel A_3 = this.Field1[new Random().Next(this.Field1.Count)];
                    if (A_3 != null && A_3.GetLeader() != null && A_3.AddPlayer(player) >= 0)
                    {
                        player.ResetPages();
                        using (PROTOCOL_ROOM_GET_SLOTONEINFO_ACK Packet = new PROTOCOL_ROOM_GET_SLOTONEINFO_ACK(player))
                            A_3.SendPacketToPlayers(Packet, player.PlayerId);
                        A_3.UpdateSlotsInfo();
                        this.Client.SendPacket(new PROTOCOL_ROOM_JOIN_ACK(0U, player));
                        this.Client.SendPacket(new PROTOCOL_LOBBY_QUICKJOIN_ROOM_ACK(0U, this.Field2, A_3, this.Field3));
                    }
                    else
                        this.Client.SendPacket(new PROTOCOL_LOBBY_QUICKJOIN_ROOM_ACK(2147483648U /*0x80000000*/, (List<QuickstartModel>)null, (RoomModel)null, (QuickstartModel)null));
                }
                this.Field1 = (List<RoomModel>)null;
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_LOBBY_QUICKJOIN_ROOM_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}