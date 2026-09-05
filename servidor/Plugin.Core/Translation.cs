// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Translation
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Plugin.Core
{
    public static class Translation
    {
        public static SortedList<string, string> Strings = new SortedList<string, string>();

        static Translation() => Translation.StaticMethod0();

        
        private static void StaticMethod0()
        {
            string str = "Config/Translate/Strings.ini";
            if (File.Exists(str))
                Translation.StaticMethod1(str);
            else
                CLogger.Print("File not found: " + str, LoggerType.Warning);
        }

        
        private static void StaticMethod1(string A_0)
        {
            try
            {
                using (StreamReader streamReader = new StreamReader(A_0))
                {
                    string str1;
                    while ((str1 = streamReader.ReadLine()) != null)
                    {
                        int length = str1.IndexOf(" = ");
                        if (length >= 0)
                        {
                            string key = str1.Substring(0, length);
                            string str2 = str1.Substring(length + 3);
                            Translation.Strings.Add(key, str2);
                        }
                    }
                    streamReader.Close();
                }
            }
            catch (Exception ex)
            {
                CLogger.Print("Translation: " + ex.Message, LoggerType.Error, ex);
            }
        }

        
        public static string GetLabel(string Title)
        {
            try
            {
                string str;
                return Translation.Strings.TryGetValue(Title, out str) ? str.Replace("\\n", '\n'.ToString()) : Title;
            }
            catch
            {
                return Title;
            }
        }

        public static string GetLabel(string Title, params object[] Argumens)
        {
            return string.Format(Translation.GetLabel(Title), Argumens);
        }
    }
}