using System;
using System.IO;
using System.Text;

class Many
{
    static void Main()
    {
        var files = new[] {
            @"C:\Users\pbcai\Downloads\source\client\PointBlank.exe",
            @"C:\Users\pbcai\Downloads\source\client\PointBlank.i3Exec"
        };
        var needles = new[] {
            "CB.exe","cb.exe","Cb.exe","CB.EXE","CB.cbm","cb.cbm",
            "CHEAT_BLOCKER","Cheat_Blocker","CheatBlocker",
            "Cheat Blocker","CHEAT BLOCKER",
            "Initialize Load Failed","Load Failed Cheat",
            "\\CB.exe","/CB.exe","CHEAT_BLOCKER\\CB",
            "_XignCode_","XignCode_","XIGNCODE"
        };
        foreach (var f in files)
        {
            Console.WriteLine("=== " + Path.GetFileName(f) + " ===");
            var data = File.ReadAllBytes(f);
            foreach (var n in needles)
            {
                FindAscii(data, n);
                FindUni(data, n);
                FindXorDiff(data, n);
            }
        }
    }

    static void FindAscii(byte[] d, string n)
    {
        var p = Encoding.ASCII.GetBytes(n);
        int i = IndexOf(d, p);
        if (i >= 0) Console.WriteLine("A '{0}' @ 0x{1:X}", n, i);
    }
    static void FindUni(byte[] d, string n)
    {
        var p = Encoding.Unicode.GetBytes(n);
        int i = IndexOf(d, p);
        if (i >= 0) Console.WriteLine("U '{0}' @ 0x{1:X}", n, i);
    }
    static void FindXorDiff(byte[] data, string needle)
    {
        var plain = Encoding.ASCII.GetBytes(needle);
        if (plain.Length < 4) return;
        var diff = new byte[plain.Length];
        for (int j = 0; j < plain.Length; j++) diff[j] = (byte)(plain[j] ^ plain[0]);
        for (int i = 0; i <= data.Length - plain.Length; i++)
        {
            bool ok = true;
            for (int j = 1; j < plain.Length; j++)
                if ((byte)(data[i + j] ^ data[i]) != diff[j]) { ok = false; break; }
            if (!ok) continue;
            int xor = data[i] ^ plain[0];
            if (xor == 0) continue; // already reported as plain A
            bool ver = true;
            for (int j = 0; j < plain.Length; j++)
                if (data[i + j] != (byte)(plain[j] ^ xor)) { ver = false; break; }
            if (ver) Console.WriteLine("X '{0}' xor=0x{1:X2} @ 0x{2:X}", needle, xor, i);
        }
    }
    static int IndexOf(byte[] hay, byte[] n)
    {
        for (int i = 0; i <= hay.Length - n.Length; i++)
        {
            bool ok = true;
            for (int j = 0; j < n.Length; j++) if (hay[i + j] != n[j]) { ok = false; break; }
            if (ok) return i;
        }
        return -1;
    }
}
