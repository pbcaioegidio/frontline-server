// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Client.RoomHitMarker
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Server.Game.Data.Models;
using Server.Game.Data.XML;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Sync.Client
{
    public static class RoomHitMarker
    {
        
        public static void Load(SyncClientPacket C)
        {
            int id = (int)C.ReadH();
            int num1 = (int)C.ReadH();
            int ServerId = (int)C.ReadH();
            byte SlotId = C.ReadC();
            byte num2 = C.ReadC();
            byte num3 = C.ReadC();
            int num4 = C.ReadD();
            if (C.ToArray().Length > 15)
                CLogger.Print($"Invalid Hit (Length > 15): {C.ToArray().Length}", LoggerType.Warning);
            int Id = num1;
            ChannelModel channel = ChannelsXML.GetChannel(ServerId, Id);
            if (channel == null)
                return;
            RoomModel room = channel.GetRoom(id);
            if (room == null || room.State != RoomState.BATTLE)
                return;
            Account playerBySlot = room.GetPlayerBySlot((int)SlotId);
            if (playerBySlot == null)
                return;
            string A_5 = "";
            if (num2 == (byte)10)
                A_5 = Translation.GetLabel("LifeRestored", (object)num4);
            switch (num3)
            {
                case 0:
                    A_5 = Translation.GetLabel("HitMarker1", (object)num4);
                    break;
                case 1:
                    A_5 = Translation.GetLabel("HitMarker2", (object)num4);
                    break;
                case 2:
                    A_5 = Translation.GetLabel("HitMarker3");
                    break;
                case 3:
                    A_5 = Translation.GetLabel("HitMarker4");
                    break;
            }
            playerBySlot.SendPacket(new PROTOCOL_LOBBY_CHATTING_ACK(Translation.GetLabel("HitMarkerName"), playerBySlot.GetSessionId(), 0, true, A_5));
        }
    }
}