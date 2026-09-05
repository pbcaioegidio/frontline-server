// Decompiled with JetBrains decompiler
// Type: Plugin.Core.SharpDX.Half3
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Runtime.CompilerServices;

namespace Plugin.Core.SharpDX
{
    public struct Half3 : IEquatable<Half3>
    {
        public Half X;
        public Half Y;
        public Half Z;

        public Half3(float A_1, float A_2, float A_3)
        {
            this.X = new Half(A_1);
            this.Y = new Half(A_2);
            this.Z = new Half(A_3);
        }

        public Half3(ushort A_1, ushort A_2, ushort A_3)
        {
            this.X = new Half(A_1);
            this.Y = new Half(A_2);
            this.Z = new Half(A_3);
        }

        public static implicit operator Half3(Vector3 value) => new Half3(value.X, value.Y, value.Z);

        public static implicit operator Vector3(Half3 value)
        {
            return new Vector3((float)value.X, (float)value.Y, (float)value.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Half3 left, Half3 right) => Half3.Equals( left,  right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Half3 left, Half3 right) => !Half3.Equals( left,  right);
        public override int GetHashCode()
        {
            return (this.X.GetHashCode() * 397 ^ this.Y.GetHashCode()) * 397 ^ this.Z.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals( Half3 value1,  Half3 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z;
        }

        public bool Equals(Half3 other) => this.X == other.X && this.Y == other.Y && this.Z == other.Z;

        public override bool Equals(object obj) => obj is Half3 other && this.Equals(other);
    }
}