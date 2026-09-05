using Executable.Utility;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Executable.Forms
{
    public partial class FormMonitor : Form
    {
        public FormMonitor()
        {
            InitializeComponent();
        }
        private void FormMonitor_Load(object sender, EventArgs e)
        {
            BackgroundImage = null;
            BackColor = FlTheme.Bg;
            SrvInfoGB.BackColor = FlTheme.Bg;
            SrvInfoGB.ForeColor = FlTheme.TextMuted;
            SrvInfoGB.Dock = DockStyle.Fill;
            foreach (Control c in SrvInfoGB.Controls)
            {
                if (c is Label lbl)
                {
                    if (lbl.Left < 180) lbl.ForeColor = FlTheme.TextMuted;
                    else lbl.ForeColor = FlTheme.Text;
                }
            }
            RegisteredUsers.Text = "Carregando...";
            OnlinePlayers.Text = "Carregando...";
            TotalClans.Text = "Carregando...";
            VipUsers.Text = "Carregando...";
            UnknownUsers.Text = "Carregando...";
            TotalBannedPlayers.Text = "Carregando...";
            RegisteredShopItems.Text = "Carregando...";
            ShopCafeItems.Text = "Carregando...";
            RepairableItems.Text = "Carregando...";
            RefresherT.Start();
            Resize += (s2, e2) => StretchMonitorLabels();
            StretchMonitorLabels();
        }

        private void StretchMonitorLabels()
        {
            if (SrvInfoGB == null) return;
            int maxRight = SrvInfoGB.ClientSize.Width - 12;
            foreach (Control c in SrvInfoGB.Controls)
            {
                if (c is Label lbl && lbl.Left >= 180)
                {
                    lbl.AutoSize = false;
                    lbl.Width = Math.Max(60, maxRight - lbl.Left);
                }
            }
        }
        private void RefresherT_Tick(object sender, EventArgs e)
        {
            RegisteredUsers.Text = StringUtility.RegisteredUserL;
            OnlinePlayers.Text = StringUtility.OnlineUserL;
            TotalClans.Text = StringUtility.TotalClansL;
            VipUsers.Text = StringUtility.VipUserL;
            UnknownUsers.Text = StringUtility.UnknownUserL;
            TotalBannedPlayers.Text = StringUtility.BannedPlayers;
            RegisteredShopItems.Text = StringUtility.RegShopItems;
            ShopCafeItems.Text = StringUtility.ShopCafeItems;
            RepairableItems.Text = StringUtility.RepairableItems;
        }
    }
}
