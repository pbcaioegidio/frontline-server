namespace Launcher.Services.Models
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        /// <summary>Token OTP (Base64) para FrontLine.exe /token. Expira em poucos minutos.</summary>
        public string Token { get; set; }

        /// <summary>Código curto para suporte (ex.: FG-101). Vazio em sucesso.</summary>
        public string Code { get; set; }

        /// <summary>Quando true o launcher deve mostrar o termo de uso e reenviar com TosAccepted.</summary>
        public bool RequireTos { get; set; }

        /// <summary>Versão do termo exigida pelo servidor.</summary>
        public int TosVersion { get; set; }

        /// <summary>Segundos de validade do token.</summary>
        public int TokenTtlSeconds { get; set; }

        /// <summary>Id de sessão do launcher (heartbeat).</summary>
        public string SessionId { get; set; }

        public long PlayerId { get; set; }
    }
}
