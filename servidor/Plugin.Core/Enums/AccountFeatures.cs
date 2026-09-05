namespace Plugin.Core.Enums
{
    public enum AccountFeatures : uint
    {
        PLAYTIME_ONLY = 0x100,
        CLAN_ONLY = 0x1000,
        TICKET_ONLY = 0x4000,
        CLAN_COUPON = 0x777E,
        TAGS_ONLY = 0x4000000,
        TOKEN_ONLY = 0x7E770000,
        TOKEN_CLAN = 0x7E779934,
        RATING_BOTH = 0x7FFFFEF8,
        TEST_MODE = 0x7FFFFEF9,
        FROM_SNIFF = 0x87FD776E,
        ALL = 0x87FD776E,
    }
}
