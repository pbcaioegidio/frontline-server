// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.Pattern`1
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Collections.Generic;

namespace Plugin.Core.Colorful
{
    public abstract class Pattern<T> : IEquatable<Pattern<T>>
    {
        public T Value { get; private set; }

        public Pattern(T A_1) => this.Value = A_1;

        public abstract IEnumerable<MatchLocation> GetMatchLocations(T input);

        public abstract IEnumerable<T> GetMatches(T input);

        public bool Equals(Pattern<T> other) => other != null && this.Value.Equals((object)other.Value);

        public override bool Equals(object obj) => this.Equals(obj as Pattern<T>);

        public override int GetHashCode() => 163 * (79 + this.Value.GetHashCode());
    }
}