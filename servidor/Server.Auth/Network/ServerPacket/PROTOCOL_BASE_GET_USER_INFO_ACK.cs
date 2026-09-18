// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_BASE_GET_USER_INFO_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Plugin.Core.XML;
using Server.Auth.Data.Managers;
using Plugin.Core.Managers;
using Plugin.Core;
using Server.Auth.Data.Models;
using Server.Auth.Data.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_BASE_GET_USER_INFO_ACK : AuthServerPacket
    {
        private readonly Account account;
        private readonly ClanModel clanModel;
        private readonly PlayerInventory playerInventory;
        private readonly PlayerEquipment playerEquipment;
        private readonly PlayerStatistic playerStatistic;
        private readonly EventVisitModel eventVisit;
        private readonly List<QuickstartModel> quickstarts;
        private readonly List<CharacterModel> characters;
        private readonly uint Error;
        private readonly uint Date;

        public PROTOCOL_BASE_GET_USER_INFO_ACK(Account Account)
        {
            account = Account;
            if (Account != null)
            {
                playerInventory = Account.Inventory;
                playerEquipment = Account.Equipment;
                playerStatistic = Account.Statistic;
                Date = uint.Parse(Account.LastLoginDate.ToString("yyMMddHHmm"));
                clanModel = ClanManager.GetClanDB((object)Account.ClanId, 1);
                quickstarts = Account.Quickstart.Quickjoins;
                characters = Account.Character.Characters;
                eventVisit = EventVisitXML.GetRunningEvent();
            }
            else
            {
                Error = (uint)EventErrorEnum.FAIL;
            }
        }

        public override void Write()
        {
            WriteH((short)2317);
            WriteH((short)0);
            WriteD(Error);
            if (Error != 0U)
                return;
            // === 121 client S2MO wire model: 48 fields, reverse-of-ctor order (sub_EAA63D). ===
            // Sizes verified from each S2MOValue<T,N> vf2/vf3 in the 121 client (tools\re-dumps\dump121).
            // Arrays = [1-byte count][count*elem]; count=0 => empty. Structs = fixed byte blocks.
            // First pass: skeleton + real USER_INFO_BASIC (nick/rank/gold/exp). Fill the rest later.
            StatisticTotal basic = playerStatistic.Basic ?? new StatisticTotal();
            StatisticSeason season = playerStatistic.Season ?? new StatisticSeason();
            StatisticWeapon weapon = playerStatistic.Weapon ?? new StatisticWeapon();
            if (playerStatistic.Battlecup == null) playerStatistic.Battlecup = new StatisticBattlecup();
            if (playerStatistic.Acemode == null) playerStatistic.Acemode = new StatisticAcemode();
            StatisticBattlecup battlecup = playerStatistic.Battlecup;
            StatisticAcemode acemode = playerStatistic.Acemode;
            WriteD(0);                  // 01 uint
            WriteD(0);                  // 02 uint
            WriteC((byte)0);            // 03 P_QUEST_VERSION_INFO[2] count
            WriteB(new byte[127]);      // 04 USER_INFO_P_QUEST
            // 05 ITEM_INFO[6] = roda de emotes (loadout+164). Antes ia count=0 → Alt+1..6
            // sempre "emocao invalida" mesmo com 3082 em batalha (BringUsedEmotionItemID=0).
            // dump121: mesma ordem do 3082 = EquipmentDataChara [Id][ObjId].
            int[] emoticons = playerEquipment?.Emoticons;
            WriteC((byte)6);
            for (int i = 0; i < 6; i++)
            {
                int emoId = emoticons != null && i < emoticons.Length ? emoticons[i] : 0;
                WriteB(playerInventory.EquipmentDataChara(emoId));
            }
            // 06 USER_INFO_FLASHSALE (15B) — drives the lobby flash-sale card + 24H countdown.
            // Client 122 handler for 2317 @0xEC10C9 copies these 15B verbatim into the shop clock
            // (sub_88AB7A -> clock.inner+0x8A1), then gates the card on them in sub_AFACBA:
            //   +4  synced : sub_88A63F requires == 1, else sub_889EAE never sets clock+193
            //                and the FlashSale_Main.i3UIs card (slot 18) is never built.
            //   +10 now    : minutes-of-day; remaining = endRef(1439) - now => counts down to 23:59.
            // Sent as 15 zero bytes before, so synced=0 => no card (now=0 also made mgr+40 read 1439).
            WriteD(0);                  // +0  unused by any consumer
            WriteD(1);                  // +4  synced
            WriteH(0);                  // +8  unused by any consumer
            WriteD((uint)(System.DateTime.Now.Hour * 60 + System.DateTime.Now.Minute)); // +10 now
            WriteC(0);                  // +14 unused by any consumer
            WriteB(new byte[21]);       // 07 USER_INFO_SEASON_CHALLENGE
            WriteB(new byte[152]);      // 08 EXCHANGE_SHOP_RECORD
            WriteB(new byte[152]);      // 09 ACCOUNT_LIMITED_RECORD
            // 10 USER_INFO_BATTLECUP_RECORD (48B = 12 dwords, client consumer sub_88E5B5)
            WriteD(battlecup.Matches); WriteD(playerStatistic.GetBCWinRatio()); WriteD(battlecup.MatchLoses);
            WriteD(battlecup.KillsCount); WriteD(battlecup.DeathsCount); WriteD(battlecup.HeadshotsCount);
            WriteD(battlecup.AssistsCount); WriteD(battlecup.EscapesCount); WriteD(playerStatistic.GetBCKDRatio());
            WriteD(battlecup.MatchWins); WriteD(battlecup.AverageDamage); WriteD(battlecup.PlayTime);
            // 11 USER_INFO_ACEMODE_RECORD (36B = 9 dwords, client consumer sub_88E59C)
            WriteD(acemode.Matches); WriteD(acemode.MatchWins); WriteD(acemode.MatchLoses);
            WriteD(acemode.Kills); WriteD(acemode.Deaths); WriteD(acemode.Headshots);
            WriteD(acemode.Assists); WriteD(acemode.Escapes); WriteD(acemode.Winstreaks);
            WriteB(new byte[5]);        // 12 USER_INFO_TICKET
            WriteC((byte)0);            // 13 bool
            WriteH((short)0);           // 14 ushort
            WriteD(0);                  // 15 uint
            WriteB(new byte[8]);        // 16 ITEM_INFO[1] (fixed 8)
            WriteC((byte)0);            // 17 LADDER_RECORD[3] count
            WriteC((byte)0);            // 18 LADDER_RECORD[3] count
            WriteD(weapon.AssaultKills);    WriteD(weapon.AssaultDeaths);    WriteB(new byte[24]);  // 19 WEAPON_RECORD entry[0] Assault
            WriteD(weapon.SmgKills);        WriteD(weapon.SmgDeaths);        WriteB(new byte[24]);  // entry[1] SMG
            WriteD(weapon.SniperKills);     WriteD(weapon.SniperDeaths);     WriteB(new byte[24]);  // entry[2] Sniper
            WriteD(weapon.ShotgunKills);    WriteD(weapon.ShotgunDeaths);    WriteB(new byte[24]);  // entry[3] Shotgun
            WriteD(weapon.MachinegunKills); WriteD(weapon.MachinegunDeaths); WriteB(new byte[24]);  // entry[4] Machinegun
            WriteD(weapon.ShieldKills);     WriteD(weapon.ShieldDeaths);     WriteB(new byte[24]);  // entry[5] Shield
            WriteC((byte)0);            // 20 uchar
            WriteC((byte)0);            // 21 uchar
            WriteC((byte)0);            // 22 bool
            WriteB(QuickstartData(quickstarts));       // 23 QUICKJOIN_INFO[3] (count + up to 3 x 4B)
            WriteB(new byte[33]);       // 24 USER_INFO_DAILY
            WriteC((byte)0);            // 25 NOTIFY_MEDAL[4] count
            WriteD(0);                  // 26 int
            WriteC((byte)0);            // 27 uchar[3] count
            WriteQ(0UL);                // 28 uint64
            WriteC((byte)0);            // 29 uchar[160] count
            // 30 QUESTING_INFO (89B): mission-card completion/slot state, restored at login.
            // Byte-identical to PROTOCOL_BASE_QUEST_DELETE_CARD_SET_ACK 89B body; consumed by the
            // 122 client via processor+120 -> UIMainFrame__ShowQuestUI -> MCardMgr__ParseAndSetQuestCompletion.
            WriteB(MissionWire.BuildQuestingInfo(account.Mission));
            WriteD(0);                  // 31 uint
            WriteD(0);                  // 32 uint
            WriteD(0);                  // 33 uint
            WriteD(0);                  // 34 uint
            WriteB(new byte[9]);        // 35 USER_INFO_LTS
            // Base__HandleGetUserInfoAck (dump122 @0xEC1362-0xEC13D2) itera SEMPRE 2 slots:
            // slot 0 alimenta UIPopupDormantAttendance (ShowDormantAttendanceNotice push 0) e
            // slot 1 alimenta a presenca normal (ShowAttendanceNotice push 1). Emitir um unico
            // elemento deixava o slot 1 zerado e o portal respondia NOT_ATTENDANCE.
            bool attendanceActive = eventVisit != null && eventVisit.EventIsEnabled() && account.Event != null;
            WriteC((byte)(attendanceActive ? 2 : 0));   // 36 ATTENDANCE_INFO[2] count
            if (attendanceActive)
            {
                WriteB(new byte[468]);                  // slot 0: dormant, sem evento
                WriteB(AttendanceData(account, eventVisit));
            }
            WriteC((byte)(attendanceActive ? 2 : 0));   // 37 ATTENDANCE_USER[2] count
            if (attendanceActive)
            {
                WriteB(new byte[9]);                    // slot 0: dormant, sem evento
                WriteB(CheckEventVisit(account, eventVisit, Date));
            }
            WriteD(0);                  // 38 uint
            WriteD(0);                  // 39 uint
            // 40 S2_MULTI_SLOT_INFO (34B) — char slots + Dino/Spray/NameCard cosmetics.
            // RE-PROVEN (dump121): the 121 2317 handler @0xEB7006 copies this block's 3 [Id][ObjId] pairs
            // (at +2) into loadout+132/140/148 (Dino/Spray/NameCard) via sub_950BD4, and the bottom-left
            // bar (UITopMenu sub_9BFA00 @0x9BFA00) draws the name card + border from loadout+148. Sent zeroed
            // before, so the card/border only appeared in the lobby (GAME 3082) and NOT on the channel-select
            // (AUTH) screen right after login. The S2_MULTI_SLOT_INFO deserializer (vf3 @0xEB2DA2) is an opaque
            // 34B memcpy; internal order pinned from ctor sub_EAB67B (zeroes bytes 2..25 = the 3 pairs) +
            // consumer offsets, and matches the 117/evo layout exactly:
            //   +0 RedSlot(u8) +1 BlueSlot(u8) +2 Dino[Id,ObjId] +10 Spray[Id,ObjId] +18 NameCard[Id,ObjId] +26 u32=0 +30 u32=0
            CharacterModel slotRed = characters.Count == 0 ? null : account.Character.GetCharacter(playerEquipment.CharaRedId);
            CharacterModel slotBlue = characters.Count == 0 ? null : account.Character.GetCharacter(playerEquipment.CharaBlueId);
            WriteC((byte)(slotRed != null ? slotRed.Slot : 0));
            WriteC((byte)(slotBlue != null ? slotBlue.Slot : 1));
            WriteB(playerInventory.EquipmentDataChara(playerEquipment.DinoItem));    // +2  Dino
            WriteB(playerInventory.EquipmentDataChara(playerEquipment.SprayId));     // +10 Spray
            WriteB(playerInventory.EquipmentDataChara(playerEquipment.NameCardId));  // +18 NameCard (bar reads loadout+148)
            WriteD(0);                  // +26
            WriteD(0);                  // +30 (total 34B, byte-stable vs the old byte[34])
            WriteD(0);                  // 41 uint
            WriteC((byte)0);            // 42 bool
            WriteT(account.PointUp());  // 43 float (MyInfo+92 PointUp, client sub_F2B820)
            WriteT(account.ExpUp());    // 44 float (MyInfo+88 ExpUp,  client sub_F2B820)
            // 45 USER_INFO_INVITEM_DATA (81B) — cosmetic block (nick color + name card + border).
            // Was WriteB(new byte[81]) => server-select screen had NO nick color / NO name card while
            // the lobby did, because the GAME/lobby push PROTOCOL_BASE_INV_ITEM_DATA_ACK (op 2395) sends
            // this same 81B struct with real data, but this AUTH 2317 sent it zeroed. Same 81B layout as 2395.
            // Client consumer (dump121): sub_88E78B @0x88E78B => qmemcpy(MyInfo.INVITEM, buf, 0x51) then
            // ROL-decode sub_96E89B. MyInfo singleton = dword_15C6310.
            //   +0  Field0                : slot/index tag (2395 sends 0)                                 [WriteC]
            //   +1  NickColor             : nick TEXT color. Read via sub_88DFC2(MyInfo,0)=MyInfo+1701 in
            //                               UITopMenu__SetMyDefaultInfo @0x9164EB (gated by config+182);
            //                               ALSO the name-card image selector in sub_9BFA00 @0x9BFA00 (v11[1]).
            //                               Non-zero here also STOPS the 2317 handler @0xEB776E from clearing
            //                               config+182 (the "use custom nick color" flag) => color renders.  [WriteC]
            //   +2  FakeRank (u32)        : rank-spoof bonus (item 1600xxx). Account.GetRank uses FakeRank!=255.[WriteD]
            //   +6  FakeRank (u32)        : duplicated exactly as 2395 does (client reads both).              [WriteD]
            //   +10 FakeNick (66B UTF-16) : nick-spoof string (empty when unused).                            [WriteU 66]
            //   +76 CrosshairColor (u16)  : in-battle crosshair tint (cosmetic).                              [WriteH]
            //   +78 MuzzleColor (u16)     : in-battle muzzle-flash tint (cosmetic).                           [WriteH]
            //   +80 NickBorderColor (u8)  : name-card OUTLINE color. Read as v11[80]-1 in sub_9BFA00 @0x9BFA00
            //                               (palette sub_9AF598); no card equipped => no outline drawn.       [WriteC]
            WriteC((byte)0);                             // +0  Field0
            WriteC((byte)account.NickColor);             // +1  NickColor
            WriteD(account.Bonus.FakeRank);              // +2  FakeRank
            WriteD(account.Bonus.FakeRank);              // +6  FakeRank (dup, matches 2395)
            WriteU(account.Bonus.FakeNick, 66);          // +10 FakeNick (66B)
            WriteH((short)account.Bonus.CrosshairColor); // +76 CrosshairColor
            WriteH((short)account.Bonus.MuzzleColor);    // +78 MuzzleColor
            WriteC((byte)account.Bonus.NickBorderColor); // +80 NickBorderColor
            // 46 USER_INFO_RECORD: Season(12) + Basic(12) dwords
            WriteD(season.Matches); WriteD(season.MatchWins); WriteD(season.MatchLoses); WriteD(season.MatchDraws);
            WriteD(season.KillsCount); WriteD(season.HeadshotsCount); WriteD(season.DeathsCount);
            WriteD(season.TotalMatchesCount); WriteD(season.TotalKillsCount);
            WriteD(season.EscapesCount); WriteD(season.AssistsCount); WriteD(season.MvpCount);
            WriteD(basic.Matches); WriteD(basic.MatchWins); WriteD(basic.MatchLoses); WriteD(basic.MatchDraws);
            WriteD(basic.KillsCount); WriteD(basic.HeadshotsCount); WriteD(basic.DeathsCount);
            WriteD(basic.TotalMatchesCount); WriteD(basic.TotalKillsCount);
            WriteD(basic.EscapesCount); WriteD(basic.AssistsCount); WriteD(basic.MvpCount);
            // 47 USER_INFO_BASIC (185B). Base__HandleGetUserInfoAck (0xEC116D) qmemcpy's these
            // 185 bytes and hands them to sub_88AEBB (0x88AEBB) — the SAME account struct that
            // opcode 2371 fills, and this AUTH packet is the one that fills it at login, before
            // the client ever reaches the game server. Keep this block byte-identical to
            // Server.Game/Network/ServerPacket/PROTOCOL_BASE_GET_MYINFO_BASIC_ACK.cs, which
            // carries the full offset-by-offset evidence; both are validated by the same oracle
            // (tools/unicorn/myinfo_basic_emu.py) and the same client_schema/2371.json.
            WriteU(account.Nickname, 66);              // 0
            WriteD(account.GetDisplayRank());          // 66  rank
            WriteD(account.GetRank());                 // 70  rank high-water
            WriteD(account.Gold);                      // 74  point
            WriteD(0);                                 // 78  core
            WriteQ((ulong)account.Exp);                // 82
            WriteD(0);                                 // 90
            WriteC(0);                                 // 94
            WriteQ(0UL);                               // 95  clan-guide latch
            WriteC(0);                                 // 103
            WriteC(0);                                 // 104
            WriteD(account.Tags);                      // 105 tag
            WriteH(0);                                 // 109 tag progress
            WriteD(0);                                 // 111
            WriteH((ushort)account.InventoryPlus);     // 115
            WriteD(0);                                 // 117
            WriteD(account.Cash);                      // 121
            WriteD(clanModel.Id);                      // 125
            WriteD(account.ClanAccess);                // 129
            WriteD(0);                                 // 133
            WriteD(0);                                 // 137
            WriteC((byte)account.CafePC);              // 141
            WriteC(0);                                 // 142
            WriteU(clanModel.Name, 34);                // 143 clan name, wchar[17]
            WriteC((byte)clanModel.Rank);              // 177
            WriteC((byte)clanModel.GetClanUnit());     // 178
            WriteD(clanModel.Logo);                    // 179
            WriteC((byte)clanModel.NameColor);         // 183
            WriteC((byte)clanModel.Effect);            // 184
            // 48 Age
            WriteC(AuthXender.Client.Config.EnableBlood ? (byte)account.Age : (byte)42);
        }

        private byte[] CheckEventVisit(Account Player, EventVisitModel eventVisit, uint A_3)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                PlayerEvent playerEvent = Player.Event;
                if (eventVisit != null && playerEvent != null && eventVisit.EventIsEnabled())
                {
                    uint num1 = uint.Parse($"{DateTimeUtil.Convert($"{A_3}"):yyMMdd}");
                    uint num2 = uint.Parse($"{DateTimeUtil.Convert($"{playerEvent.LastVisitDate}"):yyMMdd}");
                    syncServerPacket.WriteD(eventVisit.Id);
                    syncServerPacket.WriteC((byte)playerEvent.LastVisitCheckDay);
                    syncServerPacket.WriteC((byte)(playerEvent.LastVisitCheckDay - 1));
                    syncServerPacket.WriteC(num2 < num1 ? (byte)1 : (byte)2);
                    syncServerPacket.WriteC((byte)playerEvent.LastVisitSeqType);
                    syncServerPacket.WriteC((byte)1);
                }
                else
                {
                    syncServerPacket.WriteB(new byte[9]);
                }
                return syncServerPacket.ToArray();
            }
        }

        private byte[] AttendanceData(Account Player, EventVisitModel eventVisit)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                PlayerEvent playerEvent = Player.Event;
                if (eventVisit != null && eventVisit.EventIsEnabled())
                {
                    EventVisitModel eventVisitModel = EventVisitXML.GetEvent(eventVisit.Id + 1);
                    syncServerPacket.WriteU(eventVisit.Title, 70);
                    syncServerPacket.WriteC((byte)playerEvent.LastVisitCheckDay);
                    syncServerPacket.WriteC((byte)eventVisit.Checks);
                    syncServerPacket.WriteD(eventVisit.Id);
                    syncServerPacket.WriteD(eventVisit.BeginDate);
                    syncServerPacket.WriteD(eventVisit.EndedDate);
                    syncServerPacket.WriteD(eventVisitModel != null ? eventVisitModel.BeginDate : 0U);
                    syncServerPacket.WriteD(eventVisitModel != null ? eventVisitModel.EndedDate : 0U);
                    syncServerPacket.WriteD(0);
                    for (int index = 0; index < 31 /*0x1F*/; ++index)
                    {
                        VisitBoxModel box = eventVisit.Boxes[index];
                        // El cliente resuelve cada good con FindGoods y, en las rutas sin
                        // guarda, desreferencia el resultado (+0x4C). Un good que no viaja
                        // en el catalogo empacotado nunca resuelve, asi que se compacta la
                        // caja con los que el cliente si tiene.
                        List<int> rewards = new List<int>();
                        foreach (int goodId in new int[] { box.Reward1.GoodId, box.Reward2.GoodId })
                        {
                            if (goodId == 0)
                                continue;
                            if (!ShopManager.IsPackedGood(goodId))
                            {
                                CLogger.Print($"Attendance day {index + 1}: skipped good {goodId} (not in the packed client catalog)", LoggerType.Warning);
                                continue;
                            }
                            rewards.Add(goodId);
                        }
                        syncServerPacket.WriteC(box.IsBothReward && rewards.Count > 1 ? (byte)1 : (byte)0);
                        syncServerPacket.WriteC((byte)rewards.Count);
                        syncServerPacket.WriteH((short)0);
                        syncServerPacket.WriteD(rewards.Count > 0 ? rewards[0] : 0);
                        syncServerPacket.WriteD(rewards.Count > 1 ? rewards[1] : 0);
                    }
                }
                else
                {
                    syncServerPacket.WriteB(new byte[468]);
                }
                return syncServerPacket.ToArray();
            }
        }

        private byte[] QuickstartData(List<QuickstartModel> Player)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                int n = Player == null ? 0 : System.Math.Min(Player.Count, 3);
                syncServerPacket.WriteC((byte)n);
                for (int index = 0; index < n; index++)
                {
                    syncServerPacket.WriteC((byte)Player[index].MapId);
                    syncServerPacket.WriteC((byte)Player[index].Rule);
                    syncServerPacket.WriteC((byte)Player[index].StageOptions);
                    syncServerPacket.WriteC((byte)Player[index].Type);
                }
                return syncServerPacket.ToArray();
            }
        }

        private byte[] UnknownListData(int Player)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)Player);
                for (int index = 0; index < Player; ++index)
                {
                    syncServerPacket.WriteC(0);
                    syncServerPacket.WriteC(3);
                    syncServerPacket.WriteB(new byte[43]);
                }
                return syncServerPacket.ToArray();
            }
        }

        private byte[] UnknownArrayData(int Player)
        {
            using (SyncServerPacket syncServerPacket = new SyncServerPacket())
            {
                syncServerPacket.WriteC((byte)Player);
                for (int index = 0; index < Player; ++index)
                {
                    syncServerPacket.WriteB(new byte[45]);
                }
                return syncServerPacket.ToArray();
            }
        }
    }
}