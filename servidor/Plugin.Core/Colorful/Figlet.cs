// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.Figlet
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Plugin.Core.Colorful
{
    public class Figlet
    {
        private readonly FigletFont Field0;

        public Figlet() => this.Field0 = FigletFont.Default;

        
        public Figlet(FigletFont A_1)
        {
            this.Field0 = A_1 != null ? A_1 : throw new ArgumentNullException("font");
        }

        
        public StyledString ToAscii(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (Encoding.UTF8.GetByteCount(value) != value.Length)
                throw new ArgumentException("String contains non-ascii characters");
            StringBuilder stringBuilder = new StringBuilder();
            int length = Figlet.StaticMethod1(this.Field0, value);
            char[,] A_4 = new char[this.Field0.Height + 1, length];
            int[,] A_5 = new int[this.Field0.Height + 1, length];
            Color[,] colorArray = new Color[this.Field0.Height + 1, length];
            for (int index1 = 1; index1 <= this.Field0.Height; ++index1)
            {
                int A_2 = 0;
                for (int index2 = 0; index2 < value.Length; ++index2)
                {
                    string A_0 = Figlet.StaticMethod2(this.Field0, value[index2], index1);
                    stringBuilder.Append(A_0);
                    Figlet.StaticMethod0(A_0, index2, A_2, index1, A_4, A_5);
                    A_2 += A_0.Length;
                }
                stringBuilder.AppendLine();
            }
            return new StyledString(value, stringBuilder.ToString())
            {
                CharacterGeometry = A_4,
                CharacterIndexGeometry = A_5,
                ColorGeometry = colorArray
            };
        }

        private static void StaticMethod0(
          string A_0,
          int A_1,
          int A_2,
          int A_3,
          char[,] A_4,
          int[,] A_5)
        {
            for (int index = A_2; index < A_2 + A_0.Length; ++index)
            {
                A_4[A_3, index] = A_0[index - A_2];
                A_5[A_3, index] = A_1;
            }
        }

        private static int StaticMethod1(FigletFont A_0, string A_1)
        {
            List<int> source = new List<int>();
            foreach (char A_1_1 in A_1)
            {
                int num = 0;
                for (int A_2 = 1; A_2 <= A_0.Height; ++A_2)
                {
                    string str = Figlet.StaticMethod2(A_0, A_1_1, A_2);
                    num = str.Length > num ? str.Length : num;
                }
                source.Add(num);
            }
            return source.Sum();
        }

        
        private static string StaticMethod2(FigletFont A_0, char A_1, int A_2)
        {
            int num = A_0.CommentLines + (Convert.ToInt32(A_1) - 32 /*0x20*/) * A_0.Height;
            string line = A_0.Lines[num + A_2];
            char ch = line[line.Length - 1];
            string str = Regex.Replace(line, $"\\{ch.ToString()}{{1,2}}$", string.Empty);
            if (A_0.Kerning > 0)
                str += new string(' ', A_0.Kerning);
            return str.Replace(A_0.HardBlank, " ");
        }
    }
}