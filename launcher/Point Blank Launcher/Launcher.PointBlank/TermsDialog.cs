using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Launcher.PointBlank
{
    /// <summary>
    /// Termo FL GUARD — visual escuro alinhado ao splash, layout de diálogo (não MessageBox).
    /// </summary>
    internal sealed class TermsDialog : Form
    {
        private readonly string[] _bullets;
        private readonly string _footer;
        private bool _accepted;

        private TermsDialog(string[] bullets, string footer)
        {
            _bullets = bullets ?? Array.Empty<string>();
            _footer = footer ?? "";

            Text = "FL GUARD — Termo de uso";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            Size = new Size(520, 420);
            BackColor = Color.FromArgb(12, 16, 24);
            DoubleBuffered = true;
            KeyPreview = true;

            var btnYes = MakeButton("ACEITAR", Color.FromArgb(40, 120, 220), Color.White);
            btnYes.Location = new Point(Width - 248, Height - 56);
            btnYes.Click += (s, e) => { _accepted = true; DialogResult = DialogResult.Yes; Close(); };

            var btnNo = MakeButton("RECUSAR", Color.FromArgb(36, 42, 54), Color.FromArgb(200, 205, 215));
            btnNo.Location = new Point(Width - 126, Height - 56);
            btnNo.Click += (s, e) => { _accepted = false; DialogResult = DialogResult.No; Close(); };

            Controls.Add(btnYes);
            Controls.Add(btnNo);

            Paint += OnPaint;
            MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Capture = false;
                    Message m = Message.Create(Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
                    WndProc(ref m);
                }
            };
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.No; Close(); }
                if (e.KeyCode == Keys.Enter) { _accepted = true; DialogResult = DialogResult.Yes; Close(); }
            };
        }

        private static Button MakeButton(string text, Color bg, Color fg)
        {
            return new Button
            {
                Text = text,
                Size = new Size(110, 36),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = fg,
                BackColor = bg,
                Cursor = Cursors.Hand,
                FlatAppearance =
                {
                    BorderSize = 1,
                    BorderColor = Color.FromArgb(90, 110, 140),
                    MouseOverBackColor = Color.FromArgb(Math.Min(255, bg.R + 18), Math.Min(255, bg.G + 18), Math.Min(255, bg.B + 22)),
                    MouseDownBackColor = Color.FromArgb(Math.Max(0, bg.R - 12), Math.Max(0, bg.G - 12), Math.Max(0, bg.B - 12))
                }
            };
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (var bg = new LinearGradientBrush(ClientRectangle,
                Color.FromArgb(16, 22, 34), Color.FromArgb(8, 10, 16), 125f))
                g.FillRectangle(bg, ClientRectangle);

            // faixa lateral azul (diferente do splash)
            using (var accent = new LinearGradientBrush(
                new Rectangle(0, 0, 6, Height),
                Color.FromArgb(70, 160, 255), Color.FromArgb(30, 80, 160), 90f))
                g.FillRectangle(accent, 0, 0, 6, Height);

            using (var edge = new Pen(Color.FromArgb(70, 90, 120), 1f))
                g.DrawRectangle(edge, 0, 0, Width - 1, Height - 1);

            // header
            using (var titleFont = new Font("Segoe UI", 22f, FontStyle.Bold))
            using (var subFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var metal = new LinearGradientBrush(new Rectangle(28, 18, 260, 40),
                Color.FromArgb(230, 235, 245), Color.FromArgb(130, 145, 165), 90f))
            using (var cyan = new SolidBrush(Color.FromArgb(90, 175, 255)))
            {
                g.DrawString("FL GUARD", titleFont, metal, 28, 20);
                g.DrawString("TERMO DE USO  ·  ANTI-CHEAT", subFont, cyan, 32, 58);
            }

            using (var line = new Pen(Color.FromArgb(50, 70, 100), 1f))
                g.DrawLine(line, 28, 84, Width - 28, 84);

            using (var introFont = new Font("Segoe UI", 9.5f))
            using (var introBrush = new SolidBrush(Color.FromArgb(210, 215, 225)))
                g.DrawString("Para jogar FrontLine, precisamos da sua autorização para:", introFont, introBrush, 28, 98);

            float y = 130;
            using (var bulletFont = new Font("Segoe UI", 9.25f))
            using (var bulletBrush = new SolidBrush(Color.FromArgb(225, 230, 238)))
            using (var dotBrush = new SolidBrush(Color.FromArgb(70, 160, 255)))
            {
                foreach (string line in _bullets)
                {
                    g.FillEllipse(dotBrush, 32, y + 5, 7, 7);
                    RectangleF box = new RectangleF(48, y, Width - 76, 52);
                    g.DrawString(line, bulletFont, bulletBrush, box);
                    y += MeasureLines(g, line, bulletFont, Width - 76) + 14;
                }
            }

            y += 6;
            using (var footFont = new Font("Segoe UI", 8.5f))
            using (var footBrush = new SolidBrush(Color.FromArgb(150, 160, 175)))
            {
                RectangleF box = new RectangleF(28, y, Width - 56, 70);
                g.DrawString(_footer, footFont, footBrush, box);
            }

            // barra inferior
            using (var bar = new SolidBrush(Color.FromArgb(180, 6, 8, 12)))
                g.FillRectangle(bar, 0, Height - 72, Width, 72);
            using (var ask = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (var askBrush = new SolidBrush(Color.FromArgb(230, 235, 245)))
                g.DrawString("Você aceita?", ask, askBrush, 28, Height - 48);
        }

        private static float MeasureLines(Graphics g, string text, Font font, float width)
        {
            SizeF sz = g.MeasureString(text, font, (int)width);
            return Math.Max(28, sz.Height);
        }

        public static bool ShowTerms(IWin32Window owner)
        {
            string[] bullets =
            {
                "Identificar seu computador e conexão, para impedir contas banidas de voltar.",
                "Verificar os arquivos do jogo e programas em execução enquanto você joga.",
                "Capturar imagem ou vídeo SOMENTE da janela do jogo se houver suspeita de trapaça ou pedido da moderação."
            };
            string footer =
                "Não capturamos área de trabalho, outras janelas, webcam nem microfone.\n" +
                "Os dados servem só para anti-cheat e são acessíveis apenas pela moderação.\n" +
                "Se houver banimento ou evidência de trapaça, os registros ficam guardados de forma permanente.";

            using (var dlg = new TermsDialog(bullets, footer))
            {
                if (owner != null)
                    dlg.ShowDialog(owner);
                else
                    dlg.ShowDialog();
                return dlg._accepted || dlg.DialogResult == DialogResult.Yes;
            }
        }
    }
}
