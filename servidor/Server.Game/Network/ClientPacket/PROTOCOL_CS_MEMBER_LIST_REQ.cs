using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Models;
using Plugin.Core.Network;
using Plugin.Core.Utility;
using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using Server.Game.Network.ServerPacket;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ClientPacket
{
    public class PROTOCOL_CS_MEMBER_LIST_REQ : GameClientPacket
    {
        private byte Page;

        public override void Read() => Page = ReadC();

        public override void Run()
        {
            try
            {
                Account player = Client.GetAccount();
                if (player == null)
                {
                    CLogger.Print("PROTOCOL_CS_MEMBER_LIST_REQ: player is null", LoggerType.Warning);
                    return;
                }

                int ClanId = player.ClanId == 0 ? player.FindClanId : player.ClanId;
                ClanModel clanModel = ClanManager.GetClan(ClanId);

                if (clanModel == null || clanModel.Id == 0)
                {
                    Client.SendPacket(new PROTOCOL_CS_MEMBER_LIST_ACK(uint.MaxValue, byte.MaxValue, byte.MaxValue, new byte[0]));
                    return;
                }

                List<Account> clanPlayers = ClanManager.GetClanPlayers(ClanId, -1, false);
                if (clanPlayers == null)
                {
                    Client.SendPacket(new PROTOCOL_CS_MEMBER_LIST_ACK(uint.MaxValue, byte.MaxValue, byte.MaxValue, new byte[0]));
                    return;
                }

                using (SyncServerPacket syncServerPacket = new SyncServerPacket())
                {
                    byte Count = 0;
                    for (int i = (int)Page * 14; i < clanPlayers.Count; ++i)
                    {
                        WriteData(clanPlayers[i], syncServerPacket);
                        if (++Count == 14)
                        {
                            break;
                        }
                    }
                    Client.SendPacket(new PROTOCOL_CS_MEMBER_LIST_ACK(0U, Page, Count, syncServerPacket.ToArray()));
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_MEMBER_LIST_REQ " + ex.Message, LoggerType.Error, ex);
            }
        }

        private void WriteData(Account Member, SyncServerPacket s)
        {
            try
            {
                if (Member != null || s != null)
                {
                    s.WriteQ(Member.PlayerId);
                    s.WriteU(Member.Nickname, 66);
                    s.WriteC((byte)Member.Rank);
                    s.WriteC((byte)Member.ClanAccess);
                    s.WriteQ(ComDiv.GetClanStatus(Member.Status, Member.IsOnline));
                    s.WriteD(Member.ClanDate);
                    s.WriteC((byte)Member.NickColor);
                    s.WriteD(Member.Statistic?.Clan?.MatchWins ?? 0);
                    s.WriteD(Member.Statistic?.Clan?.MatchLoses ?? 0);
                    s.WriteD(Member.Equipment?.NameCardId ?? 0);
                    s.WriteC(0);
                    s.WriteD(10);
                    s.WriteD(20);
                    s.WriteD(30);
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("PROTOCOL_CS_MEMBER_LIST_REQ WriteData: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}