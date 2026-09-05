using Launcher.PointBlank.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace FileListBuilder
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            string client = args.Length > 0 ? args[0] : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "client"));
            client = Path.GetFullPath(client);
            if (!Directory.Exists(client))
            {
                Console.Error.WriteLine("Pasta client não encontrada: " + client);
                return 1;
            }

            string privateKeyPath = args.Length > 1
                ? args[1]
                : Path.GetFullPath(Path.Combine(client, "..", "launcher", "security", "filelist-private.pem"));
            if (!File.Exists(privateKeyPath))
            {
                Console.Error.WriteLine("Chave privada ausente: " + privateKeyPath);
                return 1;
            }

            Console.WriteLine("Client: " + client);
            var files = Directory.EnumerateFiles(client, "*", SearchOption.AllDirectories)
                .Select(full => IntegrityRules.NormalizeLocal(full.Substring(client.Length)))
                .Where(rel => !IntegrityRules.ShouldSkip(rel))
                .OrderBy(rel => rel, StringComparer.OrdinalIgnoreCase)
                .ToList();

            Console.WriteLine("Arquivos: " + files.Count);
            var map = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int done = 0;
            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount) }, rel =>
            {
                string path = Path.Combine(client, rel);
                using var md5 = MD5.Create();
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
                map[rel] = Convert.ToHexString(md5.ComputeHash(stream));
                int n = Interlocked.Increment(ref done);
                if (n % 100 == 0 || n == files.Count)
                    Console.WriteLine($"  {n}/{files.Count}  {rel}");
            });

            string datPath = Path.Combine(client, "UserFileList.dat");
            var xml = new XmlDocument();
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement list = xml.CreateElement("list");
            xml.AppendChild(list);
            foreach (string rel in files)
            {
                XmlElement file = xml.CreateElement("file");
                file.SetAttribute("local", rel);
                file.SetAttribute("hash", map[rel]);
                list.AppendChild(file);
            }

            var settings = new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true, IndentChars = "  " };
            using (var writer = XmlWriter.Create(datPath, settings))
                xml.Save(writer);

            string pem = File.ReadAllText(privateKeyPath);
            string sigPath = Path.Combine(client, "UserFileList.sig");
            ManifestTrust.SignFile(datPath, sigPath, pem);

            using var listMd5 = MD5.Create();
            using var datStream = File.OpenRead(datPath);
            string ufl = Convert.ToHexString(listMd5.ComputeHash(datStream)).ToLowerInvariant();
            File.WriteAllText(Path.Combine(client, "ufl-md5.txt"), ufl + Environment.NewLine);

            Console.WriteLine("UserFileList.dat  " + datPath);
            Console.WriteLine("UserFileList.sig  " + sigPath);
            Console.WriteLine("AccessUFL MD5     " + ufl);
            return 0;
        }
    }
}
