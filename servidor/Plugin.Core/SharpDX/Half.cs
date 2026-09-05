using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Plugin.Core.SharpDX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Half
    {
        private ushort Value;
        public const int PrecisionDigits = 3;
        public const int MantissaBits = 11;
        public const int MaximumDecimalExponent = 4;
        public const int MaximumBinaryExponent = 15;
        public const int MinimumDecimalExponent = -4;
        public const int MinimumBinaryExponent = -14;
        public const int ExponentRadix = 2;
        public const int AdditionRounding = 1;
        public static readonly float Epsilon;
        public static readonly float MaxValue;
        public static readonly float MinValue;
        public Half(ushort Value)
        {
            this.Value = Value;
        }
        public Half(float Value)
        {
            this.Value = HalfUtils.Pack(Value);
        }
        public ushort RawValue
        {
            get { return Value; }
            set { Value = value; }
        }
        public static float[] ConvertToFloat(Half[] values)
        {
            float[] Results = new float[values.Length];
            for (int i = 0; i < Results.Length; i++)
            {
                Results[i] = HalfUtils.Unpack(values[i].Value);
            }
            return Results;
        }
        public static Half[] ConvertToHalf(float[] values)
        {
            Half[] Results = new Half[values.Length];
            for (int i = 0; i < Results.Length; i++)
            {
                Results[i] = new Half(values[i]);
            }
            return Results;
        }
        public static implicit operator Half(float Value)
        {
            return new Half(Value);
        }
        public static implicit operator float(Half Value)
        {
            return HalfUtils.Unpack(Value.Value);
        }
        public static bool operator ==(Half left, Half right)
        {
            return left.Value == right.Value;
        }
        public static bool operator !=(Half left, Half right)
        {
            return left.Value != right.Value;
        }
        public override string ToString()
        {
            float Value = this;
            return Value.ToString(CultureInfo.CurrentCulture);
        }
        public override int GetHashCode()
        {
            ushort num = Value;
            return (((num * 3) / 2) ^ num);
        }
        [MethodImpl((MethodImplOptions)0x100)] // MethodImplOptions.AggressiveInlining
        public static bool Equals(ref Half value1, ref Half value2)
        {
            return value1.Value == value2.Value;
        }
        public bool Equals(Half other)
        {
            return other.Value == Value;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Half))
            {
                return false;
            }
            return Equals((Half)obj);
        }
        static Half()
        {
            Epsilon = 0.0004887581f;
            MaxValue = 65504f;
            MinValue = 6.103516E-05f;
        }
    }
}
