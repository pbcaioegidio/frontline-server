// Decompiled with JetBrains decompiler
// Type: Plugin.Core.SharpDX.MathUtil
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;


namespace Plugin.Core.SharpDX
{
    public static class MathUtil
    {
        public const float ZeroTolerance = 1E-06f;
        public const float Pi = 3.14159274f;
        public const float TwoPi = 6.28318548f;
        public const float PiOverTwo = 1.57079637f;
        public const float PiOverFour = 0.7853982f;

        public static bool NearEqual(float A, float B)
        {
            if (MathUtil.IsZero(A - B))
                return true;
            byte[] bytes1 = BitConverter.GetBytes(A);
            byte[] bytes2 = BitConverter.GetBytes(B);
            int int32_1 = BitConverter.ToInt32(bytes1, 0);
            int int32_2 = BitConverter.ToInt32(bytes2, 0);
            return int32_1 < 0 == int32_2 < 0 && Math.Abs(int32_1 - int32_2) <= 4;
        }

        public static bool IsZero(float A) => (double)Math.Abs(A) < 9.9999999747524271E-07;

        public static bool IsOne(float A) => MathUtil.IsZero(A - 1f);
    }
}
