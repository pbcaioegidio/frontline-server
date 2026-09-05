namespace Plugin.Core.Models
{
    public class MissionSlot
    {
        public const int ProgressSize = 40;

        public int CardSetId { get; set; }
        public int CurrentCard { get; set; }
        public byte[] Progress { get; set; } = new byte[ProgressSize];

        public bool IsEmpty => this.CardSetId == 0;

        public void Reset()
        {
            this.CardSetId = 0;
            this.CurrentCard = 0;
            this.Progress = new byte[ProgressSize];
        }

        public static byte[] NormalizeProgress(byte[] raw)
        {
            byte[] p = new byte[ProgressSize];
            if (raw != null && raw.Length > 0)
                System.Array.Copy(raw, p, System.Math.Min(raw.Length, ProgressSize));
            return p;
        }
    }
}
