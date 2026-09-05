// Decompiled with JetBrains decompiler
// Type: Server.Auth.Network.ServerPacket.PROTOCOL_SEASON_CHALLENGE_INFO_ACK
// Assembly: Server.Auth, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: D2254E5E-B0BA-4DE9-9720-2DDECE3CD4EF
// Assembly location: C:\Users\home\Desktop\dll\Server.Auth-deobfuscated-Cleaned.dll

using Plugin.Core;
using Plugin.Core.Managers;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.XML;
using Server.Auth.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Auth.Network.ServerPacket
{
    public class PROTOCOL_SEASON_CHALLENGE_INFO_ACK : AuthServerPacket
    {
        private readonly List<BattlePassCardData> cardDataList;
        private readonly BattlePassSeason activeSeason;
        private readonly Account Player;
        private readonly string seasonName;
        private readonly uint startDate;
        private readonly uint endDate;
        private readonly int isPremium;

        public PROTOCOL_SEASON_CHALLENGE_INFO_ACK(Account A_1)
        {
            if (A_1 == null)
                return;

            this.Player = A_1;

            // Cargar los datos del pase de batalla actual
            this.cardDataList = BattlePassManager.GetCards();

            // Obtener la temporada activa
            this.activeSeason = BattlePassManager.GetActiveSeason();

            if (activeSeason != null)
            {
                // Usar datos de la temporada activa
                seasonName = activeSeason.SeasonName;
                startDate = uint.Parse(activeSeason.SeasonStartDate);
                endDate = uint.Parse(activeSeason.SeasonEndDate);
            }

            isPremium = (A_1.Battlepass.HavePremium) ? 1 : 0;
        }

        public override void Write()
        {
            // Header: opcode + 0. Client payload (S2MO body) starts right after these 2 shorts.
            this.WriteH((short)8450);
            this.WriteH((short)0);

            // Client (122) reads the body as two flat S2MO blobs, concatenated, no framing:
            //   [USER_INFO_SEASON_CHALLENGE 21B][SEASON_CHALLENGE_INFO_C 1665B] = 1686B.
            // Wire order = head-first list walk (handler @0xef29f2 / PACKET_..._Read @0xef2718).
            // Field offsets are OBSERVED from the client readers (dword_15E9C34 consumers):
            //   USER_INFO: +0 D id(!=0 => visible), +4 C level, +5 D exp, +9 C freeClaimed,
            //              +10 C premiumClaimed, +11 C premium, +20 C extra
            //   INFO_C:    +0 D id(!=0), +4 C state(1=ongoing,2=ended), +5 WSTR name(42B),
            //              +47 C maxLevel, table @ +53+16*(L-1) {+0 reqExp,+4 normal,+8 premiumA,+12 premiumB},
            //              +1657 D startDate, +1661 D endDate (both DECIMAL YYMMDDHHMM).
            //              +1653 W = battle-exp->season-exp divisor (sub_B34368); client's own
            //              reset (sub_C4E118) defaults it to 1, so send 1 (0 = div-by-zero).
            // NOTE: keep byte-for-byte identical to the Server.Game copy (same opcode 8450 parser).
            byte[] user = new byte[21];
            byte[] info = new byte[1665];

            if (Player != null && activeSeason != null && cardDataList != null && cardDataList.Count > 0)
            {
                var levelInfo = BattlePass.GetLevelInfoForSeason((int)Player.Battlepass.EarnedPoints);
                int currentLevel = levelInfo.currentLevel;
                int seasonId = int.TryParse(activeSeason.SeasonId, out int sid) ? sid : 1;
                if (seasonId == 0) seasonId = 1; // must be nonzero for the card to render
                byte state = (byte)(activeSeason.SeasonEnabled == 1 ? 1 : 2);

                var ordered = cardDataList.OrderBy(c => c.Number).ToList();
                int count = Math.Min(ordered.Count, 100); // table capacity: +53+16*100 <= 1661

                // USER_INFO_SEASON_CHALLENGE (21 bytes)
                BitConverter.GetBytes(seasonId).CopyTo(user, 0);                      // +0  id
                user[4] = (byte)currentLevel;                                         // +4  current level
                BitConverter.GetBytes(Player.Battlepass.EarnedPoints).CopyTo(user, 5);// +5  accumulated exp
                // +9/+10 are the claimed free/premium levels: the reward-slot updater
                // @0xE0C3EF only draws pImg_Mark_Reward while userInfo+9 (or +10 for the
                // premium slot) >= the slot's level, so zero leaves every level unmarked.
                // Both ends clamped: the DB column is a plain int4 with no CHECK, and a
                // negative would cast to 255 and mark the whole track as claimed.
                user[9] = (byte)Math.Max(0, Math.Min(255, Player.Battlepass.BattlepassNormalLevel));
                user[10] = (byte)Math.Max(0, Math.Min(255, Player.Battlepass.BattlepassPremiumLevel));
                user[11] = (byte)isPremium;                                           // +11 premium flag

                // SEASON_CHALLENGE_INFO_C (1665 bytes)
                BitConverter.GetBytes(seasonId).CopyTo(info, 0);                      // +0  id
                info[4] = state;                                                      // +4  state
                string displayName = seasonName ?? "";
                // Cap at 20 (=42/2-1): guarantees a trailing wide-null in the 42B field, so a
                // null-scanning client reader cannot bleed the name into +47 maxLevel.
                if (displayName.Length > 20) displayName = displayName.Substring(0, 20);
                byte[] nameBytes = System.Text.Encoding.Unicode.GetBytes(displayName);
                Array.Copy(nameBytes, 0, info, 5, Math.Min(nameBytes.Length, 42));    // +5  name (WSTR, 42B)
                info[47] = (byte)count;                                               // +47 max level count

                // Level table: level L (1-based) at +53 + 16*(L-1). Entry (16B), all OBSERVED:
                //   +0 D reqExp (progress denom @0xc4cf2e), +4 D normal reward, +8 D premium reward A,
                //   +12 D premium reward B (read as entry[3] @0xe0c079/0xe0c766; premium row shows iff A&&B).
                // Reward values gated by season free/premium flags (faithful to prior server semantics).
                bool free = activeSeason.SeasonEnabledForFree;
                bool prem = activeSeason.SeasonEnabledForPremium;
                for (int i = 0; i < count; i++)
                {
                    var card = ordered[i];
                    int off = 53 + 16 * i;
                    // reqExp semantics differ: JSON stores exp to COMPLETE card L, but the client
                    // (sub_C4CF2E via sub_C4E824) wants entry L = cumulative exp to REACH level L
                    // (progress = (exp - reqExp[L]) / (reqExp[L+1] - reqExp[L])). Shift by one:
                    // entry 1 = 0, entry L = RequiredExp of card L-1. Rewards stay on their level.
                    int reachExp = (i == 0) ? 0 : ordered[i - 1].RequiredExp;
                    BitConverter.GetBytes(reachExp).CopyTo(info, off + 0);
                    BitConverter.GetBytes(free ? card.NormalCard : 0).CopyTo(info, off + 4);
                    BitConverter.GetBytes(prem ? card.PremiumCardA : 0).CopyTo(info, off + 8);
                    BitConverter.GetBytes(prem ? card.PremiumCardB : 0).CopyTo(info, off + 12);
                }

                BitConverter.GetBytes((ushort)1).CopyTo(info, 1653);                  // +1653 exp divisor (neutral)

                // +1657 start / +1661 end date: client decodes both as DECIMAL YYMMDDHHMM
                // (header formatter sub_C4D53B reads global+1703/+1707 via sub_94C3AF:
                // yr=v/1e8+2000, mo=%1e8/1e6, day, hr, min), NOT unix. Raw uint.Parse of the
                // config strings is already the wire format.
                BitConverter.GetBytes(startDate).CopyTo(info, 1657);
                BitConverter.GetBytes(endDate).CopyTo(info, 1661);
            }
            else
            {
                CLogger.Print("SEASON_CHALLENGE_INFO_ACK: no active season/cards; sending empty (card hidden).", LoggerType.Warning);
            }

            this.WriteB(user);
            this.WriteB(info);
        }
    }
}