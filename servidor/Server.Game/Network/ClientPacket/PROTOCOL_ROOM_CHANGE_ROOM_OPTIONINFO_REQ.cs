using Plugin.Core;
using Plugin.Core.Enums;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_ROOM_CHANGE_ROOM_OPTIONINFO_REQ : GameClientPacket
    {
        private string Field0;
        private int Field1;
        private byte Field2;
        private byte Field3;
        private TeamBalance Field4;
        private byte[] Field5;
        private byte Field6;
        private byte[] Field7;
        private byte Field8;

        public override void Read()
        {
            this.ReadC();
            this.Field0 = this.ReadU(66);
            this.Field1 = this.ReadD();
            this.Field2 = this.ReadC();
            this.Field3 = this.ReadC();
            this.Field4 = (TeamBalance)this.ReadH();
            this.Field5 = this.ReadB(24);
            this.Field6 = this.ReadC();
            this.Field7 = this.ReadB(4);
            this.Field8 = this.ReadC();
            this.ReadH();
            this.ReadB(68);
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
                room.LeaderName = this.Field0.Equals("") || !this.Field0.Equals(player.Nickname) ? player.Nickname : this.Field0;
                room.KillTime = this.Field1;
                room.Limit = this.Field2;
                room.WatchRuleFlag = room.RoomType == RoomCondition.Ace ? (byte)142 : this.Field3;
                room.BalanceType = this.Field4;
                room.RandomMaps = this.Field5;
                room.CountdownIG = this.Field6;
                room.LeaderAddr = this.Field7;
                room.KillCam = this.Field8;
                using (PROTOCOL_ROOM_CHANGE_ROOM_OPTIONINFO_ACK Packet = new PROTOCOL_ROOM_CHANGE_ROOM_OPTIONINFO_ACK(room))
                    room.SendPacketToPlayers(Packet);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_ROOM_CHANGE_ROOM_OPTIONINFO_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}
