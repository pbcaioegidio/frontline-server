using Plugin.Core.Enums;

namespace Plugin.Core.Models
{
    public class SlotMatch
    {
        public int Id { get; set; }
        public long PlayerId { get; set; }
        public SlotMatchState State { get; set; }
        public SlotMatch(int Id)
        {
            this.Id = Id;
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        