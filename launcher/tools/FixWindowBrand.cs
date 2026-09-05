using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Ícone DIB clássico (o PNG-no-ICO o jogo antigo não carrega) + manifesto FrontLine + splash.
/// </summary>
internal static class Program
{
    const uint RT_ICON = 3, RT_GROUP_ICON = 14, RT_MANIFEST = 24;
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr BeginUpdateResource(string f, bool d);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool UpdateResource(IntPtr h, IntPtr t, IntPtr n, ushort l, byte[] data, uint cb);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool EndUpdateResource(IntPtr h, bool discard);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr LoadLibraryEx(string lp, IntPtr h, uint f);
    [DllImport("kernel32.dll")] static extern bool FreeLibrary(IntPtr h);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr FindResource(IntPtr h, IntPtr n, IntPtr t);
    [DllImport("kernel32.dll")] static extern uint SizeofResource(IntPtr h, IntPtr r);
    [DllImport("kernel32.dll")] static extern IntPtr LoadResource(IntPtr h, IntPtr r);
    [DllImport("kernel32.dll")] static extern IntPtr LockResource(IntPtr r);
    const uint LOAD_LIBRARY_AS_DATAFILE = 2;

    static int Main(string[] args)
    {
        string exe = args[0];
        string ico = args[1];
        string splash = args.Length > 2 ? args[2] : null;

        if (!PatchManifest(exe))
            Console.WriteLine("manifest skip/fail");
        if (!InjectClassicIcons(exe, ico))
        {
            Console.WriteLine("icon fail " + Marshal.GetLastWin32Error());
            return 2;
        }
        if (!string.IsNullOrEmpty(splash))
            WriteSplash(ico, splash);

        if (Path.GetFileName(exe).IndexOf("FrontLine", StringComparison.OrdinalIgnoreCase) >= 0)
            PatchAsciiName(exe);

        Console.WriteLine("ok " + exe);
        return 0;
    }

    static void PatchAsciiName(string exe)
    {
        byte[] b = File.ReadAllBytes(exe);
        int n = 0;
        n += ReplaceAscii(b, "PointBlank.exe", "FrontLine.exe");
        n += ReplaceAscii(b, "Point Blank", "FrontLine");
        n += ReplaceAscii(b, "PointBlank", "FrontLine");
        if (n > 0)
        {
            File.WriteAllBytes(exe, b);
            Console.WriteLine("ascii replaced " + n);
        }
    }

    static int ReplaceAscii(byte[] hay, string oldText, string newText)
    {
        if (newText.Length > oldText.Length) return 0;
        byte[] needle = Encoding.ASCII.GetBytes(oldText);
        byte[] repl = Encoding.ASCII.GetBytes(newText.PadRight(oldText.Length, '\0'));
        int count = 0;
        for (int i = 0; i <= hay.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
                if (hay[i + j] != needle[j]) { match = false; break; }
            if (!match) continue;
            Buffer.BlockCopy(repl, 0, hay, i, repl.Length);
            count++;
            i += needle.Length - 1;
        }
        return count;
    }

