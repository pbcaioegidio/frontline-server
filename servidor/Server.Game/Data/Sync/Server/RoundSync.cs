// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Server.RoundSync
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
    public class RoundSync
    {
        public static void SendUDPRoundSync(RoomModel Room)
        {
            try
            {
                if (Room == null || Room.UdpServer == null)
                    return;
                using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                {
                    syncServerPacket.WriteH((short)3);
                    syncServerPacket.WriteD(Room.UniqueRoomId);
                    syncServerPacket.WriteD(Room.Seed);
                    syncServerPacket.WriteC((byte)Room.Rounds);
                    syncServerPacket.WriteC(Room.IsTeamSwap() ? (byte)1 : (byte)0);
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