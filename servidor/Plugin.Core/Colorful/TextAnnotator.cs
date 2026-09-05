// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.TextAnnotator
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace Plugin.Core.Colorful
{
    public sealed class TextAnnotator
    {
        private StyleSheet Field0;
        private Dictionary<StyleClass<TextPattern>, Styler.MatchFound> Field1 = new Dictionary<StyleClass<TextPattern>, Styler.MatchFound>();

        public TextAnnotator(StyleSheet A_1)
        {
            this.Field0 = A_1;
            foreach (StyleClass<TextPattern> style in A_1.Styles)
                this.Field1.Add(style, (style as Styler).MatchFoundHandler);
        }

        public List<KeyValuePair<string, Color>> GetAnnotationMap(string input)
        {
            return this.Method1((IEnumerable<KeyValuePair<StyleClass<TextPattern>, MatchLocation>>)this.Method0(input), input);
        }

        private List<KeyValuePair<StyleClass<TextPattern>, MatchLocation>> Method0(string A_1_1)
        {
            List<KeyValuePair<StyleClass<TextPattern>, MatchLocation>> source = new List<KeyValuePair<StyleClass<TextPattern>, MatchLocation>>();
            List<MatchLocation> matchLocationList = new List<MatchLocation>();
            foreach (StyleClass<TextPattern> style in this.Field0.Styles)
            {
                foreach (MatchLocation matchLocation in ((Pattern<string>)style.Target).GetMatchLocations(A_1_1))
                {
                    if (matchLocationList.Contains(matchLocation))
                    {
                        int index = matchLocationList.IndexOf(matchLocation);
                        source.RemoveAt(index);
                        matchLocationList.RemoveAt(index);
                    }
                    source.Add(new KeyValuePair<StyleClass<TextPattern>, MatchLocation>(style, matchLocation));
                    matchLocationList.Add(matchLocation);
                }
            }
            return source.OrderBy<KeyValuePair<StyleClass<TextPattern>, MatchLocation>, MatchLocation>((Func<KeyValuePair<StyleClass<TextPattern>, MatchLocation>, MatchLocation>)(A_1_2 => A_1_2.Value)).ToList<KeyValuePair<StyleClass<TextPattern>, MatchLocation>>();
        }

        private List<KeyValuePair<string, Color>> Method1(
          IEnumerable<KeyValuePair<StyleClass<TextPattern>, MatchLocation>> A_1,
          string A_2)
        {
            List<KeyValuePair<string, Color>> keyValuePairList = new List<KeyValuePair<string, Color>>();
            MatchLocation matchLocation1 = new MatchLocation(0, 0);
            int startIndex1 = 0;
            foreach (KeyValuePair<StyleClass<TextPattern>, MatchLocation> keyValuePair in A_1)
            {
                MatchLocation matchLocation2 = keyValuePair.Value;
                if (matchLocation1.End > matchLocation2.Beginning)
                    matchLocation1 = new MatchLocation(0, 0);
                int end = matchLocation1.End;
                int beginning = matchLocation2.Beginning;
                int startIndex2 = beginning;
                startIndex1 = matchLocation2.End;
                string key1 = A_2.Substring(end, beginning - end);
                A_2.Substring(startIndex2, startIndex1 - startIndex2);
                string key2 = this.Field1[keyValuePair.Key](A_2, keyValuePair.Value, A_2.Substring(startIndex2, startIndex1 - startIndex2));
                if (key1 != "")
                    keyValuePairList.Add(new KeyValuePair<string, Color>(key1, this.Field0.UnstyledColor));
                if (key2 != "")
                    keyValuePairList.Add(new KeyValuePair<string, Color>(key2, keyValuePair.Key.Color));
                matchLocation1 = matchLocation2.Prototype();
            }
            if (startIndex1 < A_2.Length)
            {
                string key = A_2.Substring(startIndex1, A_2.Length - startIndex1);
                keyValuePairList.Add(new KeyValuePair<string, Color>(key, this.Field0.UnstyledColor));
            }
            return keyValuePairList;
        }
    }
}