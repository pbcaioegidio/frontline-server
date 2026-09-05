// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Server.BattleLeaveSync
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Game.Data.Models;
using System;


namespace Server.Game.Data.Sync.Server
{
    public class BattleLeaveSync
    {
        public static void SendUDPPlayerLeave(RoomModel Room, int SlotId)
        {
            try
            {
                if (Room == null || Room.UdpServer == null)
                    return;
                int playingPlayers = Room.GetPlayingPlayers(TeamEnum.TEAM_DRAW, SlotState.BATTLE, 0, SlotId);
                using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                {
                    syncServerPacket.WriteH((short)2);
                    syncServerPacket.WriteD(Room.UniqueRoomId);
                    syncServerPacket.WriteD(Room.Seed);
                    syncServerPacket.WriteC((byte)SlotId);
                    syncServerPacket.WriteC((byte)playingPlayers);
                    GameXender.SyncOf(Room.ServerId)?.SendPacket(syncServerPacket.ToArray(), Room.UdpServer.Connection);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}