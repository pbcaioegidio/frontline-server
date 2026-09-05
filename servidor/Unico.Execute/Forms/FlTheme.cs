using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Executable.Forms
{
    /// <summary>Paleta FrontLine — visual tecnico/profissional.</summary>
    internal static class FlTheme
    {
        public static readonly Color Bg = Color.FromArgb(14, 18, 28);
        public static readonly Color Surface = Color.FromArgb(22, 28, 42);
        public static readonly Color SurfaceAlt = Color.FromArgb(30, 38, 56);
        public static readonly Color Border = Color.FromArgb(48, 58, 78);
        public static readonly Color Accent = Color.FromArgb(56, 132, 220);
        public static readonly Color AccentHover = Color.FromArgb(72, 150, 235);
        public static readonly Color Text = Color.FromArgb(232, 236, 242);
        public static readonly Color TextMuted = Color.FromArgb(150, 160, 176);
        public static readonly Color Ok = Color.FromArgb(72, 200, 120);
        public static readonly Color Warn = Color.FromArgb(230, 180, 70);
        public static readonly Color Danger = Color.FromArgb(220, 80, 80);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string app, string id);

        public static void TryDarkScroll(Control c)
        {
            if (c == null || !c.IsHandleCreated) return;
            try
            {
                // Win10/11: scrollbar escuro; fallback classico se falhar
                if (SetWindowTheme(c.Handle, "DarkMode_Explorer", null) != 0)
                    SetWindowTheme(c.Handle, "", "");
            }
            catch { }
        }

        public static void StylePrimaryButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Accent;
            b.FlatAppearance.MouseOverBackColor = AccentHover;
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 100, 180);
            b.BackColor = SurfaceAlt;
            b.ForeColor = Text;
            b.Cursor = Cursors.Hand;
            b.Height = 28;
        }

        public static void StyleGhostButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = Border;
            b.FlatAppearance.MouseOverBackColor = SurfaceAlt;
            b.FlatAppearance.MouseDownBackColor = Border;
            b.BackColor = Surface;
            b.ForeColor = TextMuted;
            b.Cursor = Cursors.Hand;
            b.Height = 28;
        }

        public static void StyleNavButton(Button b, bool selected)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.ForeColor = selected ? Text : TextMuted;
            b.BackColor = selected ? SurfaceAlt : Bg;
            b.Cursor = Cursors.Hand;
        }
    }
}