    static bool PatchManifest(string exe)
    {
        string xml;
        IntPtr lib = LoadLibraryEx(exe, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
        if (lib == IntPtr.Zero) return false;
        try
        {
            IntPtr r = FindResource(lib, (IntPtr)1, (IntPtr)RT_MANIFEST);
            if (r == IntPtr.Zero) return false;
            uint sz = SizeofResource(lib, r);
            byte[] raw = new byte[sz];
            Marshal.Copy(LockResource(LoadResource(lib, r)), raw, 0, (int)sz);
            xml = Encoding.UTF8.GetString(raw);
            if (xml.IndexOf('\0') >= 0)
                xml = Encoding.Unicode.GetString(raw).Trim('\0');
        }
        finally { FreeLibrary(lib); }

        if (xml.IndexOf("PointBlank", StringComparison.OrdinalIgnoreCase) < 0)
            return true;
        xml = xml.Replace("PointBlank", "FrontLine");
        byte[] neu = Encoding.UTF8.GetBytes(xml);
        IntPtr h = BeginUpdateResource(exe, false);
        if (h == IntPtr.Zero) return false;
        bool ok = UpdateResource(h, (IntPtr)RT_MANIFEST, (IntPtr)1, 0, neu, (uint)neu.Length);
        return EndUpdateResource(h, !ok) && ok;
    }

    static byte[] DibIcon(Bitmap bmp)
    {
        int w = bmp.Width, h = bmp.Height;
        var rect = new Rectangle(0, 0, w, h);
        BitmapData bd = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        int stride = w * 4;
        byte[] xor = new byte[stride * h];
        for (int y = 0; y < h; y++)
            Marshal.Copy(IntPtr.Add(bd.Scan0, (h - 1 - y) * bd.Stride), xor, y * stride, stride);
        bmp.UnlockBits(bd);
        int andRow = ((w + 31) / 32) * 4;
        byte[] and = new byte[andRow * h];
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms))
        {
            bw.Write(40);
            bw.Write(w); bw.Write(h * 2);
            bw.Write((ushort)1); bw.Write((ushort)32);
            bw.Write(0); bw.Write(xor.Length);
            bw.Write(0); bw.Write(0); bw.Write(0); bw.Write(0);
            bw.Write(xor); bw.Write(and);
            return ms.ToArray();
        }
    }

    static bool InjectClassicIcons(string exe, string icoPath)
    {
        using (var src = new Bitmap(icoPath))
        {
            int[] sizes = { 16, 32, 48 };
            IntPtr h = BeginUpdateResource(exe, false);
            if (h == IntPtr.Zero) return false;
            ushort id = 1;
            foreach (int s in sizes)
            {
                using (var bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(src, 0, 0, s, s);
                    byte[] img = DibIcon(bmp);
                    if (!UpdateResource(h, (IntPtr)RT_ICON, (IntPtr)id, 0, img, (uint)img.Length))
                    {
                        EndUpdateResource(h, true);
                        return false;
                    }
                }
                id++;
            }
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((ushort)0); bw.Write((ushort)1); bw.Write((ushort)sizes.Length);
                ushort n = 1;
                foreach (int s in sizes)
                {
                    bw.Write((byte)s); bw.Write((byte)s);
                    bw.Write((byte)0); bw.Write((byte)0);
                    bw.Write((ushort)1); bw.Write((ushort)32);
                    int imgSize = 40 + s * s * 4 + ((s + 31) / 32) * 4 * s;
                    bw.Write(imgSize);
                    bw.Write(n);
                    n++;
                }
                byte[] grp = ms.ToArray();
                if (!UpdateResource(h, (IntPtr)RT_GROUP_ICON, (IntPtr)1, 0, grp, (uint)grp.Length))
                {
                    EndUpdateResource(h, true);
                    return false;
                }
                // Barra da janela usa IDI_APPLICATION (32512), taskbar usa o grupo #1.
                UpdateResource(h, (IntPtr)RT_GROUP_ICON, (IntPtr)32512, 0, grp, (uint)grp.Length);
            }
            return EndUpdateResource(h, false);
        }
    }

    static void WriteSplash(string icoPath, string bmpPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(bmpPath));
        using (var bmp = new Bitmap(640, 400, PixelFormat.Format24bppRgb))
        using (var g = Graphics.FromImage(bmp))
        using (var src = Image.FromFile(icoPath))
        {
            g.Clear(Color.FromArgb(12, 14, 18));
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            int side = 160;
            g.DrawImage(src, (640 - side) / 2, 70, side, side);
            using (var font = new Font("Segoe UI Semibold", 28, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var br = new SolidBrush(Color.FromArgb(220, 224, 230)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("FRONTLINE", font, br, new RectangleF(0, 250, 640, 50), sf);
            }
            bmp.Save(bmpPath, ImageFormat.Bmp);
        }
        Console.WriteLine("splash " + bmpPath);
    }
}
