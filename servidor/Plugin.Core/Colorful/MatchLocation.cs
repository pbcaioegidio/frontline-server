// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.MatchLocation
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Colorful
{
    public class MatchLocation :
      IEquatable<MatchLocation>,
      IComparable<MatchLocation>,
      IPrototypable<MatchLocation>
    {
        public int Beginning { get; private set; }

        public int End { get; private set; }

        public MatchLocation(int A_1, int A_2)
        {
            this.Beginning = A_1;
            this.End = A_2;
        }

        public MatchLocation Prototype() => new MatchLocation(this.Beginning, this.End);

        public bool Equals(MatchLocation other)
        {
            return other != null && this.Beginning == other.Beginning && this.End == other.End;
        }

        public override bool Equals(object obj) => this.Equals(obj as MatchLocation);

        public override int GetHashCode()
        {
            return 163 * (79 + this.Beginning.GetHashCode()) * (79 + this.End.GetHashCode());
        }

        public int CompareTo(MatchLocation other) => this.Beginning.CompareTo(other.Beginning);

        
        public override string ToString()
        {
            return $"{this.Beginning}, {this.End}";
        }
    }
}