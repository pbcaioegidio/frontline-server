using Executable.UDP.Server;
using Executable.Utility;
using Plugin.Core;
using Plugin.Core.Filters;
using Plugin.Core.JSON;
using Plugin.Core.Managers;
using Plugin.Core.RAW;
using Plugin.Core.Settings;
using Plugin.Core.XML;
using Server.Game;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Executable.Forms
{
    public partial class FormConfig : Form
    {
        private readonly DirectoryInfo ServerDir;
        private RadioButton DefaultRB;
        private RadioButton HighRB;
        private readonly ToolTip tips = new ToolTip();

        public FormConfig(DirectoryInfo ServerDir)
        {
            this.ServerDir = ServerDir;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = FlTheme.Bg;
            ForeColor = FlTheme.Text;
            Dock = DockStyle.Fill;
            BuildUi();
            Load += (s, e) =>
            {
                if (ConfigLoader.ShowMoreInfo) MoreInfoStatus(true, false);
                else MoreInfoStatus(false, true);
            };
            FormClosed += (s, e) => tips.Dispose();
        }

        private void BuildUi()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10, 8, 10, 10),
                BackColor = FlTheme.Bg
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // modo de log (indicador)
            var logRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = FlTheme.Bg,
                Padding = new Padding(2, 4, 0, 0)
            };
            var logLbl = new Label
            {
                Text = "Modo de log:",
                AutoSize = true,
                ForeColor = FlTheme.TextMuted,
                Margin = new Padding(0, 4, 10, 0)
            };
            DefaultRB = MakeRadio("Log padrao");
            HighRB = MakeRadio("Log alto");
            logRow.Controls.Add(logLbl);
            logRow.Controls.Add(DefaultRB);
            logRow.Controls.Add(HighRB);

            // 8 botoes — 2 colunas x 4 linhas (4 esquerda, 4 direita)
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                BackColor = FlTheme.Bg,
                Margin = new Padding(0)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            for (int r = 0; r < 4; r++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

            var b1 = MakeBtn("Mudar modo de log", ChangeLogBTN_Click);
            var b2 = MakeBtn("Limpar logs", ClearLogBTN_Click);
            var b3 = MakeBtn("Recarregar config", Reload1BTN_Click);
            var b4 = MakeBtn("Recarregar loja", Reload2BTN_Click);
            var b5 = MakeBtn("Recarregar eventos", Reload3BTN_Click);
            var b6 = MakeBtn("Recarregar regras", Reload4BTN_Click);
            var b7 = MakeBtn("Recarregar anexos", Reload5BTN_Click);
            var b8 = MakeBtn("Abrir Settings.ini", OpenConfigBTN_Click);

            tips.SetToolTip(b1, "Alterna log padrao / alto (MoreInfo).");
            tips.SetToolTip(b2, "Apaga arquivos da pasta Logs.");
            tips.SetToolTip(b3, "ServerConfig, comandos e resolucoes.");
            tips.SetToolTip(b4, "Itens e dados da loja.");
            tips.SetToolTip(b5, "Eventos (login, quest, visita...).");
            tips.SetToolTip(b6, "Regras de torneio / classic mode.");
            tips.SetToolTip(b7, "Mapas, ranks, missoes, boxes...");
            tips.SetToolTip(b8, "Abre Config/Settings.ini no Notepad.");

            // esquerda (coluna 0)
            grid.Controls.Add(b1, 0, 0);
            grid.Controls.Add(b2, 0, 1);
            grid.Controls.Add(b3, 0, 2);
            grid.Controls.Add(b4, 0, 3);
            // direita (coluna 1)
            grid.Controls.Add(b5, 1, 0);
            grid.Controls.Add(b6, 1, 1);
            grid.Controls.Add(b7, 1, 2);
            grid.Controls.Add(b8, 1, 3);

            root.Controls.Add(logRow, 0, 0);
            root.Controls.Add(grid, 0, 1);
            Controls.Add(root);
        }

        private RadioButton MakeRadio(string text)
        {
            return new RadioButton
            {
                Text = text,
                AutoSize = true,
                AutoCheck = false,
                ForeColor = FlTheme.Text,
                BackColor = FlTheme.Bg,
                Margin = new Padding(0, 2, 16, 0),
                Cursor = Cursors.Hand
            };
        }

        private Button MakeBtn(string text, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                Font = new Font("Segoe UI", 8.5f),
                TextAlign = ContentAlignment.MiddleCenter
            };
            FlTheme.StyleGhostButton(b);
            b.FlatAppearance.BorderColor = FlTheme.Accent;
            b.Click += onClick;
            return b;
        }

        private void MoreInfoStatus(bool High, bool Default)
        {
            if (HighRB != null) HighRB.Checked = High;
            if (DefaultRB != null) DefaultRB.Checked = Default;
        }

        private static void PrintSection(string Name, string Type)
        {
            StringBuilder ST = new StringBuilder(80);
            if (Name != null) ST.Append("---[").Append(Name).Append(']');
            string End = (Type == null) ? "" : ($"[{Type}]---");
            int Count = 79 - End.Length;
            while (ST.Length != Count) ST.Append('-');
            ST.Append(End);
            try { Console.WriteLine($"{(Type.Equals("Ended") ? $"{ST}\n" : $"\n{ST}")}"); } catch { }
        }

        private void OpenConfigBTN_Click(object sender, EventArgs e)
        {
            new Thread(() => MemoryUtility.ShellMode(@"Config/Settings.ini", "notepad.exe", "open")).Start();
        }

        private void ChangeLogBTN_Click(object sender, EventArgs e)
        {
            ConfigEngine CFG = new ConfigEngine("Config/Settings.ini");
            ConfigLoader.ShowMoreInfo = !ConfigLoader.ShowMoreInfo;
            MoreInfoStatus(ConfigLoader.ShowMoreInfo, !ConfigLoader.ShowMoreInfo);
            new Thread(() =>
            {
                string Key = "MoreInfo", Section = "Server";
                if (!CFG.KeyExists(Key, Section))
                {
                    BeginInvoke(new Action(() =>
                        MessageBox.Show($"Chave '{Key}' na secao '{Section}' nao existe!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                    return;
                }
                CFG.WriteX(Key, ConfigLoader.ShowMoreInfo, Section);
            }).Start();
            MessageBox.Show(
                $"Modo de log: {(ConfigLoader.ShowMoreInfo ? "Alto" : "Padrao")}.",
                "FrontLine", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClearLogBTN_Click(object sender, EventArgs e)
        {
            new Thread(() => MemoryUtility.ClearCache(ServerDir)).Start();
            MessageBox.Show("Logs limpos.", "FrontLine", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Reload1BTN_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                PrintSection("Config", "Begin");
                if (ConfigLoader.ProcessSplit) SupervisorCommands.BroadcastReload(1);
                ServerConfigJSON.Reload();
                CommandHelperJSON.Reload();
                ResolutionJSON.Reload();
                PrintSection("Config", "Ended");
            }).Start();
            MessageBox.Show("Config recarregada.", "FrontLine", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Reload2BTN_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                PrintSection("Shop Data", "Begin");
                if (ConfigLoader.ProcessSplit)
                {
                    SupervisorCommands.BroadcastReload(2);
                    PrintSection("Shop Data", "Ended");
                    return;
                }
                ShopManager.Reset();
                ShopManager.Load(1);
                ShopManager.Load(2);
                PrintSection("Shop Data", "Ended");
                GameXender.UpdateShop();
            }).Start();
            MessageBox.Show("Loja recarregada.", "FrontLine", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Reload3BTN_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                PrintSection("Events Data", "Begin");
                if (ConfigLoader.ProcessSplit)
                {
                    SupervisorCommands.BroadcastReload(3);
                    PrintSection("Events Data", "Ended");
                    return;
                }
                EventLoginXML.Reload();
                EventBoostXML.Reload();
                EventPlaytimeJSON.Reload();
                EventQuestXML.Reload();
                EventRankUpXML.Reload();
                EventVisitXML.Reload();
                EventXmasXML.Reload();
                PrintSection("Events Data", "Ended");
                GameXender.UpdateEvents();
            }).Start();
            MessageBox.Show("Eventos recarregados.", "FrontLine", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Reload4BTN_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                PrintSection("Classic Mode", "Begin");
                if (ConfigLoader.ProcessSplit) SupervisorCommands.BroadcastReload(4);
                GameRuleXML.Reload();
                PrintSection("Classic Mode", "Ended");
            }).Start();
            MessageBox.Show("Regras recarregadas.", "FrontLine", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Reload5BTN_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                PrintSection("Server Data", "Begin");
                if (ConfigLoader.ProcessSplit)
                {
                    SupervisorCommands.BroadcastReload(5);
                    SChannelXML.Reload();
                    ChannelTypeConditionManager.Reload();
                    SynchronizeXML.Reload();
                    PrintSection("Server Data", "Ended");
                    return;
                }
                TemplatePackXML.Reload();
                TitleSystemXML.Reload();
                TitleAwardXML.Reload();
                MissionAwardXML.Reload();
                MissionConfigXML.Reload();
                MissionStreamXML.Reload();
                SChannelXML.Reload();
                ChannelTypeConditionManager.Reload();
                SynchronizeXML.Reload();
                SystemMapXML.Reload();
                ClanRankXML.Reload();
                PlayerRankXML.Reload();
                CouponEffectXML.Reload();
                PermissionXML.Reload();
                RandomBoxXML.Reload();
                BattleBoxXML.Reload();
                DirectLibraryXML.Reload();
                InternetCafeXML.Reload();
                RedeemCodeXML.Reload();
                NickFilter.Reload();
                Server.Auth.Data.XML.ChannelsXML.Reload();
                Server.Game.Data.XML.ChannelsXML.Reload();
                Server.Match.Data.XML.MapStructureXML.Reload();
                Server.Match.Data.XML.CharaStructureXML.Reload();
                Server.Match.Data.XML.ItemStatisticXML.Reload();
                PrintSection("Server Data", "Ended");
            }).Start();
            MessageBox.Show("Anexos recarregados.", "FrontLine", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // stubs do Designer antigo
        private void FormConfig_Load(object sender, EventArgs e) { }
    }
}
