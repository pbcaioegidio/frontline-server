using Microsoft.Win32.SafeHandles;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using Plugin.Core.Models;
using Plugin.Core.Utility;
using System;
using System.IO;
using Server.Game.Data.Models;

namespace Server.Game.Network
{
    public abstract class GameServerPacket : BaseServerPacket, IDisposable
    {
        public GameServerPacket()
        {
            MStream = new MemoryStream();
            BWriter = new BinaryWriter(MStream);
            Disposed = false;
            SECURITY_KEY = Bitwise.CRYPTO[0];
            HASH_CODE = Bitwise.CRYPTO[1];
            SEED_LENGTH = Bitwise.CRYPTO[2];
            NATIONS = ConfigLoader.National;
        }

        public byte[] GetBytes(string Name)
        {
            try
            {
                Write();
                return MStream.ToArray();
            }
            catch (Exception Ex)
            {
                CLogger.Print($"GetBytes problem at: {Name}; {Ex.Message}", LoggerType.Error, Ex);
                return new byte[0];
            }
        }

        public byte[] GetCompleteBytes(string Name)
        {
            try
            {
                byte[] Data = GetBytes("GameServerPacket.GetCompleteBytes");
                if (Data.Length >= 2)
                {
                    byte[] Length = BitConverter.GetBytes(Convert.ToUInt16(Data.Length - 2));
                    byte[] Buffer = new byte[Data.Length + 2];
                    Array.Copy(Length, 0, Buffer, 0, Length.Length);
                    Array.Copy(Data, 0, Buffer, Length.Length, Data.Length);
                    return Buffer;
                }
                return new byte[0];
            }
            catch (Exception ex)
            {
                CLogger.Print($"GetCompleteBytes problem at: {Name}; {ex.Message}", LoggerType.Error, ex);
                return new byte[0];
            }
        }

        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (Disposed)
                {
                    return;
                }
                MStream.Dispose();
                BWriter.Dispose();
                Disposed = true;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
            }
        }

        // Shared RoomInfo body block (71B basic + 175B option = 246B) emitted by
        // ROOM_CREATE_ACK (3593) and LOBBY_STAGE_RULE_ACK (2566). Extracted verbatim
        // from PROTOCOL_ROOM_CREATE_ACK.Write() so both packets stay byte-identical;
        // the 71/175 split is opaque on the wire (client reads two raw blocks).
        protected void WriteRoomInfoBlock(RoomModel room)
        {
            this.WriteD(room.RoomId);
            this.WriteU(room.Name, 46);
            this.WriteC((byte)room.MapId);
            this.WriteC((byte)room.Rule);
            this.WriteC((byte)room.Stage);
            this.WriteC((byte)room.RoomType);
            this.WriteC((byte)room.State);
            this.WriteC((byte)room.GetCountPlayers());
            this.WriteC((byte)room.GetSlotCount());
            this.WriteC((byte)room.Ping);
            this.WriteH((ushort)room.WeaponsFlag);
            this.WriteD(room.GetFlag());
            this.WriteH((short)0);
            this.WriteD(room.NewInt);
            this.WriteH((short)0);
            this.WriteU(room.LeaderName, 66);
            this.WriteD(room.KillTime);
            this.WriteC(room.Limit);
            this.WriteC(room.WatchRuleFlag);
            this.WriteH((ushort)room.BalanceType);
            this.WriteB(room.RandomMaps);
            this.WriteC(room.CountdownIG == 0 ? (byte)5 : (byte)5);
            this.WriteB(room.LeaderAddr);
            this.WriteC(room.KillCam);
            this.WriteH((short)0);
            this.WriteB(new byte[68]);
            // Modo Desafio/AI: client precisa de AiCount+AiLevel no fim do bloco
            // (igual PROTOCOL_ROOM_CHANGE_ROOMINFO_ACK). Sem isso a partida inicia sem bots.
            if (room.IsBotMode())
            {
                byte aiCount = room.AiCount == 0 ? (byte)8 : room.AiCount;
                byte aiLevel = room.AiLevel == 0 ? (byte)1 : room.AiLevel;
                this.WriteC(aiCount);
                this.WriteC(aiLevel);
            }
        }

        protected void WriteMatchStageRule(MatchStageRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));
            this.WriteC(rule.ConfigA);
            this.WriteC(rule.ConfigB);
            this.WriteC(rule.ConfigC);
            for (int i = 0; i < MatchStageRule.StageCount; i++)
                this.WriteD(rule.StageIds[i]);
        }

        protected void WriteMatchSlotInfo(MatchSlotInfo s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (s.ClanNameBlock == null || s.ClanNameBlock.Length != MatchSlotInfo.ClanBlockSize)
                throw new ArgumentException("MatchSlotInfo.ClanNameBlock must be exactly 35 bytes", nameof(s));
            string nick = s.Nick ?? string.Empty;
            if (nick.Length > 32)
                throw new ArgumentException("MatchSlotInfo.Nick exceeds 32 UTF-16 units", nameof(s));
            if (nick.IndexOf('\0') >= 0)
                throw new ArgumentException("MatchSlotInfo.Nick contains an embedded null", nameof(s));

            this.WriteC(s.State);
            this.WriteC(s.Rank);
            this.WriteD(s.ClanId);
            this.WriteD(s.ClanAccess);
            this.WriteC(s.ClanRankMark);
            this.WriteD(s.ClanLogo);
            this.WriteC(s.CafeIcon);
            this.WriteC(s.EsportsLevel);
            this.WriteQ(s.Effects);
            this.WriteC(s.ClanEffect);
            this.WriteC(s.ViewType);
            this.WriteC(s.Nations);
            this.WriteC(s.RankMarkIdx);
            this.WriteD(s.NameCardId);
            this.WriteC(s.NickOutlineColor);
            this.WriteC(s.ClanBuffStep);
            this.WriteB(s.ClanNameBlock);
            this.WriteD(s.PlayerId);
            this.WriteC(s.SlotIndex);
            this.WriteU(nick, MatchSlotInfo.NickWireBytes);
            this.WriteC(s.NickColor);
            this.WriteD(s.TeamType);
            this.WriteD(s.ClanMarkId);
            this.WriteC(s.ClanMarkColor);
        }

        // S2MO StrW<max> field node: [count:u8][wchar_t x count], no terminator.
        // The 122 reader (e.g. 0xD13CA7 for StrW<17>) aborts the WHOLE packet parse
        // when count > max, so clamping here is a correctness guard, not cosmetics.
        protected void WriteStrW(string text, int max)
        {
            string value = text ?? string.Empty;
            if (value.Length > max)
                value = value.Substring(0, max);
            this.WriteC((byte)value.Length);
            if (value.Length > 0)
                this.WriteU(value, value.Length * 2);
        }

        // S2MO S2MOValue<T,N> array field node prefix. Count width follows N:
        // u8 for N < 255, u16 for N < 65535 (reader 0x10462C0). Every clan-war
        // array is well under 255, so u8; count > N aborts the parse.
        protected void WriteS2MOCount(int count, int max)
        {
            this.WriteC((byte)(count > max ? max : count));
        }

        // MATCH_TEAM_INFO, 6 bytes, shared by CREATE_TEAM_ACK (6919) and
        // MATCH_TEAM_LIST_ACK (6917). The client bulk-copies it off the wire and
        // expands it at 0xD1F674 into {u16 match_id, u8 friend_id, u32 state,
        // u8 training, u8 player_count}:
        //   +0 u16 match_id     — the id JOIN_TEAM_REQ (6920) echoes back (0xAA0B21)
        //   +2 u8  friend_id    — squad number, displayed as friend_id+1 (0xD1F4FC)
        //   +3 u8  state        — MatchState; 0 (Invisible) reads as "no match" (0x9C60C6)
        //   +4 u8  training     — max players; the full check is [+5] == [+4] (0xAA0A6C)
        //   +5 u8  player_count — current members
        protected void WriteMatchTeamInfo(MatchModel match)
        {
            this.WriteH((ushort)match.MatchId);
            this.WriteC((byte)match.FriendId);
            this.WriteC((byte)match.State);
            this.WriteC((byte)match.Training);
            this.WriteC((byte)match.GetCountPlayers());
        }

        // ITEM_INFO[3] (25B) + CHAR_EQUIP_INFO (144B) + uchar[3] (4B) = 173 bytes, the three
        // S2MO member nodes shared by ROOM_GET_PLAYERINFO_ACK (3597) and
        // ROOM_GET_ACEMODE_PLAYERINFO_ACK (3682). Both packets derive from
        // PACKET_ROOM_GET_PLAYERINFO_BASE, whose ctor 0xED8728 builds the member list by
        // prepending, so the serializer walks it in reverse registration order:
        //   ITEM_INFO[3] (this+103) -> CHAR_EQUIP_INFO (this+64) -> uchar[3] (this+60)
        //   -> USER_INFO_BASIC (this+10) -> uchar[1] (this+6)
        // with each derived ctor's own nodes ahead of all of them. That model reproduces the
        // official 3597 capture exactly: 8 header + 8 + 96 + 25 + 144 + 4 + 185 + 2 = 472.
        protected void WriteRoomEquipBlocks(Account account)
        {
            PlayerInventory inv = account.Inventory;
            PlayerEquipment eq = account.Equipment;

            this.WriteC((byte)3);
            this.WriteB(inv.EquipmentDataChara(eq.DinoItem));
            this.WriteB(inv.EquipmentDataChara(eq.SprayId));
            this.WriteB(inv.EquipmentDataChara(eq.NameCardId));

            // 18 x 8B: [0..4] weapons, [5] Arma Especial 2, [6..7] reserved, [8] team character, [9..17] parts
            byte[][] block = new byte[18][];
            for (int i = 0; i < 18; i++)
                block[i] = new byte[8];
            block[0] = inv.EquipmentDataChara(eq.WeaponPrimary);
            block[1] = inv.EquipmentDataChara(eq.WeaponSecondary);
            block[2] = inv.EquipmentDataChara(eq.WeaponMelee);
            block[3] = inv.EquipmentDataChara(eq.WeaponExplosive);
            block[4] = inv.EquipmentDataChara(eq.WeaponSpecial);
            block[5] = inv.EquipmentDataChara(eq.WeaponSpecial2);
            block[8] = TeamCharaEntry(account, inv, eq);
            block[9] = inv.EquipmentDataChara(eq.PartHead);
            block[10] = inv.EquipmentDataChara(eq.PartFace);
            block[11] = inv.EquipmentDataChara(eq.PartJacket);
            block[12] = inv.EquipmentDataChara(eq.PartPocket);
            block[13] = inv.EquipmentDataChara(eq.PartGlove);
            block[14] = inv.EquipmentDataChara(eq.PartBelt);
            block[15] = inv.EquipmentDataChara(eq.PartHolster);
            block[16] = inv.EquipmentDataChara(eq.PartSkin);
            block[17] = inv.EquipmentDataChara(eq.BeretItem);
            for (int i = 0; i < 18; i++)
                this.WriteB(block[i]);

            this.WriteC((byte)3);
            this.WriteB(new byte[3]);
        }

        // Which character model shows depends on the player's team, so it needs the room slot.
        // A player with no slot still owes the wire its 8 bytes: returning an empty array here
        // would shorten CHAR_EQUIP_INFO to 136 and desynchronise everything after it.
        private static byte[] TeamCharaEntry(Account account, PlayerInventory inv, PlayerEquipment eq)
        {
            RoomModel room = account.Room;
            SlotModel slot;
            if (room == null || !room.GetSlot(account.SlotId, out slot))
                return new byte[8];
            int itemId = room.ValidateTeam(slot.Team, slot.CostumeTeam) == TeamEnum.FR_TEAM
                ? eq.CharaRedId
                : eq.CharaBlueId;
            return inv.EquipmentDataChara(itemId);
        }

        // USER_INFO_BASIC, the 185-byte account struct. Four packets carry it verbatim
        // (2371 MYINFO_BASIC, 2317 GET_USER_INFO, 3597 ROOM_GET_PLAYERINFO, 3682
        // ROOM_GET_ACEMODE_PLAYERINFO) and each client handler bulk-copies it with a single
        // qmemcpy(dst, src, 0xB9): 0xEBFC09, 0xEC10C9, 0xEDC0B0 and 0xEDC160. Nothing in it is
        // parsed field by field on the wire, so the byte map below is the whole contract and it
        // belongs in one place. (2317 lives in Server.Auth and keeps its own copy; 5140
        // ENDBATTLE re-encodes the same fields with length-prefixed strings and cannot use this.)
        //
        // The map comes from the 89 callers of the one deobfuscating accessor sub_88A53E
        // (0x88A53E) — the struct initialisers sub_889A01 / sub_889A51 are NOT field
        // boundaries, their dword store at 141 spans pccafe, 142 and the first wchar of the
        // clan name. It is confirmed end to end against an official server capture of 3597
        // (artifacts/official-3597, block at wire +285): clan name at 143 ("McCalls"),
        // tag at 105, cash at 121, clan id at 125, access at 129, rank/unit at 177/178 and
        // logo at 179, sent as -1 when the account has no clan.
        //
        // OBSERVED (offset, consumer):
        //     0 nick wchar[33]  sub_88AE10 -> sub_888FF9(base, 33)
        //    66 rank            UIFloatReadyRoomUserInfo__RefreshDisplay 0xAD880E -> CharToRankIconIndex;
        //                       0xEBFC75 asserts it did not change across this packet
        //    70 rank high-water Parse_Base_RankUp 0xEC3188 (gates the rank-up reward popup)
        //    74 point           UITopMenu__RefreshMoney 0x8F50D3 slot 0 (STR_SHOP_INFO_POINT);
        //                       Parse_Base_RankUp 0xEC31AD adds the reward here (32-bit add)
        //    78 core            same, slot 3 (STR_TBL_GUI_ETC_MENUBAR_CORE) — a field of its
        //                       own, not the high half of a 64-bit point
        //    82 exp QWORD       UITopMenu__SetUserExp 0x8F7F47, RankUI__UpdateRankProgress 0xB69529
        //    94 battle flag     Battle__Recv_5189 0xEE4EF7 writes it, GameMode__IsPlayerSelecting reads ==1
        //    95 QWORD latch     UIMainFrame__EnableClanGuide 0x9C83B5 tests it and writes (1,0)
        //   105 tag             UITopMenu__RefreshMoney slot 2 (STR_TBL_GUI_ETC_MENUBAR_TAG)
        //   109 tag progress    sub_E1592F 0xE15A1F -> prog_Tag / text_Tag_GetCountValue
        //   115 inventory plus  sub_88A1F2 getter, sub_889ECB adder (the 3337 delta target)
        //   117 dismantle slot  written by the client itself in sub_ED8221
        //   121 cash            UITopMenu__RefreshMoney slot 1 (STR_SHOP_INFO_CASH)
        //   125 clan id         sub_C3F60F; RefreshDisplay treats 0 as "no clan"
        //   129 clan access     sub_88A440, ClanContext__HandleChatModeSwitch
        //   141 pccafe byte     sub_EBE2DD (PacketBaseChangePccafeStatusAck)
        //   143 clan name       wchar[17], 34 bytes: Clan__OnReplaceNameResult 0xEE893D does
        //                       sub_AC57CF(base + 143, name, 17); RefreshDisplay 0xAD88E7 passes
        //                       base + 143 as the string; sub_88BA80 clears 143..177 on leave
        //   177 clan rank, 178 clan unit, 179 clan logo (-1 = none), 183 mark colour, 184 effect
        //
        // UNKNOWN: 103 and 104 have a reader (sub_88AAC8, from Base__HandleGetUserInfoAck) whose
        // expected value is not identified, and 90, 111, 133, 137 and 142 have no reader at all
        // among the callers of sub_88A53E. None of them may be invented, so they go out as zero;
        // the same holds for 94, 95 and 117, which the client owns and writes itself. 78 and 109
        // have a real consumer but no identified server-side source yet, so they are zero too.
        // (The official capture puts a timestamp at 111 and 1 at 95; neither is reproduced here
        // because their semantics are not established.)
        protected void WriteUserInfoBasicBlock(Account account, ClanModel clan)
        {
            this.WriteU(account.Nickname, 66);              // 0
            this.WriteD(account.GetDisplayRank());          // 66
            this.WriteD(account.GetRank());                 // 70
            this.WriteD(account.Gold);                      // 74  point
            this.WriteD(0);                                 // 78  core
            this.WriteQ((ulong)account.Exp);                // 82
            this.WriteD(0);                                 // 90
            this.WriteC(0);                                 // 94
            this.WriteQ(0UL);                               // 95  clan-guide latch
            this.WriteC(0);                                 // 103
            this.WriteC(0);                                 // 104
            this.WriteD(account.Tags);                      // 105 tag
            this.WriteH((short)0);                          // 109 tag progress
            this.WriteD(0);                                 // 111
            this.WriteH((ushort)account.InventoryPlus);     // 115
            this.WriteD(0);                                 // 117
            this.WriteD(account.Cash);                      // 121
            this.WriteD(clan.Id);                           // 125
            this.WriteD(account.ClanAccess);                // 129
            this.WriteD(0);                                 // 133
            this.WriteD(0);                                 // 137
            this.WriteC((byte)account.CafePC);              // 141 pccafe
            this.WriteC(0);                                 // 142
            this.WriteU(clan.Name, 34);                     // 143 clan name, wchar[17]
            this.WriteC((byte)clan.Rank);                   // 177
            this.WriteC((byte)clan.GetClanUnit());          // 178
            this.WriteD(clan.Id == 0 ? -1 : (int)clan.Logo); // 179 logo, -1 = none
            this.WriteC((byte)clan.NameColor);              // 183
            this.WriteC((byte)clan.Effect);                 // 184
        }

        public abstract void Write();
    }
}