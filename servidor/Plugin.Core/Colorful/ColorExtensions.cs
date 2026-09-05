// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.ColorExtensions
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Colorful
{
    public static class ColorExtensions
    {
        
        public static ConsoleColor ToNearestConsoleColor(this Color color)
        {
            ConsoleColor nearestConsoleColor1 = ConsoleColor.Black;
            double num1 = double.MaxValue;
            foreach (ConsoleColor nearestConsoleColor2 in Enum.GetValues(typeof(ConsoleColor)))
            {
                string name = Enum.GetName(typeof(ConsoleColor), (object)nearestConsoleColor2);
                Color color1 = Color.FromName(string.Equals(name, "DarkYellow", StringComparison.Ordinal) ? "Orange" : name);
                double num2 = Math.Pow((double)((int)color1.R - (int)color.R), 2.0) + Math.Pow((double)((int)color1.G - (int)color.G), 2.0) + Math.Pow((double)((int)color1.B - (int)color.B), 2.0);
                if (num2 == 0.0)
                    return nearestConsoleColor2;
                if (num2 < num1)
                {
                    num1 = num2;
                    nearestConsoleColor1 = nearestConsoleColor2;
                }
            }
            return nearestConsoleColor1;
        }
    }
}