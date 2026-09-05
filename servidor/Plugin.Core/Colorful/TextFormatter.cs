// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.TextFormatter
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Plugin.Core.Colorful
{
    public sealed class TextFormatter
    {
        private Color Field0;
        private TextPattern Field1;
        private readonly string Field2 = "{[0-9][^}]*}";

        
        public TextFormatter(Color A_1)
        {
            this.Field0 = A_1;
            this.Field1 = new TextPattern(this.Field2);
        }

        
        public TextFormatter(Color A_1, string A_2)
        {
            this.Field0 = A_1;
            this.Field1 = new TextPattern(this.Field2);
        }

        public List<KeyValuePair<string, Color>> GetFormatMap(
          string input,
          object[] args,
          Color[] colors)
        {
            List<KeyValuePair<string, Color>> formatMap = new List<KeyValuePair<string, Color>>();
            List<MatchLocation> list1 = ((Pattern<string>)this.Field1).GetMatchLocations(input).ToList<MatchLocation>();
            List<string> list2 = ((Pattern<string>)this.Field1).GetMatches(input).ToList<string>();
            this.Method0( args,  colors);
            int startIndex1 = 0;
            for (int index1 = 0; index1 < list1.Count<MatchLocation>(); ++index1)
            {
                int index2 = int.Parse(list2[index1].TrimStart('{').TrimEnd('}'));
                int startIndex2 = 0;
                if (index1 > 0)
                    startIndex2 = list1[index1 - 1].End;
                int beginning = list1[index1].Beginning;
                startIndex1 = list1[index1].End;
                string key1 = input.Substring(startIndex2, beginning - startIndex2);
                string key2 = args[index2].ToString();
                formatMap.Add(new KeyValuePair<string, Color>(key1, this.Field0));
                formatMap.Add(new KeyValuePair<string, Color>(key2, colors[index2]));
            }
            if (startIndex1 < input.Length)
            {
                string key = input.Substring(startIndex1, input.Length - startIndex1);
                formatMap.Add(new KeyValuePair<string, Color>(key, this.Field0));
            }
            return formatMap;
        }

        private void Method0( object[] A_1,  Color[] A_2)
        {
            if (A_2.Length >= A_1.Length)
                return;
            Color color = A_2[0];
            A_2 = new Color[A_1.Length];
            for (int index = 0; index < A_1.Length; ++index)
                A_2[index] = color;
        }
    }
}