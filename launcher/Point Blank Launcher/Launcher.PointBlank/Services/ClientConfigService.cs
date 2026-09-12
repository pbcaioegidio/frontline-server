using Launcher.PointBlank.Models;
using System;
using System.IO;
using System.Xml.Linq;

namespace Launcher.PointBlank.Services
{
    public class ClientConfigService
    {
        public const string EncryptedFileName = "config.zpt";

        private readonly string _folderPath;
        public ClientConfigService(string folderPath)
        {
            _folderPath = folderPath;
        }
        public static ClientConfigService FromFolder(string folderPath)
        {
            return new ClientConfigService(folderPath);
        }
        public ClientConfig Load()
        {
            string encryptedPath = Path.Combine(_folderPath, EncryptedFileName);

            if (!File.Exists(encryptedPath))
                throw new FileNotFoundException("Arquivo de configuração encriptado não encontrado.", encryptedPath);

            XDocument document = XDocument.Parse(ConfigCryptoService.DecryptFile(encryptedPath));

            XElement root = document.Element("CLIENT_CONFIG");
            if (root == null)
            {
                return new ClientConfig();
            }
            XElement versionElement    = root.Element("CLIENT_VERSION");
            XElement ipElement         = root.Element("IP_ADRESS");
            XElement portElement       = root.Element("PORT");
            XElement executableElement = root.Element("EXECUTABLE");
            XElement base64Element     = root.Element("USE_BASE64_LOGIN");
            XElement localeElement     = root.Element("BASE64_LOCALE");
            XElement tokenElement      = root.Element("USE_TOKEN_LOGIN");

            int port = 9000;
            if (portElement != null)
                int.TryParse(portElement.Value.Trim(), out port);

            bool useBase64 = false;
            if (base64Element != null)
                bool.TryParse(base64Element.Value.Trim(), out useBase64);

            bool useToken = false;
            if (tokenElement != null)
                bool.TryParse(tokenElement.Value.Trim(), out useToken);

            return new ClientConfig
            {
                ClientVersion  = versionElement    != null ? versionElement.Value.Trim()    : "0",
                IpAddress      = ipElement         != null ? ipElement.Value.Trim()         : "127.0.0.1",
                Port           = port,
                Executable     = ResolveClientExecutable(_folderPath,
                    executableElement != null ? executableElement.Value.Trim() : "FrontLine.exe"),
                UseBase64Login = useBase64,
                Base64Locale   = localeElement     != null ? localeElement.Value.Trim()     : "locale-ph",
                UseTokenLogin  = useToken
            };
        }

        /// <summary>Prioriza FrontLine.exe se existir na pasta do client.</summary>
        public static string ResolveClientExecutable(string folderPath, string configured)
        {
            string frontLine = Path.Combine(folderPath, "FrontLine.exe");
            if (File.Exists(frontLine))
                return "FrontLine.exe";

            if (!string.IsNullOrWhiteSpace(configured)
                && File.Exists(Path.Combine(folderPath, configured)))
                return configured;

            return "FrontLine.exe";
        }

        /// <summary>Grava CLIENT_VERSION (e demais campos) no config.zpt após patch.</summary>
        public void Save(ClientConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            string encryptedPath = Path.Combine(_folderPath, EncryptedFileName);
            string xml =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<CLIENT_CONFIG>\n" +
                $"  <CLIENT_VERSION>{config.ClientVersion}</CLIENT_VERSION>\n" +
                $"  <IP_ADRESS>{config.IpAddress}</IP_ADRESS>\n" +
                $"  <PORT>{config.Port}</PORT>\n" +
                $"  <EXECUTABLE>{config.Executable}</EXECUTABLE>\n" +
                $"  <USE_BASE64_LOGIN>{config.UseBase64Login.ToString().ToLowerInvariant()}</USE_BASE64_LOGIN>\n" +
                $"  <USE_TOKEN_LOGIN>{config.UseTokenLogin.ToString().ToLowerInvariant()}</USE_TOKEN_LOGIN>\n" +
                "</CLIENT_CONFIG>\n";
            ConfigCryptoService.EncryptFile(encryptedPath, xml);
        }
    }
}
