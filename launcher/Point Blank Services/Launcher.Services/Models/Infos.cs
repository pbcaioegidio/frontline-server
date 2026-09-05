using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Services.Models
{
    public class Infos
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string LauncherVersion { get; set; }
        public string ClientVersion { get; set; }
        public bool Maintenance { get; set; }
        public string MaintenanceMessage { get; set; }
        public string ManifestUrl { get; set; }
    }
}
