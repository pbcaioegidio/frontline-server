// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.PatternCollection`1
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Collections.Generic;

namespace Plugin.Core.Colorful
{
    public abstract class PatternCollection<T> : IPrototypable<PatternCollection<T>>
    {
        protected List<Pattern<T>> patterns = new List<Pattern<T>>();

        public PatternCollection<T> Prototype() => this.PrototypeCore();

        protected abstract PatternCollection<T> PrototypeCore();

        public abstract bool MatchFound(string input);
    }
}