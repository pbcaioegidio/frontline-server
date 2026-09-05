using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Troca ícone + strings visíveis "Point Blank" no PE do client (UAC / barra).
/// </summary>
internal static class Program
{
    const uint RT_ICON = 3;
    const uint RT_GROUP_ICON = 14;
    const ushort LANG_NEUTRAL = 0;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr BeginUpdateResource(string fileName, bool deleteExisting);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, byte[] lpData, uint cbData);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool EndUpdateResource(IntPtr hUpdate, bool discard);

    static int Main(string[] args)
    {
        string exe = args.Length > 0 ? args[0] : @"c:\Users\pbcai\Downloads\source\client\FrontLine.exe";
        string ico = args.Length > 1 ? args[1] : @"c:\Users\pbcai\Downloads\source\client\Icon.ico";
        bool iconOnly = Array.Exists(args, a => a == "--icon-only");
        bool asciiTitle = Array.Exists(args, a => a == "--ascii-title");
        if (!File.Exists(exe)) { Console.WriteLine("missing exe " + exe); return 1; }
        if (!File.Exists(ico)) { Console.WriteLine("missing ico " + ico); return 1; }

        byte[] bytes = File.ReadAllBytes(exe);
        if (!iconOnly)
        {
            int n1 = ReplaceUtf16(bytes, "Point Blank", "FrontLine");
            int n2 = ReplaceUtf16(bytes, "PointBlank", "FrontLine");
            int n3 = ReplaceUtf16(bytes, "PointBlank.exe", "FrontLine.exe");
            Console.WriteLine("utf16: Point Blank=" + n1 + " PointBlank=" + n2 + " PointBlank.exe=" + n3);
        }
        if (asciiTitle)
        {
            int a1 = ReplaceAscii(bytes, "Point Blank", "FrontLine");
            Console.WriteLine("ascii title Point Blank=" + a1);
        }
        File.WriteAllBytes(exe, bytes);

        if (!InjectIcon(exe, ico))
        {
            Console.WriteLine("icon fail GetLastError=" + Marshal.GetLastWin32Error());
            return 2;
        }
        Console.WriteLine("icon ok " + exe);
        return 0;
    }

    static int ReplaceAscii(byte[] hay, string oldText, string newText)
    {
        if (newText.Length > oldText.Length)
            throw new ArgumentException("new text longer than old");
        byte[] needle = Encoding.ASCII.GetBytes(oldText);
        byte[] repl = Encoding.ASCII.GetBytes(newText.PadRight(oldText.Length, '\0'));
        int count = 0;
        for (int i = 0; i <= hay.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (hay[i + j] != needle[j]) { match = false; break; }
            }
            if (!match) continue;
            Buffer.BlockCopy(repl, 0, hay, i, repl.Length);
            count++;
            i += needle.Length - 1;
        }
        return count;
    }

    static int ReplaceUtf16(byte[] hay, string oldText, string newText)
    {
        if (newText.Length > oldText.Length)
            throw new ArgumentException("new text longer than old");

        byte[] needle = Encoding.Unicode.GetBytes(oldText);
        byte[] repl = Encoding.Unicode.GetBytes(newText.PadRight(oldText.Length, '\0'));
        int count = 0;
        for (int i = 0; i <= hay.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (hay[i + j] != needle[j]) { match = false; break; }
            }
            if (!match) continue;
            Buffer.BlockCopy(repl, 0, hay, i, repl.Length);
            count++;
            i += needle.Length - 1;
        }
        return count;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IconDir
    {
        public ushort Reserved;
        public ushort Type;
        public ushort Count;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct IconDirEntry
    {
        public byte Width;
        public byte Height;
        public byte ColorCount;
        public byte Reserved;
        public ushort Planes;
        public ushort BitCount;
        public uint BytesInRes;
        public uint ImageOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct GrpIconDirEntry
    {
        public byte Width;
        public byte Height;
        public byte ColorCount;
        public byte Reserved;
        public ushort Planes;
        public ushort BitCount;
        public uint BytesInRes;
        public ushort Id;
    }

    static bool InjectIcon(string exePath, string icoPath)
    {
        byte[] ico = File.ReadAllBytes(icoPath);
        var dir = BytesToStruct<IconDir>(ico, 0);
        if (dir.Type != 1 || dir.Count == 0)
        {
            Console.WriteLine("ico inválido");
            return false;
        }

        IntPtr h = BeginUpdateResource(exePath, false);
        if (h == IntPtr.Zero) return false;

        int entrySize = Marshal.SizeOf(typeof(IconDirEntry));
        int grpEntrySize = Marshal.SizeOf(typeof(GrpIconDirEntry));
        byte[] group = new byte[6 + dir.Count * grpEntrySize];
        BitConverter.GetBytes((ushort)0).CopyTo(group, 0);
        BitConverter.GetBytes((ushort)1).CopyTo(group, 2);
        BitConverter.GetBytes(dir.Count).CopyTo(group, 4);

        for (int i = 0; i < dir.Count; i++)
        {
            var e = BytesToStruct<IconDirEntry>(ico, 6 + i * entrySize);
            byte[] img = new byte[e.BytesInRes];
            Buffer.BlockCopy(ico, (int)e.ImageOffset, img, 0, img.Length);
            ushort id = (ushort)(1 + i);
            if (!UpdateResource(h, (IntPtr)RT_ICON, (IntPtr)id, LANG_NEUTRAL, img, (uint)img.Length))
            {
                EndUpdateResource(h, true);
                return false;
            }

            var g = new GrpIconDirEntry
            {
                Width = e.Width,
                Height = e.Height,
                ColorCount = e.ColorCount,
                Reserved = e.Reserved,
                Planes = e.Planes,
                BitCount = e.BitCount,
                BytesInRes = e.BytesInRes,
                Id = id
            };
            StructToBytes(g).CopyTo(group, 6 + i * grpEntrySize);
        }

        // RT_GROUP_ICON name 1 (MAKEINTRESOURCE(1))
        bool ok = UpdateResource(h, (IntPtr)RT_GROUP_ICON, (IntPtr)1, LANG_NEUTRAL, group, (uint)group.Length);
        if (!ok)
        {
            EndUpdateResource(h, true);
            return false;
        }
        return EndUpdateResource(h, false);
    }

    static T BytesToStruct<T>(byte[] data, int offset) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        IntPtr p = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(data, offset, p, size);
            return (T)Marshal.PtrToStructure(p, typeof(T));
        }
        finally { Marshal.FreeHGlobal(p); }
    }

    static byte[] StructToBytes<T>(T value) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        byte[] arr = new byte[size];
        IntPtr p = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(value, p, false);
            Marshal.Copy(p, arr, 0, size);
            return arr;
        }
        finally { Marshal.FreeHGlobal(p); }
    }
}
