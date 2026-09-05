// Decompiled with JetBrains decompiler
// Type: Plugin.Core.SharpDX.Vector3Union
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Runtime.InteropServices;

namespace Plugin.Core.SharpDX
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector3Union
    {
        [FieldOffset(0)]
        public Vector3 Vec3;
        [FieldOffset(0)]
        public RawVector3 RawVec3;
    }
}
