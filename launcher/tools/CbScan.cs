using System;
using System.IO;
using System.Text;
class S {
  static void Main(string[] args) {
    var path = args[0];
    var data = File.ReadAllBytes(path);
    var needles = new[]{"CheatBlocker","CHEAT_BLOCKER","CB.exe","Initialize Load Failed","CB.cbm","Load Failed"};
    foreach (var n in needles) {
      var plain = Encoding.ASCII.GetBytes(n);
      for (int xor=0; xor<256; xor++) {
        var pat = new byte[plain.Length];
        for (int j=0;j<plain.Length;j++) pat[j]=(byte)(plain[j]^xor);
        int idx = IndexOf(data, pat);
        if (idx>=0) Console.WriteLine("ASCII '{0}' xor=0x{1:X2} @ 0x{2:X}", n, xor, idx);
      }
      // utf16
      var up = Encoding.Unicode.GetBytes(n);
      for (int xor=0; xor<256; xor++) {
        var pat = new byte[up.Length];
        for (int j=0;j<up.Length;j++) pat[j]=(byte)(up[j]^xor);
        int idx = IndexOf(data, pat);
        if (idx>=0) Console.WriteLine("UNI '{0}' xor=0x{1:X2} @ 0x{2:X}", n, xor, idx);
      }
    }
    foreach (var api in new[]{"CreateProcessW","CreateProcessA","LoadLibraryW","LoadLibraryExW","ShellExecuteW"}) {
      var p = Encoding.ASCII.GetBytes(api);
      int idx = IndexOf(data, p);
      Console.WriteLine("API {0}={1}", api, idx>=0 ? "0x"+idx.ToString("X") : "MISS");
    }
  }
  static int IndexOf(byte[] hay, byte[] needle) {
    for (int i=0;i<=hay.Length-needle.Length;i++) {
      bool ok=true;
      for (int j=0;j<needle.Length;j++) if (hay[i+j]!=needle[j]) { ok=false; break; }
      if (ok) return i;
    }
    return -1;
  }
}
