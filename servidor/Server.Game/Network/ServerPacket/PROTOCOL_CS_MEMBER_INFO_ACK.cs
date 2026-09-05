using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Utility;
using Server.Game.Data.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_CS_MEMBER_INFO_ACK : GameServerPacket
    {
        private readonly List<Account> listAccount;

        public PROTOCOL_CS_MEMBER_INFO_ACK(List<Account> ListAccount) => listAccount = ListAccount;

        public override void Write()
        {
            try
            {
                if (listAccount == null)
                {
                    WriteH(845);
                    WriteC(0);
                }
                else if (listAccount != null)
                {
                    WriteH(845);
                    WriteC((byte)listAccount.Count);
                    foreach (Account account in listAccount)
                    {
                        if (account != null && account.Nickname != string.Empty && account.PlayerId >= 1 && account.Statistic != null && account.Equipment != null && account.Bonus != null)
                        {
                            WriteC((byte)(account.Nickname.Length + 1));
                            WriteN(account.Nickname, account.Nickname.Length + 2, "UTF-16LE");
                            WriteQ(account.PlayerId);
                            WriteQ(ComDiv.GetClanStatus(account.Status, account.IsOnline));
                            WriteC((byte)account.Rank);
                            WriteD(account.Statistic?.Clan?.MatchWins ?? 0);
                            WriteD(account.Statistic?.Clan?.MatchLoses ?? 0);
                            WriteD(account.Equipment.NameCardId);
                            WriteC((byte)account.Bonus.NickBorderColor);
                            WriteD(10);
                            WriteD(20);
                            WriteD(30);
                        }
                    }
                }
                else
                {
                    CLogger.Print($"else listAccount == null PROTOCOL_CS_MEMBER_INFO_ACK", LoggerType.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
        }
    }
}