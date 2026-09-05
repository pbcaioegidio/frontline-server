namespace Plugin.Core.Models
{
    // Neutral carrier for a system_channels row: Server.Auth and Server.Game each own their
    // own ChannelModel, so Plugin.Core cannot hand back either of them.
    public class ChannelRow
    {
        public int ServerId { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }
        public int MaxRooms { get; set; }
        public int ExpBonus { get; set; }
        public int GoldBonus { get; set; }
        public int CashBonus { get; set; }
        public string Password { get; set; }
    }
}
