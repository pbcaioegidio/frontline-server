using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class StringsDump
{
    static void Main(string[] args)
    {
        var data = File.ReadAllBytes(args[0]);
        var min = args.Length > 1 ? int.Parse(args[1]) : 6;
        // ASCII
        var cur = new StringBuilder();
        int start = 0;
        for (int i = 0; i <= data.Length; i++)
        {
            byte b = i < data.Length ? data[i] : (byte)0;
            if (b >= 32 && b < 127)
            {
                if (cur.Length == 0) start = i;
                cur.Append((char)b);
            }
            else
            {
                if (cur.Length >= min)
                {
                    var s = cur.ToString();
                    if (Interesting(s)) Console.WriteLine("A 0x{0:X} {1}", start, s);
                }
                cur.Clear();
            }
        }
        // UTF16
        cur.Clear();
        for (int i = 0; i <= data.Length - 1; i += 2)
        {
            char c = i + 1 < data.Length ? (char)(data[i] | (data[i + 1] << 8)) : '\0';
            if (c >= 32 && c < 127)
            {
                if (cur.Length == 0) start = i;
                cur.Append(c);
            }
            else
            {
                if (cur.Length >= min)
                {
                    var s = cur.ToString();
                    if (Interesting(s)) Console.WriteLine("U 0x{0:X} {1}", start, s);
                }
                cur.Clear();
                if (c != 0 && (c < 32 || c >= 127)) { /* keep align */ }
            }
        }
    }

    static bool Interesting(string s)
    {
        var l = s.ToLowerInvariant();
        return l.Contains("cheat") || l.Contains("block") || l.Contains("cb.") || l.Contains("cb\\")
            || l.Contains("cb/") || l.Contains("load failed") || l.Contains("initialize load")
            || l.Contains("anticheat") || l.Contains("anti-cheat") || l.Contains("hack")
            || l.Contains("xign") || l.Contains("wellbia") || l.Contains("cptr")
            || l.Contains("cb.exe") || l.Contains("cheat_blocker") || l.Contains("cheatblocker");
    }
}
