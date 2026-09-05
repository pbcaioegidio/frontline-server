namespace Launcher.PointBlank
{
    partial class Init
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.INIT_TEXT = new System.Windows.Forms.Label();
            this.loadingTrack = new System.Windows.Forms.Panel();
            this.loadingFill = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // INIT_TEXT
            // 
            this.INIT_TEXT.BackColor = System.Drawing.Color.Transparent;
            this.INIT_TEXT.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.INIT_TEXT.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(230)))), ((int)(((byte)(240)))));
            this.INIT_TEXT.Location = new System.Drawing.Point(40, 188);
            this.INIT_TEXT.Name = "INIT_TEXT";
            this.INIT_TEXT.Size = new System.Drawing.Size(710, 22);
            this.INIT_TEXT.TabIndex = 0;
            this.INIT_TEXT.Text = "Carregando...";
            this.INIT_TEXT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // loadingTrack
            // 
            this.loadingTrack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(40)))), ((int)(((byte)(55)))));
            this.loadingTrack.Controls.Add(this.loadingFill);
            this.loadingTrack.Location = new System.Drawing.Point(40, 214);
            this.loadingTrack.Name = "loadingTrack";
            this.loadingTrack.Size = new System.Drawing.Size(710, 10);
            this.loadingTrack.TabIndex = 1;
            // 
            // loadingFill
            // 
            this.loadingFill.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(57)))), ((int)(((byte)(220)))), ((int)(((byte)(90)))));
            this.loadingFill.Location = new System.Drawing.Point(0, 0);
            this.loadingFill.Name = "loadingFill";
            this.loadingFill.Size = new System.Drawing.Size(20, 10);
            this.loadingFill.TabIndex = 0;
            // 
            // Init
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(12)))), ((int)(((byte)(20)))));
            this.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.LoadingSplash;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(790, 250);
            this.ControlBox = false;
            this.Controls.Add(this.INIT_TEXT);
            this.Controls.Add(this.loadingTrack);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Init";
            this.ShowInTaskbar = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FRONTLINE";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label INIT_TEXT;
        private System.Windows.Forms.Panel loadingTrack;
        private System.Windows.Forms.Panel loadingFill;
    }
}
