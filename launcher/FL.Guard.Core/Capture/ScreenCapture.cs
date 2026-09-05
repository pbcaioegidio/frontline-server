using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FL.Guard.Core.Capture
{
    /// <summary>
    /// Captura a janela do jogo.
    /// DirectX: PrintWindow → BitBlt → CopyFromScreen.
    /// allowOccluded=false (ring/clip): CopyFromScreen só com jogo em foco (não grava Cursor/browser no buffer).
    /// allowOccluded=true (screenshot GM): captura o retângulo mesmo coberta — mostra overlay se houver.
    /// </summary>
    public static class ScreenCapture
    {
        private static readonly string[] GameProcessNames =
        {
            "FrontLine", "PointBlank", "PointBlank.i3Exec"
        };

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
            IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        private const int SRCCOPY = 0x00CC0020;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        public sealed class CaptureResult
        {
            public bool Ok;
            public bool Blocked;
            public string Error = "";
            public byte[] Jpeg;
            public int Width;
            public int Height;
            public string ProcessName = "";
            public DateTime CapturedAtUtc = DateTime.UtcNow;
        }

        /// <summary>Screenshot JPEG da janela do jogo, com marca d'água FL GUARD.</summary>
        /// <param name="allowOccluded">Se true, CopyFromScreen mesmo com outra janela na frente (screenshot GM).</param>
        public static CaptureResult CaptureGameWindow(string preferredProcess = null, int maxWidth = 1280, long jpegQuality = 70L, bool allowOccluded = false)
        {
            var result = new CaptureResult();
            try
            {
                Process proc = FindGameProcess(preferredProcess);
                if (proc == null)
                {
                    result.Blocked = true;
                    result.Error = "janela do jogo não encontrada";
                    return result;
                }

                result.ProcessName = proc.ProcessName;
                IntPtr hwnd = proc.MainWindowHandle;
                if (hwnd == IntPtr.Zero || !IsWindowVisible(hwnd))
                {
                    result.Blocked = true;
                    result.Error = "janela do jogo inacessível";
                    return result;
                }

                if (!GetClientRect(hwnd, out RECT client) || client.Right - client.Left < 32 || client.Bottom - client.Top < 32)
                {
                    result.Blocked = true;
                    result.Error = "área da janela inválida";
                    return result;
                }

                int w = client.Right - client.Left;
                int h = client.Bottom - client.Top;
                POINT origin = new POINT { X = 0, Y = 0 };
                ClientToScreen(hwnd, ref origin);

                using (var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb))
                {
                    bool gotPixels = false;
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        IntPtr hdc = g.GetHdc();
                        try
                        {
                            // 1) PrintWindow (funciona em algumas janelas GDI)
                            if (PrintWindow(hwnd, hdc, 2 /* PW_RENDERFULLCONTENT */) && !IsMostlyBlack(bmp))
                                gotPixels = true;

                            // 2) BitBlt do DC da janela (melhor que capturar a tela toda)
                            if (!gotPixels)
                            {
                                IntPtr src = GetWindowDC(hwnd);
                                if (src != IntPtr.Zero)
                                {
                                    try
                                    {
                                        if (BitBlt(hdc, 0, 0, w, h, src, 0, 0, SRCCOPY) && !IsMostlyBlack(bmp))
                                            gotPixels = true;
                                    }
                                    finally { ReleaseDC(hwnd, src); }
                                }
                            }
                        }
                        finally { g.ReleaseHdc(hdc); }

                        // 3) CopyFromScreen — DX costuma precisar. Ring: só com foco. Screenshot GM: permite overlay.
                        if (!gotPixels)
                        {
                            bool gameFg = IsGameForeground(hwnd, proc);
                            if (gameFg || allowOccluded)
                            {
                                g.CopyFromScreen(origin.X, origin.Y, 0, 0, new Size(w, h), CopyPixelOperation.SourceCopy);
                                if (!IsMostlyBlack(bmp))
                                    gotPixels = true;
                            }
                            else
                            {
                                result.Blocked = true;
                                result.Error = "jogo fora de foco — frame ignorado (evita Cursor/browser no clip)";
                                return result;
                            }
                        }

                        if (!gotPixels || IsMostlyBlack(bmp))
                        {
                            result.Blocked = true;
                            result.Error = IsGameForeground(hwnd, proc)
                                ? "conteúdo da janela bloqueado/vazio (DirectX)"
                                : "janela do jogo sem pixels (minimizada ou DX sem composição)";
                            return result;
                        }

                        DrawWatermark(g, w, h);
                    }

                    using (Bitmap scaled = ScaleDown(bmp, maxWidth))
                    {
                        result.Width = scaled.Width;
                        result.Height = scaled.Height;
                        result.Jpeg = EncodeJpeg(scaled, jpegQuality);
                        result.Ok = result.Jpeg != null && result.Jpeg.Length > 0;
                        if (!result.Ok)
                        {
                            result.Blocked = true;
                            result.Error = "falha ao codificar JPEG";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Blocked = true;
                result.Error = Trunc(ex.Message, 200);
            }
            return result;
        }

        /// <summary>Escolhe o processo do jogo com a maior janela visível (evita stubs sem UI).</summary>
        public static Process FindGameProcess(string preferred = null)
        {
            Process best = null;
            int bestArea = 0;

            void Consider(Process p)
            {
                try
                {
                    IntPtr hwnd = p.MainWindowHandle;
                    if (hwnd == IntPtr.Zero || !IsWindowVisible(hwnd)) return;
                    if (!GetClientRect(hwnd, out RECT r)) return;
                    int area = Math.Max(0, r.Right - r.Left) * Math.Max(0, r.Bottom - r.Top);
                    if (area < 100 * 80) return;
                    if (area > bestArea)
                    {
                        bestArea = area;
                        best = p;
                    }
                }
                catch { }
            }

            if (!string.IsNullOrWhiteSpace(preferred))
            {
                string name = Path.GetFileNameWithoutExtension(preferred);
                foreach (Process p in Process.GetProcessesByName(name))
                    Consider(p);
                if (best != null) return best;
            }

            foreach (string name in GameProcessNames)
            {
                foreach (Process p in Process.GetProcessesByName(name))
                    Consider(p);
            }
            if (best != null) return best;

            // Último recurso: janela em foco se o título for do jogo
            try
            {
                IntPtr fg = GetForegroundWindow();
                if (fg != IntPtr.Zero)
                {
                    var sb = new StringBuilder(256);
                    GetWindowText(fg, sb, sb.Capacity);
                    string title = sb.ToString();
                    if (title.IndexOf("FrontLine", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        title.IndexOf("Point Blank", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foreach (Process p in Process.GetProcesses())
                        {
                            try
                            {
                                if (p.MainWindowHandle == fg) return p;
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }

            return null;
        }

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private static bool IsGameForeground(IntPtr gameHwnd, Process gameProc)
        {
            IntPtr fg = GetForegroundWindow();
            if (fg == IntPtr.Zero) return false;
            if (fg == gameHwnd) return true;
            try
            {
                GetWindowThreadProcessId(fg, out uint pid);
                if (gameProc != null && !gameProc.HasExited && pid == (uint)gameProc.Id)
                    return true;
                // qualquer processo FrontLine/PointBlank em foco conta
                using (Process fgProc = Process.GetProcessById((int)pid))
                {
                    string n = fgProc.ProcessName;
                    return n.Equals("FrontLine", StringComparison.OrdinalIgnoreCase)
                        || n.Equals("PointBlank", StringComparison.OrdinalIgnoreCase)
                        || n.Equals("PointBlank.i3Exec", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { return false; }
        }

        private static bool IsChildOrSame(IntPtr a, IntPtr b) => a != IntPtr.Zero && a == b;

        private static void DrawWatermark(Graphics g, int w, int h)
        {
            string text = "FL GUARD  " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
            using (var font = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
            using (var shadow = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            {
                SizeF size = g.MeasureString(text, font);
                float x = 12f;
                float y = h - size.Height - 12f;
                g.DrawString(text, font, shadow, x + 1, y + 1);
                g.DrawString(text, font, brush, x, y);
            }
        }

        private static Bitmap ScaleDown(Bitmap src, int maxWidth)
        {
            if (src.Width <= maxWidth) return (Bitmap)src.Clone();
            int nh = (int)(src.Height * (maxWidth / (double)src.Width));
            var dst = new Bitmap(maxWidth, Math.Max(1, nh), PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.DrawImage(src, 0, 0, maxWidth, nh);
            }
            return dst;
        }

        private static byte[] EncodeJpeg(Bitmap bmp, long quality)
        {
            ImageCodecInfo codec = null;
            foreach (ImageCodecInfo c in ImageCodecInfo.GetImageEncoders())
                if (c.FormatID == ImageFormat.Jpeg.Guid) { codec = c; break; }
            if (codec == null) return null;

            using (var ms = new MemoryStream())
            using (var ep = new EncoderParameters(1))
            {
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                bmp.Save(ms, codec, ep);
                return ms.ToArray();
            }
        }

        private static bool IsMostlyBlack(Bitmap bmp)
        {
            int black = 0, samples = 0;
            int stepX = Math.Max(1, bmp.Width / 8);
            int stepY = Math.Max(1, bmp.Height / 8);
            for (int y = stepY / 2; y < bmp.Height; y += stepY)
            {
                for (int x = stepX / 2; x < bmp.Width; x += stepX)
                {
                    Color c = bmp.GetPixel(x, y);
                    samples++;
                    if (c.R < 8 && c.G < 8 && c.B < 8) black++;
                }
            }
            return samples > 0 && black * 100 / samples >= 92;
        }

        private static string Trunc(string s, int max)
        {
            if (s == null) return "";
            return s.Length <= max ? s : s.Substring(0, max);
        }
    }
}
