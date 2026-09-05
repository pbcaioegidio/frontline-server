using System.Collections.Generic;

namespace Launcher.PointBlank.Models
{
    public class FileCheckResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> InvalidFiles { get; set; }
        public FileCheckResult()
        {
            InvalidFiles = new List<string>();
        }
    }
}