using System.Runtime.InteropServices;

namespace FLConfig;

internal static class DisplayModes
{
    private const int ENUM_CURRENT_SETTINGS = -1;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool EnumDisplaySettingsW(string? deviceName, int modeNum, ref DEVMODE devMode);

    public static IReadOnlyList<int> GetRefreshRates(int? width = null, int? height = null)
    {
        var set = CollectRates(width, height);

        // Resolução sem modos listados → usa todos os Hz que o Windows reporta
        if (set.Count == 0 && (width is not null || height is not null))
            set = CollectRates(null, null);

        if (set.Count == 0)
        {
            foreach (var hz in new[] { 60, 75, 120, 144, 160, 165, 240 })
                set.Add(hz);
        }

        return set.ToList();
    }

    private static SortedSet<int> CollectRates(int? width, int? height)
    {
        var set = new SortedSet<int>();
        var mode = new DEVMODE { dmSize = (short)Marshal.SizeOf<DEVMODE>() };

        for (int i = 0; EnumDisplaySettingsW(null, i, ref mode); i++)
        {
            if (mode.dmBitsPerPel < 24 || mode.dmDisplayFrequency < 30)
                continue;
            if (width is int w && height is int h && (mode.dmPelsWidth != w || mode.dmPelsHeight != h))
                continue;
            set.Add(mode.dmDisplayFrequency);
        }

        return set;
    }

    public static IReadOnlyList<(int W, int H)> GetResolutions()
    {
        var set = new SortedSet<(int W, int H)>();
        var mode = new DEVMODE { dmSize = (short)Marshal.SizeOf<DEVMODE>() };

        for (int i = 0; EnumDisplaySettingsW(null, i, ref mode); i++)
        {
            if (mode.dmBitsPerPel < 24 || mode.dmPelsWidth < 800 || mode.dmPelsHeight < 600)
                continue;
            set.Add((mode.dmPelsWidth, mode.dmPelsHeight));
        }

        foreach (var s in Screen.AllScreens)
            set.Add((s.Bounds.Width, s.Bounds.Height));

        if (set.Count == 0)
        {
            set.Add((1920, 1080));
            set.Add((2560, 1440));
        }

        return set.ToList();
    }
}
