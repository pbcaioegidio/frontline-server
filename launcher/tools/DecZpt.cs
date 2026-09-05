using System;
using System.IO;
using System.Text;

// Decrypt config.zpt / launcher.svl with ConfigFileEncrypter algorithm (inverse)
class DecZpt
{
    const byte InitialFeedback = 0xA7;
    const int RotateBits = 3;
    const string CryptoKey = "PointBlank.Config.Security";

    static byte Ror(byte v, int bits) => (byte)((v >> bits) | (v << (8 - bits)));

    static byte[] Decrypt(byte[] enc)
    {
        var plain = new byte[enc.Length];
        byte feedback = InitialFeedback;
        var key = Encoding.ASCII.GetBytes(CryptoKey);
        for (int i = 0; i < enc.Length; i++)
        {
            byte cipher = enc[i];
            byte k = key[i % key.Length];
            byte positionMask = (byte)((i * 37) + 0x5A);
            byte mask = (byte)(k + positionMask + feedback);
            plain[i] = (byte)(Ror(cipher, RotateBits) ^ mask);
            feedback = cipher;
        }
        return plain;
    }

    static void Main(string[] args)
    {
        foreach (var path in args)
        {
            if (!File.Exists(path)) { Console.WriteLine("missing " + path); continue; }
            var enc = File.ReadAllBytes(path);
            var plain = Decrypt(enc);
            var outPath = path + ".dec.txt";
            File.WriteAllBytes(outPath, plain);
            var text = Encoding.UTF8.GetString(plain);
            Console.WriteLine("=== " + Path.GetFileName(path) + " ===");
            Console.WriteLine(text.Length > 2000 ? text.Substring(0, 2000) : text);
            if (text.IndexOf("Cheat", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("CHEAT", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("CB.", StringComparison.OrdinalIgnoreCase) >= 0)
                Console.WriteLine("** CONTAINS CHEAT/CB REF **");
        }
    }
}
