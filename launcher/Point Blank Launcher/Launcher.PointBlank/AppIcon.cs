using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Launcher.PointBlank
{
    /// <summary>
    /// Aplica o ícone FrontLine em todas as janelas (Explorer + barra de tarefas).
    /// </summary>
    internal static class AppIcon
    {
        private static Icon _cached;

        public static void Apply(Form form)
        {
            if (form == null)
                return;

            try
            {
                form.Icon = GetIcon();
            }
            catch
            {
                // ignore — fallback do sistema
            }
        }

        private static Icon GetIcon()
        {
            if (_cached != null)
                return _cached;

            // 1) ícone embutido no EXE (ApplicationIcon)
            try
            {
                string exe = Application.ExecutablePath;
                if (!string.IsNullOrEmpty(exe) && File.Exists(exe))
                {
                    Icon extracted = Icon.ExtractAssociatedIcon(exe);
                    if (extracted != null)
                    {
                        _cached = extracted;
                        return _cached;
                    }
                }
            }
            catch { }

            // 2) Icon.ico ao lado do assembly (dev)
            try
            {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Application.StartupPath;
                string path = Path.Combine(dir, "Icon.ico");
                if (File.Exists(path))
                {
                    _cached = new Icon(path);
                    return _cached;
                }
            }
            catch { }

            return SystemIcons.Application;
        }
    }
}
