using Executable.Utility;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Executable.Forms
{
    public partial class MainForm : Form, IMessageFilter
    {
        #region Moveable
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        public const int WM_LBUTTONDOWN = 0x0201;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion Moveable
        #region Corner
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthElipse, int nHeightElipse);
        #endregion Corner
        private readonly HashSet<Control> ControlsToMove = new HashSet<Control>();
        private readonly int ProcessId;
        private readonly DirectoryInfo ServerDir;
        private readonly Color BaseColor = FlTheme.Bg;
        private readonly Color SelectedColor = FlTheme.SurfaceAlt;
        private Button MaximizeBTN;
        private bool _layoutReady;
        private Form _activeChild;
        private FormMonitor _formMonitor;
        private FormAdditional _formAdditional;
        private FormLogs _formLogs;
        private FormConfig _formConfig;
        private FormServices _formServices;
        private NotifyIcon _tray;
        private ContextMenuStrip _trayMenu;
        public MainForm(int ProcessId, DirectoryInfo ServerDir)
        {
            InitializeComponent();
            this.ProcessId = ProcessId;
            this.ServerDir = ServerDir;
            BackColor = FlTheme.Bg;
            Application.AddMessageFilter(this);
            ControlsToMove.Add(this);
            ControlsToMove.Add(MonitorLogo);
            ControlsToMove.Add(MonitorName);
            ApplyBrandIcon();
            SetupTray();
            SetupMaximizeButton();
            StyleChrome();
            Resize += (s, e) =>
            {
                if (_layoutReady) ApplyChromeLayout();
                if (WindowState == FormWindowState.Minimized)
                    HideToTray();
            };
        }
        #region Attachments
        private string ResolveIconPath()
        {
            string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FLMonitor.ico");
            if (!File.Exists(icoPath))
                icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "FLMonitor.ico");
            return File.Exists(icoPath) ? icoPath : null;
        }

        private void ApplyBrandIcon()
        {
            try
            {
                string icoPath = ResolveIconPath();
                if (icoPath == null) return;
                using (var ico = new Icon(icoPath, 256, 256))
                {
                    Icon = (Icon)ico.Clone();
                    MonitorLogo.Image = new Bitmap(ico.ToBitmap(), 38, 38);
                }
            }
            catch { }
        }

        private void SetupTray()
        {
            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("Abrir FrontLine", null, (s, e) => RestoreFromTray());
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("Sair", null, (s, e) =>
            {
                RestoreFromTray();
                Close();
            });

            _tray = new NotifyIcon
            {
                Text = "FrontLine Monitor",
                Visible = false,
                ContextMenuStrip = _trayMenu
            };
            try
            {
                string icoPath = ResolveIconPath();
                if (icoPath != null)
                {
                    using (var ico = new Icon(icoPath, 16, 16))
                        _tray.Icon = (Icon)ico.Clone();
                }
                else if (Icon != null)
                    _tray.Icon = (Icon)Icon.Clone();
            }
            catch
            {
                if (Icon != null) _tray.Icon = (Icon)Icon.Clone();
            }

            _tray.DoubleClick += (s, e) => RestoreFromTray();
            _tray.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left) RestoreFromTray();
            };
        }

        private void HideToTray()
        {
            if (_tray == null) return;
            ShowInTaskbar = false;
            Hide();
            _tray.Visible = true;
            _tray.Text = $"FrontLine · {StringUtility.ServerStatusL} · {StringUtility.OnlineUserL} online";
        }

        private void RestoreFromTray()
        {
            if (_tray != null) _tray.Visible = false;
            ShowInTaskbar = true;
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            Show();
            Activate();
            BringToFront();
        }

        private void StyleChrome()
        {
            label48.Visible = false;
            label48.Text = "";
            MonitorName.ForeColor = FlTheme.Text;
            MonitorName.Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold);
            AppTitle.ForeColor = FlTheme.TextMuted;
            AppStatus.ForeColor = FlTheme.Ok;
            label47.ForeColor = FlTheme.TextMuted;
            label51.ForeColor = FlTheme.TextMuted;
            RamPercent.ForeColor = FlTheme.Text;
            LogFileSize.ForeColor = FlTheme.Text;
            groupBox2.ForeColor = FlTheme.Border;
            FormLoaderPNL.BackColor = FlTheme.Bg;
            NavPNL.BackColor = FlTheme.Accent;

            foreach (var b in new[] { CloseBTN, MinimizeBTN, MaximizeBTN })
            {
                if (b == null) continue;
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.ForeColor = FlTheme.TextMuted;
                b.BackColor = FlTheme.Bg;
                b.Cursor = Cursors.Hand;
            }
            CloseBTN.FlatAppearance.MouseOverBackColor = FlTheme.Danger;
            CloseBTN.FlatAppearance.MouseDownBackColor = Color.FromArgb(160, 40, 40);

            foreach (var b in new[] { MonitorBTN, AdditionalBTN, LogsBTN, ConfigBTN, ServicesBTN })
                FlTheme.StyleNavButton(b, false);
        }

        private void SetupMaximizeButton()
        {
            MaximizeBTN = new Button
            {
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f),
                ForeColor = FlTheme.TextMuted,
                Text = "□",
                Size = new Size(38, 40),
                TabIndex = 100,
                BackColor = FlTheme.Bg
            };
            MaximizeBTN.FlatAppearance.BorderSize = 0;
            MaximizeBTN.Click += (s, e) =>
            {
                if (WindowState == FormWindowState.Maximized)
                {
                    WindowState = FormWindowState.Normal;
                    MaximizeBTN.Text = "□";
                }
                else
                {
                    WindowState = FormWindowState.Maximized;
                    MaximizeBTN.Text = "❐";
                }
                ApplyChromeLayout();
            };
            Controls.Add(MaximizeBTN);
            MaximizeBTN.BringToFront();
        }

        /// <summary>
        /// Layout compacto sem titulo "Monitore seu servidor".
        /// </summary>
        private void ApplyChromeLayout()
        {
            groupBox3.Visible = false;
            MemoryVPB.Visible = false;
            groupBox4.Visible = false;
            RefreshBTN.Visible = false;
            label48.Visible = false;

            int w = Math.Max(ClientSize.Width, 480);
            int h = Math.Max(ClientSize.Height, 520);
            const int navH = 48;
            const int headerH = 168;

            CloseBTN.SetBounds(w - 48, 4, 42, 36);
            MaximizeBTN.SetBounds(w - 90, 4, 38, 36);
            MinimizeBTN.SetBounds(w - 128, 4, 38, 36);

            MonitorLogo.SetBounds(14, 10, 36, 36);
            MonitorName.SetBounds(56, 12, Math.Max(120, w - 200), 32);
            AppTitle.SetBounds(14, 56, w - 28, 18);
            AppStatus.SetBounds(14, 76, w - 28, 20);
            groupBox2.SetBounds(16, 102, w - 32, 1);
            groupBox2.BackColor = FlTheme.Border;

            int half = (w - 40) / 2;
            label47.Text = "Memoria";
            label47.SetBounds(16, 112, half, 18);
            RamPercent.SetBounds(16, 130, half, 28);
            RamPercent.Font = new Font("Segoe UI Semibold", 16f, FontStyle.Bold);
            label51.SetBounds(20 + half, 112, half, 18);
            LogFileSize.SetBounds(20 + half, 130, half, 28);
            LogFileSize.Font = new Font("Segoe UI Semibold", 16f, FontStyle.Bold);

            FormLoaderPNL.SetBounds(1, headerH, w - 2, Math.Max(120, h - headerH - navH - 4));
            FormLoaderPNL.BackColor = FlTheme.Bg;

            int tabW = w / 5;
            Button[] tabs = { MonitorBTN, AdditionalBTN, LogsBTN, ConfigBTN, ServicesBTN };
            Panel[] tops = { MonitorPanelN, AdditionalPanelN, LogsPanelN, ConfigPanelN, ServicesPanelN };
            Panel[] bots = { FMonitorPanel, FAdditionalPanel, FLogsPanel, FConfigPanel, FServicesPanel };
            for (int i = 0; i < 5; i++)
            {
                int x = i * tabW;
                int tw = (i == 4) ? (w - x) : tabW;
                tabs[i].SetBounds(x, h - navH, tw, navH);
                tops[i].SetBounds(x, h - navH - 5, tw, 5);
                tops[i].BackColor = FlTheme.Bg;
                bots[i].SetBounds(x, h - 3, tw, 3);
                bots[i].BackColor = FlTheme.Bg;
            }

            if (MonitorBTN.BackColor == SelectedColor) SelectNav(MonitorBTN, FMonitorPanel);
            else if (AdditionalBTN.BackColor == SelectedColor) SelectNav(AdditionalBTN, FAdditionalPanel);
            else if (LogsBTN.BackColor == SelectedColor) SelectNav(LogsBTN, FLogsPanel);
            else if (ConfigBTN.BackColor == SelectedColor) SelectNav(ConfigBTN, FConfigPanel);
            else if (ServicesBTN.BackColor == SelectedColor) SelectNav(ServicesBTN, FServicesPanel);
        }

        private void FitToScreen()
        {
            // Janela compacta (nao tela cheia)
            Size = new Size(560, 700);
            MinimumSize = new Size(480, 600);
            Rectangle wa = Screen.FromControl(this).WorkingArea;
            Location = new Point(
                Math.Max(wa.Left, wa.Right - Width - 20),
                Math.Max(wa.Top, wa.Bottom - Height - 20));
            StartPosition = FormStartPosition.Manual;
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
        }
        private void LoadCleanLabels()
        {
            AppTitle.Text = "Versao ****";
            AppStatus.Text = "AGUARDE...";
            RamPercent.Text = "--";
            LogFileSize.Text = "--";
        }
        public bool PreFilterMessage(ref Message Msg)
        {
            if (Msg.Msg == WM_LBUTTONDOWN && ControlsToMove.Contains(FromHandle(Msg.HWnd)))
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                return true;
            }
            return false;
        }
        private IEnumerable<Control> GetAllControls(Control CTRL)
        {
            Stack<Control> STC = new Stack<Control>();
            STC.Push(CTRL);
            while (STC.Any())
            {
                Control NextCTRL = STC.Pop();
                foreach (Control ChildCTRL in NextCTRL.Controls)
                {
                    STC.Push(ChildCTRL);
                }
                yield return NextCTRL;
            }
        }
        private void ValidateFont(string[] Files, PrivateFontCollection PFC)
        {
            foreach (string File in Files)
            {
                PFC.AddFontFile(File);
            }
        }
        private string FontName(FontFamily[] FamililyArray)
        {
            foreach (FontFamily Family in FamililyArray)
            {
                if (Family.Name == FontSet())
                {
                    return Family.Name;
                }
            }
            return "Consolas";
        }
        private string FontSet()
        {
            string Text = "";
            try
            {
                string Path = "Config/FontSet.ini";
                if (!File.Exists(Path))
                {
                    MessageBox.Show($"Arquivo nao encontrado! {Path}", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return Text;
                }
                string[] Lines = File.ReadAllLines(Path, Encoding.UTF8);
                foreach (string Line in Lines)
                {
                    if (!(Line.StartsWith(";") || Line.StartsWith("[")))
                    {
                        Text = Line;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return Text;
        }
        private void SelectNav(Button active, Panel footerMarker)
        {
            NavPNL.Width = footerMarker.Width;
            NavPNL.Top = footerMarker.Top;
            NavPNL.Left = footerMarker.Left;
            NavPNL.BackColor = FlTheme.Accent;
            FlTheme.StyleNavButton(MonitorBTN, false);
            FlTheme.StyleNavButton(AdditionalBTN, false);
            FlTheme.StyleNavButton(LogsBTN, false);
            FlTheme.StyleNavButton(ConfigBTN, false);
            FlTheme.StyleNavButton(ServicesBTN, false);
            FlTheme.StyleNavButton(active, true);
            active.BackColor = SelectedColor;
        }
        private void ShowChild(Form child)
        {
            if (_activeChild == child && FormLoaderPNL.Controls.Contains(child))
                return;

            if (_activeChild != null)
            {
                FormLoaderPNL.Controls.Remove(_activeChild);
                _activeChild.Hide();
            }

            child.Dock = DockStyle.Fill;
            child.TopLevel = false;
            child.TopMost = true;
            if (!FormLoaderPNL.Controls.Contains(child))
                FormLoaderPNL.Controls.Add(child);
            child.Show();
            _activeChild = child;
        }

        private T KeepTab<T>(ref T field, Func<T> create) where T : Form
        {
            if (field == null || field.IsDisposed)
                field = create();
            return field;
        }
        #endregion Attachments
        private void Monitor1_Load(object sender, EventArgs e)
        {
            FitToScreen();
            FormLoaderPNL.BackColor = FlTheme.Bg;
            MonitorPanelN.BackColor = FlTheme.Bg;
            AdditionalPanelN.BackColor = FlTheme.Bg;
            LogsPanelN.BackColor = FlTheme.Bg;
            ConfigPanelN.BackColor = FlTheme.Bg;
            ServicesPanelN.BackColor = FlTheme.Bg;
            LoadCleanLabels();
            using (PrivateFontCollection PFC = new PrivateFontCollection())
            {
                string[] Files = Directory.GetFiles($"Font/");
                if (Files.Length > 0)
                {
                    ValidateFont(Files, PFC);
                    string SelectedFont = FontName(PFC.Families);
                    foreach (Control CTRL in GetAllControls(this))
                    {
                        CTRL.Font = new Font(SelectedFont, CTRL.Font.Size, CTRL.Font.Style);
                    }
                }
                else
                {
                    MessageBox.Show("Fonte nao encontrada. Verifique a pasta Font/.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WindowUtility.KillProcessAndChildren(ProcessId);
                }
            }
            ApplyChromeLayout();
            _layoutReady = true;
            FormLoaderPNL.BringToFront();
            MaximizeBTN.BringToFront();
            CloseBTN.BringToFront();
            MinimizeBTN.BringToFront();
            StyleChrome();
            SelectNav(MonitorBTN, FMonitorPanel);
            ShowChild(KeepTab(ref _formMonitor, () => new FormMonitor()));
            RefresherT.Start();
        }
        private void RefreshBTN_Click(object sender, EventArgs e)
        {
            Refresh();
        }
        private void CloseBTN_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void MinimizeBTN_Click(object sender, EventArgs e)
        {
            HideToTray();
        }
        private void Monitor1_Paint(object sender, PaintEventArgs e)
        {
            Rectangle BorderRectangle = ClientRectangle;
            BorderRectangle.Inflate(0, 0);
            ControlPaint.DrawBorder(e.Graphics, BorderRectangle, Color.FromArgb(255, 54, 54, 164), ButtonBorderStyle.Solid);
        }
        private void Monitor1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult Result = MessageBox.Show("Tem certeza que deseja sair? O servidor sera encerrado.", "FrontLine", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (Result == DialogResult.Yes)
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
                DisposeTab(ref _formMonitor);
                DisposeTab(ref _formAdditional);
                DisposeTab(ref _formLogs);
                DisposeTab(ref _formConfig);
                DisposeTab(ref _formServices);
                WindowUtility.KillProcessAndChildren(ProcessId);
            }
            else
            {
                e.Cancel = true;
                if (!Visible) RestoreFromTray();
            }
        }
        private void RefresherT_Tick(object sender, EventArgs e)
        {
            AppTitle.Text = $"Versao {StringUtility.ServerVersionL}";
            AppStatus.ForeColor = (StringUtility.ServerStatusL.Equals("SERVIDOR ONLINE") ? FlTheme.Ok : StringUtility.ServerStatusL.Equals("SERVIDOR OFFLINE") ? FlTheme.Danger : FlTheme.Text);
            AppStatus.Text = $"{StringUtility.ServerStatusL}  ·  {StringUtility.OnlineUserL} online";
            RamPercent.Text = StringUtility.MemoryUsageL;
            LogFileSize.Text = StringUtility.LogFileSize;
        }
        private void MonitorBTN_Click(object sender, EventArgs e)
        {
            SelectNav(MonitorBTN, FMonitorPanel);
            ShowChild(KeepTab(ref _formMonitor, () => new FormMonitor()));
        }
        private void AdditionalBTN_Click(object sender, EventArgs e)
        {
            SelectNav(AdditionalBTN, FAdditionalPanel);
            ShowChild(KeepTab(ref _formAdditional, () => new FormAdditional()));
        }
        private void LogsBTN_Click(object sender, EventArgs e)
        {
            SelectNav(LogsBTN, FLogsPanel);
            ShowChild(KeepTab(ref _formLogs, () => new FormLogs()));
        }
        private void ConfigBTN_Click(object sender, EventArgs e)
        {
            SelectNav(ConfigBTN, FConfigPanel);
            ShowChild(KeepTab(ref _formConfig, () => new FormConfig(ServerDir)));
        }
        private void ServicesBTN_Click(object sender, EventArgs e)
        {
            SelectNav(ServicesBTN, FServicesPanel);
            ShowChild(KeepTab(ref _formServices, () => new FormServices()));
        }

        private static void DisposeTab<T>(ref T field) where T : Form
        {
            if (field == null) return;
            try { if (!field.IsDisposed) field.Dispose(); } catch { }
            field = null;
        }
    }
}
