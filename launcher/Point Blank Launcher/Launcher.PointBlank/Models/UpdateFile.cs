using Newtonsoft.Json;

namespace Launcher.PointBlank.Models
{
    public class UpdateFile
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }
    }
}