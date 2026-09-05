// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.Formatter
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Drawing;

namespace Plugin.Core.Colorful
{
    public sealed class Formatter
    {
        private StyleClass<object> Field0;

        public object Target => this.Field0.Target;

        public Color Color => this.Field0.Color;

        public Formatter(object A_1, Color A_2) => this.Field0 = new StyleClass<object>(A_1, A_2);
    }
}