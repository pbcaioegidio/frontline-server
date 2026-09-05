namespace Launcher.PointBlank.Models
{
    public class FileCheckProgress
    {
        public string CurrentFile { get; set; }

        public int CurrentIndex { get; set; }
        public int TotalFiles { get; set; }

        public int FileBarWidth { get; set; }
        public int TotalBarWidth { get; set; }

        public string StatusMessage { get; set; }
    }
}