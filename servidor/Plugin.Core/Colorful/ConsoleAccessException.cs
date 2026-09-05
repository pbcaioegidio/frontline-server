// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.ConsoleAccessException
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Colorful
{
    public sealed class ConsoleAccessException : Exception
    {
        
        public ConsoleAccessException()
          : base(string.Format("Color conversion failed because a handle to the actual windows console was not found."))
        {
        }
    }
}