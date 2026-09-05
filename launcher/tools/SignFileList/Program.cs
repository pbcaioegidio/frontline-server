using System;
using System.IO;
using Launcher.PointBlank.Services;

namespace SignFileList
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Uso: SignFileList <UserFileList.dat> <UserFileList.sig> <private.pem>");
                return 1;
            }
            string dat = args[0], sig = args[1], pemPath = args[2];
            if (!File.Exists(dat)) { Console.Error.WriteLine("dat ausente: " + dat); return 1; }
            if (!File.Exists(pemPath)) { Console.Error.WriteLine("pem ausente: " + pemPath); return 1; }
            ManifestTrust.SignFile(dat, sig, File.ReadAllText(pemPath));
            Console.WriteLine("Assinado: " + sig);
            return 0;
        }
    }
}
