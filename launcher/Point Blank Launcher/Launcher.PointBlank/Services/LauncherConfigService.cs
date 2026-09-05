using Launcher.PointBlank.Models;
using System.IO;
using System.Xml.Linq;

namespace Launcher.PointBlank.Services
{
    public class LauncherConfigService
    {
        public const string EncryptedFileName = "launcher.svl";

        private readonly string _folderPath;
        public LauncherConfigService(string folderPath)
        {
            _folderPath = folderPath;
        }
        public static LauncherConfigService FromFolder(string folderPath)
        {
            return new LauncherConfigService(folderPath);
        }
        public LauncherConfig Load()
        {
            string encryptedPath = Path.Combine(_folderPath, EncryptedFileName);

            if (!File.Exists(encryptedPath))
                throw new FileNotFoundException("Arquivo de versão do launcher encriptado não encontrado.", encryptedPath);

            XDocument document = XDocument.Parse(ConfigCryptoService.DecryptFile(encryptedPath));

            XElement root = document.Element("LauncherConfig");
            if (root == null)
                return new LauncherConfig();
            XElement versionElement = root.Element("LauncherVersion");
            return new LauncherConfig
            {
                LauncherVersion = versionElement != null
                    ? versionElement.Value.Trim()
                    : "0"
            };
        }
    }
}
