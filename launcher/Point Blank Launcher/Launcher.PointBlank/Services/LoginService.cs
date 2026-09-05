using FL.Guard.Core.Identity;
using Launcher.PointBlank.Models;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.PointBlank.Services
{
    public class LoginService
    {
        private readonly LauncherConnection _connection;

        public LoginService(LauncherConnection connection)
        {
            _connection = connection;
        }

        /// <param name="tosAccepted">Versão do termo aceita nesta sessão (0 = não enviou).</param>
        public async Task<LoginResult> LoginAsync(string username, string password, int tosAccepted = 0)
        {
            string passwordMd5 = ComputeMd5(password);

            // Identidade da máquina (hashes; nada bruto sai do PC). Coleta em thread separada.
            HardwareComponents hw = await HardwareFingerprint.CollectAsync();

            string json = JsonConvert.SerializeObject(new
            {
                Username = username,
                Password = passwordMd5,
                Hwid = hw.Fingerprint,
                Components = hw.ToJson(),
                TosAccepted = tosAccepted,
                LauncherVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? ""
            });

            return await _connection.SendLoginAsync(json);
        }

        private static string ComputeMd5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
