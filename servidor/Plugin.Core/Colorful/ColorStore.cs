// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.ColorStore
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Colorful
{
    public sealed class ColorStore
    {
        public ConcurrentDictionary<Color, ConsoleColor> Colors { get; private set; }

        public ConcurrentDictionary<ConsoleColor, Color> ConsoleColors { get; private set; }

        public ColorStore(
          ConcurrentDictionary<Color, ConsoleColor> A_1,
          ConcurrentDictionary<ConsoleColor, Color> A_2)
        {
            this.Colors = A_1;
            this.ConsoleColors = A_2;
        }

        public void Update(ConsoleColor oldColor, Color newColor)
        {
            this.Colors.TryAdd(newColor, oldColor);
            this.ConsoleColors[oldColor] = newColor;
        }

        
        public ConsoleColor Replace(Color oldColor, Color newColor)
        {
            ConsoleColor key;
            if (!this.Colors.TryRemove(oldColor, out key))
                throw new ArgumentException("An attempt was made to replace a nonexistent color in the ColorStore!");
            this.Colors.TryAdd(newColor, key);
            this.ConsoleColors[key] = newColor;
            return key;
        }

        public bool RequiresUpdate(Color color) => !this.Colors.ContainsKey(color);
    }
}