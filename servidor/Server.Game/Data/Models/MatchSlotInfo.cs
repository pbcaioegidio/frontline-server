namespace Server.Game.Data.Models
{
    public sealed class MatchSlotInfo
    {
        public const int WireSize = 151;
        public const int ClanBlockSize = 35;
        public const int NickWireBytes = 66;
        public const uint EmptyPlayerId = 0xFFFFFFFFu;

        public byte State;
        public byte Rank;
        public uint ClanId;
        public uint ClanAccess;
        public byte ClanRankMark;
        public uint ClanLogo;
        public byte CafeIcon;
        public byte EsportsLevel;
        public ulong Effects;
        public byte ClanEffect;
        public byte ViewType;
        public byte Nations;
        public byte RankMarkIdx;
        public uint NameCardId;
        public byte NickOutlineColor;
        public byte ClanBuffStep;
        public byte[] ClanNameBlock;
        public uint PlayerId;
        public byte SlotIndex;
        public string Nick;
        public byte NickColor;
        public uint TeamType;
        public uint ClanMarkId;
        public byte ClanMarkColor;

        public MatchSlotInfo()
        {
            this.ClanNameBlock = new byte[ClanBlockSize];
            this.Nick = string.Empty;
        }

        public static MatchSlotInfo Empty(byte slotIndex)
        {
            return new MatchSlotInfo
            {
                PlayerId = EmptyPlayerId,
                SlotIndex = slotIndex,
            };
        }
    }
}
