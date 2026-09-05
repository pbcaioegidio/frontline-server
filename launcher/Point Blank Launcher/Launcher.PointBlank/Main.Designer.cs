namespace Launcher.PointBlank
{
    partial class Main
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.CheckButton = new System.Windows.Forms.PictureBox();
            this.StartButton = new System.Windows.Forms.PictureBox();
            this.MinimizeButton = new System.Windows.Forms.PictureBox();
            this.CloseButton = new System.Windows.Forms.PictureBox();
            this.FileBar = new System.Windows.Forms.PictureBox();
            this.TotalBar = new System.Windows.Forms.PictureBox();
            this.ConfigButton = new System.Windows.Forms.PictureBox();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.contentPlaceholder = new System.Windows.Forms.Panel();
            this.contentTitle = new System.Windows.Forms.Label();
            this.contentHint = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.FILE_TEXT = new System.Windows.Forms.Label();
            this.TOTAL_TEXT = new System.Windows.Forms.Label();
            this.TEXT_STATUS = new System.Windows.Forms.Label();
            this.UpdateButton = new System.Windows.Forms.PictureBox();
            this.LoginScreen = new System.Windows.Forms.PictureBox();
            this.TEXT_USER = new System.Windows.Forms.Label();
            this.LogoutButton = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.CheckButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StartButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinimizeButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FileBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TotalBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ConfigButton)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UpdateButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoginScreen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoutButton)).BeginInit();
            this.SuspendLayout();
            // 
            // CheckButton
            // 
            this.CheckButton.BackColor = System.Drawing.Color.Transparent;
            this.CheckButton.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.CheckLeave;
            this.CheckButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CheckButton.Location = new System.Drawing.Point(507, 450);
            this.CheckButton.Name = "CheckButton";
            this.CheckButton.Size = new System.Drawing.Size(106, 76);
            this.CheckButton.TabIndex = 0;
            this.CheckButton.TabStop = false;
            this.CheckButton.Click += new System.EventHandler(this.CheckButton_Click);
            this.CheckButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CheckButton_MouseDown);
            this.CheckButton.MouseEnter += new System.EventHandler(this.CheckButton_MouseEnter);
            this.CheckButton.MouseLeave += new System.EventHandler(this.CheckButton_MouseLeave);
            // 
            // StartButton
            // 
            this.StartButton.BackColor = System.Drawing.Color.Transparent;
            this.StartButton.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.StartLeave;
            this.StartButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.StartButton.Location = new System.Drawing.Point(619, 450);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(166, 76);
            this.StartButton.TabIndex = 1;
            this.StartButton.TabStop = false;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            this.StartButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.StartButton_MouseDown);
            this.StartButton.MouseEnter += new System.EventHandler(this.StartButton_MouseEnter);
            this.StartButton.MouseLeave += new System.EventHandler(this.StartButton_MouseLeave);
            // 
            // MinimizeButton
            // 
            this.MinimizeButton.BackColor = System.Drawing.Color.Transparent;
            this.MinimizeButton.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.Bitmap136;
            this.MinimizeButton.Location = new System.Drawing.Point(737, 1);
            this.MinimizeButton.Name = "MinimizeButton";
            this.MinimizeButton.Size = new System.Drawing.Size(25, 25);
            this.MinimizeButton.TabIndex = 2;
            this.MinimizeButton.TabStop = false;
            this.MinimizeButton.Click += new System.EventHandler(this.MinimizeButton_Click);
            this.MinimizeButton.MouseEnter += new System.EventHandler(this.MinimizeButton_MouseEnter);
            this.MinimizeButton.MouseLeave += new System.EventHandler(this.MinimizeButton_MouseLeave);
            // 
            // CloseButton
            // 
            this.CloseButton.BackColor = System.Drawing.Color.Transparent;
            this.CloseButton.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.Bitmap133;
            this.CloseButton.Location = new System.Drawing.Point(764, 1);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(25, 25);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.TabStop = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            this.CloseButton.MouseEnter += new System.EventHandler(this.CloseButton_MouseEnter);
            this.CloseButton.MouseLeave += new System.EventHandler(this.CloseButton_MouseLeave);
            // 
            // FileBar
            // 
            this.FileBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(172)))), ((int)(((byte)(240)))));
            this.FileBar.Location = new System.Drawing.Point(11, 41);
            this.FileBar.Name = "FileBar";
            this.FileBar.Size = new System.Drawing.Size(463, 9);
            this.FileBar.TabIndex = 4;
            this.FileBar.TabStop = false;
            // 
            // TotalBar
            // 
            this.TotalBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(172)))), ((int)(((byte)(240)))));
            this.TotalBar.Location = new System.Drawing.Point(11, 73);
            this.TotalBar.Name = "TotalBar";
            this.TotalBar.Size = new System.Drawing.Size(463, 9);
            this.TotalBar.TabIndex = 5;
            this.TotalBar.TabStop = false;
            // 
            // ConfigButton
            // 
            this.ConfigButton.BackColor = System.Drawing.Color.Transparent;
            this.ConfigButton.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.Bitmap1271;
            this.ConfigButton.Location = new System.Drawing.Point(709, 1);
            this.ConfigButton.Name = "ConfigButton";
            this.ConfigButton.Size = new System.Drawing.Size(25, 25);
            this.ConfigButton.TabIndex = 6;
            this.ConfigButton.TabStop = false;
            this.ConfigButton.Click += new System.EventHandler(this.ConfigButton_Click);
            this.ConfigButton.MouseEnter += new System.EventHandler(this.ConfigButton_MouseEnter);
            this.ConfigButton.MouseLeave += new System.EventHandler(this.ConfigButton_MouseLeave);
            // 
            // webBrowser1 (desligado — área web fica para depois)
            // 
            this.webBrowser1.Location = new System.Drawing.Point(2, 26);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScrollBarsEnabled = false;
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(787, 416);
            this.webBrowser1.TabIndex = 7;
            this.webBrowser1.Visible = false;
            // 
            // contentPlaceholder
            // 
            this.contentPlaceholder.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.ContentHero;
            this.contentPlaceholder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.contentPlaceholder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(11)))), ((int)(((byte)(18)))), ((int)(((byte)(32)))));
            this.contentPlaceholder.Location = new System.Drawing.Point(2, 26);
            this.contentPlaceholder.Name = "contentPlaceholder";
            this.contentPlaceholder.Size = new System.Drawing.Size(787, 416);
            this.contentPlaceholder.TabIndex = 14;
            // 
            // contentTitle (sem texto escrito — titulo vem da arte)
            // 
            this.contentTitle.Visible = false;
            this.contentTitle.Text = "";
            this.contentTitle.Size = new System.Drawing.Size(0, 0);
            // 
            // contentHint (oculto)
            // 
            this.contentHint.Visible = false;
            this.contentHint.Name = "contentHint";
            this.contentHint.Size = new System.Drawing.Size(0, 0);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.FILE_TEXT);
            this.panel1.Controls.Add(this.TOTAL_TEXT);
            this.panel1.Controls.Add(this.TEXT_STATUS);
            this.panel1.Controls.Add(this.FileBar);
            this.panel1.Controls.Add(this.TotalBar);
            this.panel1.Location = new System.Drawing.Point(3, 440);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(500, 90);
            this.panel1.TabIndex = 8;
            // 
            // FILE_TEXT
            // 
            this.FILE_TEXT.BackColor = System.Drawing.Color.Transparent;
            this.FILE_TEXT.Font = new System.Drawing.Font("Microsoft Tai Le", 9.75F);
            this.FILE_TEXT.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.FILE_TEXT.Location = new System.Drawing.Point(11, 22);
            this.FILE_TEXT.Name = "FILE_TEXT";
            this.FILE_TEXT.Size = new System.Drawing.Size(463, 16);
            this.FILE_TEXT.TabIndex = 10;
            this.FILE_TEXT.Text = "Arquivo";
            this.FILE_TEXT.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // TOTAL_TEXT
            // 
            this.TOTAL_TEXT.AutoSize = true;
            this.TOTAL_TEXT.BackColor = System.Drawing.Color.Transparent;
            this.TOTAL_TEXT.Font = new System.Drawing.Font("Microsoft Tai Le", 9.75F);
            this.TOTAL_TEXT.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.TOTAL_TEXT.Location = new System.Drawing.Point(11, 53);
            this.TOTAL_TEXT.Name = "TOTAL_TEXT";
            this.TOTAL_TEXT.Size = new System.Drawing.Size(36, 16);
            this.TOTAL_TEXT.TabIndex = 11;
            this.TOTAL_TEXT.Text = "Total";
            this.TOTAL_TEXT.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // TEXT_STATUS
            // 
            this.TEXT_STATUS.BackColor = System.Drawing.Color.Transparent;
            this.TEXT_STATUS.Font = new System.Drawing.Font("Microsoft Tai Le", 9.75F);
            this.TEXT_STATUS.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.TEXT_STATUS.Location = new System.Drawing.Point(11, 3);
            this.TEXT_STATUS.Name = "TEXT_STATUS";
            this.TEXT_STATUS.Size = new System.Drawing.Size(463, 16);
            this.TEXT_STATUS.TabIndex = 9;
            this.TEXT_STATUS.Text = "Non String";
            this.TEXT_STATUS.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // UpdateButton
            // 
            this.UpdateButton.BackColor = System.Drawing.Color.Transparent;
            this.UpdateButton.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.UpdateLeave;
            this.UpdateButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.UpdateButton.Location = new System.Drawing.Point(619, 450);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(166, 76);
            this.UpdateButton.TabIndex = 9;
            this.UpdateButton.TabStop = false;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            this.UpdateButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.UpdateButton_MouseDown);
            this.UpdateButton.MouseEnter += new System.EventHandler(this.UpdateButton_MouseEnter);
            this.UpdateButton.MouseLeave += new System.EventHandler(this.UpdateButton_MouseLeave);
            // 
            // LoginScreen
            // 
            this.LoginScreen.BackColor = System.Drawing.Color.Transparent;
            this.LoginScreen.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.Bitmap15;
            this.LoginScreen.Location = new System.Drawing.Point(586, 2);
            this.LoginScreen.Name = "LoginScreen";
            this.LoginScreen.Size = new System.Drawing.Size(122, 24);
            this.LoginScreen.TabIndex = 10;
            this.LoginScreen.TabStop = false;
            this.LoginScreen.Click += new System.EventHandler(this.LoginButton_Click);
            this.LoginScreen.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LoginScreen_MouseDown);
            this.LoginScreen.MouseEnter += new System.EventHandler(this.LoginScreen_MouseEnter);
            this.LoginScreen.MouseLeave += new System.EventHandler(this.LoginScreen_MouseLeave);
            // 
            // TEXT_USER
            // 
            this.TEXT_USER.BackColor = System.Drawing.Color.Transparent;
            this.TEXT_USER.Font = new System.Drawing.Font("Microsoft Tai Le", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TEXT_USER.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.TEXT_USER.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.TEXT_USER.Location = new System.Drawing.Point(424, 4);
            this.TEXT_USER.Name = "TEXT_USER";
            this.TEXT_USER.Size = new System.Drawing.Size(156, 20);
            this.TEXT_USER.TabIndex = 12;
            this.TEXT_USER.Text = "Non String";
            this.TEXT_USER.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // LogoutButton
            // 
            this.LogoutButton.BackColor = System.Drawing.Color.Transparent;
            this.LogoutButton.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.Bitmap18;
            this.LogoutButton.Location = new System.Drawing.Point(586, 2);
            this.LogoutButton.Name = "LogoutButton";
            this.LogoutButton.Size = new System.Drawing.Size(122, 24);
            this.LogoutButton.TabIndex = 13;
            this.LogoutButton.TabStop = false;
            this.LogoutButton.Click += new System.EventHandler(this.LogoutButton_Click);
            this.LogoutButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LogoutButton_MouseDown);
            this.LogoutButton.MouseEnter += new System.EventHandler(this.LogoutButton_MouseEnter);
            this.LogoutButton.MouseLeave += new System.EventHandler(this.LogoutButton_MouseLeave);
            // 
            // Main
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.Bitmap99;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(790, 531);
            this.MaximumSize = new System.Drawing.Size(790, 531);
            this.MinimumSize = new System.Drawing.Size(790, 531);
            this.Controls.Add(this.TEXT_USER);
            this.Controls.Add(this.contentPlaceholder);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.ConfigButton);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.MinimizeButton);
            this.Controls.Add(this.CheckButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.LogoutButton);
            this.Controls.Add(this.LoginScreen);
            this.Controls.Add(this.UpdateButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FRONTLINE";
            this.Load += new System.EventHandler(this.Main_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Main_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.CheckButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.StartButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MinimizeButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FileBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TotalBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ConfigButton)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UpdateButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoginScreen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LogoutButton)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox CheckButton;
        private System.Windows.Forms.PictureBox StartButton;
        private System.Windows.Forms.PictureBox MinimizeButton;
        private System.Windows.Forms.PictureBox CloseButton;
        private System.Windows.Forms.PictureBox FileBar;
        private System.Windows.Forms.PictureBox TotalBar;
        private System.Windows.Forms.PictureBox ConfigButton;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Panel contentPlaceholder;
        private System.Windows.Forms.Label contentTitle;
        private System.Windows.Forms.Label contentHint;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label TEXT_STATUS;
        private System.Windows.Forms.Label FILE_TEXT;
        private System.Windows.Forms.Label TOTAL_TEXT;
        private System.Windows.Forms.PictureBox UpdateButton;
        private System.Windows.Forms.PictureBox LoginScreen;
        private System.Windows.Forms.Label TEXT_USER;
        private System.Windows.Forms.PictureBox LogoutButton;
    }
}

