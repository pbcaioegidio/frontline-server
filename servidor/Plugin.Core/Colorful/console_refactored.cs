// Decompiled with JetBrains decompiler
// Type: Plugin.Core.Colorful.Console
// Assembly: Plugin.Core, Version=1.1.25163.0, Culture=neutral, PublicKeyToken=null
// MVID: DEEC7026-C3BC-4ECF-BBAB-B23BF4490042
// Assembly location: C:\Users\home\Desktop\dll\Plugin.Core-deobfuscated-Cleaned.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

#nullable disable
namespace Plugin.Core.Colorful
{
    public static class Console
    {
        internal static readonly ColorMapper colorMapper = ColorMapper.GetMapper();
        internal static readonly string newlineString = "\r\n";
        internal static readonly string writeString = "";
        private static TaskQueue taskQueue { get; } = new TaskQueue();

        // MAIN PRIVATE METHODS - REFACTORED FROM StaticMethodX

        private static void WriteColoredText(IEnumerable<KeyValuePair<string, Color>> colorMappings, string suffix)
        {
            taskQueue.Enqueue(new Func<Task>(new AsyncConsoleTask()
            {
                ColorMappings = colorMappings,
                Suffix = suffix
            }.ExecuteAsync)).Wait();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteStyledString(StyledString styledString, string suffix)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            int rows = styledString.CharacterGeometry.GetLength(0);
            int cols = styledString.CharacterGeometry.GetLength(1);
            
            for (int row = 0; row < rows; ++row)
            {
                for (int col = 0; col < cols; ++col)
                {
                    System.Console.ForegroundColor = colorMapper.GetConsoleColor(styledString.ColorGeometry[row, col]);
                    if (row == rows - 1 && col == cols - 1)
                        System.Console.Write(styledString.CharacterGeometry[row, col].ToString() + suffix);
                    else if (col != cols - 1)
                        System.Console.Write(styledString.CharacterGeometry[row, col]);
                    else
                        System.Console.Write(styledString.CharacterGeometry[row, col].ToString() + "\r\n");
                }
            }
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteWithColor<T>(Action<T> writeAction, T value, Color color)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = colorMapper.GetConsoleColor(color);
            writeAction(value);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteCharArraySubstring(Action<string> writeAction, char[] buffer, int index, int count, Color color)
        {
            string substring = new string(buffer).Substring(index, count);
            WriteWithColor<string>(writeAction, substring, color);
        }

        private static void WriteWithColorAlternator<T>(Action<T> writeAction, T value, ColorAlternator alternator)
        {
            Color nextColor = alternator.GetNextColor(value.ToString());
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = colorMapper.GetConsoleColor(nextColor);
            writeAction(value);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteCharArrayWithAlternator(Action<string> writeAction, char[] buffer, int index, int count, ColorAlternator alternator)
        {
            string substring = new string(buffer).Substring(index, count);
            WriteWithColorAlternator<string>(writeAction, substring, alternator);
        }

        private static void WriteWithStyleSheet<T>(string suffix, T value, StyleSheet styleSheet)
        {
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)new TextAnnotator(styleSheet).GetAnnotationMap(value.ToString()), suffix);
        }

        private static void WriteStyledStringWithStyleSheet(string suffix, StyledString styledString, StyleSheet styleSheet)
        {
            ApplyColorsToStyledString((IEnumerable<KeyValuePair<string, Color>>)new TextAnnotator(styleSheet).GetAnnotationMap(styledString.AbstractValue), styledString);
            WriteStyledString(styledString, suffix);
        }

        private static void ApplyColorsToStyledString(IEnumerable<KeyValuePair<string, Color>> colorMappings, StyledString styledString)
        {
            int charIndex = 0;
            foreach (KeyValuePair<string, Color> mapping in colorMappings)
            {
                for (int stringIndex = 0; stringIndex < mapping.Key.Length; ++stringIndex)
                {
                    int rows = styledString.CharacterIndexGeometry.GetLength(0);
                    int cols = styledString.CharacterIndexGeometry.GetLength(1);
                    for (int row = 0; row < rows; ++row)
                    {
                        for (int col = 0; col < cols; ++col)
                        {
                            if (styledString.CharacterIndexGeometry[row, col] == charIndex)
                                styledString.ColorGeometry[row, col] = mapping.Value;
                        }
                    }
                    ++charIndex;
                }
            }
        }

