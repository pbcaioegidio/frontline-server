// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Utility.ColorUtil
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Drawing;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Utility
{
    public class ColorUtil
    {
        public static Color White = Color.FromArgb(255, 255, 255, 255);
        public static Color Black = Color.FromArgb(255, 0, 0, 0);
        public static Color Red = Color.FromArgb(255, 255, 0, 0);
        public static Color Green = Color.FromArgb(255, 0, 255, 0);
        public static Color Blue = Color.FromArgb(255, 0, 0, 255);
        public static Color Yellow = Color.FromArgb(255, 255, 255, 0);
        public static Color Fuchsia = Color.FromArgb(255, 255, 0, 255);
        public static Color Cyan = Color.FromArgb(255, 0, 255, 255);
        public static Color Silver = Color.FromArgb(255, 192, 192, 192);
        public static Color LightGrey = Color.FromArgb(255, 211, 211, 211);

        
        static ColorUtil()
        {
        }
    }
}