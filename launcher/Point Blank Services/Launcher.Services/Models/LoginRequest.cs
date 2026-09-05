namespace Launcher.Services.Models
{
    public class LoginRequest
    {
        public string Username { get; set; }
        /// <summary>MD5 hex da senha.</summary>
        public string Password { get; set; }

        /// <summary>Fingerprint composto da máquina (SHA-256 hex, 64). Vazio em launcher antigo.</summary>
        public string Hwid { get; set; }

        /// <summary>JSON de FL.Guard.Core.Identity.HardwareComponents (hashes por componente).</summary>
        public string Components { get; set; }

        /// <summary>Versão do termo de uso aceita nesta sessão (0 = não enviou).</summary>
        public int TosAccepted { get; set; }

        /// <summary>Versão do launcher (informativo/auditoria).</summary>
        public string LauncherVersion { get; set; }
    }
}
