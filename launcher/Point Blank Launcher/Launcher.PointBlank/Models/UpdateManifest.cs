using System.Collections.Generic;
using Newtonsoft.Json;

namespace Launcher.PointBlank.Models
{
    public class UpdateManifest
    {
        [JsonProperty("clientVersion")]
        public string ClientVersion { get; set; }

        [JsonProperty("patchUrl")]
        public string PatchUrl { get; set; }

        [JsonProperty("files")]
        public List<UpdateFile> Files { get; set; }

        public UpdateManifest()
        {
            Files = new List<UpdateFile>();
        }
    }
}