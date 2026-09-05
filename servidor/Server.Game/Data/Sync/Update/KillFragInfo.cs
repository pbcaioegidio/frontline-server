// Decompiled with JetBrains decompiler
// Type: Server.Game.Data.Sync.Update.KillFragInfo
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Client;
using Server.Game.Data.Utils;
using Server.Game.Network;
using Server.Game.Network.ServerPacket;
using System.Runtime.CompilerServices;


namespace Server.Game.Data.Sync.Update
{
    public class KillFragInfo
    {
        
        public static void GenDeath(RoomModel Room, SlotModel Killer, FragInfos Kill, bool IsSuicide)
        {
            bool IsBotMode = Room.IsBotMode();
            int Score;
            RoomDeath.RegistryFragInfos(Room, Killer, out Score, IsBotMode, IsSuicide, Kill);
            if (IsBotMode)
            {
                Killer.Score += Killer.KillsOnLife + (int)Room.IngameAiLevel + Score;
                if (Killer.Score > (int)ushort.MaxValue)
                {
                    Killer.Score = (int)ushort.MaxValue;
                    CLogger.Print($"[PlayerId: {Killer.Id.ToString()}] reached the maximum score of the BOT.", LoggerType.Warning);
                }
                Kill.Score = Killer.Score;
            }
            else
            {
                Killer.Score += Score;
                AllUtils.CompleteMission(Room, Killer, Kill, MissionType.NA, 0);
                Kill.Score = Score;
            }
            if (IsSuicide && Killer.SpecGM)
            {
                Kill.KillerSlot = 255;
            }

            using (PROTOCOL_BATTLE_DEATH_ACK Packet = new PROTOCOL_BATTLE_DEATH_ACK(Room, Kill, Killer))
                Room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);

            RoomDeath.EndBattleByDeath(Room, Killer, IsBotMode, IsSuicide, Kill);


        }
    }
}