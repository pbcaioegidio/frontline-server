// Decompiled with JetBrains decompiler
// Type: Server.Game.Network.ClientPacket.PROTOCOL_BATTLE_DEATH_REQ
// Assembly: Server.Game, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: 2BF67F5F-ABA1-4CD4-BD5E-51B3899CA9A8
// Assembly location: C:\Users\home\Desktop\dll\Server.Game-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Server.Game.Data.Models;
using Server.Game.Data.Sync.Client;
using Server.Game.Data.Utils;
using Server.Game.Network.ServerPacket;
using System;
using System.Runtime.CompilerServices;


namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_BATTLE_DEATH_REQ : GameClientPacket
    {
        private FragInfos Field0;
        private bool Field1;

        public override void Read()
        {
            this.Field0 = new FragInfos()
            {
                KillingType = (CharaKillType)this.ReadC(),
                KillsCount = this.ReadC(),
                KillerSlot = this.ReadC(),
                WeaponId = this.ReadD(),
                X = this.ReadT(),
                Y = this.ReadT(),
                Z = this.ReadT(),
                Flag = this.ReadC(),
                Unk = this.ReadC()
            };
            for (int index = 0; index < (int)this.Field0.KillsCount; ++index)
            {
                FragModel fragModel = new FragModel()
                {
                    VictimSlot = this.ReadC(),
                    WeaponClass = this.ReadC(),
                    HitspotInfo = this.ReadC(),
                    KillFlag = (KillingMessage)this.ReadH(),
                    Unk = this.ReadC(),
                    X = this.ReadT(),
                    Y = this.ReadT(),
                    Z = this.ReadT(),
                    AssistSlot = this.ReadC(),
                    Unks = this.ReadB(4)
                };
                this.Field0.Frags.Add(fragModel);
                if ((int)fragModel.VictimSlot == (int)this.Field0.KillerSlot)
                    this.Field1 = true;
            }
        }

        
        public override void Run()
        {
            try
            {
                Account player = this.Client.GetAccount();
                if (player == null)
                    return;
                RoomModel room = player.Room;
                if (room == null || room.RoundTime.IsTimer() || room.State < RoomState.BATTLE)
                    return;
                bool IsBotMode = room.IsBotMode();
                // PvP: kills só via Match→Game (RoomDeath sync). Cliente DEATH_REQ é spoofável.
                if (!IsBotMode)
                {
                    CLogger.Print(
                        $"[AntiCheat] BATTLE_DEATH_REQ ignorado (PvP) PlayerId={player.PlayerId} Slot={player.SlotId} Kills={Field0.KillsCount}",
                        LoggerType.Warning);
                    return;
                }
                SlotModel slot = room.GetSlot((int)this.Field0.KillerSlot);
                if (slot == null || !IsBotMode && (slot.State < SlotState.BATTLE || slot.Id != player.SlotId))
                    return;
                if (this.Field0.Frags.Count > 0 && this.Field0.Frags.TrueForAll(f => room.GetSlot((int)f.VictimSlot)?.DeathState.HasFlag(DeadEnum.Dead) == true))
                    return;
                if (Field0.KillsCount > 16 || Field0.Frags.Count > 16)
                    return;
                int Score;
                RoomDeath.RegistryFragInfos(room, slot, out Score, IsBotMode, this.Field1, this.Field0);
                if (!IsBotMode)
                {
                    slot.Score += Score;
                    AllUtils.CompleteMission(room, slot, this.Field0, MissionType.NA, 0);
                    this.Field0.Score = Score;
                }
                else
                {
                    // Desafio/AI: kills não passam pelo Match — anti-burst aqui
                    AllUtils.TrackBotKillBurst(player, slot, this.Field0);

                    slot.Score += slot.KillsOnLife + (int)room.IngameAiLevel + Score;
                    if (slot.Score > (int)ushort.MaxValue)
                    {
                        slot.Score = (int)ushort.MaxValue;
                        AllUtils.ValidateBanPlayer(player, $"AI Score Cheating! ({slot.Score})", "AI_SCORE",
                            "{\"score\":" + slot.Score + ",\"max\":" + (int)ushort.MaxValue + "}", "FG-125");
                    }
                    this.Field0.Score = slot.Score;
                }
                using (PROTOCOL_BATTLE_DEATH_ACK Packet = new PROTOCOL_BATTLE_DEATH_ACK(room, this.Field0, slot))
                    room.SendPacketToPlayers(Packet, SlotState.BATTLE, 0);
                RoomDeath.EndBattleByDeath(room, slot, IsBotMode, this.Field1, this.Field0);
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_BATTLE_DEATH_REQ: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}