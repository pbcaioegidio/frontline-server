// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.ColorAlternator
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Drawing;

namespace Plugin.Core.Colorful
{
    public abstract class ColorAlternator : IPrototypable<ColorAlternator>
    {
        protected int nextColorIndex;
        public Color[] Colors { get; set; }
        public ColorAlternator() => this.Colors = new Color[0];
        public ColorAlternator(params Color[] A_1) => this.Colors = A_1;
        public ColorAlternator Prototype() => this.PrototypeCore();
        protected abstract ColorAlternator PrototypeCore();
        public abstract Color GetNextColor(string input);
        protected abstract void TryIncrementColorIndex();
    }
}
