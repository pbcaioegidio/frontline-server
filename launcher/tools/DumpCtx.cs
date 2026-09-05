using System;
using System.IO;
using System.Text;

class DumpCtx
{
    static void Main(string[] args)
    {
        var path = args[0];
        var needle = Encoding.Unicode.GetBytes(args[1]);
        var data = File.ReadAllBytes(path);
        for (int i = 0; i <= data.Length - needle.Length; i++)
        {
            bool ok = true;
            for (int j = 0; j < needle.Length; j++) if (data[i+j] != needle[j]) { ok = false; break; }
            if (!ok) continue;
            int start = Math.Max(0, i - 64);
            int end = Math.Min(data.Length, i + needle.Length + 96);
            var sb = new StringBuilder();
            for (int k = start; k < end; k += 2)
            {
                if (k+1 >= end) break;
                char c = (char)(data[k] | (data[k+1] << 8));
                if (c >= 32 && c < 127) sb.Append(c); else sb.Append('.');
            }
            Console.WriteLine("0x{0:X}: {1}", i, sb);
        }

        // also ascii Initialize / Failed / Cheat fragments plain
        foreach (var n in new[] { "Initialize", "Failed", "Cheat", "BLOCKER", "cb.exe", "CB.EXE", ".cbm", "CheatB" })
        {
            var p = Encoding.ASCII.GetBytes(n);
            int c = 0;
            for (int i = 0; i <= data.Length - p.Length && c < 8; i++)
            {
                bool ok = true;
                for (int j = 0; j < p.Length; j++) if (data[i+j] != p[j]) { ok = false; break; }
                if (!ok) continue;
                c++;
                Console.WriteLine("ASCII '{0}' @ 0x{1:X}", n, i);
            }
        }
    }
}
