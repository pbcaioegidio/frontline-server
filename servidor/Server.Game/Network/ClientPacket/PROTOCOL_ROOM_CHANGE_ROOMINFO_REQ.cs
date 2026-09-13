// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_ROOM_CHANGE_ROOMINFO_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_CHANGE_ROOMINFO_REQ : GameClientPacket
    {
        private string Field0;
        private string Field1;
        private MapIdEnum Field2;
        private MapRules Field3;
        private StageOptions Field4;
        private TeamBalance Field5;
        private byte[] Field6;
        private byte[] Field7;
        private int Field8;
        private int Field9;
        private int Field10;
        private int Field11;
        private int Field12;
        private byte Field13;
        private byte Field14;
        private byte Field15;
        private byte Field16;
        private byte Field17;
        private byte Field18;
        private byte Field23; // AiType — mesmo layout do CREATE_REQ
        private string FieldPassword;
        private RoomCondition Field19;
        private RoomState Field20;
        private RoomWeaponsFlag Field21;
        private RoomStageFlag Field22;

        public override void Read()
        {
            this.ReadD();
            this.Field0 = this.ReadU(46);
            this.Field2 = (MapIdEnum)this.ReadC();
            this.Field3 = (MapRules)this.ReadC();
            this.Field4 = (StageOptions)this.ReadC();
            this.Field19 = (RoomCondition)this.ReadC();
            this.Field20 = (RoomState)this.ReadC();
            this.Field12 = (int)this.ReadC();
            this.Field9 = (int)this.ReadC();
            this.Field10 = (int)this.ReadC();
            this.Field21 = (RoomWeaponsFlag)this.ReadH();
            this.Field22 = (RoomStageFlag)this.ReadD();
            int num1 = (int)this.ReadH();
            this.Field8 = this.ReadD();
            int num2 = (int)this.ReadH();
            this.Field1 = this.ReadU(66);
            this.Field11 = this.ReadD();
            this.Field13 = this.ReadC();
            this.Field14 = this.ReadC();
            this.Field5 = (TeamBalance)this.ReadH();
            this.Field6 = this.ReadB(24);
            this.Field17 = this.ReadC();
            this.Field7 = this.ReadB(4);
            this.Field18 = this.ReadC();
            int num3 = (int)this.ReadH();
            // CREATE manda senha(4)+pad(68)+Ai* (~75B). A engrenagem do client 122 manda
            // só o pad (~70B) — senha vai no 3602. Ler 75B no CHANGE causava EndOfStream
            // → sem ACK 3601 → Please Wait em loop.
            long left = this.MStream.Length - this.MStream.Position;
            if (left >= 75L)
            {
                this.FieldPassword = this.ReadS(4);
                this.ReadB(68);
                this.Field15 = this.ReadC();
                this.Field16 = this.ReadC();
                this.Field23 = this.ReadC();
            }
            else if (left > 0L)
            {
                this.ReadB((int)left);
            }
        }

        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null || room.Leader != player.SlotId)
                    return;

                bool nameChanged = !room.Name.Equals(this.Field0);
                bool modeChanged = room.Rule != this.Field3 || room.Stage != this.Field4 || room.RoomType != this.Field19
                    || room.MapId != this.Field2 || this.Field21 != room.WeaponsFlag || this.Field9 != room.CountMaxSlots;

                room.Name = this.Field0;
                room.MapId = this.Field2;
                room.Rule = this.Field3;
                room.Stage = this.Field4;
                room.RoomType = this.Field19;
                room.Ping = this.Field10;
                room.Flag = this.Field22;
                room.NewInt = this.Field8;
                room.KillTime = this.Field11;
                room.Limit = this.Field13;
                room.WatchRuleFlag = room.RoomType == RoomCondition.Ace ? (byte)142 : this.Field14;
                room.BalanceType = room.RoomType == RoomCondition.Ace ? TeamBalance.None : this.Field5;
                room.RandomMaps = this.Field6 ?? new byte[24];
                room.CountdownIG = NormalizeCountdown(this.Field17);
                room.LeaderAddr = this.Field7 ?? new byte[4];
                room.KillCam = this.Field18;
                room.AiCount = this.Field15;
                room.AiLevel = this.Field16;
                room.AiType = this.Field23;
                room.WeaponsFlag = this.Field21;
                room.CountPlayers = this.Field12;
                if (!string.IsNullOrEmpty(this.FieldPassword))
                    room.Password = this.FieldPassword;

                room.SetSlotCount(this.Field9, false, true);

                if (this.Field20 < RoomState.READY || string.IsNullOrEmpty(this.Field1) || !this.Field1.Equals(player.Nickname) || nameChanged || modeChanged)
                {
                    room.State = this.Field20 < RoomState.READY ? RoomState.READY : this.Field20;
                    room.LeaderName = string.IsNullOrEmpty(this.Field1) || !this.Field1.Equals(player.Nickname) ? player.Nickname : this.Field1;
                    if (room.ResetReadyPlayers() > 0)
                        room.UpdateSlotsInfo();
                }

                CLogger.Print(
                    $"CHANGE_ROOMINFO room={room.RoomId} map={(int)room.MapId} type={(int)room.RoomType} killTime={room.KillTime} slots={room.GetSlotCount()} cd={room.CountdownIG} bal={(int)room.BalanceType} killCam={room.KillCam}",
                    LoggerType.Info);

                room.UpdateRoomInfo();
                using (PROTOCOL_ROOM_CHANGE_ROOM_OPTIONINFO_ACK Packet = new PROTOCOL_ROOM_CHANGE_ROOM_OPTIONINFO_ACK(room))
                    room.SendPacketToPlayers(Packet);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_ROOM_CHANGE_ROOMINFO_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }

        private static byte NormalizeCountdown(byte value)
        {
            if (value == 3 || value == 5 || value == 7 || value == 9)
                return value;
            return 5;
        }
    }
}
