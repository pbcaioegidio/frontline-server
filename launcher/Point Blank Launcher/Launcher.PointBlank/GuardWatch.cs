using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher.PointBlank
{
    /// <summary>
    /// Splash no cantinho (loading + proteção). Depois some; processo fica em segundo plano
    /// só para acompanhar o jogo e encerrar junto.
    /// </summary>
    internal sealed class GuardWatch : Form
    {
        private const int WsExNoActivate = 0x08000000;
        private const int WsExToolWindow = 0x00000080;
        private const int WsExTopMost = 0x00000008;

        private static readonly Size SplashSize = new Size(440, 268);

        private readonly Timer _anim;
        private readonly Timer _watch;
        private readonly DateTime _born = DateTime.UtcNow;
        private float _spin;
        private string _line1 = "Verificando integridade do jogo e anti-cheat... (FL GUARD)";
        private string _line2 = "Aguardando conexão segura...";
        private bool _failed;
        private string _clientRoot;
        private Action _launchGame;

        private bool _sawGameWindow;
        private int _goneTicks;

        private GuardWatch(string clientRoot, Action launchGame)
        {
            _clientRoot = clientRoot;
            _launchGame = launchGame;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Size = SplashSize;
            BackColor = Color.FromArgb(12, 16, 24);
            DoubleBuffered = true;
            PlaceCorner(SplashSize);

            _anim = new Timer { Interval = 33 };
            _anim.Tick += (s, e) =>
            {
                _spin += 0.14f;
                if (_spin > Math.PI * 2) _spin = 0;
                Invalidate();
            };
            _anim.Start();

            // A cada 2s: se o jogo (janela) sumiu depois de ter aberto → encerra launcher inteiro
            _watch = new Timer { Interval = 2000 };
            _watch.Tick += (s, e) =>
            {
                if (GameWindowAlive())
                {
                    _sawGameWindow = true;
                    _goneTicks = 0;
                    return;
                }

                // ainda abrindo shaders / sem janela
                if (!_sawGameWindow)
                {
                    if ((DateTime.UtcNow - _born).TotalSeconds < 120) return;
                    _watch.Stop();
                    ExitLauncher();
                    return;
                }

                // janela sumiu — confirma 2 ticks (~4s) pra não sair no Alt+Tab falso
                _goneTicks++;
                if (_goneTicks < 2) return;
                _watch.Stop();
                ExitLauncher();
            };

            Paint += OnPaintSplash;
            Shown += async (s, e) => await RunBootAsync();
        }

        private static void ExitLauncher()
        {
            try { Application.Exit(); }
            catch
            {
                try { Environment.Exit(0); } catch { }
            }
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WsExNoActivate | WsExToolWindow | WsExTopMost;
                return cp;
            }
        }

        private void PlaceCorner(Size size)
        {
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(wa.Right - size.Width - 18, wa.Bottom - size.Height - 18);
        }

        private async Task RunBootAsync()
        {
            var result = await GuardProtection.RunAsync(_clientRoot, (a, b) =>
            {
                if (IsDisposed) return;
                BeginInvoke(new Action(() =>
                {
                    _line1 = a;
                    _line2 = b;
                    Invalidate();
                }));
            }).ConfigureAwait(true);

            if (IsDisposed) return;

            if (!result.Ok)
            {
                _failed = true;
                _line1 = "FL GUARD bloqueou a inicialização.";
                _line2 = result.Message ?? "Falha de proteção.";
                Invalidate();
                await Task.Delay(2500);
                MessageBox.Show(_line2, "FL GUARD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            _line1 = "Proteção ativa. Abrindo o jogo...";
            _line2 = "Canal seguro estabelecido.";
            Invalidate();
            await Task.Delay(350);

            try { _launchGame?.Invoke(); }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possível iniciar o jogo.\n" + ex.Message, "FL GUARD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            await Task.Delay(250);
            _anim.Stop();
            Hide();
            _watch.Start();
        }

        private void OnPaintSplash(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (var bg = new LinearGradientBrush(ClientRectangle, Color.FromArgb(14, 18, 28), Color.FromArgb(8, 10, 16), 90f))
                g.FillRectangle(bg, ClientRectangle);

            using (var grid = new Pen(Color.FromArgb(18, 80, 140, 200), 1f))
            {
                for (int x = 0; x < Width; x += 28)
                    g.DrawLine(grid, x, 0, x, Height);
                for (int y = 0; y < Height; y += 28)
                    g.DrawLine(grid, 0, y, Width, y);
            }

            using (var edge = new Pen(Color.FromArgb(90, 110, 140), 1f))
                g.DrawRectangle(edge, 0, 0, Width - 1, Height - 1);

            DrawSpinner(g, Width / 2f, 58, 28);

            using (var titleFont = new Font("Segoe UI", 28f, FontStyle.Bold))
            using (var subFont = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var metal = new LinearGradientBrush(new Rectangle(40, 95, 280, 50), Color.FromArgb(220, 220, 230), Color.FromArgb(140, 150, 165), 90f))
            using (var white = new SolidBrush(Color.White))
            {
                g.DrawString("FL GUARD", titleFont, metal, 48, 98);
                g.DrawString("ANTI-CHEAT", subFont, white, 52, 148);
            }

            int barY = Height - 72;
            using (var bar = new SolidBrush(Color.FromArgb(200, 6, 8, 12)))
                g.FillRectangle(bar, 0, barY, Width, 72);

            using (var f1 = new Font("Segoe UI", 8.5f))
            using (var f2 = new Font("Segoe UI", 8f))
            using (var c1 = new SolidBrush(Color.FromArgb(230, 230, 235)))
            using (var c2 = new SolidBrush(Color.FromArgb(160, 170, 185)))
            {
                g.DrawString(_line1, f1, c1, 16, barY + 12);
                g.DrawString(_line2, f2, c2, 16, barY + 32);
            }

            Color sec = _failed ? Color.FromArgb(200, 80, 80) : Color.FromArgb(70, 160, 255);
            using (var sf = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (var sb = new SolidBrush(sec))
            {
                string label = _failed ? "Blocked" : "Secured";
                SizeF sz = g.MeasureString(label, sf);
                float lx = Width - 16 - sz.Width;
                DrawSignal(g, lx - 22, barY + 28, sec);
                g.DrawString(label, sf, sb, lx, barY + 28);
            }

            using (var foot = new Font("Segoe UI", 7f))
            using (var muted = new SolidBrush(Color.FromArgb(110, 120, 135)))
                g.DrawString("FL GUARD  ·  FrontLine  ·  v1.0", foot, muted, Width - 168, Height - 16);
        }

        private void DrawSpinner(Graphics g, float cx, float cy, float r)
        {
            Color blue = Color.FromArgb(70, 160, 255);
            using (var pen = new Pen(Color.FromArgb(40, blue), 4f))
                g.DrawEllipse(pen, cx - r, cy - r, r * 2, r * 2);
            using (var pen = new Pen(blue, 4f) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                float deg = _spin * 180f / (float)Math.PI;
                g.DrawArc(pen, cx - r, cy - r, r * 2, r * 2, deg, 110);
                g.DrawArc(pen, cx - r * 0.62f, cy - r * 0.62f, r * 1.24f, r * 1.24f, -deg * 1.3f, 90);
            }
        }

        private static void DrawSignal(Graphics g, float x, float y, Color c)
        {
            using (var p = new Pen(c, 2f))
            {
                g.DrawArc(p, x, y + 8, 8, 8, 200, 140);
                g.DrawArc(p, x - 3, y + 4, 14, 14, 200, 140);
                g.DrawArc(p, x - 6, y, 20, 20, 200, 140);
            }
            using (var b = new SolidBrush(c))
                g.FillEllipse(b, x + 2.5f, y + 11, 3, 3);
        }

        /// <summary>
        /// True só se existir processo do jogo com janela visível.
        /// Ignora stubs/zumbis FrontLine sem UI (que impediam o launcher de fechar).
        /// </summary>
        private static bool GameWindowAlive()
        {
            foreach (string name in new[] { "FrontLine", "PointBlank", "PointBlank.i3Exec" })
            {
                try
                {
                    foreach (Process p in Process.GetProcessesByName(name))
                    {
                        try
                        {
                            if (p.HasExited) continue;
                            if (p.MainWindowHandle == IntPtr.Zero) continue;
                            if (string.IsNullOrWhiteSpace(p.MainWindowTitle)) continue;
                            return true;
                        }
                        catch { }
                    }
                }
                catch { }
            }
            return false;
        }

        public static void Start(string clientRoot, Action launchGame)
        {
            var hud = new GuardWatch(clientRoot, launchGame);
            hud.Show();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _anim?.Dispose();
                _watch?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
