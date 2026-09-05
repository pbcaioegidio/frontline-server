using Launcher.PointBlank.Services;
using Launcher.PointBlank.Utils;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Launcher.PointBlank
{
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]

        static void Main()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Logger.LogEnd();

            Process aProcess = Process.GetCurrentProcess();
            string aProcName = aProcess.ProcessName;
            if (Process.GetProcessesByName(aProcName).Length > 1)
            {
                Logger.LogStart();
                Logger.Log("Você não pode executar dois programas ao mesmo tempo.");
                MessageBox.Show("Não é permitido abrir dois programas ao mesmo tempo.", "FRONTLINE", MessageBoxButtons.OK);
                return;
            }
            else
            {
                Logger.LogHeader("20260515");
                Logger.LogState(UpdaterState.UPDATER_STATE_UNKNOWN, UpdaterState.UPDATER_STATE_START);

                AppDomain.CurrentDomain.ProcessExit += (s, e) => SocketBootstrap.StopOwned();
                Application.ApplicationExit += (s, e) => SocketBootstrap.StopOwned();

                // UI do launcher é pixel-fixa + WebBrowser IE: DPI do Windows bagunça layout
                Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Init());
            }
        }
    }
}
