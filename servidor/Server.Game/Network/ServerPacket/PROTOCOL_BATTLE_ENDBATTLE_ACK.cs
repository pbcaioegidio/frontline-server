using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Data.Utils;
using System;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_BATTLE_ENDBATTLE_ACK : GameServerPacket
    {
        private readonly RoomModel roomModel;
        private readonly Account account;
        private readonly ClanModel Field2;
        private readonly int Winner = 2;
        private readonly int SlotFlag;
        private readonly int MissionsFlag;
        private readonly bool IsBotMode;
        private readonly byte[] SlotInfoData;

        public PROTOCOL_BATTLE_ENDBATTLE_ACK(Account Acccount)
        {
            account = Acccount;
            if (Acccount != null)
            {
                roomModel = Acccount.GetRoom();
                if (roomModel != null)
                {
                    Winner = roomModel.RoomType == RoomCondition.Tutorial ? 0 : (int)AllUtils.GetWinnerTeam(roomModel);
                    Field2 = ClanManager.GetClan(Acccount.ClanId);
                    IsBotMode = roomModel.IsBotMode();
                    AllUtils.GetBattleResult(roomModel, out MissionsFlag, out SlotFlag, out SlotInfoData);
                }
            }
        }

        public PROTOCOL_BATTLE_ENDBATTLE_ACK(Account Acccount, int Winner, int SlotFlag, int MissionsFlag, bool IsBotMode, byte[] SlotInfoData)
        {
            account = Acccount;
            this.Winner = Winner;
            this.SlotFlag = SlotFlag;
            this.MissionsFlag = MissionsFlag;
            this.IsBotMode = IsBotMode;
            this.SlotInfoData = SlotInfoData;
            if (Acccount != null)
            {
                roomModel = Acccount.GetRoom();
                Field2 = ClanManager.GetClan(Acccount.ClanId);
            }
        }

        public PROTOCOL_BATTLE_ENDBATTLE_ACK(Account Acccount, TeamEnum Winner, int SlotFlag, int MissionsFlag, bool IsBotMode, byte[] SlotInfoData)
        {
            account = Acccount;
            this.Winner = (int)Winner;
            this.SlotFlag = SlotFlag;
            this.MissionsFlag = MissionsFlag;
            this.IsBotMode = IsBotMode;
            this.SlotInfoData = SlotInfoData;
            if (Acccount != null)
            {
                roomModel = Acccount.GetRoom();
                Field2 = ClanManager.GetClan(Acccount.ClanId);
            }
        }

        private int GetClientMode()
        {
            if (roomModel == null)
                return 0;
            if (roomModel.RoomType == RoomCondition.BattleRoyale)
                return 11;
            return (int)roomModel.RoomType / 4;
        }

        private static bool IsTeamBasedMode(int mode)
        {
            switch (mode)
            {
                case 2:
                case 3:
                case 4:
                case 5:
                case 7:
                case 8:
                case 12:
                case 13:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsPVPMode(int mode)
        {
            switch (mode)
            {
                case 0:
                case 7:
                case 9:
                case 10:
                case 12:
                    return false;
                default:
                    return true;
            }
        }

        public override void Write()
        {
            int mode = GetClientMode();
            StatisticWeapon weapon = account.Statistic.Weapon ?? new StatisticWeapon();
            WriteH((short)5140);
            WriteD(SlotFlag);
            WriteC((byte)Winner);
            WriteB(SlotInfoData);
            WriteD(MissionsFlag);
            if (IsTeamBasedMode(mode))
            {
                WriteH(roomModel.IsDinoMode("DE") ? (ushort)roomModel.FRDino : (roomModel.IsDinoMode("CC") ? (ushort)roomModel.FRKills : (ushort)roomModel.FRRounds));
                WriteH(roomModel.IsDinoMode("DE") ? (ushort)roomModel.CTDino : (roomModel.IsDinoMode("CC") ? (ushort)roomModel.CTKills : (ushort)roomModel.CTRounds));
                WriteH((short)0);
                WriteH((short)0);
                WriteB(new byte[36]);
            }
            if (IsPVPMode(mode))
                WriteB(new byte[36]);
            WriteB(new byte[5]);
            WriteB(Method5(roomModel));
            WriteB(Method2(roomModel));
            WriteB(new byte[72]);
            WriteB(new byte[36]);
            // Account struct, compact form. ClientTCPSocket__ParseBattleEnd (0xEE10D1) does NOT
            // bulk-read 185 bytes like 2371/2317: it reads these fields one by one into a fresh
            // buffer at ebp-296 and stores it with sub_88AEBB (0xEE15BE), so the wire order here
            // must be the parser's own order, and the two strings are length-prefixed.
            // Read order OBSERVED at 0xEE13E2..0xEE154C; the destination offsets are the same
            // account-struct offsets documented in PROTOCOL_BASE_GET_MYINFO_BASIC_ACK.cs.
            // The parser never reads 78, 109, 111, 115 or 117, so the client zeroes those on
            // battle end; that is client behaviour, not something the server can carry here.
            WriteC((byte)(account.Nickname.Length * 2));
            WriteU(account.Nickname, account.Nickname.Length * 2);
            WriteD(account.GetDisplayRank());          // 66
            WriteD(account.GetRank());                 // 70
            WriteD(account.Gold);                      // 74  point
            WriteQ((ulong)account.Exp);                // 82
            WriteD(0);                                 // 90
            WriteC(0);                                 // 94
            WriteQ(0UL);                               // 95  clan-guide latch
            WriteC(0);                                 // 103
            WriteC(0);                                 // 104
            WriteD(account.Tags);                      // 105 tag
            WriteD(account.Cash);                      // 121
            WriteD(Field2.Id);                         // 125
            WriteD(account.ClanAccess);                // 129
            WriteD(0);                                 // 133
            WriteD(0);                                 // 137
            WriteC((byte)account.CafePC);              // 141
            WriteC(0);                                 // 142
            WriteC((byte)(Field2.Name.Length * 2));
            WriteU(Field2.Name, Field2.Name.Length * 2);
            WriteC((byte)Field2.Rank);                 // 177
            WriteC((byte)Field2.GetClanUnit());        // 178
            WriteD(Field2.Logo);                       // 179
            WriteC((byte)Field2.NameColor);            // 183
            WriteC((byte)Field2.Effect);               // 184
            WriteD(account.Statistic.Season.Matches);
            WriteD(account.Statistic.Season.MatchWins);
            WriteD(account.Statistic.Season.MatchLoses);
            WriteD(account.Statistic.Season.MatchDraws);
            WriteD(account.Statistic.Season.KillsCount);
            WriteD(account.Statistic.Season.HeadshotsCount);
            WriteD(account.Statistic.Season.DeathsCount);
            WriteD(account.Statistic.Season.TotalMatchesCount);
            WriteD(account.Statistic.Season.TotalKillsCount);
            WriteD(account.Statistic.Season.EscapesCount);
            WriteD(account.Statistic.Season.AssistsCount);
            WriteD(account.Statistic.Season.MvpCount);
            WriteD(account.Statistic.Basic.Matches);
            WriteD(account.Statistic.Basic.MatchWins);
            WriteD(account.Statistic.Basic.MatchLoses);
            WriteD(account.Statistic.Basic.MatchDraws);
            WriteD(account.Statistic.Basic.KillsCount);
            WriteD(account.Statistic.Basic.HeadshotsCount);
            WriteD(account.Statistic.Basic.DeathsCount);
            WriteD(account.Statistic.Basic.TotalMatchesCount);
            WriteD(account.Statistic.Basic.TotalKillsCount);
            WriteD(account.Statistic.Basic.EscapesCount);
            WriteD(account.Statistic.Basic.AssistsCount);
            WriteD(account.Statistic.Basic.MvpCount);
            CharacterModel slotRed = account.Character.GetCharacter(account.Equipment.CharaRedId);
            CharacterModel slotBlue = account.Character.GetCharacter(account.Equipment.CharaBlueId);
            WriteC((byte)(slotRed != null ? slotRed.Slot : 0));
            WriteC((byte)(slotBlue != null ? slotBlue.Slot : 1));
            WriteB(account.Inventory.EquipmentDataChara(account.Equipment.DinoItem));
            WriteB(account.Inventory.EquipmentDataChara(account.Equipment.SprayId));
            WriteB(account.Inventory.EquipmentDataChara(account.Equipment.NameCardId));
            WriteD(0);
            WriteB(new byte[3]);
            WriteB(Method3(account));
            WriteD(weapon.AssaultKills); WriteD(weapon.AssaultDeaths); WriteB(new byte[24]);
            WriteD(weapon.SmgKills); WriteD(weapon.SmgDeaths); WriteB(new byte[24]);
            WriteD(weapon.SniperKills); WriteD(weapon.SniperDeaths); WriteB(new byte[24]);
            WriteD(weapon.ShotgunKills); WriteD(weapon.ShotgunDeaths); WriteB(new byte[24]);
            WriteD(weapon.MachinegunKills); WriteD(weapon.MachinegunDeaths); WriteB(new byte[24]);
            WriteD(weapon.ShieldKills); WriteD(weapon.ShieldDeaths); WriteB(new byte[24]);
            WriteC((byte)0);
            WriteC((byte)0);
            WriteH((short)0);
            WriteH((short)0);
            WriteH((short)0);
            WriteC((byte)0);
            WriteB(new byte[16]);
            WriteC((byte)0);
            if (mode == 11)
            {
                if (account.Statistic.Battlecup == null)
                    account.Statistic.Battlecup = new StatisticBattlecup();
                StatisticBattlecup battlecup = account.Statistic.Battlecup;
                WriteD(battlecup.Matches);
                WriteD(account.Statistic.GetBCWinRatio());
                WriteD(battlecup.MatchLoses);
                WriteD(battlecup.KillsCount);
                WriteD(battlecup.DeathsCount);
                WriteD(battlecup.HeadshotsCount);
                WriteD(battlecup.AssistsCount);
                WriteD(battlecup.EscapesCount);
                WriteD(account.Statistic.GetBCKDRatio());
                WriteD(battlecup.MatchWins);
                WriteD(battlecup.AverageDamage);
                WriteD(battlecup.PlayTime);
            }
            else if (mode == 8)
            {
                if (account.Statistic.Acemode == null)
                    account.Statistic.Acemode = new StatisticAcemode();
                StatisticAcemode acemode = account.Statistic.Acemode;
                WriteD(acemode.Matches);
                WriteD(acemode.MatchWins);
                WriteD(acemode.MatchLoses);
                WriteD(acemode.Kills);
                WriteD(acemode.Deaths);
                WriteD(acemode.Headshots);
                WriteD(acemode.Assists);
                WriteD(acemode.Escapes);
                WriteD(acemode.Winstreaks);
            }
            SlotModel slot = roomModel != null ? roomModel.GetSlot(account.SlotId) : null;
            WriteH((ushort)(slot != null ? slot.SeasonPoint : 0));
            WriteH((ushort)(slot != null ? slot.BonusBattlePass : 0));
            WriteB(new byte[21]);
            WriteD(0);
            WriteH((ushort)(600 + account.InventoryPlus + 8));
        }

        private byte[] Method2(RoomModel Acccount)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                foreach (SlotModel slot in Acccount.Slots)
                {
                    Account Player;
                    if (Acccount.GetPlayerBySlot(slot, out Player))
                        syncServerPacket.WriteC((byte)Player.GetRank());
                    else
                        syncServerPacket.WriteC((byte)AllUtils.InitBotRank(Acccount.IsStartingMatch() ? (int)Acccount.IngameAiLevel : (int)Acccount.AiLevel));
                    syncServerPacket.WriteH((short)0);
                    syncServerPacket.WriteD(1);
                }
                return syncServerPacket.ToArray();
            }
        }

        private byte[] Method3(Account Acccount)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                PlayerEvent playerEvent = Acccount.Event;
                if (playerEvent != null)
                {
                    syncServerPacket.WriteC((byte)playerEvent.LastPlaytimeFinish);
                    syncServerPacket.WriteD((uint)playerEvent.LastPlaytimeValue);
                }
                else
                    syncServerPacket.WriteB(new byte[5]);
                return syncServerPacket.ToArray();
            }
        }

        private byte[] Method5(RoomModel Acccount)
        {
            byte[] slots = Acccount?.SlotRewards.Item1;
            int[] items = Acccount?.SlotRewards.Item2;
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                for (int i = 0; i < 5; ++i)
                    syncServerPacket.WriteC(slots != null && i < slots.Length ? slots[i] : byte.MaxValue);
                for (int i = 0; i < 5; ++i)
                    syncServerPacket.WriteD(items != null && i < items.Length ? items[i] : 0);
                return syncServerPacket.ToArray();
            }
        }
    }
}
