using Plugin.Core;
using Plugin.Core.Enums;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Executable.Utility
{
    public class WindowUtility
    {
        #region Essentials
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;
        private static Size GetScreenSize()
        {
            return new Size(GetSystemMetrics(0), GetSystemMetrics(1));
        }
        private struct Size
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public Size(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }
        [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(HandleRef hWnd, out Rect lpRect);
        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        private static Size GetWindowSize(IntPtr window)
        {
            if (!GetWindowRect(new HandleRef(null, window), out Rect rect))
            {
            }
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            return new Size(width, height);
        }
        #endregion Essentials
        public static void MoveWindowToCenter()
        {
            IntPtr window = Process.GetCurrentProcess().MainWindowHandle;
            if (window == IntPtr.Zero) return;
            Size screenSize = GetScreenSize();
            Size windowSize = GetWindowSize(window);
            int x = (screenSize.Width - windowSize.Width) / 2;
            int y = (screenSize.Height - windowSize.Height) / 2;
            SetWindowPos(window, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        /// <summary>
        /// Encerra o processo e filhos sem System.Management (quebra em WinExe/.NET 8).
        /// </summary>
        public static void KillProcessAndChildren(int processId)
        {
            if (processId <= 0) return;
            try
            {
                using (var proc = Process.GetProcessById(processId))
                {
                    if (!proc.HasExited)
                        proc.Kill(entireProcessTree: true);
                }
                return;
            }
            catch (ArgumentException) { return; }
            catch (Exception ex)
            {
                CLogger.Print($"KillProcessAndChildren: {ex.Message}", LoggerType.Warning);
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/F /T /PID {processId}",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var p = Process.Start(psi))
                    p?.WaitForExit(8000);
            }
            catch (Exception ex)
            {
                CLogger.Print($"taskkill fallback: {ex.Message}", LoggerType.Warning);
            }
        }
    }
}
