// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.ColorAlternatorFactory
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Drawing;

namespace Plugin.Core.Colorful
{
    public sealed class ColorAlternatorFactory
    {
        public ColorAlternator GetAlternator(string[] patterns, params Color[] colors)
        {
            return (ColorAlternator)new PatternBasedColorAlternator<string>((PatternCollection<string>)new TextPatternCollection(patterns), colors);
        }

        public ColorAlternator GetAlternator(int frequency, params Color[] colors)
        {
            return (ColorAlternator)new FrequencyBasedColorAlternator(frequency, colors);
        }
    }
}