using Server.Game.Data.Managers;
using Server.Game.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;


namespace Server.Game.Network.ServerPacket
{
    public class PROTOCOL_ROOM_GET_LOBBY_USER_LIST_ACK : GameServerPacket
    {
        private readonly List<Account> Players;

        public PROTOCOL_ROOM_GET_LOBBY_USER_LIST_ACK(List<Account> players)
        {
            this.Players = players ?? new List<Account>();
        }

        public override void Write()
        {
            int count = this.Players.Count;
            this.WriteH((short)3631);
            this.WriteD((uint)count);
            this.WriteD(0u);
            this.WriteD((uint)count);
            foreach (Account acc in this.Players)
            {
                this.WriteB(BuildLobbyUserRecord(acc, (int)this.NATIONS));
            }
        }

        private static byte[] BuildLobbyUserRecord(Account account, int nation)
        {
            byte[] record = new byte[120];
            var clan = ClanManager.GetClan(account.ClanId);

            WriteInt32(record, 0, account.GetSessionId());
            WriteInt32(record, 4, unchecked((int)clan.Logo));
            record[8] = (byte)clan.Effect;
            WriteUnicodeFixed(record, 9, clan.Name, 34);
            record[43] = (byte)account.GetRank();
            record[44] = 0;
            record[45] = (byte)account.NickColor;
            WriteUnicodeFixed(record, 46, account.Nickname, 66);
            record[112] = (byte)nation;
            record[113] = 0;
            WriteInt32(record, 114, account.Equipment.NameCardId);
            record[118] = (byte)account.Bonus.NickBorderColor;
            record[119] = 0;

            return record;
        }

        private static void WriteInt32(byte[] buffer, int offset, int value)
        {
            BitConverter.GetBytes(value).CopyTo(buffer, offset);
        }

        private static void WriteUnicodeFixed(byte[] buffer, int offset, string value, int byteCount)
        {
            if (string.IsNullOrEmpty(value))
                return;

            byte[] bytes = Encoding.Unicode.GetBytes(value);
            Array.Copy(bytes, 0, buffer, offset, Math.Min(bytes.Length, Math.Max(0, byteCount - 2)));
        }
    }
}
