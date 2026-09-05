using Plugin.Core.Utility;

namespace Plugin.Core.Models
{
    public class PlayerMissions
    {
        public const int SlotCount = 4;

        public long OwnerId { get; set; }
        public int ActualMission { get; set; }
        public bool SelectedCard { get; set; }

        private readonly MissionSlot[] Slots;

        public PlayerMissions()
        {
            this.Slots = new MissionSlot[SlotCount];
            for (int i = 0; i < SlotCount; i++)
                this.Slots[i] = new MissionSlot();
        }

        public MissionSlot this[int index]
        {
            get
            {
                if (index < 0 || index >= SlotCount)
                    index = 0;
                return this.Slots[index];
            }
        }

        public MissionSlot Current => this[this.ActualMission];

        public int OwnedCardSetMask
        {
            get
            {
                int mask = 0;
                for (int i = 0; i < SlotCount; i++)
                {
                    int id = this.Slots[i].CardSetId;
                    if (id > 0 && id < 32)
                        mask |= 1 << id;
                }
                return mask;
            }
        }

        public bool OwnsCardSet(int cardSetId)
        {
            if (cardSetId <= 0)
                return false;
            for (int i = 0; i < SlotCount; i++)
                if (this.Slots[i].CardSetId == cardSetId)
                    return true;
            return false;
        }

        public int FindFreeSlot()
        {
            for (int i = 0; i < SlotCount; i++)
                if (this.Slots[i].IsEmpty)
                    return i;
            return -1;
        }

        public int FirstOwnedSlot()
        {
            for (int i = 0; i < SlotCount; i++)
                if (!this.Slots[i].IsEmpty)
                    return i;
            return 0;
        }

        public void NormalizeActiveSlot()
        {
            if (this.Current.IsEmpty)
                this.ActualMission = this.FirstOwnedSlot();
        }

        public void UpdateSelectedCard()
        {
            MissionSlot slot = this.Current;
            if (ushort.MaxValue != ComDiv.GetMissionCardFlags(slot.CardSetId, slot.CurrentCard, slot.Progress))
                return;
            this.SelectedCard = true;
        }
    }
}
