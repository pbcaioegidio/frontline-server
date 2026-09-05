// Decompiled with JetBrains decompiler
// Type: Server.Match.Data.Sync.Client.MatchRoundSync
// Assembly: Server.Match, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: CE18A1E1-67C7-4FA9-8510-2DD553448D5A
// Assembly location: C:\Users\home\Desktop\dll\Server.Match-deobfuscated-Cleaned.dll

using Plugin.Core.Network;
using Server.Match.Data.Managers;
using Server.Match.Data.Models;


namespace Server.Match.Data.Sync.Client
{
    public class MatchRoundSync
    {
        public static void Load(SyncClientPacket C)
        {
            int UniqueRoomId = (int)C.ReadUD();
            uint num1 = C.ReadUD();
            int num2 = (int)C.ReadC();
            bool flag = C.ReadC() == (byte)1;
            int Seed = (int)num1;
            RoomModel room = RoomsManager.GetRoom((uint)UniqueRoomId, (uint)Seed);
            if (room == null)
                return;
            room.ServerRound = num2;
            room.IsTeamSwap = flag;
        }
    }
}