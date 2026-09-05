using Launcher.PointBlank.Models;
using Launcher.PointBlank.Services;
using Launcher.PointBlank.Utils;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher.PointBlank
{
    public partial class Init : Form
    {
        private readonly LauncherConnection connectionService;
        private readonly ClientConfig _clientConfig;
        private const int BarMaxWidth = 710;

        public Init()
        {
            InitializeComponent();
            AppIcon.Apply(this);

            _clientConfig = ClientConfigService.FromFolder(Application.StartupPath).Load();
            connectionService = new LauncherConnection(_clientConfig.IpAddress, _clientConfig.Port);
            Load += Init_Load;
        }

        private async void Init_Load(object sender, EventArgs e)
        {
            await CheckServerAsync();
        }

        private void SetLoading(string text, int percent)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetLoading(text, percent)));
                return;
            }

            INIT_TEXT.Text = text;
            int w = Math.Max(8, Math.Min(BarMaxWidth, (int)(BarMaxWidth * (percent / 100.0))));
            loadingFill.Width = w;
            loadingFill.Refresh();
            INIT_TEXT.Refresh();
        }

        private async Task CheckServerAsync()
        {
            try
            {
                SetLoading("Carregando...", 8);
                await Task.Delay(120);

                Logger.LogState(UpdaterState.UPDATER_STATE_START, UpdaterState.UPDATER_STATE_PRE_CONNECT);
                SetLoading("Iniciando serviço...", 25);
                await SocketBootstrap.EnsureRunningAsync(
                    Application.StartupPath, _clientConfig.IpAddress, _clientConfig.Port);
                await Task.Delay(300);

                SetLoading("Conectando ao servidor...", 55);
                Logger.LogState(UpdaterState.UPDATER_STATE_PRE_CONNECT, UpdaterState.UPDATER_STATE_PRE_UPDATER_VERSION_ACK);
                LauncherConnectionResult result = await connectionService.CheckConnectionAsync();

                if (!result.Success)
                {
                    Logger.LogFail(UpdaterState.UPDATER_STATE_PRE_UPDATER_VERSION_ACK, result.Message);
                    if (result.Maintenance)
                        MessageBox.Show(result.MaintenanceMessage, "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                        MessageBox.Show(result.Message, "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }

                SetLoading("Quase pronto...", 85);
                Logger.Log($"Set ReposURL Priority : 0");
                Logger.LogSuccess(UpdaterState.UPDATER_STATE_PRE_UPDATER_VERSION_ACK);
                Logger.LogState(UpdaterState.UPDATER_STATE_PRE_UPDATER_VERSION_ACK, UpdaterState.UPDATER_STATE_PATCH_END);

                await Task.Delay(350);
                SetLoading("Abrindo launcher...", 100);
                await Task.Delay(150);

                Main main = new Main(result, connectionService);
                main.Show();
                Hide();
            }
            catch (Exception ex)
            {
                Logger.LogFail(UpdaterState.UPDATER_STATE_PRE_CONNECT, ex.Message);
                MessageBox.Show("Não foi possível conectar ao servidor", "FRONTLINE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            // faixa escura embaixo pra legibilidade da barra
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(160, 8, 12, 20)))
            {
                e.Graphics.FillRectangle(brush, 0, ClientSize.Height - 80, ClientSize.Width, 80);
            }
        }
    }
}
