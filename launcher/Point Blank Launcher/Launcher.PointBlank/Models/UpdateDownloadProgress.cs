namespace Launcher.PointBlank.Models
{
    public class UpdateDownloadProgress
    {
        public string CurrentFile { get; set; }
        public long BytesReceived { get; set; }
        public long TotalBytes { get; set; }
        public int CurrentFileIndex { get; set; }
        public int TotalFiles { get; set; }
    }
}
