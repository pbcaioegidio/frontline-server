using System; using System.IO; using System.Text;
class X {
  static void Main(string[] a) {
    var data=File.ReadAllBytes(a[0]);
    foreach(var n in new[]{"_XignCode","XignCode","XIGNCODE","CheatBlocker.dll","CB.dll","cb.dll","CheatBlocker.exe","Initialize Load Failed CheatBlocker","Load Failed CheatBlocker","CHEAT_BLOCKER\\","CHEAT_BLOCKER/"}) {
      Find(data,n);
    }
  }
  static void Find(byte[] data, string needle) {
    var plain=Encoding.ASCII.GetBytes(needle);
    if(plain.Length<4) return;
    var diff=new byte[plain.Length];
    for(int j=0;j<plain.Length;j++) diff[j]=(byte)(plain[j]^plain[0]);
    for(int i=0;i<=data.Length-plain.Length;i++){
      bool ok=true;
      for(int j=1;j<plain.Length;j++) if((byte)(data[i+j]^data[i])!=diff[j]){ok=false;break;}
      if(!ok) continue;
      int xor=data[i]^plain[0];
      bool ver=true;
      for(int j=0;j<plain.Length;j++) if(data[i+j]!=(byte)(plain[j]^xor)){ver=false;break;}
      if(ver) Console.WriteLine("'{0}' xor=0x{1:X2} @ 0x{2:X}", needle, xor, i);
    }
    // plain utf16
    var u=Encoding.Unicode.GetBytes(needle);
    for(int i=0;i<=data.Length-u.Length;i++){
      bool ok=true; for(int j=0;j<u.Length;j++) if(data[i+j]!=u[j]){ok=false;break;}
      if(ok) Console.WriteLine("UNI plain '{0}' @ 0x{1:X}", needle, i);
    }
  }
}
