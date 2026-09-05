// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BATTLE_MISSION_BOMB_UNINSTALL_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Client;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_MISSION_BOMB_UNINSTALL_REQ : GameClientPacket
    {
        private int Field0;

        public override void Read() => this.Field0 = this.ReadD();

        public override void Run()
        {
            Account player = this.Client.GetAccount();
            if (player == null)
                return;
            RoomModel room = player.Room;
            if (room == null || room.RoundTime.IsTimer() || room.State != RoomState.BATTLE || !room.ActiveC4)
                return;
            SlotModel slot = room.GetSlot(this.Field0);
            if (slot == null || slot.State != SlotState.BATTLE)
                return;
            RoomBombC4.UninstallBomb(room, slot);
        }
    }
}