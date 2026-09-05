using Launcher.PointBlank.Models;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Launcher.PointBlank.Services
{
    public class LauncherUpdateCheckService
    {
        private readonly string _clientPath;

        public LauncherUpdateCheckService(string clientPath)
        {
            _clientPath = clientPath;
        }

        public List<UpdateFile> GetFilesToUpdate(UpdateManifest manifest)
        {
            List<UpdateFile> filesToUpdate = new List<UpdateFile>();

            foreach (UpdateFile file in manifest.Files)
            {
                string localPath = Path.Combine(
                    _clientPath,
                    file.Path.TrimStart('\\', '/')
                );

                if (!File.Exists(localPath))
                {
                    filesToUpdate.Add(file);
                    continue;
                }

                if (!string.IsNullOrEmpty(file.Md5) &&
                    !string.Equals(GetMd5(localPath), file.Md5, System.StringComparison.OrdinalIgnoreCase))
                {
                    filesToUpdate.Add(file);
                    continue;
                }
            }

            return filesToUpdate;
        }

        private string GetMd5(string filePath)
        {
            using (MD5 md5 = MD5.Create())
            using (FileStream stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                return System.BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();
            }
        }
    }
}