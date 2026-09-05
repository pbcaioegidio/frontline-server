using Plugin.Core.Logging;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Executable.Forms
{
    public class FormLogs : Form
    {
        private readonly RichTextBox box = new RichTextBox();
        private readonly CheckBox chkPackets = new CheckBox();
        private readonly CheckBox chkDebug = new CheckBox();
        private readonly Button btnClear = new Button();
        private readonly Button btnPause = new Button();
        private readonly ToolTip tips = new ToolTip();
        private bool paused;
        private bool showPackets;
        private bool showDebug;

        public FormLogs()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = FlTheme.Bg;
            ForeColor = FlTheme.Text;
            Dock = DockStyle.Fill;

            var bar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = FlTheme.Bg,
                Padding = new Padding(10, 7, 10, 7)
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = FlTheme.Bg
            };

            StyleCheck(chkPackets, "Pacotes");
            StyleCheck(chkDebug, "Debug");
            chkPackets.CheckedChanged += (s, e) => showPackets = chkPackets.Checked;
            chkDebug.CheckedChanged += (s, e) => showDebug = chkDebug.Checked;

            tips.SetToolTip(chkPackets, "Mostra trafego de pacotes de rede (Auth/Game/Match).\nDesligado por padrao — enche o log muito rapido.");
            tips.SetToolTip(chkDebug, "Mostra mensagens de nivel Debug.\nDesligado por padrao — so Info/Warn/Error/Hack.");
            tips.SetToolTip(btnClear, "Limpa o painel e o buffer de logs na memoria.");
            tips.SetToolTip(btnPause, "Pausa a rolagem automatica de novas linhas.\nClique de novo (Seguir) para continuar.");

            FlTheme.StyleGhostButton(btnClear);
            btnClear.Text = "Limpar";
            btnClear.Width = 86;
            btnClear.Margin = new Padding(10, 0, 6, 0);
            btnClear.Click += (s, e) =>
            {
                UiLogHub.Clear();
                box.Clear();
            };

            FlTheme.StylePrimaryButton(btnPause);
            btnPause.Text = "Pausar";
            btnPause.Width = 86;
            btnPause.Margin = new Padding(0, 0, 0, 0);
            btnPause.Click += (s, e) =>
            {
                paused = !paused;
                btnPause.Text = paused ? "Seguir" : "Pausar";
                if (paused) FlTheme.StyleGhostButton(btnPause);
                else FlTheme.StylePrimaryButton(btnPause);
                btnPause.Text = paused ? "Seguir" : "Pausar";
            };

            flow.Controls.Add(chkPackets);
            flow.Controls.Add(chkDebug);
            flow.Controls.Add(btnClear);
            flow.Controls.Add(btnPause);
            bar.Controls.Add(flow);

            var frame = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = FlTheme.Bg,
                Padding = new Padding(1)
            };

            box.Dock = DockStyle.Fill;
            box.ReadOnly = true;
            box.BorderStyle = BorderStyle.None;
            box.BackColor = FlTheme.Bg;
            box.ForeColor = FlTheme.Text;
            box.Font = new Font("Consolas", 8.5f);
            box.DetectUrls = false;
            box.HideSelection = true;
            box.WordWrap = true;
            box.ScrollBars = RichTextBoxScrollBars.Vertical;
            box.HandleCreated += (s, e) => FlTheme.TryDarkScroll(box);

            frame.Controls.Add(box);
            Controls.Add(frame);
            Controls.Add(bar);

            Load += OnLoad;
            FormClosed += (s, e) =>
            {
                UiLogHub.LineAdded -= OnLine;
                tips.Dispose();
            };
        }

        private static void StyleCheck(CheckBox c, string text)
        {
            c.Text = text;
            c.AutoSize = true;
            c.ForeColor = FlTheme.TextMuted;
            c.Margin = new Padding(2, 4, 12, 0);
            c.Cursor = Cursors.Hand;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            FlTheme.TryDarkScroll(box);
            foreach (var line in UiLogHub.Snapshot())
                AppendLine(line, scroll: false);
            if (box.TextLength > 0)
            {
                box.SelectionStart = box.TextLength;
                box.ScrollToCaret();
            }
            UiLogHub.LineAdded += OnLine;
        }

        private void OnLine(UiLogLine line)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(new Action(() => AppendLine(line, scroll: true))); }
            catch { }
        }

        private void AppendLine(UiLogLine line, bool scroll)
        {
            if (paused) return;
            if (line.Cat == LogCat.Packet && !showPackets) return;
            if (line.Level == LogLevel.Debug && !showDebug && line.Cat != LogCat.Hack) return;

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = ColorFor(line.Level, line.Cat);
            box.AppendText(line.Text + Environment.NewLine);

            if (box.Lines.Length > UiLogHub.MaxLines + 50)
            {
                box.ReadOnly = false;
                int cut = box.GetFirstCharIndexFromLine(Math.Max(0, box.Lines.Length - UiLogHub.MaxLines));
                if (cut > 0)
                {
                    box.Select(0, cut);
                    box.SelectedText = "";
                }
                box.ReadOnly = true;
            }

            if (scroll)
            {
                box.SelectionStart = box.TextLength;
                box.ScrollToCaret();
            }
        }

        private static Color ColorFor(LogLevel level, LogCat cat)
        {
            switch (level)
            {
                case LogLevel.Error: return FlTheme.Danger;
                case LogLevel.Warn: return FlTheme.Warn;
                case LogLevel.Hack: return Color.FromArgb(220, 120, 220);
                case LogLevel.Debug: return FlTheme.TextMuted;
                default:
                    return cat == LogCat.Packet ? Color.FromArgb(100, 200, 220)
                         : cat == LogCat.Opcode ? Color.FromArgb(180, 140, 255)
                         : FlTheme.Text;
            }
        }
    }
}
