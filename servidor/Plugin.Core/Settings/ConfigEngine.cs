// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Settings.ConfigEngine
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using Plugin.Core.Enums;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plugin.Core.Settings
{
    public class ConfigEngine
    {
        private readonly FileInfo Field0;
        private readonly FileAccess Field1;
        private readonly string Field2 = Assembly.GetExecutingAssembly().GetName().Name;

        public ConfigEngine(string A_1 = null, FileAccess A_2 = FileAccess.ReadWrite)
        {
            this.Field1 = A_2;
            string path = A_1 ?? this.Field2;
            if (!Path.IsPathRooted(path))
                path = Path.Combine(RuntimePaths.ContentRoot, path);
            this.Field0 = new FileInfo(path);
        }

        private static int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder output, int capacity, string path)
        {
            string currentSection = string.Empty;
            if (File.Exists(path))
            {
                foreach (string raw in File.ReadLines(path))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                        continue;
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        currentSection = line.Substring(1, line.Length - 2).Trim();
                        continue;
                    }
                    if (!string.Equals(currentSection, section, StringComparison.OrdinalIgnoreCase))
                        continue;
                    int separator = line.IndexOf('=');
                    if (separator < 0 || !string.Equals(line.Substring(0, separator).Trim(), key, StringComparison.OrdinalIgnoreCase))
                        continue;
                    string value = line.Substring(separator + 1).Trim();
                    if (value.Length >= capacity)
                        value = value.Substring(0, capacity - 1);
                    output.Append(value);
                    return value.Length;
                }
            }
            output.Append(defaultValue ?? string.Empty);
            return output.Length;
        }

        private static long WritePrivateProfileString(string section, string key, string value, string path)
        {
            List<string> lines = File.Exists(path) ? File.ReadAllLines(path).ToList() : new List<string>();
            int sectionStart = -1;
            int sectionEnd = lines.Count;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (!line.StartsWith("[") || !line.EndsWith("]"))
                    continue;
                string name = line.Substring(1, line.Length - 2).Trim();
                if (sectionStart >= 0)
                {
                    sectionEnd = i;
                    break;
                }
                if (string.Equals(name, section, StringComparison.OrdinalIgnoreCase))
                    sectionStart = i;
            }

            if (key == null)
            {
                if (sectionStart >= 0)
                    lines.RemoveRange(sectionStart, sectionEnd - sectionStart);
            }
            else if (sectionStart < 0)
            {
                if (value != null)
                {
                    if (lines.Count > 0 && lines[lines.Count - 1].Length > 0)
                        lines.Add(string.Empty);
                    lines.Add("[" + section + "]");
                    lines.Add(key + " =" + value);
                }
            }
            else
            {
                int keyIndex = -1;
                for (int i = sectionStart + 1; i < sectionEnd; i++)
                {
                    int separator = lines[i].IndexOf('=');
                    if (separator >= 0 && string.Equals(lines[i].Substring(0, separator).Trim(), key, StringComparison.OrdinalIgnoreCase))
                    {
                        keyIndex = i;
                        break;
                    }
                }
                if (value == null)
                {
                    if (keyIndex >= 0) lines.RemoveAt(keyIndex);
                }
                else if (keyIndex >= 0)
                    lines[keyIndex] = key + " =" + value;
                else
                    lines.Insert(sectionEnd, key + " =" + value);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            File.WriteAllLines(path, lines, new UTF8Encoding(false));
            return 1;
        }

        
        public byte ReadC(string Key, byte Defaultprop, string Section = null)
        {
            try
            {
                return byte.Parse(this.Method0(Key, Section));
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        public short ReadH(string Key, short Defaultprop, string Section = null)
        {
            try
            {
                return short.Parse(this.Method0(Key, Section));
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        public ushort ReadUH(string Key, ushort Defaultprop, string Section = null)
        {
            try
            {
                return ushort.Parse(this.Method0(Key, Section));
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        public int ReadD(string Key, int Defaultprop, string Section = null)
        {
            try
            {
                return int.Parse(this.Method0(Key, Section));
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        public uint ReadUD(string Key, uint Defaultprop, string Section = null)
        {
            try
            {
                return uint.Parse(this.Method0(Key, Section));
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        public long ReadQ(string Key, long Defaultprop, string Section = null)
        {
            try
            {
                return long.Parse(this.Method0(Key, Section));
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        public ulong ReadUQ(string Key, ulong Defaultprop, string Section = null)
        {
            try
            {
                return ulong.Parse(this.Method0(Key, Section));
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        public double ReadF(string Key, double Defaultprop, string Section = null)
        {
            try
            {
                return double.Parse(this.Method0(Key, Section), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        public float ReadT(string Key, float Defaultprop, string Section = null)
        {
            try
            {
                return float.Parse(this.Method0(Key, Section), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        public bool ReadX(string Key, bool Defaultprop, string Section = null)
        {
            try
            {
                return bool.Parse(this.Method0(Key, Section));
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        public string ReadS(string Key, string Defaultprop, string Section = null)
        {
            try
            {
                return this.Method0(Key, Section);
            }
            catch
            {
                CLogger.Print("Falha ao ler parametro: " + Key, LoggerType.Warning);
                return Defaultprop;
            }
        }

        
        private string Method0(string A_1, string A_2 = null)
        {
            StringBuilder A_3 = new StringBuilder(65025);
            if (this.Field1 == FileAccess.Write)
                throw new Exception("Can`t read the file! No access!");
            ConfigEngine.GetPrivateProfileString(A_2 ?? this.Field2, A_1, "", A_3, 65025, this.Field0.FullName);
            return A_3.ToString();
        }

        
        public void WriteC(string Key, byte Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value.ToString(), Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        public void WriteH(string Key, short Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value.ToString(), Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        public void WriteH(string Key, ushort Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value.ToString(), Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        public void WriteD(string Key, int Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value.ToString(), Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        public void WriteD(string Key, uint Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value.ToString(), Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        public void WriteQ(string Key, long Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value.ToString(), Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        public void WriteQ(string Key, ulong Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value.ToString(), Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        public void WriteF(string Key, double Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value.ToString(), Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        public void WriteT(string Key, float Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value.ToString(), Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        public void WriteX(string Key, bool Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value.ToString(), Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        public void WriteS(string Key, string Value, string Section = null)
        {
            try
            {
                this.Method1(Key, Value, Section);
            }
            catch
            {
                CLogger.Print("Write Parameter Failure: " + Key, LoggerType.Warning);
            }
        }

        
        private void Method1(string A_1, string A_2, string A_3 = null)
        {
            if (this.Field1 == FileAccess.Read)
                throw new Exception("Can`t write to file! No access!");
            ConfigEngine.WritePrivateProfileString(A_3 ?? this.Field2, A_1, " " + A_2, this.Field0.FullName);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            this.Method1(Key, (string)null, Section ?? this.Field2);
        }

        public void DeleteSection(string Section = null)
        {
            this.Method1((string)null, (string)null, Section ?? this.Field2);
        }

        public bool KeyExists(string Key, string Section = null) => this.Method0(Key, Section).Length > 0;
    }
}
