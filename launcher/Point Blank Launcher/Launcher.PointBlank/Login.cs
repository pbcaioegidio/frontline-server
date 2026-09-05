using Launcher.PointBlank.Models;
using Launcher.PointBlank.Properties;
using Launcher.PointBlank.Services;
using Launcher.PointBlank.Utils;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher.PointBlank
{
    public partial class Login : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private readonly LauncherConnection _connection;
        private readonly LoginService _loginService;

        public string LoggedUsername { get; private set; }
        public string LoggedPassword { get; private set; }
        public string LoggedToken { get; private set; }
        public string LoggedSessionId { get; private set; }
        public long LoggedPlayerId { get; private set; }

        public Login(LauncherConnectionResult connectionResult, LauncherConnection connection)
        {
            InitializeComponent();
            AppIcon.Apply(this);
            LoadSavedCredentials();
            Shown += Login_Shown;
            _connection = connection;
            _loginService = new LoginService(connection);
        }
        public static bool TryGetSavedCredentials(out string username, out string password)
        {
            username = Settings.Default.SavedUsername ?? string.Empty;
            password = UnprotectPassword(Settings.Default.SavedPassword);

            return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrEmpty(password);
        }
        private void LoadSavedCredentials()
        {
            if (!TryGetSavedCredentials(out string savedUsername, out string savedPassword))
            {
                if (!string.IsNullOrWhiteSpace(Settings.Default.SavedUsername))
                {
                    Login_Box.Text = Settings.Default.SavedUsername;
                    LoginSave.Checked = true;
                }
                return;
            }

            Login_Box.Text = savedUsername;
            Password_Box.Text = savedPassword;
            LoginSave.Checked = true;
        }
        private void SaveCredentialsPreference(string username, string password)
        {
            if (LoginSave.Checked)
            {
                Settings.Default.SavedUsername = username;
                Settings.Default.SavedPassword = ProtectPassword(password);
            }
            else
            {
                Settings.Default.SavedUsername = string.Empty;
                Settings.Default.SavedPassword = string.Empty;
            }

            Settings.Default.Save();
        }
        private static string ProtectPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            try
            {
                byte[] plain = Encoding.UTF8.GetBytes(password);
                byte[] protectedBytes = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(protectedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
        private static string UnprotectPassword(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored))
                return string.Empty;

            try
            {
                byte[] protectedBytes = Convert.FromBase64String(stored);
                byte[] plain = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plain);
            }
            catch
            {
                return string.Empty;
            }
        }
        private void Login_Shown(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                TextBox focusedTextBox = LoginSave.Checked ? Password_Box : Login_Box;
                focusedTextBox.Focus();
                ClearTextSelection(Login_Box);
                ClearTextSelection(Password_Box);
            }));
        }
        private void ClearTextSelection(TextBox textBox)
        {
            textBox.SelectionStart = textBox.TextLength;
            textBox.SelectionLength = 0;
        }
        private void LoginFields_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            if (LoginButton.Enabled)
                LoginButton_Click(LoginButton, EventArgs.Empty);
        }
        #region ButtonEffects
        private void LoginButton_MouseEnter(object sender, EventArgs e)
        {
            LoginButton.BackgroundImage = Resources.Bitmap13;
            LoginButton.BackColor = Color.Transparent;
        }
        private void LoginButton_MouseDown(object sender, MouseEventArgs e)
        {
            LoginButton.BackgroundImage = Resources.Bitmap14;
            LoginButton.BackColor = Color.Transparent;
        }
        private void LoginButton_MouseLeave(object sender, EventArgs e)
        {
            LoginButton.BackgroundImage = Resources.Bitmap12;
            LoginButton.BackColor = Color.Transparent;
        }
        private void CloseButton_MouseEnter(object sender, EventArgs e)
        {
            CloseButton.BackgroundImage = Resources.Bitmap23;
            CloseButton.BackColor = Color.Transparent;
        }
        private void CloseButton_MouseLeave(object sender, EventArgs e)
        {
            CloseButton.BackgroundImage = Resources.Bitmap21;
            CloseButton.BackColor = Color.Transparent;
        }
        #endregion
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Login_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
        private async void LoginButton_Click(object sender, EventArgs e)
        {
            string username = Login_Box.Text.Trim();
            string password = Password_Box.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Preencha o usuário e a senha.", "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LoginButton.Enabled = false;

            try
            {
                LoginResult result = await _loginService.LoginAsync(username, password);

                // Termo de uso pendente: diálogo FL GUARD (sem MessageBox)
                if (!result.Success && result.RequireTos)
                {
                    bool accept = TermsDialog.ShowTerms(this);
                    if (accept)
                        result = await _loginService.LoginAsync(username, password, result.TosVersion);
                    else
                    {
                        LoginButton.Enabled = true;
                        return;
                    }
                }

                if (result.Success)
                {
                    Logger.LogLoginSuccess();
                    SaveCredentialsPreference(username, password);
                    LoggedUsername = username;
                    LoggedPassword = password;
                    LoggedToken = result.Token ?? string.Empty;
                    LoggedSessionId = result.SessionId ?? string.Empty;
                    LoggedPlayerId = result.PlayerId;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    Logger.LogLoginFail();
                    MessageBox.Show(result.Message, "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoginButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogLoginFail();
                MessageBox.Show("Erro ao realizar login:\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoginButton.Enabled = true;
            }
        }
    }
}
