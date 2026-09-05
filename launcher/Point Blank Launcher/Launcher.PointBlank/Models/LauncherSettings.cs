namespace Launcher.PointBlank.Models
{
    public class LauncherSettings
    {
        public bool UseBase64Login { get; set; }
        public string Locale { get; set; }

        public LauncherSettings()
        {
            UseBase64Login = false;
            Locale = "locale-ph";
        }
    }
}
