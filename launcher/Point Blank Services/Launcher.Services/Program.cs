using Launcher.Server.Config;
using Launcher.Server.Network;
using Launcher.Services;
using Launcher.Services.Config;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Launcher.Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveLocalAssembly;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            ServiceLog.Init(baseDir);
            Console.SetOut(new LogTextWriter());
            Console.SetError(new LogTextWriter());

            try
            {
                string configDirectory = Path.Combine(baseDir, "config");
                Directory.CreateDirectory(configDirectory);
                string configPath = Path.Combine(configDirectory, "config.ini");
                ServerConfig config = ConfigFile.Load(configPath);
                LAUNCHER_TCP_SERVICE_ACK server = new LAUNCHER_TCP_SERVICE_ACK(config);
                if (!server.Start())
                {
                    ServiceLog.Write("Falha ao iniciar o Socket.");
                    return;
                }

                ServiceLog.Write("Socket em segundo plano (sem janela).");
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                ServiceLog.Write("FATAL: " + ex);
            }
        }

        private static Assembly ResolveLocalAssembly(object sender, ResolveEventArgs args)
        {
            AssemblyName requestedAssembly = new AssemblyName(args.Name);

            if (!requestedAssembly.Name.Equals("System.Threading.Tasks.Extensions", StringComparison.OrdinalIgnoreCase))
                return null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                AssemblyName loadedAssembly = assembly.GetName();

                if (loadedAssembly.Name.Equals(requestedAssembly.Name, StringComparison.OrdinalIgnoreCase))
                    return assembly;
            }

            string assemblyPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                requestedAssembly.Name + ".dll");

            if (!File.Exists(assemblyPath))
                return null;

            return Assembly.LoadFrom(assemblyPath);
        }

        private sealed class LogTextWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
            public override void WriteLine(string value) => ServiceLog.Write(value ?? string.Empty);
            public override void Write(string value)
            {
                if (!string.IsNullOrEmpty(value) && value != "\r" && value != "\n" && value != "\r\n")
                    ServiceLog.Write(value);
            }
        }
    }
}
