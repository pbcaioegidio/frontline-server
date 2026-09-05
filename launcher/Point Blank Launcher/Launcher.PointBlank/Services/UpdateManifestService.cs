using Launcher.PointBlank.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Launcher.PointBlank.Services
{
    public class UpdateManifestService
    {
        public UpdateManifest LoadFromFile(string manifestPath)
        {
            if (!File.Exists(manifestPath))
                return new UpdateManifest();
            string json = File.ReadAllText(manifestPath);
            UpdateManifest manifest = JsonConvert.DeserializeObject<UpdateManifest>(json);
            return manifest ?? new UpdateManifest();
        }
        public async Task<UpdateManifest> LoadFromUrlAsync(string manifestUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                string json = await client.GetStringAsync(manifestUrl);
                UpdateManifest manifest = JsonConvert.DeserializeObject<UpdateManifest>(json);
                return manifest ?? new UpdateManifest();
            }
        }
    }
}