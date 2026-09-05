// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BATTLE_MISSION_BOMB_INSTALL_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Client;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_MISSION_BOMB_INSTALL_REQ : GameClientPacket
    {
        private int Field0;
        private float Field1;
        private float Field2;
        private float Field3;
        private byte Field4;
        private int Field5;

        public override void Read()
        {
            this.Field0 = this.ReadD();
            this.Field4 = this.ReadC();
            this.Field5 = this.ReadD();
            this.Field1 = this.ReadT();
            this.Field2 = this.ReadT();
            this.Field3 = this.ReadT();
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null || room.RoundTime.IsTimer() || room.State != RoomState.BATTLE || room.ActiveC4)
                    return;
                SlotModel slot = room.GetSlot(this.Field0);
                if (slot == null || slot.State != SlotState.BATTLE)
                    return;
                RoomBombC4.InstallBomb(room, slot, this.Field4, this.Field5 == 0 ? (ushort)42 : (ushort)0, this.Field1, this.Field2, this.Field3);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_MISSION_BOMB_INSTALL_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}