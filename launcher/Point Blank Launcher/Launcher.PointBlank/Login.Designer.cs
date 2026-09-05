namespace Launcher.PointBlank
{
    partial class Login
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Login_Box = new System.Windows.Forms.TextBox();
            this.Password_Box = new System.Windows.Forms.TextBox();
            this.LoginButton = new System.Windows.Forms.PictureBox();
            this.CloseButton = new System.Windows.Forms.PictureBox();
            this.LoginSave = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.LoginButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseButton)).BeginInit();
            this.SuspendLayout();
            // 
            // Login_Box
            // 
            this.Login_Box.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.Login_Box.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Login_Box.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Login_Box.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(120)))), ((int)(((byte)(254)))));
            this.Login_Box.Location = new System.Drawing.Point(308, 140);
            this.Login_Box.Margin = new System.Windows.Forms.Padding(0);
            this.Login_Box.Name = "Login_Box";
            this.Login_Box.Size = new System.Drawing.Size(197, 20);
            this.Login_Box.TabIndex = 0;
            this.Login_Box.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LoginFields_KeyDown);
            // 
            // Password_Box
            // 
            this.Password_Box.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.Password_Box.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Password_Box.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Password_Box.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(120)))), ((int)(((byte)(254)))));
            this.Password_Box.Location = new System.Drawing.Point(308, 191);
            this.Password_Box.Margin = new System.Windows.Forms.Padding(0);
            this.Password_Box.Name = "Password_Box";
            this.Password_Box.PasswordChar = '*';
            this.Password_Box.Size = new System.Drawing.Size(197, 20);
            this.Password_Box.TabIndex = 1;
            this.Password_Box.KeyDown += new System.Windows.Forms.KeyEventHandler(this.LoginFields_KeyDown);
            // 
            // LoginButton
            // 
            this.LoginButton.BackColor = System.Drawing.Color.Transparent;
            this.LoginButton.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.Bitmap12;
            this.LoginButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.LoginButton.Location = new System.Drawing.Point(270, 255);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(250, 36);
            this.LoginButton.TabIndex = 2;
            this.LoginButton.TabStop = false;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            this.LoginButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LoginButton_MouseDown);
            this.LoginButton.MouseEnter += new System.EventHandler(this.LoginButton_MouseEnter);
            this.LoginButton.MouseLeave += new System.EventHandler(this.LoginButton_MouseLeave);
            // 
            // CloseButton
            // 
            this.CloseButton.BackColor = System.Drawing.Color.Transparent;
            this.CloseButton.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.Bitmap21;
            this.CloseButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CloseButton.Location = new System.Drawing.Point(745, 21);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(24, 24);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.TabStop = false;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            this.CloseButton.MouseEnter += new System.EventHandler(this.CloseButton_MouseEnter);
            this.CloseButton.MouseLeave += new System.EventHandler(this.CloseButton_MouseLeave);
            // 
            // LoginSave
            // 
            this.LoginSave.AutoSize = true;
            this.LoginSave.BackColor = System.Drawing.Color.Transparent;
            this.LoginSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.LoginSave.Location = new System.Drawing.Point(270, 232);
            this.LoginSave.Name = "LoginSave";
            this.LoginSave.Size = new System.Drawing.Size(120, 17);
            this.LoginSave.TabIndex = 4;
            this.LoginSave.Text = "Manter conectado";
            this.LoginSave.UseVisualStyleBackColor = false;
            // 
            // Login
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackgroundImage = global::Launcher.PointBlank.Properties.Resources.Bitmap30;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(790, 531);
            this.MaximumSize = new System.Drawing.Size(790, 531);
            this.MinimumSize = new System.Drawing.Size(790, 531);
            this.Controls.Add(this.LoginSave);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.LoginButton);
            this.Controls.Add(this.Password_Box);
            this.Controls.Add(this.Login_Box);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Login";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Login";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Login_MouseDown);
            ((System.ComponentModel.ISupportInitialize)(this.LoginButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.CloseButton)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Login_Box;
        private System.Windows.Forms.TextBox Password_Box;
        private System.Windows.Forms.PictureBox LoginButton;
        private System.Windows.Forms.PictureBox CloseButton;
        private System.Windows.Forms.CheckBox LoginSave;
    }
}