using Plugin.Core.Enums;
using Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Plugin.Core.JSON
{
    public class ResolutionJSON
    {
        public static List<string> ARS = new List<string>();
        public static void Load()
        {
            string Path = "Data/DisplayRes.json";
            if (File.Exists(Path))
            {
                Parse(Path);
            }
            else
            {
                CLogger.Print($"File not found: {Path}", LoggerType.Warning);
            }
            CLogger.Print($"Plugin carregado: {ARS.Count} Allowed DRAR", LoggerType.Info);
        }
        public static void Reload()
        {
            ARS.Clear();
            Load();
        }
        public static string GetDisplay(string R)
        {
            lock (ARS)
            {
                foreach (string AR in ARS)
                {
                    string[] RS = ComDiv.SplitObjects(R, "x");
                    if (AR == ComDiv.AspectRatio(int.Parse(RS[0]), int.Parse(RS[1])))
                    {
                        return AR;
                    }
                }
                return "Invalid";
            }
        }
        private static void Parse(string Path)
        {
            using (FileStream STR = new FileStream(Path, FileMode.Open, FileAccess.Read))
            {
                if (STR.Length == 0)
                {
                    CLogger.Print($"File is empty: {Path}", LoggerType.Warning);
                }
                else
                {
                    try
                    {
                        using (StreamReader Stream = new StreamReader(STR, Encoding.UTF8))
                        {
                            JsonDocument DataDeserialize = JsonDocument.Parse(Stream.ReadToEnd());
                            foreach (JsonElement Element in DataDeserialize.RootElement.GetProperty("Resolution").EnumerateArray())
                            {
                                string ConfigId = Element.GetProperty("AspectRatio").GetString();
                                ARS.Add(ConfigId);
                            }
                            Stream.Dispose();
                            Stream.Close();
                        }
                    }
                    catch (Exception Ex)
                    {
                        CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                    }
                }
                STR.Dispose();
                STR.Close();
            }
        }
    }
}
