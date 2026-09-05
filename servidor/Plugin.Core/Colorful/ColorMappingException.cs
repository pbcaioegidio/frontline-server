// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.ColorMappingException
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Colorful
{
    public sealed class ColorMappingException : Exception
    {
        public int ErrorCode { get; private set; }

        
        public ColorMappingException(int A_1)
          : base($"Color conversion failed with system error code {A_1}!")
        {
            this.ErrorCode = A_1;
        }
    }
}