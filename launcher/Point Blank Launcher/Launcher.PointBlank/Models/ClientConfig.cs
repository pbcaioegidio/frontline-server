namespace Launcher.PointBlank.Models
{
    public class ClientConfig
    {
        public string ClientVersion { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Executable { get; set; }
        public bool UseBase64Login { get; set; }
        public string Base64Locale { get; set; }
        /// <summary>Client 122 BR: FrontLine.exe /token TOKEN /launcher FLLauncher</summary>
        public bool UseTokenLogin { get; set; }

        public ClientConfig()
        {
            ClientVersion = "0";
            IpAddress = "127.0.0.1";
            Port = 9000;
            Executable = "FrontLine.exe";
            UseBase64Login = false;
            Base64Locale = "locale-ph";
            UseTokenLogin = false;
        }
    }
}