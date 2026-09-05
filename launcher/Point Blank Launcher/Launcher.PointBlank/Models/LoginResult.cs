namespace Launcher.PointBlank.Models
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        /// <summary>Token OTP de curta duração para FrontLine.exe /token.</summary>
        public string Token { get; set; }
        /// <summary>Código curto de suporte (ex.: FG-101).</summary>
        public string Code { get; set; }
        public bool RequireTos { get; set; }
        public int TosVersion { get; set; }
        public int TokenTtlSeconds { get; set; }
        public string SessionId { get; set; }
        public long PlayerId { get; set; }
    }
}
