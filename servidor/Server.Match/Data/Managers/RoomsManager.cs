// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Managers.RoomsManager
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Server.Match.Data.Models;
using Server.Match.Data.Utils;
using System.Collections.Generic;


namespace Server.Match.Data.Managers
{
    public static class RoomsManager
    {
        private static readonly List<RoomModel> Field0 = new List<RoomModel>();

        public static RoomModel CreateOrGetRoom(uint UniqueRoomId, uint Seed)
        {
            lock (RoomsManager.Field0)
            {
                foreach (RoomModel orGetRoom in RoomsManager.Field0)
                {
                    if ((int)orGetRoom.UniqueRoomId == (int)UniqueRoomId && (int)orGetRoom.RoomSeed == (int)Seed)
                        return orGetRoom;
                }
                int roomInfo1 = AllUtils.GetRoomInfo(UniqueRoomId, 2);
                int roomInfo2 = AllUtils.GetRoomInfo(UniqueRoomId, 1);
                int roomInfo3 = AllUtils.GetRoomInfo(UniqueRoomId, 0);
                RoomModel orGetRoom1 = new RoomModel(roomInfo1)
                {
                    UniqueRoomId = UniqueRoomId,
                    RoomSeed = Seed,
                    RoomId = roomInfo3,
                    ChannelId = roomInfo2,
                    MapId = (MapIdEnum)AllUtils.GetSeedInfo(Seed, 2),
                    RoomType = (RoomCondition)AllUtils.GetSeedInfo(Seed, 0),
                    Rule = (MapRules)AllUtils.GetSeedInfo(Seed, 1)
                };
                RoomsManager.Field0.Add(orGetRoom1);
                return orGetRoom1;
            }
        }

        public static RoomModel GetRoom(uint UniqueRoomId, uint Seed)
        {
            lock (RoomsManager.Field0)
            {
                foreach (RoomModel room in RoomsManager.Field0)
                {
                    if (room != null && (int)room.UniqueRoomId == (int)UniqueRoomId && (int)room.RoomSeed == (int)Seed)
                        return room;
                }
                return (RoomModel)null;
            }
        }

        public static void RemoveRoom(uint UniqueRoomId, uint Seed)
        {
            lock (RoomsManager.Field0)
            {
                for (int index = 0; index < RoomsManager.Field0.Count; ++index)
                {
                    RoomModel roomModel = RoomsManager.Field0[index];
                    if ((int)roomModel.UniqueRoomId == (int)UniqueRoomId && (int)roomModel.RoomSeed == (int)Seed)
                    {
                        RoomsManager.Field0.RemoveAt(index);
                        break;
                    }
                }
            }
        }
    }
}