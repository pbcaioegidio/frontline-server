// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_ROOM_CREATE_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_CREATE_REQ : GameClientPacket
    {
        private uint Field0;
        private string Field1;
        private string Field2;
        private string Field3;
        private MapIdEnum Field4;
        private MapRules Field5;
        private StageOptions Field6;
        private TeamBalance Field7;
        private byte[] Field8;
        private byte[] Field9;
        private int Field10;
        private int Field11;
        private int Field12;
        private int Field13;
        private int Field14;
        private byte Field15;
        private byte Field16;
        private byte Field17;
        private byte Field18;
        private byte Field19;
        private byte Field20;
        private byte Field21;
        private RoomCondition Field22;
        private RoomState Field23;
        private RoomWeaponsFlag Field24;
        private RoomStageFlag Field25;

        public override void Read()
        {
            ReadD();
            Field1 = ReadU(46);
            Field4 = (MapIdEnum)ReadC();
            Field5 = (MapRules)ReadC();
            Field6 = (StageOptions)ReadC();
            Field22 = (RoomCondition)ReadC();
            Field23 = (RoomState)ReadC();
            Field13 = (int)ReadC();
            Field10 = (int)ReadC();
            Field11 = (int)ReadC();
            Field24 = (RoomWeaponsFlag)ReadH();
            Field25 = (RoomStageFlag)ReadD();
            int num1 = (int)ReadH();
            Field14 = ReadD();
            int num2 = (int)ReadH();
            Field3 = ReadU(66);
            Field12 = ReadD();
            Field15 = ReadC();
            Field16 = ReadC();
            Field7 = (TeamBalance)ReadH();
            if (Field7 != TeamBalance.Count && Field7 != TeamBalance.Rank)
                Field7 = TeamBalance.None;
            Field8 = ReadB(24);
            Field21 = ReadC();
            if (Field21 != 3 && Field21 != 5 && Field21 != 7 && Field21 != 9)
                Field21 = 5;
            Field9 = ReadB(4);
            Field20 = ReadC();
            int num3 = (int)ReadH();
            Field2 = ReadS(4);
            ReadB(68);
            Field17 = ReadC();
            Field18 = ReadC();
            Field19 = ReadC();
        }

        
        public override void Run()
        {
            try
            {
                Account player = Client.GetAccount();
                if (player == null || ComDiv.GetDuration(player.LastCreateRoom) < 4.0)
                    return;
                ChannelModel channel = player.GetChannel();
                if (channel != null && player.Nickname.Length != 0 && player.Room == null && player.Match == null)
                {
                    player.LastCreateRoom = DateTimeUtil.Now();
                    lock (channel.Rooms)
                    {
                        for (int index = 0; index < channel.MaxRooms; ++index)
                        {
                            if (channel.GetRoom(index) == null)
                            {
                                RoomModel roomModel = new RoomModel(index, channel)
                                {
                                    Name = Field1,
                                    MapId = Field4,
                                    Rule = Field5,
                                    Stage = Field6,
                                    RoomType = Field22
                                };
                                roomModel.GenerateSeed();
                                roomModel.State = Field23 < RoomState.READY ? RoomState.READY : Field23;
                                roomModel.LeaderName = Field3.Equals("") || !Field3.Equals(player.Nickname) ? player.Nickname : Field3;
                                roomModel.Ping = Field11;
                                roomModel.WeaponsFlag = Field24;
                                roomModel.Flag = Field25;
                                roomModel.NewInt = Field14;
                                bool flag;
                                if (!(flag = roomModel.IsBotMode()) || roomModel.ChannelType != ChannelType.Clan)
                                {
                                    roomModel.KillTime = Field12;
                                    roomModel.Limit = channel.Type == ChannelType.Clan ? (byte)1 : Field15;
                                    roomModel.WatchRuleFlag = roomModel.RoomType == RoomCondition.Ace ? (byte)142 : Field16;
                                    roomModel.BalanceType = channel.Type == ChannelType.Clan || roomModel.RoomType == RoomCondition.Ace ? TeamBalance.None : Field7;
                                    roomModel.RandomMaps = Field8;
                                    roomModel.CountdownIG = Field21;
                                    roomModel.LeaderAddr = Field9;
                                    roomModel.KillCam = Field20;
                                    roomModel.Password = Field2;
                                    if (flag)
                                    {
                                        roomModel.AiCount = Field17;
                                        roomModel.AiLevel = Field18;
                                        roomModel.AiType = Field19;
                                    }
                                    roomModel.SetSlotCount(Field10, true, false);
                                    roomModel.CountPlayers = Field13;
                                    if (roomModel.AddPlayer(player) >= 0)
                                    {
                                        player.ResetPages();
                                        channel.AddRoom(roomModel);
                                        Client.SendPacket(new PROTOCOL_ROOM_CREATE_ACK(Field0, roomModel));
                                        if (!roomModel.IsBotMode())
                                            break;
                                        roomModel.ChangeSlotState(1, SlotState.CLOSE, true);
                                        roomModel.ChangeSlotState(3, SlotState.CLOSE, true);
                                        roomModel.ChangeSlotState(5, SlotState.CLOSE, true);
                                        roomModel.ChangeSlotState(7, SlotState.CLOSE, true);
                                        Client.SendPacket(new PROTOCOL_ROOM_GET_SLOTINFO_ACK(roomModel));
                                        break;
                                    }
                                }
                                else
                                {
                                    Field0 = 2147487869U;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                    Field0 = 2147483648U /*0x80000000*/;
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_LOBBY_CREATE_ROOM_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}