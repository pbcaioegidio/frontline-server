using Launcher.PointBlank.Models;
using Launcher.PointBlank.Properties;
using Launcher.PointBlank.Services;
using Launcher.PointBlank.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Launcher.PointBlank
{
    public partial class Main : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        string startupPath = Application.StartupPath;
        private bool isChecking = false;
        private bool _versionOk = false;
        private bool _integrityOk = false;
        private bool _isLoggedIn = false;
        private string _loggedUsername = string.Empty;
        private string _loggedPassword = string.Empty;
        private string _loggedToken = string.Empty;
        private string _loggedSessionId = string.Empty;
        private long _loggedPlayerId;
        private GuardSession _guardSession;
        private readonly LauncherConnectionResult _connectionResult;
        private readonly LauncherConnection _connection;
        private NotifyIcon _tray;
        private ContextMenuStrip _trayMenu;
        private bool _allowExit;

        public Main(LauncherConnectionResult connectionResult, LauncherConnection connection)
        {
            InitializeComponent();
            AppIcon.Apply(this);
            _connectionResult = connectionResult;
            _connection = connection;
            Load += Main_Load;
            Shown += Main_Shown;
            Resize += Main_Resize;
            FormClosing += Main_FormClosing;
            FormClosed += (s, e) =>
            {
                try { _guardSession?.Dispose(); } catch { }
                _guardSession = null;
                _connection?.Dispose();
                SocketBootstrap.StopOwned();
                DisposeTray();
            };
            TEXT_STATUS.Text = "Você pode iniciar o jogo.";
            LogoutButton.Visible = false;
            TEXT_USER.Visible = false;
            InitTray();
        }

        private void InitTray()
        {
            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("Abrir launcher", null, (s, e) => RestoreFromTray());
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("Sair", null, (s, e) => RequestExit());

            _tray = new NotifyIcon
            {
                Text = "FRONTLINE · FL Guard",
                Visible = false,
                ContextMenuStrip = _trayMenu
            };
            try
            {
                if (Icon != null)
                    _tray.Icon = (Icon)Icon.Clone();
                else
                {
                    string icoPath = Path.Combine(Application.StartupPath, "Icon.ico");
                    if (File.Exists(icoPath))
                        _tray.Icon = new Icon(icoPath);
                }
            }
            catch { }

            _tray.DoubleClick += (s, e) => RestoreFromTray();
            _tray.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    RestoreFromTray();
            };
        }

        private void DisposeTray()
        {
            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
                _tray = null;
            }
            if (_trayMenu != null)
            {
                _trayMenu.Dispose();
                _trayMenu = null;
            }
        }

        private void HideToTray()
        {
            if (_tray == null) return;
            // Sem animação de minimizar do Windows — some na hora
            try { Opacity = 0; } catch { }
            ShowInTaskbar = false;
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            Hide();
            try { Opacity = 1; } catch { }
            _tray.Visible = true;
            _tray.Text = _isLoggedIn
                ? $"FRONTLINE · {_loggedUsername} · FL Guard"
                : "FRONTLINE · FL Guard";
        }

        private void RestoreFromTray()
        {
            if (_tray != null) _tray.Visible = false;
            try { Opacity = 1; } catch { }
            ShowInTaskbar = true;
            Show();
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            Activate();
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            // Evita animação/minimize nativo: manda direto pra bandeja
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
                HideToTray();
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_allowExit) return;
            // Jogo fechou / Application.Exit → sai de vez (sem perguntar)
            if (e.CloseReason == CloseReason.ApplicationExitCall ||
                e.CloseReason == CloseReason.TaskManagerClosing ||
                e.CloseReason == CloseReason.WindowsShutDown)
            {
                _allowExit = true;
                return;
            }
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HideToTray();
            }
        }

        private void RequestExit()
        {
            DialogResult result = MessageBox.Show(
                "Sair do launcher encerra o FL Guard.\nSe estiver no jogo, o servidor vai kickar.",
                "FRONTLINE",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;
            _allowExit = true;
            try { _guardSession?.Dispose(); } catch { }
            _guardSession = null;
            DisposeTray();
            Application.Exit();
        }
        #region Button Effects
        private void CheckButton_MouseEnter(object sender, EventArgs e)
        {
            if (isChecking)
                return;
            CheckButton.BackgroundImage = Resources.CheckEnter;
            CheckButton.BackColor = Color.Transparent;
        }
        private void CheckButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (isChecking)
                return;
            CheckButton.BackgroundImage = Resources.CheckDown;
            CheckButton.BackColor = Color.Transparent;
        }
        private void CheckButton_MouseLeave(object sender, EventArgs e)
        {
            if (isChecking)
                return;
            CheckButton.BackgroundImage = Resources.CheckLeave;
            CheckButton.BackColor = Color.Transparent;
        }
        private void StartButton_MouseEnter(object sender, EventArgs e)
        {
            StartButton.BackgroundImage = Resources.StartEnter;
            StartButton.BackColor = Color.Transparent;
        }
        private void StartButton_MouseDown(object sender, MouseEventArgs e)
        {
            StartButton.BackgroundImage = Resources.StartDown;
            StartButton.BackColor = Color.Transparent;
        }
        private void StartButton_MouseLeave(object sender, EventArgs e)
        {
            StartButton.BackgroundImage = Resources.StartLeave;
            StartButton.BackColor = Color.Transparent;
        }
        private void CloseButton_MouseEnter(object sender, EventArgs e)
        {
            CloseButton.BackgroundImage = Resources.Bitmap132;
            CloseButton.BackColor = Color.Transparent;
        }
        private void CloseButton_MouseLeave(object sender, EventArgs e)
        {
            CloseButton.BackgroundImage = Resources.Bitmap133;
            CloseButton.BackColor = Color.Transparent;
        }
        private void MinimizeButton_MouseEnter(object sender, EventArgs e)
        {
            MinimizeButton.BackgroundImage = Resources.Bitmap137;
            MinimizeButton.BackColor = Color.Transparent;
        }
        private void MinimizeButton_MouseLeave(object sender, EventArgs e)
        {
            MinimizeButton.BackgroundImage = Resources.Bitmap136;
            MinimizeButton.BackColor = Color.Transparent;
        }
        private void ConfigButton_MouseEnter(object sender, EventArgs e)
        {
            ConfigButton.BackgroundImage = Resources.Bitmap1270;
            ConfigButton.BackColor = Color.Transparent;
        }
        private void ConfigButton_MouseLeave(object sender, EventArgs e)
        {
            ConfigButton.BackgroundImage = Resources.Bitmap1271;
            ConfigButton.BackColor = Color.Transparent;
        }
        private void UpdateButton_MouseEnter(object sender, EventArgs e)
        {

            UpdateButton.BackgroundImage = Resources.UpdateEnter;
            UpdateButton.BackColor = Color.Transparent;
        }
        private void UpdateButton_MouseDown(object sender, MouseEventArgs e)
        {
            UpdateButton.BackgroundImage = Resources.UpdateDown;
            UpdateButton.BackColor = Color.Transparent;
        }
        private void UpdateButton_MouseLeave(object sender, EventArgs e)
        {

            UpdateButton.BackgroundImage = Resources.UpdateLeave;
            UpdateButton.BackColor = Color.Transparent;
        }
        private void LoginScreen_MouseEnter(object sender, EventArgs e)
        {
            LoginScreen.BackgroundImage = Resources.Bitmap16;
            LoginScreen.BackColor = Color.Transparent;
        }
        private void LoginScreen_MouseDown(object sender, MouseEventArgs e)
        {
            LoginScreen.BackgroundImage = Resources.Bitmap17;
            LoginScreen.BackColor = Color.Transparent;
        }
        private void LoginScreen_MouseLeave(object sender, EventArgs e)
        {
            LoginScreen.BackgroundImage = Resources.Bitmap15;
            LoginScreen.BackColor = Color.Transparent;
        }
        private void LogoutButton_MouseEnter(object sender, EventArgs e)
        {
            LogoutButton.BackgroundImage = Resources.Bitmap19;
            LogoutButton.BackColor = Color.Transparent;
        }
        private void LogoutButton_MouseDown(object sender, MouseEventArgs e)
        {
            LogoutButton.BackgroundImage = Resources.Bitmap20;
            LogoutButton.BackColor = Color.Transparent;
        }
        private void LogoutButton_MouseLeave(object sender, EventArgs e)
        {
            LogoutButton.BackgroundImage = Resources.Bitmap18;
            LogoutButton.BackColor = Color.Transparent;
        }
        #endregion
        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
        public void SetProgressBar(ulong received, ulong maximum)
        {
            if (FileBar.Width <= 463)
            {
                FileBar.Width = (int)(received * 463 / maximum);
            }
        }
        public void Set2ProgressBar(ulong received, ulong maximum)
        {
            if (TotalBar.Width <= 463)
            {
                TotalBar.Width = (int)(received * 463 / maximum);
            }
        }
        public void SetButtonsVisible(bool start, bool check, bool update)
        {
            StartButton.Visible = start;
            CheckButton.Visible = check;
            UpdateButton.Visible = update;
        }
        public void SetButtonsEnable(bool start, bool check, bool update)
        {
            StartButton.Enabled = start;
            CheckButton.Enabled = check;
            UpdateButton.Enabled = update;
        }
        private void ConfigButton_Click(object sender, EventArgs e)
        {
            string configPath = Path.Combine(startupPath, "FLConfig.exe");

            if (File.Exists(configPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = configPath,
                    WorkingDirectory = startupPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("FLConfig.exe não encontrado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            RequestExit();
        }
        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            HideToTray();
        }
        private async void CheckButton_Click(object sender, EventArgs e)
        {
            isChecking = true;
            CheckButton.BackgroundImage = Resources.CheckDisable;
            await StartFileCheckAsync();
        }
        private async Task StartFileCheckAsync()
        {
            CheckButton.BackgroundImage = Resources.CheckDisable; // check disable
            StartButton.BackgroundImage = Resources.StartDisable; // start disable

            SetButtonsEnable(false, false, false);
            SetButtonsVisible(true, true, false);

            TEXT_STATUS.Text = "FL Guard conferindo arquivos...";
            FILE_TEXT.Visible = true;
            FILE_TEXT.Text = "Arquivo";
            Logger.LogState(UpdaterState.UPDATER_STATE_PATCH_END, UpdaterState.UPDATER_STATE_FILE_CHECK);
            Logger.Log($"Verificando a integridade dos arquivos.");
            Dictionary<string, string> userFiles = LoadUserFile(Path.Combine(Application.StartupPath, "UserFileList.dat"));

            var checkService = new FileCheck(Application.StartupPath);
            // Progress<T> posta na UI depois do await — flag evita sobrescrever o texto final
            bool acceptProgress = true;

            var progress = new Progress<FileCheckProgress>(p =>
            {
                if (!acceptProgress)
                    return;

                FILE_TEXT.Text = $"Arquivo {p.CurrentFile}";

                FileBar.Width = p.FileBarWidth;
                TotalBar.Width = p.TotalBarWidth;

                Set2ProgressBar((ulong)p.CurrentIndex, (ulong)p.TotalFiles);
            });

            FileCheckResult result = await checkService.CheckFilesAsync(userFiles, progress);
            acceptProgress = false;

            if (!result.Success)
            {
                bool canRestore = result.InvalidFiles != null && result.InvalidFiles.Count > 0;
                if (canRestore)
                {
                    DialogResult choice = MessageBox.Show(
                        result.Message + "\n\nDeseja restaurar o padrão baixando estes arquivos do servidor?",
                        "FRONTLINE — FL Guard",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1);

                    if (choice == DialogResult.Yes)
                    {
                        bool restored = await RestoreIntegrityFilesAsync(result.InvalidFiles);
                        await ReportIntegrityAsync(result, restored);
                        if (restored)
                        {
                            TEXT_STATUS.Text = "FL Guard: conferindo de novo após restaurar...";
                            await StartFileCheckAsync();
                            return;
                        }
                    }
                    else
                    {
                        await ReportIntegrityAsync(result, restored: false);
                    }
                }
                else
                {
                    MessageBox.Show(result.Message, "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    await ReportIntegrityAsync(result, restored: null);
                }

                FinishCheckFailed(result.Message);
                return;
            }

            await ReportIntegrityAsync(result, restored: null);
            FinishCheckSuccess();
        }

        /// <summary>Envia FileCheck ao Socket para staff/IA (integrity_events).</summary>
        private async Task ReportIntegrityAsync(FileCheckResult result, bool? restored)
        {
            if (result == null || _connection == null) return;
            // Só reporta se falhou, removeu extras, ou restaurou — evita spam em check limpo
            bool hasExtras = result.RemovedExtras != null && result.RemovedExtras.Count > 0;
            if (result.Success && !hasExtras && restored != true) return;

            string launcherVer = "";
            try
            {
                launcherVer = _connectionResult?.LauncherVersion
                    ?? LauncherConfigService.FromFolder(Application.StartupPath).Load()?.LauncherVersion
                    ?? "";
            }
            catch { /* ignore */ }

            try
            {
                await _connection.SendIntegrityReportAsync(new
                {
                    player_id = _loggedPlayerId,
                    username = _loggedUsername ?? "",
                    ok = result.Success,
                    restored,
                    invalid = result.InvalidFiles ?? new System.Collections.Generic.List<string>(),
                    extras_removed = result.RemovedExtras ?? new System.Collections.Generic.List<string>(),
                    launcher_ver = launcherVer,
                    message = result.Message ?? ""
                });
            }
            catch (Exception ex)
            {
                Logger.Log("ReportIntegrity: " + ex.Message);
            }
        }

        /// <summary>
        /// Baixa do Socket só os arquivos inválidos do Check e grava no client.
        /// </summary>
        private async Task<bool> RestoreIntegrityFilesAsync(List<string> invalidFiles)
        {
            if (invalidFiles == null || invalidFiles.Count == 0)
                return false;
            if (_connection == null)
            {
                MessageBox.Show(
                    "Sem conexão com o servidor de patch (Socket).\nNão foi possível restaurar.",
                    "FRONTLINE",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            SetButtonsEnable(false, false, false);
            SetButtonsVisible(false, false, false);
            FILE_TEXT.Visible = true;
            FileBar.Width = 0;
            TotalBar.Width = 0;
            TEXT_STATUS.Text = "Restaurando arquivos padrão...";
            Logger.Log($"FL Guard restore: {invalidFiles.Count} arquivo(s).");

            try
            {
                var progress = new Progress<UpdateDownloadProgress>(p =>
                {
                    FILE_TEXT.Text = $"Arquivo {p.CurrentFile}";
                    FileBar.Width = p.TotalBytes > 0
                        ? (int)(p.BytesReceived * 463 / Math.Max(1, p.TotalBytes))
                        : 0;
                    TotalBar.Width = p.TotalFiles > 0
                        ? (int)(p.CurrentFileIndex * 463 / p.TotalFiles)
                        : 0;
                    TEXT_STATUS.Text =
                        $"Restaurando [{p.CurrentFileIndex}/{p.TotalFiles}] {Path.GetFileName(p.CurrentFile)}";
                });

                var downloadService = new PatchDownloadService(Application.StartupPath, _connection);
                bool needsRestart = await downloadService.DownloadRelativePathsAsync(invalidFiles, progress);

                if (needsRestart)
                {
                    TEXT_STATUS.Text = "Reiniciando para aplicar o launcher...";
                    MessageBox.Show(
                        "O launcher foi restaurado.\nVai fechar e abrir de novo.",
                        "FRONTLINE",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    SelfUpdateHelper.StartRestartAndExit(Application.StartupPath);
                    return false;
                }

                Logger.Log("FL Guard restore: download concluído.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"FL Guard restore falhou: {ex.Message}");
                MessageBox.Show(
                    "Não foi possível restaurar alguns arquivos.\n" + ex.Message,
                    "FRONTLINE",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                FinishCheckFailed("Falha ao restaurar. Tente de novo ou use Update.");
                return false;
            }
        }
        private void FinishCheckFailed(string message)
        {
            isChecking = false;
            _integrityOk = false;
            SetButtonsEnable(false, true, false);
            SetButtonsVisible(true, true, false);
            StartButton.BackgroundImage = Resources.StartLeave;
            CheckButton.BackgroundImage = Resources.CheckLeave;
            FILE_TEXT.Text = "Arquivo";
            TEXT_STATUS.Text = string.IsNullOrWhiteSpace(message)
                ? "Falha na verificação."
                : message;
            // Garante limpeza depois de qualquer Progress ainda na fila da UI
            BeginInvoke(new Action(() =>
            {
                FILE_TEXT.Text = "Arquivo";
            }));
        }
        private void FinishCheckSuccess()
        {
            isChecking = false;
            _integrityOk = true;
            FILE_TEXT.Visible = true;

            SetButtonsEnable(true, true, false);
            SetButtonsVisible(true, true, false);

            FileBar.Width = 463;
            TotalBar.Width = 463;

            FILE_TEXT.Text = "Arquivo";
            TEXT_STATUS.Text = "FL Guard ok. Você já pode jogar.";

            StartButton.BackgroundImage = Resources.StartLeave;
            CheckButton.BackgroundImage = Resources.CheckLeave;

            // Progress pendente pode rodar depois do await e repor o nome do arquivo
            BeginInvoke(new Action(() =>
            {
                FILE_TEXT.Text = "Arquivo";
                TEXT_STATUS.Text = "FL Guard ok. Você já pode jogar.";
            }));
        }
        public static Dictionary<string, string> LoadUserFile(string path)
        {
            Dictionary<string, string> strs = new Dictionary<string, string>();
            XmlDocument xmlDocument = new XmlDocument();
            if (!File.Exists(path))
            {
                Logger.Log($"COMMAND_FILE_CHECK_START: UserFileList.dat não encontrado em {path}.");
                return strs;
            }

            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fileStream.Length == 0)
                        return strs;

                    xmlDocument.Load(fileStream);
                }

                XmlNode listNode = xmlDocument.SelectSingleNode("/list");
                if (listNode == null)
                    return strs;

                foreach (XmlNode fileNode in listNode.ChildNodes)
                {
                    if (!"file".Equals(fileNode.Name) || fileNode.Attributes == null)
                        continue;

                    string filePath = GetAttributeValue(fileNode.Attributes, "local", "n");
                    string hash = GetAttributeValue(fileNode.Attributes, "hash", "m");

                    if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(hash))
                    {
                        Logger.Log($"COMMAND_FILE_CHECK_START: Entrada inválida no UserFileList.dat.");
                        continue;
                    }

                    if (!strs.ContainsKey(filePath))
                        strs.Add(filePath, hash);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"COMMAND_FILE_CHECK_START: Ocorreu um erro ao ler os arquivos da lista. {ex.Message}");
            }

            return strs;
        }
        private static string GetAttributeValue(XmlNamedNodeMap attributes, params string[] names)
        {
            foreach (string name in names)
            {
                XmlNode node = attributes.GetNamedItem(name);
                if (node != null)
                    return node.Value;
            }

            return string.Empty;
        }
        private async void StartButton_Click(object sender, EventArgs e)
        {
            if (isChecking)
            {
                MessageBox.Show("Espere o FL Guard terminar a verificação.", "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!_integrityOk)
            {
                isChecking = true;
                CheckButton.BackgroundImage = Resources.CheckDisable;
                await StartFileCheckAsync();
                if (!_integrityOk)
                    return;
            }
            if (!_isLoggedIn)
            {
                using (Login login = new Login(_connectionResult, _connection))
                {
                    if (login.ShowDialog(this) != DialogResult.OK)
                        return;

                    _loggedUsername = login.LoggedUsername;
                    _loggedPassword = login.LoggedPassword;
                    _loggedToken = login.LoggedToken;
                    _loggedSessionId = login.LoggedSessionId;
                    _loggedPlayerId = login.LoggedPlayerId;
                    _isLoggedIn = true;
                    SetLoggedInState(_loggedUsername);
                }
            }
            try
            {
                Logger.LogState(UpdaterState.UPDATER_STATE_PATCH_END, UpdaterState.UPDATER_STATE_UNKNOWN);
                ClientConfig config = ClientConfigService.FromFolder(Application.StartupPath).Load();
                string exePath = Path.Combine(Application.StartupPath, config.Executable);

                if (File.Exists(exePath))
                {
                    string args;
                    if (config.UseTokenLogin)
                    {
                        if (string.IsNullOrWhiteSpace(_loggedToken))
                        {
                            MessageBox.Show("Login sem token. Refaça o login.", "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        args = $"/token {_loggedToken} /launcher FLLauncher";
                    }
                    else if (config.UseBase64Login)
                    {
                        string combined = $"{_loggedUsername} {_loggedPassword}";
                        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(combined));
                        args = $"{config.Base64Locale} {encoded}";
                    }
                    else
                    {
                        args = $"{_loggedUsername} {_loggedPassword}";
                    }

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = args,
                        WorkingDirectory = Application.StartupPath,
                        UseShellExecute = true
                    };
                    // Splash Guard primeiro (TopMost); launcher some na hora sem animação de −
                    GuardWatch.Start(Application.StartupPath, () => Process.Start(startInfo));
                    HideToTray();

                    // Sessão FL GUARD: heartbeat + ring buffer (screenshot/clip sob demanda)
                    try { _guardSession?.Dispose(); } catch { }
                    if (_loggedPlayerId > 0)
                    {
                        string exeName = Path.GetFileNameWithoutExtension(config.Executable);
                        _guardSession = new GuardSession(
                            config.IpAddress, config.Port,
                            _loggedPlayerId, _loggedSessionId, _loggedUsername, exeName);
                        _guardSession.Start();
                    }
                }
                else
                {
                    MessageBox.Show($"{config.Executable} não encontrado.", "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Falha ao iniciar o jogo: {ex.Message}");
                MessageBox.Show($"Não foi possível iniciar o jogo.\n{ex.Message}", "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Main_Load(object sender, EventArgs e)
        {
            bool clientOk = CheckClientVersion();
            if (!clientOk)
                return;
            bool launcherOk = CheckLauncherVersion();
            if (!launcherOk)
                return;
        }
        private async void Main_Shown(object sender, EventArgs e)
        {
            await TryAutoLoginAsync();
            if (_versionOk && !_integrityOk && !isChecking)
            {
                isChecking = true;
                CheckButton.BackgroundImage = Resources.CheckDisable;
                await StartFileCheckAsync();
            }
        }
        private async Task TryAutoLoginAsync()
        {
            if (_isLoggedIn)
                return;

            if (!Login.TryGetSavedCredentials(out string username, out string password))
                return;

            try
            {
                LoginService loginService = new LoginService(_connection);
                LoginResult result = await loginService.LoginAsync(username, password);

                if (!result.Success)
                    return;

                _loggedUsername = username;
                _loggedPassword = password;
                _loggedToken = result.Token ?? string.Empty;
                _loggedSessionId = result.SessionId ?? string.Empty;
                _loggedPlayerId = result.PlayerId;
                _isLoggedIn = true;
                SetLoggedInState(username);
                Logger.LogLoginSuccess();
            }
            catch
            {
                // Mantém tela pedindo Entrar se o auto-login falhar.
            }
        }
        private void MarkLocalVersionsFromServer()
        {
            try
            {
                if (_connectionResult == null)
                    return;

                if (!string.IsNullOrEmpty(_connectionResult.LauncherVersion))
                {
                    var launcherCfg = LauncherConfigService.FromFolder(Application.StartupPath);
                    launcherCfg.Save(new LauncherConfig { LauncherVersion = _connectionResult.LauncherVersion });
                }

                if (!string.IsNullOrEmpty(_connectionResult.ClientVersion))
                {
                    var clientSvc = ClientConfigService.FromFolder(Application.StartupPath);
                    ClientConfig local = clientSvc.Load();
                    local.ClientVersion = _connectionResult.ClientVersion;
                    clientSvc.Save(local);
                    Logger.Log($"CLIENT_VERSION local -> {_connectionResult.ClientVersion}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Aviso ao gravar versoes locais: " + ex.Message);
            }
        }

        private bool CheckClientVersion()
        {
            ClientConfigService clientConfigService = ClientConfigService.FromFolder(Application.StartupPath);
            ClientConfig localConfig = clientConfigService.Load();

            long localVersion;
            long serverVersion;

            bool localOk = long.TryParse(localConfig.ClientVersion, out localVersion);
            bool serverOk = long.TryParse(_connectionResult.ClientVersion, out serverVersion);

            Logger.LogState(UpdaterState.UPDATER_STATE_PATCH_END, UpdaterState.UPDATER_STATE_PRE_CLIENT_VERSION);

            if (!localOk || !serverOk || localVersion < serverVersion)
            {
                Logger.LogFail(UpdaterState.UPDATER_STATE_PRE_CLIENT_VERSION,
                    $"local={localConfig.ClientVersion} server={_connectionResult.ClientVersion}");
                Logger.LogState(UpdaterState.UPDATER_STATE_PRE_CLIENT_VERSION, UpdaterState.UPDATER_STATE_PATCH_START);

                TEXT_STATUS.Text = "Clique no botão Update.";
                StartButton.BackgroundImage = Resources.Bitmap171;
                CheckButton.BackgroundImage = Resources.Bitmap163;
                SetButtonsVisible(false, true, true);
                SetButtonsEnable(false, false, true);
                FileBar.Width = 0;
                TotalBar.Width = 0;
                return false;
            }

            Logger.LogSuccess(UpdaterState.UPDATER_STATE_PRE_CLIENT_VERSION);
            Logger.LogState(UpdaterState.UPDATER_STATE_PRE_CLIENT_VERSION, UpdaterState.UPDATER_STATE_PATCH_END);

            TEXT_STATUS.Text = "FL Guard conferindo arquivos...";
            FileBar.Width = 0;
            TotalBar.Width = 0;
            SetButtonsVisible(true, true, false);
            SetButtonsEnable(false, true, false);
            _versionOk = true;
            return true;
        }
        private bool CheckLauncherVersion()
        {
            LauncherConfigService launcherConfigService = LauncherConfigService.FromFolder(Application.StartupPath);
            LauncherConfig localConfig = launcherConfigService.Load();

            long localVersion;
            long serverVersion;

            bool localOk = long.TryParse(localConfig.LauncherVersion, out localVersion);
            bool serverOk = long.TryParse(_connectionResult.LauncherVersion, out serverVersion);

            Logger.LogState(UpdaterState.UPDATER_STATE_PATCH_END, UpdaterState.UPDATER_STATE_PRE_UPDATER_VERSION_ACK);

            if (!localOk || !serverOk || localVersion < serverVersion)
            {
                Logger.LogFail(UpdaterState.UPDATER_STATE_PRE_UPDATER_VERSION_ACK,
                    $"local={localConfig.LauncherVersion} server={_connectionResult.LauncherVersion}");
                Logger.LogState(UpdaterState.UPDATER_STATE_PRE_UPDATER_VERSION_ACK, UpdaterState.UPDATER_STATE_PATCH_START);

                TEXT_STATUS.Text = "Clique no botão Update.";
                SetButtonsVisible(false, false, true);
                SetButtonsEnable(false, false, true);
                FileBar.Width = 0;
                TotalBar.Width = 0;
                _versionOk = false;
                return false;
            }

            Logger.LogSuccess(UpdaterState.UPDATER_STATE_PRE_UPDATER_VERSION_ACK);
            Logger.LogState(UpdaterState.UPDATER_STATE_PRE_UPDATER_VERSION_ACK, UpdaterState.UPDATER_STATE_PATCH_END);

            FileBar.Width = 0;
            TotalBar.Width = 0;
            SetButtonsVisible(true, true, false);
            SetButtonsEnable(false, true, false);
            return true;
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                object ax = webBrowser1.ActiveXInstance;
                if (ax == null)
                    return;
                ax.GetType().InvokeMember(
                    "ExecWB",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    ax,
                    new object[] { 63, 2, 100, IntPtr.Zero });
            }
            catch
            {
            }
        }

        private async void UpdateButton_Click(object sender, EventArgs e)
        {
            SetButtonsEnable(false, false, false);
            SetButtonsVisible(false, false, false);

            FILE_TEXT.Visible = true;
            FileBar.Width = 0;
            TotalBar.Width = 0;
            TEXT_STATUS.Text = "Fazendo download dos arquivos de patch.";

            try
            {
                UpdateManifest manifest = await _connection.GetManifestAsync();

                LauncherUpdateCheckService checkService = new LauncherUpdateCheckService(Application.StartupPath);
                List<UpdateFile> filesToUpdate = checkService.GetFilesToUpdate(manifest);

                if (filesToUpdate.Count == 0)
                {
                    MarkLocalVersionsFromServer();
                    FileBar.Width = 463;
                    TotalBar.Width = 463;
                    FILE_TEXT.Visible = false;
                    TEXT_STATUS.Text = "Você pode iniciar o jogo.";
                    SetButtonsEnable(true, true, false);
                    SetButtonsVisible(true, true, false);
                    return;
                }
                Logger.Log($"Command: UPDATER_STATE_CLIENT_VERSION");

                var progress = new Progress<UpdateDownloadProgress>(p =>
                {
                    FILE_TEXT.Text = $"File {p.CurrentFile}";

                    FileBar.Width = p.TotalBytes > 0
                        ? (int)(p.BytesReceived * 463 / p.TotalBytes)
                        : 0;

                    TotalBar.Width = p.TotalFiles > 0
                        ? (int)(p.CurrentFileIndex * 463 / p.TotalFiles)
                        : 0;

                    TEXT_STATUS.Text = $"[{p.CurrentFileIndex}/{p.TotalFiles}] {Path.GetFileName(p.CurrentFile)}";
                });

                PatchDownloadService downloadService = new PatchDownloadService(Application.StartupPath, _connection);
                bool needsRestart = await downloadService.DownloadFilesAsync(filesToUpdate, progress);

                MarkLocalVersionsFromServer();

                if (needsRestart)
                {
                    TEXT_STATUS.Text = "Reiniciando para aplicar o launcher...";
                    Logger.Log("Logger: Auto-update do FLLauncher — reiniciando.");
                    MessageBox.Show(
                        "O launcher foi baixado.\nVai fechar e abrir de novo para aplicar a atualização.",
                        "FRONTLINE",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    SelfUpdateHelper.StartRestartAndExit(Application.StartupPath);
                    return;
                }

                FileBar.Width = 463;
                TotalBar.Width = 463;
                FILE_TEXT.Text = "File";
                TEXT_STATUS.Text = "Você pode iniciar o jogo.";
                SetButtonsEnable(true, true, false);
                SetButtonsVisible(true, true, false);
                CheckButton.BackgroundImage = Resources.Bitmap164;
                StartButton.BackgroundImage = Resources.Bitmap172;
                Logger.Log("Logger: Atualização finalizada.");
            }
            catch (Exception ex)
            {
                FileBar.Width = 0;
                TotalBar.Width = 0;
                FILE_TEXT.Text = "File";
                TEXT_STATUS.Text = "Erro durante a atualização.";
                SetButtonsEnable(false, false, true);
                SetButtonsVisible(false, false, true);
                Logger.Log($"Erro no update: {ex.Message}");
                MessageBox.Show($"Erro durante a atualização.\n{ex.Message}", "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoginButton_Click(object sender, EventArgs e)
        {
            using (Login login = new Login(_connectionResult, _connection))
            {
                if (login.ShowDialog(this) == DialogResult.OK)
                {
                    _loggedUsername = login.LoggedUsername;
                    _loggedPassword = login.LoggedPassword;
                    _loggedToken = login.LoggedToken;
                    _loggedSessionId = login.LoggedSessionId;
                    _loggedPlayerId = login.LoggedPlayerId;
                    _isLoggedIn = true;
                    SetLoggedInState(_loggedUsername);
                }
            }
        }
        private void SetLoggedInState(string username)
        {
            TEXT_USER.Text = username;
            TEXT_USER.Visible = true;
            LogoutButton.Visible = true;
            LoginScreen.Visible = false;
        }
        private void LogoutButton_Click(object sender, EventArgs e)
        {
            try { _guardSession?.Dispose(); } catch { }
            _guardSession = null;
            _isLoggedIn = false;
            _loggedUsername = string.Empty;
            _loggedPassword = string.Empty;
            _loggedToken = string.Empty;
            _loggedSessionId = string.Empty;
            _loggedPlayerId = 0;
            TEXT_USER.Text = string.Empty;
            TEXT_USER.Visible = false;
            LogoutButton.Visible = false;
            LoginScreen.Visible = true;
        }
    }
}
