// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.FigletFont
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Plugin.Core.Colorful
{
    public class FigletFont
    {
        public static FigletFont Default => FigletFont.Parse(DefaultFonts.SmallSlant);

        public int BaseLine { get; private set; }

        public int CodeTagCount { get; private set; }

        public int CommentLines { get; private set; }

        public int FullLayout { get; private set; }

        public string HardBlank { get; private set; }

        public int Height { get; private set; }

        public int Kerning { get; private set; }

        public string[] Lines { get; private set; }

        public int MaxLength { get; private set; }

        public int OldLayout { get; private set; }

        public int PrintDirection { get; private set; }

        public string Signature { get; private set; }

        public static FigletFont Load(byte[] bytes)
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
                return FigletFont.Load((Stream)memoryStream);
        }

        
        public static FigletFont Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            List<string> fontLines = new List<string>();
            using (StreamReader streamReader = new StreamReader(stream))
            {
                while (!streamReader.EndOfStream)
                    fontLines.Add(streamReader.ReadLine());
            }
            return FigletFont.Parse((IEnumerable<string>)fontLines);
        }

        
        public static FigletFont Load(string filePath)
        {
            return filePath != null ? FigletFont.Parse(File.ReadLines(filePath)) : throw new ArgumentNullException(nameof(filePath));
        }

        
        public static FigletFont Parse(string fontContent)
        {
            return fontContent != null ? FigletFont.Parse((IEnumerable<string>)fontContent.Split(new string[3]
            {
      "\r\n",
      "\r",
      "\n"
            }, StringSplitOptions.None)) : throw new ArgumentNullException(nameof(fontContent));
        }

        
        public static FigletFont Parse(IEnumerable<string> fontLines)
        {
            FigletFont figletFont = fontLines != null ? new FigletFont()
            {
                Lines = fontLines.ToArray<string>()
            } : throw new ArgumentNullException(nameof(fontLines));
            string[] strArray = ((IEnumerable<string>)figletFont.Lines).First<string>().Split(' ');
            figletFont.Signature = ((IEnumerable<string>)strArray).First<string>().Remove(((IEnumerable<string>)strArray).First<string>().Length - 1);
            if (figletFont.Signature == "flf2a")
            {
                figletFont.HardBlank = ((IEnumerable<string>)strArray).First<string>().Last<char>().ToString();
                figletFont.Height = FigletFont.StaticMethod0(strArray, 1);
                figletFont.BaseLine = FigletFont.StaticMethod0(strArray, 2);
                figletFont.MaxLength = FigletFont.StaticMethod0(strArray, 3);
                figletFont.OldLayout = FigletFont.StaticMethod0(strArray, 4);
                figletFont.CommentLines = FigletFont.StaticMethod0(strArray, 5);
                figletFont.PrintDirection = FigletFont.StaticMethod0(strArray, 6);
                figletFont.FullLayout = FigletFont.StaticMethod0(strArray, 7);
                figletFont.CodeTagCount = FigletFont.StaticMethod0(strArray, 8);
            }
            return figletFont;
        }

        private static int StaticMethod0(string[] A_0, int A_1)
        {
            int result = 0;
            if (A_0.Length > A_1)
                int.TryParse(A_0[A_1], out result);
            return result;
        }
    }
}