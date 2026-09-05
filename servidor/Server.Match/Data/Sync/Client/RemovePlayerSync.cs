// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Sync.Client.RemovePlayerSync
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Network;
using Server.Match.Data.Managers;
using Server.Match.Data.Models;


namespace Server.Match.Data.Sync.Client
{
    public class RemovePlayerSync
    {
        public static void Load(SyncClientPacket C)
        {
            int UniqueRoomId = (int)C.ReadUD();
            uint num1 = C.ReadUD();
            int Slot = (int)C.ReadC();
            int num2 = (int)C.ReadC();
            int Seed = (int)num1;
            RoomModel room = RoomsManager.GetRoom((uint)UniqueRoomId, (uint)Seed);
            if (room == null)
                return;
            if (num2 == 0)
                RoomsManager.RemoveRoom(room.UniqueRoomId, room.RoomSeed);
            else
                room.GetPlayer(Slot, false)?.ResetAllInfos();
        }
    }
}