// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.StyledString
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Drawing;

namespace Plugin.Core.Colorful
{
    public sealed class StyledString : IEquatable<StyledString>
    {
        public string AbstractValue { get; private set; }

        public string ConcreteValue { get; private set; }

        public Color[,] ColorGeometry { get; set; }

        public char[,] CharacterGeometry { get; set; }

        public int[,] CharacterIndexGeometry { get; set; }

        public StyledString(string A_1) => this.AbstractValue = A_1;

        public StyledString(string A_1, string A_2)
        {
            this.AbstractValue = A_1;
            this.ConcreteValue = A_2;
        }

        public bool Equals(StyledString other)
        {
            return other != null && this.AbstractValue == other.AbstractValue && this.ConcreteValue == other.ConcreteValue;
        }

        public override bool Equals(object obj) => this.Equals(obj as StyledString);

        public override int GetHashCode()
        {
            return 163 * (79 + this.AbstractValue.GetHashCode()) * (79 + this.ConcreteValue.GetHashCode());
        }

        public override string ToString() => this.ConcreteValue;
    }
}