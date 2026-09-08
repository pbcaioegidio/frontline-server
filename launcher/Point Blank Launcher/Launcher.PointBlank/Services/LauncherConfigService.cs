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
            {
                // Instalador antigo excluia o .svl — cria baseline pra nao travar o jogador.
                var created = new LauncherConfig { LauncherVersion = "0" };
                Save(created);
                return created;
            }

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

        public void Save(LauncherConfig config)
        {
            string encryptedPath = Path.Combine(_folderPath, EncryptedFileName);
            string xml =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<LauncherConfig>\r\n" +
                "  <LauncherVersion>" + (config?.LauncherVersion ?? "0") + "</LauncherVersion>\r\n" +
                "</LauncherConfig>\r\n";
            ConfigCryptoService.EncryptFile(encryptedPath, xml);
        }
    }
}
