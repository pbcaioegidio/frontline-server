// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.StyleClass`1
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Drawing;

namespace Plugin.Core.Colorful
{
    public class StyleClass<T> : IEquatable<StyleClass<T>>
    {
        public T Target { get; protected set; }

        public Color Color { get; protected set; }

        public StyleClass()
        {
        }

        public StyleClass(T A_1, Color A_2)
        {
            this.Target = A_1;
            this.Color = A_2;
        }

        public bool Equals(StyleClass<T> other)
        {
            return other != null && this.Target.Equals((object)other.Target) && this.Color == other.Color;
        }

        public override bool Equals(object obj) => this.Equals(obj as StyleClass<T>);

        public override int GetHashCode()
        {
            return 163 * (79 + this.Target.GetHashCode()) * (79 + this.Color.GetHashCode());
        }
    }
}