using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Plugin.Core.SharpDX
{
    public static class Half3Extensions
    {
        public static Vector3 ToVector3(this Half3 half3)
        {
            return new Vector3((float)half3.X, (float)half3.Y, (float)half3.Z);
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector3 : IEquatable<Vector3>, IFormattable
    {
        public static readonly Vector3 Zero = new Vector3();
        public static readonly Vector3 UnitX = new Vector3(1.0f, 0.0f, 0.0f);
        public static readonly Vector3 UnitY = new Vector3(0.0f, 1.0f, 0.0f);
        public static readonly Vector3 UnitZ = new Vector3(0.0f, 0.0f, 1.0f);
        public static readonly Vector3 One = new Vector3(1.0f, 1.0f, 1.0f);
        public static readonly Vector3 Up = new Vector3(0.0f, 1.0f, 0.0f);
        public static readonly Vector3 Down = new Vector3(0.0f, -1.0f, 0.0f);
        public static readonly Vector3 Left = new Vector3(-1.0f, 0.0f, 0.0f);
        public static readonly Vector3 Right = new Vector3(1.0f, 0.0f, 0.0f);
        public static readonly Vector3 ForwardRH = new Vector3(0.0f, 0.0f, -1.0f);
        public static readonly Vector3 ForwardLH = new Vector3(0.0f, 0.0f, 1.0f);
        public static readonly Vector3 BackwardRH = new Vector3(0.0f, 0.0f, 1.0f);
        public static readonly Vector3 BackwardLH = new Vector3(0.0f, 0.0f, -1.0f);
        public float X;
        public float Y;
        public float Z;
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public static float Distance(Vector3 value1, Vector3 value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            float z = value1.Z - value2.Z;
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }
        public Vector3(float[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (values.Length != 3)
            {
                throw new ArgumentOutOfRangeException("values", "There must be three and only three input values for Vector3.");
            }
            X = values[0];
            Y = values[1];
            Z = values[2];
        }
        public bool IsNormalized
        {
            get { return MathUtil.IsOne((X * X) + (Y * Y) + (Z * Z)); }
        }
        public bool IsZero
        {
            get { return X == 0 && Y == 0 && Z == 0; }
        }
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                }
                throw new ArgumentOutOfRangeException("index", "Indices for Vector3 run from 0 to 2, inclusive.");
            }
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default: throw new ArgumentOutOfRangeException("index", "Indices for Vector3 run from 0 to 2, inclusive.");
                }
            }
        }
        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
        public static Vector3 operator *(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }
        public static Vector3 operator +(Vector3 value)
        {
            return value;
        }
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }
        public static Vector3 operator -(Vector3 value)
        {
            return new Vector3(-value.X, -value.Y, -value.Z);
        }
        public static Vector3 operator *(float scale, Vector3 value)
        {
            return new Vector3(value.X * scale, value.Y * scale, value.Z * scale);
        }
        public static Vector3 operator *(Vector3 value, float scale)
        {
            return new Vector3(value.X * scale, value.Y * scale, value.Z * scale);
        }
        public static Vector3 operator /(Vector3 value, float scale)
        {
            return new Vector3(value.X / scale, value.Y / scale, value.Z / scale);
        }
        public static Vector3 operator /(float scale, Vector3 value)
        {
            return new Vector3(scale / value.X, scale / value.Y, scale / value.Z);
        }
        public static Vector3 operator /(Vector3 value, Vector3 scale)
        {
            return new Vector3(value.X / scale.X, value.Y / scale.Y, value.Z / scale.Z);
        }
        public static Vector3 operator +(Vector3 value, float scalar)
        {
            return new Vector3(value.X + scalar, value.Y + scalar, value.Z + scalar);
        }
        public static Vector3 operator +(float scalar, Vector3 value)
        {
            return new Vector3(scalar + value.X, scalar + value.Y, scalar + value.Z);
        }
        public static Vector3 operator -(Vector3 value, float scalar)
        {
            return new Vector3(value.X - scalar, value.Y - scalar, value.Z - scalar);
        }
        public static Vector3 operator -(float scalar, Vector3 value)
        {
            return new Vector3(scalar - value.X, scalar - value.Y, scalar - value.Z);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }
        public string ToString(string format)
        {
            if (format == null)
            {
                return ToString();
            }
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
        }
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
            {
                return ToString(formatProvider);
            }
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
        [MethodImpl((MethodImplOptions)0x100)] // MethodImplOptions.AggressiveInlining
        public bool Equals(ref Vector3 other)
        {
            return MathUtil.NearEqual(other.X, X) && MathUtil.NearEqual(other.Y, Y) && MathUtil.NearEqual(other.Z, Z);
        }
        [MethodImpl((MethodImplOptions)0x100)] // MethodImplOptions.AggressiveInlining
        public bool Equals(Vector3 other)
        {
            return Equals(ref other);
        }
        public override bool Equals(object value)
        {
            if (!(value is Vector3))
            {
                return false;
            }
            var strongValue = (Vector3)value;
            return Equals(ref strongValue);
        }
        public unsafe static implicit operator RawVector3(Vector3 value)
        {
            return *(RawVector3*)&value;
        }
        public unsafe static implicit operator Vector3(RawVector3 value)
        {
            return *(Vector3*)&value;
        }
    }
}
