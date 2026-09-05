// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.ColorManagerFactory
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Collections.Concurrent;
using System.Drawing;

namespace Plugin.Core.Colorful
{
    public sealed class ColorManagerFactory
    {
        public ColorManager GetManager(ColorStore colorStore, int maxColorChanges, int initialColorChangeCountValue, bool isInCompatibilityMode)
        {
            return new ColorManager(colorStore, new ColorMapper(), maxColorChanges, initialColorChangeCountValue, isInCompatibilityMode);
        }

        public ColorManager GetManager(ConcurrentDictionary<Color, ConsoleColor> colorMap, ConcurrentDictionary<ConsoleColor, Color> consoleColorMap, int maxColorChanges, int initialColorChangeCountValue, bool isInCompatibilityMode)
        {
            return new ColorManager(new ColorStore(colorMap, consoleColorMap), new ColorMapper(), maxColorChanges, initialColorChangeCountValue, isInCompatibilityMode);
        }
    }
}