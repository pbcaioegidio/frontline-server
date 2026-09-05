using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

static class Program
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, byte[] lpData, uint cbData);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, EnumResNameProc lpEnumFunc, IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool EnumResourceLanguages(IntPtr hModule, IntPtr lpType, IntPtr lpName, EnumResLangProc lpEnumFunc, IntPtr lParam);

    delegate bool EnumResNameProc(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);
    delegate bool EnumResLangProc(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, ushort wIDLanguage, IntPtr lParam);

    const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
    static readonly IntPtr RT_ICON = (IntPtr)3;
    static readonly IntPtr RT_GROUP_ICON = (IntPtr)14;

    struct ResRef
    {
        public IntPtr Type;
        public IntPtr Name;
        public ushort Lang;
    }

    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Uso: IconTool <png> <ico> [exe1 exe2 ...]");
            return 1;
        }

        string pngPath = args[0];
        string icoPath = args[1];
        int[] sizes = { 16, 32, 48, 64, 128, 256 };

        List<byte[]> imageData = new List<byte[]>();
        using (var src = (Bitmap)Image.FromFile(pngPath))
        {
            foreach (int s in sizes)
            {
                using var bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(src, 0, 0, s, s);
                }
                imageData.Add(BuildIconImage(bmp));
            }
        }

        WriteIco(icoPath, sizes, imageData);
        Console.WriteLine($"ICO OK {icoPath} ({new FileInfo(icoPath).Length} bytes)");

        for (int i = 2; i < args.Length; i++)
        {
            string exe = args[i];
            if (!File.Exists(exe))
            {
                Console.WriteLine($"SKIP missing {exe}");
                continue;
            }
            try
            {
                ApplyIconToExe(exe, sizes, imageData);
                Console.WriteLine($"EXE OK {exe}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXE FAIL {exe}: {ex.Message}");
            }
        }
        return 0;
    }

    static byte[] BuildIconImage(Bitmap bmp)
    {
        int w = bmp.Width, h = bmp.Height;
        int stride = w * 4;
        byte[] xor = new byte[stride * h];

        var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            byte[] row = new byte[stride];
            for (int y = 0; y < h; y++)
            {
                Marshal.Copy(IntPtr.Add(data.Scan0, y * data.Stride), row, 0, stride);
                // bottom-up
                Buffer.BlockCopy(row, 0, xor, (h - 1 - y) * stride, stride);
            }
        }
        finally { bmp.UnlockBits(data); }

        int andRow = ((w + 31) / 32) * 4;
        byte[] and = new byte[andRow * h];

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write(40);          // biSize
        bw.Write(w);           // biWidth
        bw.Write(h * 2);       // biHeight (xor+and)
        bw.Write((short)1);    // planes
        bw.Write((short)32);   // bitcount
        bw.Write(0);           // compression
        bw.Write(xor.Length + and.Length);
        bw.Write(0); bw.Write(0); bw.Write(0); bw.Write(0);
        bw.Write(xor);
        bw.Write(and);
        return ms.ToArray();
    }

    static void WriteIco(string path, int[] sizes, List<byte[]> images)
    {
        int count = sizes.Length;
        int offset = 6 + 16 * count;
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);
        bw.Write((ushort)0);
        bw.Write((ushort)1);
        bw.Write((ushort)count);
        for (int i = 0; i < count; i++)
        {
            int s = sizes[i];
            byte dim = (byte)(s >= 256 ? 0 : s);
            bw.Write(dim);
            bw.Write(dim);
            bw.Write((byte)0);
            bw.Write((byte)0);
            bw.Write((ushort)1);
            bw.Write((ushort)32);
            bw.Write(images[i].Length);
            bw.Write(offset);
            offset += images[i].Length;
        }
        foreach (var img in images)
            bw.Write(img);
    }

    static List<ResRef> EnumIcons(string exePath)
    {
        var list = new List<ResRef>();
        IntPtr mod = LoadLibraryEx(exePath, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
        if (mod == IntPtr.Zero)
            throw new InvalidOperationException($"LoadLibraryEx: {Marshal.GetLastWin32Error()}");

        try
        {
            foreach (IntPtr type in new[] { RT_GROUP_ICON, RT_ICON })
            {
                EnumResourceNames(mod, type, (hModule, lpszType, lpszName, lParam) =>
                {
                    EnumResourceLanguages(hModule, lpszType, lpszName, (hm, lt, ln, lang, lp) =>
                    {
                        list.Add(new ResRef { Type = lt, Name = ln, Lang = lang });
                        return true;
                    }, IntPtr.Zero);
                    return true;
                }, IntPtr.Zero);
            }
        }
        finally
        {
            FreeLibrary(mod);
        }
        return list;
    }

    static void ApplyIconToExe(string exePath, int[] sizes, List<byte[]> images)
    {
        var existing = EnumIcons(exePath);
        Console.WriteLine($"  existing icon resources: {existing.Count}");

        IntPtr h = BeginUpdateResource(exePath, false);
        if (h == IntPtr.Zero)
            throw new InvalidOperationException($"BeginUpdateResource failed: {Marshal.GetLastWin32Error()}");

        try
        {
            // Remove todos os ícones antigos (todas as linguagens)
            foreach (var r in existing)
            {
                UpdateResource(h, r.Type, r.Name, r.Lang, null, 0);
            }

            ushort lang = 1033; // en-US — padrão do Explorer

            for (int i = 0; i < images.Count; i++)
            {
                if (!UpdateResource(h, RT_ICON, (IntPtr)(i + 1), lang, images[i], (uint)images[i].Length))
                    throw new InvalidOperationException($"UpdateResource ICON {i + 1}: {Marshal.GetLastWin32Error()}");
            }

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write((ushort)0);
            bw.Write((ushort)1);
            bw.Write((ushort)images.Count);
            for (int i = 0; i < images.Count; i++)
            {
                int s = sizes[i];
                byte dim = (byte)(s >= 256 ? 0 : s);
                bw.Write(dim);
                bw.Write(dim);
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((ushort)1);
                bw.Write((ushort)32);
                bw.Write(images[i].Length);
                bw.Write((ushort)(i + 1));
            }
            byte[] group = ms.ToArray();

            if (!UpdateResource(h, RT_GROUP_ICON, (IntPtr)1, lang, group, (uint)group.Length))
                throw new InvalidOperationException($"UpdateResource GROUP: {Marshal.GetLastWin32Error()}");

            if (!EndUpdateResource(h, false))
                throw new InvalidOperationException($"EndUpdateResource: {Marshal.GetLastWin32Error()}");
            h = IntPtr.Zero;
        }
        finally
        {
            if (h != IntPtr.Zero)
                EndUpdateResource(h, true);
        }
    }
}
