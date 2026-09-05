// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.FrequencyBasedColorAlternator
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Colorful
{
    public sealed class FrequencyBasedColorAlternator :
    ColorAlternator,
    IPrototypable<FrequencyBasedColorAlternator>
    {
        private int Field0;
        private int Field1;

        public FrequencyBasedColorAlternator(int A_1, params Color[] A_2)
            : base(A_2)
        {
            this.Field0 = A_1;
        }

        public new FrequencyBasedColorAlternator Prototype()
        {
            return new FrequencyBasedColorAlternator(this.Field0, this.Colors.ToArray());
        }

        protected override ColorAlternator PrototypeCore()
        {
            return this.Prototype();
        }

        public override Color GetNextColor(string input)
        {
            Color nextColor = this.Colors.Length != 0 ? this.Colors[this.nextColorIndex] : throw new InvalidOperationException("No colors have been supplied over which to alternate!");
            this.TryIncrementColorIndex();
            return nextColor;
        }

        protected override void TryIncrementColorIndex()
        {
            if (this.Field1 >= this.Colors.Length * this.Field0 - 1)
            {
                this.nextColorIndex = 0;
                this.Field1 = 0;
            }
            else
            {
                ++this.Field1;
                this.nextColorIndex = (int)Math.Floor((double)this.Field1 / (double)this.Field0);
            }
        }
    }
}