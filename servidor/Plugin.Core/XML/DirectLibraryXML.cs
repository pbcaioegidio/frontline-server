using Plugin.Core.Enums;
using Plugin.Core.SQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Plugin.Core.XML
{
    public class DirectLibraryXML
    {
        private static readonly object SyncRoot = new object();

        public static List<string> HashFiles = new List<string>();

        public static void Load()
        {
            lock (SyncRoot)
            {
                List<string> Loaded = DaoManagerSQL.GetDirectLibraryHashes();
                if (Loaded == null || Loaded.Count == 0)
                {
                    CLogger.Print("Lib Hases: sem dados no banco, falling back para Data/DirectLibrary.xml", LoggerType.Warning);
                    Loaded = ParseFile("Data/DirectLibrary.xml");
                }

                HashFiles = Loaded;
                CLogger.Print($"Plugin carregado: {Loaded.Count} Lib Hases", LoggerType.Info);
            }
        }

        public static void Reload()
        {
            Load();
        }

        public static bool IsValid(string md5)
        {
            if (string.IsNullOrEmpty(md5))
                return true;

            List<string> Snapshot = HashFiles;
            for (int Index = 0; Index < Snapshot.Count; ++Index)
            {
                if (Snapshot[Index] == md5)
                    return true;
            }
            return false;
        }

        private static List<string> ParseFile(string Path)
        {
            List<string> Result = new List<string>();
            if (!File.Exists(Path))
            {
                CLogger.Print("File not found: " + Path, LoggerType.Warning);
                return Result;
            }

            try
            {
                XmlDocument Document = new XmlDocument();
                using (FileStream Stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (Stream.Length == 0L)
                    {
                        CLogger.Print("File is empty: " + Path, LoggerType.Warning);
                        return Result;
                    }
                    Document.Load(Stream);
                }

                for (XmlNode Root = Document.FirstChild; Root != null; Root = Root.NextSibling)
                {
                    if (!Root.Name.Equals("List"))
                        continue;

                    for (XmlNode Node = Root.FirstChild; Node != null; Node = Node.NextSibling)
                    {
                        if (Node.Name.Equals("D3D9"))
                            Result.Add(Node.Attributes["MD5"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print(ex.Message, LoggerType.Error, ex);
            }
            return Result;
        }
    }
}
