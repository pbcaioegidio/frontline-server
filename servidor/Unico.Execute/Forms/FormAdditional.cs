using Executable.Utility;
using Plugin.Core.Utility;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Executable.Forms
{
    public partial class FormAdditional : Form
    {
        private readonly Label[] _values = new Label[10];
        private Timer _timer;

        public FormAdditional()
        {
            // UI nova (sem watermark PB / sem corte)
            FormBorderStyle = FormBorderStyle.None;
            BackColor = FlTheme.Bg;
            ForeColor = FlTheme.Text;
            Dock = DockStyle.Fill;
            BuildUi();
            Load += (s, e) =>
            {
                _timer = new Timer { Interval = 1000 };
                _timer.Tick += RefreshValues;
                _timer.Start();
                RefreshValues(null, null);
            };
            FormClosed += (s, e) =>
            {
                if (_timer != null) { _timer.Stop(); _timer.Dispose(); }
            };
        }

        private void BuildUi()
        {
            var box = new GroupBox
            {
                Text = "Informacoes extras",
                Dock = DockStyle.Fill,
                ForeColor = FlTheme.TextMuted,
                FlatStyle = FlatStyle.Flat,
                BackColor = FlTheme.Bg,
                Padding = new Padding(12, 18, 12, 12),
                Font = new Font("Segoe UI", 9f)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                AutoSize = false,
                BackColor = FlTheme.Bg
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58f));

            string[] titles =
            {
                "Versao do servidor",
                "Regiao do servidor",
                "Endereco local",
                "Portas",
                "Tempo online",
                "Indice de config",
                "ProcessSplit",
                "Regra de torneio",
                "Internet Cafe / Auto-conta",
                "Auto-ban / Banco"
            };

            for (int i = 0; i < titles.Length; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 10f));
                var title = new Label
                {
                    Text = titles[i],
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = FlTheme.TextMuted,
                    Font = new Font("Segoe UI", 9f),
                    AutoEllipsis = true
                };
                var value = new Label
                {
                    Text = "...",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = FlTheme.Text,
                    Font = new Font("Segoe UI Semibold", 9f),
                    AutoEllipsis = true
                };
                _values[i] = value;
                table.Controls.Add(title, 0, i);
                table.Controls.Add(value, 1, i);
            }

            box.Controls.Add(table);
            Controls.Add(box);
        }

        private void SetValue(int i, string text, Color? color = null)
        {
            if (_values[i] == null) return;
            _values[i].Text = text ?? "";
            _values[i].ForeColor = color ?? FlTheme.Text;
        }

        private static Color FlagColor(string v)
        {
            if (v == "Ativado") return FlTheme.Ok;
            if (v == "Desativado") return FlTheme.Warn;
            return FlTheme.Text;
        }

        private void RefreshValues(object sender, EventArgs e)
        {
            SetValue(0, StringUtility.ForGameVersionL);
            SetValue(1, StringUtility.ForGameRegionL);
            SetValue(2, StringUtility.LocalAddressL);
            SetValue(3, StringUtility.PortsL);
            SetValue(4, StringUtility.ServerTimelineL);
            SetValue(5, StringUtility.SelectedServerConfigL);
            SetValue(6, StringUtility.ProcessSplitL, FlagColor(StringUtility.ProcessSplitL));
            SetValue(7, StringUtility.TournamentRuleL, FlagColor(StringUtility.TournamentRuleL));
            SetValue(8, $"{StringUtility.InternetCafeL} / {StringUtility.EnableAutoAccountL}", FlagColor(StringUtility.InternetCafeL));
            SetValue(9, $"{StringUtility.AutoBanPlayerL} / {StringUtility.DbHostL}", FlagColor(StringUtility.AutoBanPlayerL));
        }

        // stubs do Designer antigo (nao usados)
        private void FormAdditional_Load(object sender, EventArgs e) { }
        private void RefresherT_Tick(object sender, EventArgs e) { }
    }
}
