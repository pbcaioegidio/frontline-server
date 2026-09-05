namespace Server.Game.Network.ServerPacket
{
    // Client 122 (OBSERVED, hand-rolled handler 0xEF1514 — this opcode has no S2MO
    // packet class): opcode 6929 reads, in order,
    //   u32                      -> result           (0xEF1563, 4-byte stream read)
    //   u8  nickLen              -> clamped to 33    (0xEF1570, cmp/cmova against 0x21)
    //   wchar_t x nickLen        -> sender nickname  (0xEF1591, nickLen*2 bytes)
    //   u8  chatType             -> fed to the chat-add call at 0xEF162C
    //   u8  flag                 -> read at 0xEF15A9, unused by the handler
    //   u8  msgLen                                   (0xEF15B5)
    //   wchar_t x msgLen         -> message text     (0xEF15CA, msgLen*2 bytes)
    // Counts are the number of wide chars actually on the wire; the client's buffers
    // are pre-zeroed (memset at 0xEF153E/0xEF1550), so no terminator is transmitted.
    //
    // The 068 body opened with a single byte instead of the u32 and folded the
    // terminator into both counts.
    public class PROTOCOL_CLAN_WAR_TEAM_CHATTING_ACK : GameServerPacket
    {
        public const int MaxNickChars = 33;

        private readonly uint Result;
        private readonly string Nickname;
        private readonly string Message;
        private readonly byte ChatType;

        public PROTOCOL_CLAN_WAR_TEAM_CHATTING_ACK(string nickname, string message, byte chatType = 0)
        {
            this.Nickname = nickname ?? string.Empty;
            this.Message = message ?? string.Empty;
            this.ChatType = chatType;
        }

        public PROTOCOL_CLAN_WAR_TEAM_CHATTING_ACK(uint result)
        {
            this.Result = result;
            this.Nickname = string.Empty;
            this.Message = string.Empty;
        }

        public override void Write()
        {
            this.WriteH((short)6929);
            this.WriteD(this.Result);
            if (this.Result != 0U)
                return;
            string nick = this.Nickname.Length > MaxNickChars ? this.Nickname.Substring(0, MaxNickChars) : this.Nickname;
            this.WriteC((byte)nick.Length);
            if (nick.Length > 0)
                this.WriteU(nick, nick.Length * 2);
            this.WriteC(this.ChatType);
            this.WriteC((byte)0);
            this.WriteC((byte)this.Message.Length);
            if (this.Message.Length > 0)
                this.WriteU(this.Message, this.Message.Length * 2);
        }
    }
}