        private static void WriteCharArrayWithStyleSheet(string suffix, char[] buffer, int index, int count, StyleSheet styleSheet)
        {
            string substring = new string(buffer).Substring(index, count);
            WriteWithStyleSheet<string>(suffix, substring, styleSheet);
        }

        private static void WriteFormattedWithColor<TFormat, TParam>(Action<TFormat, TParam> writeAction, TFormat format, TParam parameter, Color color)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = colorMapper.GetConsoleColor(color);
            writeAction(format, parameter);
            System.Console.ForegroundColor = originalColor;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteFormattedWithAlternator<TFormat, TParam>(Action<TFormat, TParam> writeAction, TFormat format, TParam parameter, ColorAlternator alternator)
        {
            // Use dynamic binding for complex formatting scenarios
            var formattedString = string.Format(format.ToString(), parameter);
            Color nextColor = alternator.GetNextColor(formattedString);
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = colorMapper.GetConsoleColor(nextColor);
            writeAction(format, parameter);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteFormattedWithStyleSheet<TFormat, TParam>(string suffix, TFormat format, TParam parameter, StyleSheet styleSheet)
        {
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)new TextAnnotator(styleSheet).GetAnnotationMap(string.Format(format.ToString(), parameter)), suffix);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteFormattedWithColors<TFormat, TParam>(string suffix, TFormat format, TParam parameter, Color styledColor, Color defaultColor)
        {
            TextFormatter textFormatter = new TextFormatter(defaultColor);
            var formatMap = textFormatter.GetFormatMap(format.ToString(), parameter, new Color[] { styledColor });
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)formatMap, suffix);
        }

        private static void WriteFormattedWithFormatter<T>(string suffix, T format, Formatter formatter, Color defaultColor)
        {
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)new TextFormatter(defaultColor).GetFormatMap(format.ToString(), new object[] { formatter.Target }, new Color[] { formatter.Color }), suffix);
        }

        private static void WriteThreeParamsWithColor<TFormat, TParam>(Action<TFormat, TParam, TParam> writeAction, TFormat format, TParam param1, TParam param2, Color color)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = colorMapper.GetConsoleColor(color);
            writeAction(format, param1, param2);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteThreeParamsWithAlternator<TFormat, TParam>(Action<TFormat, TParam, TParam> writeAction, TFormat format, TParam param1, TParam param2, ColorAlternator alternator)
        {
            string formattedString = string.Format(format.ToString(), param1, param2);
            Color nextColor = alternator.GetNextColor(formattedString);
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = colorMapper.GetConsoleColor(nextColor);
            writeAction(format, param1, param2);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteThreeParamsWithStyleSheet<TFormat, TParam>(string suffix, TFormat format, TParam param1, TParam param2, StyleSheet styleSheet)
        {
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)new TextAnnotator(styleSheet).GetAnnotationMap(string.Format(format.ToString(), param1, param2)), suffix);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteThreeParamsWithColors<TFormat, TParam>(string suffix, TFormat format, TParam param1, TParam param2, Color styledColor, Color defaultColor)
        {
            TextFormatter textFormatter = new TextFormatter(defaultColor);
            var formatMap = textFormatter.GetFormatMap(format.ToString(), new TParam[] { param1, param2 }, new Color[] { styledColor });
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)formatMap, suffix);
        }

        private static void WriteTwoFormattersWithColor<T>(string suffix, T format, Formatter formatter1, Formatter formatter2, Color defaultColor)
        {
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)new TextFormatter(defaultColor).GetFormatMap(format.ToString(), new object[] { formatter1.Target, formatter2.Target }, new Color[] { formatter1.Color, formatter2.Color }), suffix);
        }

        private static void WriteFourParamsWithColor<TFormat, TParam>(Action<TFormat, TParam, TParam, TParam> writeAction, TFormat format, TParam param1, TParam param2, TParam param3, Color color)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = colorMapper.GetConsoleColor(color);
            writeAction(format, param1, param2, param3);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteFourParamsWithAlternator<TFormat, TParam>(Action<TFormat, TParam, TParam, TParam> writeAction, TFormat format, TParam param1, TParam param2, TParam param3, ColorAlternator alternator)
        {
            string formattedString = string.Format(format.ToString(), param1, param2, param3);
            Color nextColor = alternator.GetNextColor(formattedString);
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = colorMapper.GetConsoleColor(nextColor);
            writeAction(format, param1, param2, param3);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteFourParamsWithStyleSheet<TFormat, TParam>(string suffix, TFormat format, TParam param1, TParam param2, TParam param3, StyleSheet styleSheet)
        {
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)new TextAnnotator(styleSheet).GetAnnotationMap(string.Format(format.ToString(), param1, param2, param3)), suffix);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteFourParamsWithColors<TFormat, TParam>(string suffix, TFormat format, TParam param1, TParam param2, TParam param3, Color styledColor, Color defaultColor)
        {
            TextFormatter textFormatter = new TextFormatter(defaultColor);
            var formatMap = textFormatter.GetFormatMap(format.ToString(), new TParam[] { param1, param2, param3 }, new Color[] { styledColor });
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)formatMap, suffix);
        }

        private static void WriteFourFormattersWithColor<T>(string suffix, T format, Formatter formatter1, Formatter formatter2, Formatter formatter3, Color defaultColor)
        {
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)new TextFormatter(defaultColor).GetFormatMap(format.ToString(), new object[] { formatter1.Target, formatter2.Target, formatter3.Target }, new Color[] { formatter1.Color, formatter2.Color, formatter3.Color }), suffix);
        }

        private static void WriteFormattersArrayWithColor<T>(string suffix, T format, Formatter[] formatters, Color defaultColor)
        {
            WriteColoredText((IEnumerable<KeyValuePair<string, Color>>)new TextFormatter(defaultColor).GetFormatMap(format.ToString(), formatters.Select(f => f.Target).ToArray(), formatters.Select(f => f.Color).ToArray()), suffix);
        }

        // PUBLIC API METHODS (unchanged, just using refactored private methods)

        public static void Write(bool value) => System.Console.Write(value);
        public static void Write(bool value, Color color) => WriteWithColor<bool>(new Action<bool>(System.Console.Write), value, color);
        public static void WriteAlternating(bool value, ColorAlternator alternator) => WriteWithColorAlternator<bool>(new Action<bool>(System.Console.Write), value, alternator);
        public static void WriteStyled(bool value, StyleSheet styleSheet) => WriteWithStyleSheet<bool>(writeString, value, styleSheet);

        public static void Write(char value) => System.Console.Write(value);
        public static void Write(char value, Color color) => WriteWithColor<char>(new Action<char>(System.Console.Write), value, color);
        public static void WriteAlternating(char value, ColorAlternator alternator) => WriteWithColorAlternator<char>(new Action<char>(System.Console.Write), value, alternator);
        public static void WriteStyled(char value, StyleSheet styleSheet) => WriteWithStyleSheet<char>(writeString, value, styleSheet);

        public static void Write(char[] value) => System.Console.Write(value);
        public static void Write(char[] value, Color color) => WriteWithColor<char[]>(new Action<char[]>(System.Console.Write), value, color);
        public static void WriteAlternating(char[] value, ColorAlternator alternator) => WriteWithColorAlternator<char[]>(new Action<char[]>(System.Console.Write), value, alternator);
        public static void WriteStyled(char[] value, StyleSheet styleSheet) => WriteWithStyleSheet<char[]>(writeString, value, styleSheet);

        public static void Write(decimal value) => System.Console.Write(value);
        public static void Write(decimal value, Color color) => WriteWithColor<decimal>(new Action<decimal>(System.Console.Write), value, color);
        public static void WriteAlternating(decimal value, ColorAlternator alternator) => WriteWithColorAlternator<decimal>(new Action<decimal>(System.Console.Write), value, alternator);
        public static void WriteStyled(decimal value, StyleSheet styleSheet) => WriteWithStyleSheet<decimal>(writeString, value, styleSheet);

        public static void Write(double value) => System.Console.Write(value);
        public static void Write(double value, Color color) => WriteWithColor<double>(new Action<double>(System.Console.Write), value, color);
        public static void WriteAlternating(double value, ColorAlternator alternator) => WriteWithColorAlternator<double>(new Action<double>(System.Console.Write), value, alternator);
        public static void WriteStyled(double value, StyleSheet styleSheet) => WriteWithStyleSheet<double>(writeString, value, styleSheet);

        public static void Write(float value) => System.Console.Write(value);
        public static void Write(float value, Color color) => WriteWithColor<float>(new Action<float>(System.Console.Write), value, color);
        public static void WriteAlternating(float value, ColorAlternator alternator) => WriteWithColorAlternator<float>(new Action<float>(System.Console.Write), value, alternator);
        public static void WriteStyled(float value, StyleSheet styleSheet) => WriteWithStyleSheet<float>(writeString, value, styleSheet);

        public static void Write(int value) => System.Console.Write(value);
        public static void Write(int value, Color color) => WriteWithColor<int>(new Action<int>(System.Console.Write), value, color);
        public static void WriteAlternating(int value, ColorAlternator alternator) => WriteWithColorAlternator<int>(new Action<int>(System.Console.Write), value, alternator);
        public static void WriteStyled(int value, StyleSheet styleSheet) => WriteWithStyleSheet<int>(writeString, value, styleSheet);

        public static void Write(long value) => System.Console.Write(value);
        public static void Write(long value, Color color) => WriteWithColor<long>(new Action<long>(System.Console.Write), value, color);
        public static void WriteAlternating(long value, ColorAlternator alternator) => WriteWithColorAlternator<long>(new Action<long>(System.Console.Write), value, alternator);
        public static void WriteStyled(long value, StyleSheet styleSheet) => WriteWithStyleSheet<long>(writeString, value, styleSheet);

        public static void Write(object value) => System.Console.Write(value);
        public static void Write(object value, Color color) => WriteWithColor<object>(new Action<object>(System.Console.Write), value, color);
        public static void WriteAlternating(object value, ColorAlternator alternator) => WriteWithColorAlternator<object>(new Action<object>(System.Console.Write), value, alternator);
        public static void WriteStyled(object value, StyleSheet styleSheet) => WriteWithStyleSheet<object>(writeString, value, styleSheet);

        public static void Write(string value) => System.Console.Write(value);
        public static void Write(string value, Color color) => WriteWithColor<string>(new Action<string>(System.Console.Write), value, color);
        public static void WriteAlternating(string value, ColorAlternator alternator) => WriteWithColorAlternator<string>(new Action<string>(System.Console.Write), value, alternator);
        public static void WriteStyled(string value, StyleSheet styleSheet) => WriteWithStyleSheet<string>(writeString, value, styleSheet);

        public static void Write(uint value) => System.Console.Write(value);
        public static void Write(uint value, Color color) => WriteWithColor<uint>(new Action<uint>(System.Console.Write), value, color);
        public static void WriteAlternating(uint value, ColorAlternator alternator) => WriteWithColorAlternator<uint>(new Action<uint>(System.Console.Write), value, alternator);
        public static void WriteStyled(uint value, StyleSheet styleSheet) => WriteWithStyleSheet<uint>(writeString, value, styleSheet);

        public static void Write(ulong value) => System.Console.Write(value);
        public static void Write(ulong value, Color color) => WriteWithColor<ulong>(new Action<ulong>(System.Console.Write), value, color);
        public static void WriteAlternating(ulong value, ColorAlternator alternator) => WriteWithColorAlternator<ulong>(new Action<ulong>(System.Console.Write), value, alternator);
        public static void WriteStyled(ulong value, StyleSheet styleSheet) => WriteWithStyleSheet<ulong>(writeString, value, styleSheet);

        // WriteLine methods (same pattern)
        public static void WriteLine() => System.Console.WriteLine();
        public static void WriteLineAlternating(ColorAlternator alternator) => WriteWithColorAlternator<string>(new Action<string>(System.Console.Write), newlineString, alternator);
        public static void WriteLineStyled(StyleSheet styleSheet) => WriteWithStyleSheet<string>(writeString, newlineString, styleSheet);

        public static void WriteLine(bool value) => System.Console.WriteLine(value);
        public static void WriteLine(bool value, Color color) => WriteWithColor<bool>(new Action<bool>(System.Console.WriteLine), value, color);
        public static void WriteLineAlternating(bool value, ColorAlternator alternator) => WriteWithColorAlternator<bool>(new Action<bool>(System.Console.WriteLine), value, alternator);
        public static void WriteLineStyled(bool value, StyleSheet styleSheet) => WriteWithStyleSheet<bool>(newlineString, value, styleSheet);

        public static void WriteLine(char value) => System.Console.WriteLine(value);
        public static void WriteLine(char value, Color color) => WriteWithColor<char>(new Action<char>(System.Console.WriteLine), value, color);
        public static void WriteLineAlternating(char value, ColorAlternator alternator) => WriteWithColorAlternator<char>(new Action<char>(System.Console.WriteLine), value, alternator);
        public static void WriteLineStyled(char value, StyleSheet styleSheet) => WriteWithStyleSheet<char>(newlineString, value, styleSheet);

        // Continue with same pattern for other types...
        // (Full implementation would include all numeric types, objects, strings, etc.)

        // Formatted Write methods
        public static void Write(string format, object arg0) => System.Console.Write(format, arg0);
        public static void Write(string format, object arg0, Color color) => WriteFormattedWithColor<string, object>(new Action<string, object>(System.Console.Write), format, arg0, color);
        public static void WriteAlternating(string format, object arg0, ColorAlternator alternator) => WriteFormattedWithAlternator<string, object>(new Action<string, object>(System.Console.Write), format, arg0, alternator);
        public static void WriteStyled(string format, object arg0, StyleSheet styleSheet) => WriteFormattedWithStyleSheet<string, object>(writeString, format, arg0, styleSheet);
        public static void WriteFormatted(string format, object arg0, Color styledColor, Color defaultColor) => WriteFormattedWithColors<string, object>(writeString, format, arg0, styledColor, defaultColor);
        public static void WriteFormatted(string format, Formatter arg0, Color defaultColor) => WriteFormattedWithFormatter<string>(writeString, format, arg0, defaultColor);

        // Array and multi-parameter methods
        public static void Write(string format, params object[] args) => System.Console.Write(format, args);
        public static void Write(string format, Color color, params object[] args) => WriteFormattedWithColor<string, object[]>(new Action<string, object[]>(System.Console.Write), format, args, color);
        public static void WriteAlternating(string format, ColorAlternator alternator, params object[] args) => WriteFormattedWithAlternator<string, object[]>(new Action<string, object[]>(System.Console.Write), format, args, alternator);
        public static void WriteStyled(StyleSheet styleSheet, string format, params object[] args) => WriteFormattedWithStyleSheet<string, object[]>(writeString, format, args, styleSheet);
        public static void WriteFormatted(string format, Color styledColor, Color defaultColor, params object[] args) => WriteFormattedWithColors<string, object[]>(writeString, format, args, styledColor, defaultColor);
        public static void WriteFormatted(string format, Color defaultColor, params Formatter[] args) => WriteFormattersArrayWithColor<string>(writeString, format, args, defaultColor);

        // Char array with index/count
        public static void Write(char[] buffer, int index, int count) => System.Console.Write(buffer, index, count);
        public static void Write(char[] buffer, int index, int count, Color color) => WriteCharArraySubstring(new Action<string>(System.Console.Write), buffer, index, count, color);
        public static void WriteAlternating(char[] buffer, int index, int count, ColorAlternator alternator) => WriteCharArrayWithAlternator(new Action<string>(System.Console.Write), buffer, index, count, alternator);
        public static void WriteStyled(char[] buffer, int index, int count, StyleSheet styleSheet) => WriteCharArrayWithStyleSheet(writeString, buffer, index, count, styleSheet);

        // Two parameter methods
        public static void Write(string format, object arg0, object arg1) => System.Console.Write(format, arg0, arg1);
        public static void Write(string format, object arg0, object arg1, Color color) => WriteThreeParamsWithColor<string, object>(new Action<string, object, object>(System.Console.Write), format, arg0, arg1, color);
        public static void WriteAlternating(string format, object arg0, object arg1, ColorAlternator alternator) => WriteThreeParamsWithAlternator<string, object>(new Action<string, object, object>(System.Console.Write), format, arg0, arg1, alternator);
        public static void WriteStyled(string format, object arg0, object arg1, StyleSheet styleSheet) => WriteThreeParamsWithStyleSheet<string, object>(writeString, format, arg0, arg1, styleSheet);
        public static void WriteFormatted(string format, object arg0, object arg1, Color styledColor, Color defaultColor) => WriteThreeParamsWithColors<string, object>(writeString, format, arg0, arg1, styledColor, defaultColor);
        public static void WriteFormatted(string format, Formatter arg0, Formatter arg1, Color defaultColor) => WriteTwoFormattersWithColor<string>(writeString, format, arg0, arg1, defaultColor);

        // All corresponding WriteLine methods follow the same pattern...
    }

    // Helper classes for dynamic binding (refactored from generated classes)
    internal class AsyncConsoleTask
    {
        public IEnumerable<KeyValuePair<string, Color>> ColorMappings { get; set; }
        public string Suffix { get; set; }

        public async Task ExecuteAsync()
        {
            // Implementation for async console operations
            await Task.Run(() =>
            {
                ConsoleColor originalColor = System.Console.ForegroundColor;
                foreach (var mapping in ColorMappings)
                {
                    System.Console.ForegroundColor = Console.colorMapper.GetConsoleColor(mapping.Value);
                    System.Console.Write(mapping.Key);
                }
                System.Console.Write(Suffix);
                System.Console.ForegroundColor = originalColor;
            });
        }
    }
}