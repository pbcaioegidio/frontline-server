using System;
using System.IO;
using System.Text;

// Busca string ASCII ofuscada com XOR de 1 byte (qualquer chave) via diferencial.
class CbScanFast
{
    static void Main(string[] args)
    {
        foreach (var path in args)
        {
            if (!File.Exists(path)) continue;
            Console.WriteLine("=== " + Path.GetFileName(path) + " ===");
            var data = File.ReadAllBytes(path);
            foreach (var n in new[] {
                "CheatBlocker", "CHEAT_BLOCKER", "CB.exe", "Initialize Load Failed",
                "CB.cbm", "Load Failed", "Cheat Blocker"
            })
            {
                FindXorAscii(data, n);
                FindXorUtf16(data, n);
            }
            foreach (var api in new[] { "CreateProcessW", "CreateProcessA", "LoadLibraryW", "LoadLibraryExW", "ShellExecuteW", "WinExec" })
            {
                int i = IndexOf(data, Encoding.ASCII.GetBytes(api));
                Console.WriteLine("API {0}={1}", api, i >= 0 ? "0x" + i.ToString("X") : "MISS");
            }
        }
    }

    static void FindXorAscii(byte[] data, string needle)
    {
        var plain = Encoding.ASCII.GetBytes(needle);
        if (plain.Length < 3) return;
        var diff = new byte[plain.Length];
        for (int j = 0; j < plain.Length; j++) diff[j] = (byte)(plain[j] ^ plain[0]);

        for (int i = 0; i <= data.Length - plain.Length; i++)
        {
            bool ok = true;
            for (int j = 1; j < plain.Length; j++)
            {
                if ((byte)(data[i + j] ^ data[i]) != diff[j]) { ok = false; break; }
            }
            if (!ok) continue;
            int xor = data[i] ^ plain[0];
            // verify full
            bool ver = true;
            for (int j = 0; j < plain.Length; j++)
                if (data[i + j] != (byte)(plain[j] ^ xor)) { ver = false; break; }
            if (ver)
                Console.WriteLine("ASCII '{0}' xor=0x{1:X2} @ 0x{2:X}", needle, xor, i);
        }
    }

    static void FindXorUtf16(byte[] data, string needle)
    {
        var plain = Encoding.Unicode.GetBytes(needle);
        if (plain.Length < 4) return;
        var diff = new byte[plain.Length];
        for (int j = 0; j < plain.Length; j++) diff[j] = (byte)(plain[j] ^ plain[0]);

        for (int i = 0; i <= data.Length - plain.Length; i++)
        {
            bool ok = true;
            for (int j = 1; j < plain.Length; j++)
            {
                if ((byte)(data[i + j] ^ data[i]) != diff[j]) { ok = false; break; }
            }
            if (!ok) continue;
            int xor = data[i] ^ plain[0];
            bool ver = true;
            for (int j = 0; j < plain.Length; j++)
                if (data[i + j] != (byte)(plain[j] ^ xor)) { ver = false; break; }
            if (ver)
                Console.WriteLine("UNI '{0}' xor=0x{1:X2} @ 0x{2:X}", needle, xor, i);
        }
    }

    static int IndexOf(byte[] hay, byte[] needle)
    {
        for (int i = 0; i <= hay.Length - needle.Length; i++)
        {
            bool ok = true;
            for (int j = 0; j < needle.Length; j++)
                if (hay[i + j] != needle[j]) { ok = false; break; }
            if (ok) return i;
        }
        return -1;
    }
}
