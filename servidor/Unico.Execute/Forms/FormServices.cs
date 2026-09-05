using Executable.Supervision;
using Plugin.Core;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Executable.Forms
{
    public class FormServices : Form
    {
        private readonly ListView list = new ListView();
        private readonly Button startBtn = new Button();
        private readonly Button stopBtn = new Button();
        private readonly Button restartBtn = new Button();
        private readonly Timer refresher = new Timer();

        public FormServices()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = FlTheme.Bg;
            ForeColor = FlTheme.Text;
            Dock = DockStyle.Fill;

            list.View = View.Details;
            list.FullRowSelect = true;
            list.HideSelection = false;
            list.MultiSelect = false;
            list.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            list.BackColor = FlTheme.Bg;
            list.ForeColor = FlTheme.Text;
            list.BorderStyle = BorderStyle.FixedSingle;
            list.Columns.Add("Servico", 90);
            list.Columns.Add("Status", 90);
            list.Columns.Add("PID", 70);
            list.Columns.Add("RAM (MB)", 80);
            list.Columns.Add("Uptime", 90);
            list.Columns.Add("Restarts", 70);
            list.Dock = DockStyle.Fill;
            list.HandleCreated += (s, e) => FlTheme.TryDarkScroll(list);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                Padding = new Padding(8, 8, 8, 8),
                BackColor = FlTheme.Bg
            };
            SetupButton(startBtn, "Iniciar", (s, e) => WithSelected(n => ProcessSupervisor.Instance.Start(n)));
            SetupButton(stopBtn, "Parar", (s, e) => WithSelected(n =>
            {
                if (MessageBox.Show($"Parar o servico '{n}'? Jogadores conectados serao derrubados.",
                        "Confirmacao", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    ProcessSupervisor.Instance.Stop(n);
            }));
            SetupButton(restartBtn, "Reiniciar", (s, e) => WithSelected(n =>
            {
                if (MessageBox.Show($"Reiniciar o servico '{n}'? Jogadores conectados serao derrubados.",
                        "Confirmacao", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    ProcessSupervisor.Instance.Restart(n);
            }));
            buttons.Controls.Add(startBtn);
            buttons.Controls.Add(stopBtn);
            buttons.Controls.Add(restartBtn);

            Controls.Add(list);
            Controls.Add(buttons);

            refresher.Interval = 1000;
            refresher.Tick += (s, e) => RefreshList();
            Load += OnLoadForm;
        }

        private void OnLoadForm(object sender, EventArgs e)
        {
            FlTheme.TryDarkScroll(list);
            if (!ConfigLoader.ProcessSplit)
            {
                var warn = new Label
                {
                    Text = "ProcessSplit desativado no Settings.ini:\nservicos rodando in-process.",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = FlTheme.TextMuted,
                    BackColor = FlTheme.Bg
                };
                Controls.Add(warn);
                warn.BringToFront();
                startBtn.Enabled = stopBtn.Enabled = restartBtn.Enabled = false;
                return;
            }
            RefreshList();
            refresher.Start();
        }

        private void SetupButton(Button b, string text, EventHandler onClick)
        {
            b.Text = text;
            b.Width = 100;
            FlTheme.StyleGhostButton(b);
            b.Margin = new Padding(0, 0, 8, 0);
            b.Click += onClick;
        }

        private void WithSelected(Action<string> action)
        {
            if (list.SelectedItems.Count == 0) return;
            string name = list.SelectedItems[0].Text.ToLowerInvariant();
            new System.Threading.Thread(() => action(name)) { IsBackground = true }.Start();
        }

        private void RefreshList()
        {
            var statuses = ProcessSupervisor.Instance.GetStatus();
            list.BeginUpdate();
            list.Items.Clear();
            foreach (var s in statuses)
            {
                string uptime = s.Running ? (DateTime.Now - s.StartedAt).ToString(@"hh\:mm\:ss") : "-";
                var item = new ListViewItem(new[]
                {
                    s.Name.ToUpperInvariant(),
                    s.Running ? "RUNNING" : "STOPPED",
                    s.Running ? s.Pid.ToString() : "-",
                    s.Running ? s.MemoryMB.ToString("0") : "-",
                    uptime,
                    s.RestartCount.ToString()
                });
                item.ForeColor = s.Running ? FlTheme.Ok : FlTheme.Danger;
                list.Items.Add(item);
            }
            list.EndUpdate();
        }
    }
}
