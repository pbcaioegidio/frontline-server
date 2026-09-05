# Gera docs\frontline-setup.ico multi-tamanho a partir do ícone do EXE.
param(
    [Parameter(Mandatory = $true)][string] $SourceExe,
    [Parameter(Mandatory = $true)][string] $OutIco
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$cs = @"
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public static class FlSetupIcon {
  [DllImport("Shell32.dll", CharSet=CharSet.Unicode)]
  static extern uint ExtractIconEx(string file, int index, IntPtr[] large, IntPtr[] small, uint n);
  [DllImport("User32.dll")]
  static extern bool DestroyIcon(IntPtr h);

  public static void Save(string exe, string outIco) {
    IntPtr[] large = new IntPtr[1];
    IntPtr[] small = new IntPtr[1];
    if (ExtractIconEx(exe, 0, large, small, 1) == 0 || large[0] == IntPtr.Zero)
      throw new Exception("EXE sem icone: " + exe);
    using (Icon ico = Icon.FromHandle(large[0]))
    using (Bitmap bmp = ico.ToBitmap()) {
      int[] sizes = new[] {16,32,48,64,128,256};
      using (var fs = File.Create(outIco))
      using (var bw = new BinaryWriter(fs)) {
        bw.Write((ushort)0); bw.Write((ushort)1); bw.Write((ushort)sizes.Length);
        long offsetTable = fs.Position;
        for (int i = 0; i < sizes.Length; i++) bw.Write(new byte[16]);
        var pngs = new List<byte[]>();
        foreach (int s in sizes) {
          using (var resized = new Bitmap(bmp, s, s))
          using (var ms = new MemoryStream()) {
            resized.Save(ms, ImageFormat.Png);
            pngs.Add(ms.ToArray());
          }
        }
        long dataPos = fs.Position;
        for (int i = 0; i < sizes.Length; i++) {
          fs.Position = offsetTable + i * 16;
          int s = sizes[i];
          bw.Write((byte)(s >= 256 ? 0 : s));
          bw.Write((byte)(s >= 256 ? 0 : s));
          bw.Write((byte)0); bw.Write((byte)0);
          bw.Write((ushort)1); bw.Write((ushort)32);
          bw.Write(pngs[i].Length);
          bw.Write((int)dataPos);
          long next = fs.Position;
          fs.Position = dataPos;
          bw.Write(pngs[i]);
          dataPos = fs.Position;
          fs.Position = next;
        }
      }
    }
    if (large[0] != IntPtr.Zero) DestroyIcon(large[0]);
    if (small[0] != IntPtr.Zero) DestroyIcon(small[0]);
  }
}
"@
if (-not ("FlSetupIcon" -as [type])) {
    Add-Type -TypeDefinition $cs -ReferencedAssemblies System.Drawing
}
[FlSetupIcon]::Save((Resolve-Path $SourceExe).Path, $OutIco)
