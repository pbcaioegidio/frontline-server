// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Filters.NickFilter
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Filters
{
    public static class NickFilter
    {
        public static List<string> Filters = new List<string>();

        
        public static void Load()
        {
            string str = "Config/Filters/Nicks.txt";
            if (!File.Exists(str))
                CLogger.Print("File not found: " + str, LoggerType.Warning);
            else
                NickFilter.StaticMethod0(str);
            CLogger.Print($"Plugin carregado: {NickFilter.Filters.Count} Nick Filters", LoggerType.Info);
        }

        public static void Reload()
        {
            NickFilter.Filters.Clear();
            NickFilter.Load();
        }

        
        private static void StaticMethod0(string A_0)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(A_0))
                {
                    string str;
                    while ((str = streamReader.ReadLine()) != null)
                        NickFilter.Filters.Add(str);
                    streamReader.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("Filter: " + ex.Message, LoggerType.Error, ex);
            }
        }
    }
}