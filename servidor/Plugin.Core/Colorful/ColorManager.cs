// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.ColorManager
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Plugin.Core.Colorful
{
    public sealed class ColorManager
    {
        private ColorMapper Field1;
        private ColorStore Field2;
        private int Field3;
        private int Field4;

        public bool IsInCompatibilityMode { get; private set; }

        public ColorManager(ColorStore A_1, ColorMapper A_2, int A_3, int A_4, bool A_5)
        {
            this.Field2 = A_1;
            this.Field1 = A_2;
            this.Field3 = A_4;
            this.Field4 = A_3;
            this.IsInCompatibilityMode = A_5;
        }

        public Color GetColor(ConsoleColor color) => this.Field2.ConsoleColors[color];

        public void ReplaceColor(Color oldColor, Color newColor)
        {
            if (this.IsInCompatibilityMode)
                return;
            this.Field1.MapColor(this.Field2.Replace(oldColor, newColor), newColor);
        }

        public ConsoleColor GetConsoleColor(Color color)
        {
            if (this.IsInCompatibilityMode)
                return color.ToNearestConsoleColor();
            try
            {
                return this.Method1(color);
            }
            catch
            {
                return color.ToNearestConsoleColor();
            }
        }

        private ConsoleColor Method1(Color A_1)
        {
            if (this.Method2() && this.Field2.RequiresUpdate(A_1))
            {
                ConsoleColor field3 = (ConsoleColor)this.Field3;
                this.Field1.MapColor(field3, A_1);
                this.Field2.Update(field3, A_1);
                ++this.Field3;
            }
            return this.Field2.Colors.ContainsKey(A_1) ? this.Field2.Colors[A_1] : this.Field2.Colors.Last<KeyValuePair<Color, ConsoleColor>>().Value;
        }

        private bool Method2() => this.Field3 < this.Field4;
    }
}