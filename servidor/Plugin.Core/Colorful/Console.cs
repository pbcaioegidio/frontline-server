using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Core.Colorful
{
    /// <summary>
    /// Wrapper mejorado de System.Console con soporte para colores RGB y estilos avanzados
    /// </summary>
    public static class Console
    {
        #region Private Static Fields

        private static ColorStore _colorStore;
        private static ColorManagerFactory _colorManagerFactory;
        private static ColorManager _colorManager;
        private static Dictionary<string, COLORREF> _bufferColorMap;

        private const int MAX_GRADIENT_COLORS = 16;
        private const int COLOR_DISTANCE = 1;

        private static readonly string NEW_LINE = "\r\n";
        private static readonly string EMPTY_STRING = "";

        // Paleta de colores estándar
        private static readonly Color COLOR_BLACK = Color.FromArgb(0, 0, 0);
        private static readonly Color COLOR_BLUE = Color.FromArgb(0, 0, 255);
        private static readonly Color COLOR_CYAN = Color.FromArgb(0, 255, 255);
        private static readonly Color COLOR_DARK_BLUE = Color.FromArgb(0, 0, 128);
        private static readonly Color COLOR_DARK_CYAN = Color.FromArgb(0, 128, 128);
        private static readonly Color COLOR_DARK_GRAY = Color.FromArgb(128, 128, 128);
        private static readonly Color COLOR_DARK_GREEN = Color.FromArgb(0, 128, 0);
        private static readonly Color COLOR_DARK_MAGENTA = Color.FromArgb(128, 0, 128);
        private static readonly Color COLOR_DARK_RED = Color.FromArgb(128, 0, 0);
        private static readonly Color COLOR_DARK_YELLOW = Color.FromArgb(128, 128, 0);
        private static readonly Color COLOR_GRAY = Color.FromArgb(192, 192, 192);
        private static readonly Color COLOR_GREEN = Color.FromArgb(0, 255, 0);
        private static readonly Color COLOR_MAGENTA = Color.FromArgb(255, 0, 255);
        private static readonly Color COLOR_RED = Color.FromArgb(255, 0, 0);
        private static readonly Color COLOR_WHITE = Color.FromArgb(255, 255, 255);
        private static readonly Color COLOR_YELLOW = Color.FromArgb(255, 255, 0);

        #endregion

        #region Private Static Properties

        private static TaskQueue WriteQueue { get; } = new TaskQueue();

        #endregion

        #region Private Core Methods

        private static void WriteWithColorMap(IEnumerable<KeyValuePair<string, Color>> colorMap, string lineSuffix)
        {
            WriteQueue.Enqueue(new Func<Task>(new ColorMapWriteTask()
            {
                ColorMap = colorMap,
                LineSuffix = lineSuffix
            }.ExecuteAsync)).Wait();
        }

        
        private static void WriteStyledString(StyledString styledString, string lineSuffix)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            int rows = styledString.CharacterGeometry.GetLength(0);
            int cols = styledString.CharacterGeometry.GetLength(1);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    System.Console.ForegroundColor = _colorManager.GetConsoleColor(styledString.ColorGeometry[row, col]);

                    if (row == rows - 1 && col == cols - 1)
                        System.Console.Write(styledString.CharacterGeometry[row, col].ToString() + lineSuffix);
                    else if (col != cols - 1)
                        System.Console.Write(styledString.CharacterGeometry[row, col]);
                    else
                        System.Console.Write(styledString.CharacterGeometry[row, col].ToString() + "\r\n");
                }
            }

            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteColored<T>(Action<T> writeAction, T value, Color color)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = _colorManager.GetConsoleColor(color);
            writeAction(value);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteCharArrayColored(Action<string> writeAction, char[] buffer, int index, int count, Color color)
        {
            string text = new string(buffer).Substring(index, count);
            WriteColored<string>(writeAction, text, color);
        }

        private static void WriteAlternatingColor<T>(Action<T> writeAction, T value, ColorAlternator alternator)
        {
            Color nextColor = alternator.GetNextColor(value.ToString());
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = _colorManager.GetConsoleColor(nextColor);
            writeAction(value);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteCharArrayAlternating(Action<string> writeAction, char[] buffer, int index, int count, ColorAlternator alternator)
        {
            string text = new string(buffer).Substring(index, count);
            WriteAlternatingColor<string>(writeAction, text, alternator);
        }

        private static void WriteStyled<T>(string lineSuffix, T value, StyleSheet styleSheet)
        {
            WriteWithColorMap(new TextAnnotator(styleSheet).GetAnnotationMap(value.ToString()), lineSuffix);
        }

        private static void WriteStyledStringWithStyleSheet(string lineSuffix, StyledString styledString, StyleSheet styleSheet)
        {
            ApplyColorMapToStyledString(new TextAnnotator(styleSheet).GetAnnotationMap(styledString.AbstractValue), styledString);
            WriteStyledString(styledString, lineSuffix);
        }

        private static void ApplyColorMapToStyledString(IEnumerable<KeyValuePair<string, Color>> colorMap, StyledString styledString)
        {
            int charIndex = 0;

            foreach (var kvp in colorMap)
            {
                for (int i = 0; i < kvp.Key.Length; i++)
                {
                    int rows = styledString.CharacterIndexGeometry.GetLength(0);
                    int cols = styledString.CharacterIndexGeometry.GetLength(1);

                    for (int row = 0; row < rows; row++)
                    {
                        for (int col = 0; col < cols; col++)
                        {
                            if (styledString.CharacterIndexGeometry[row, col] == charIndex)
                                styledString.ColorGeometry[row, col] = kvp.Value;
                        }
                    }
                    charIndex++;
                }
            }
        }

        private static void WriteCharArrayStyled(string lineSuffix, char[] buffer, int index, int count, StyleSheet styleSheet)
        {
            string text = new string(buffer).Substring(index, count);
            WriteStyled<string>(lineSuffix, text, styleSheet);
        }

        private static void WriteColored2Args<T0, T1>(Action<T0, T1> writeAction, T0 format, T1 arg, Color color)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = _colorManager.GetConsoleColor(color);
            writeAction(format, arg);
            System.Console.ForegroundColor = originalColor;
        }

        
        private static void WriteAlternating2Args<T0, T1>(Action<T0, T1> writeAction, T0 format, T1 arg, ColorAlternator alternator)
        {
            string formattedText = string.Format(format.ToString(), arg);
            Color nextColor = alternator.GetNextColor(formattedText);
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = _colorManager.GetConsoleColor(nextColor);
            writeAction(format, arg);
            System.Console.ForegroundColor = originalColor;
        }

        
        private static void WriteStyled2Args<T0, T1>(string lineSuffix, T0 format, T1 arg, StyleSheet styleSheet)
        {
            TextAnnotator annotator = new TextAnnotator(styleSheet);
            string formattedText = string.Format(format.ToString(), arg);
            WriteWithColorMap(annotator.GetAnnotationMap(formattedText), lineSuffix);
        }

        
        private static void WriteFormatted2Args<T0, T1>(string lineSuffix, T0 format, T1 arg, Color styledColor, Color defaultColor)
        {
            TextFormatter formatter = new TextFormatter(defaultColor);
            //var formatMap = formatter.GetFormatMap(format.ToString(), arg, new Color[] { styledColor });
            var formatMap = formatter.GetFormatMap(
                format.ToString(),
                new object[] { arg }, // Convertir 'arg' a un array de objetos
                new Color[] { styledColor }
            );
            WriteWithColorMap(formatMap, lineSuffix);
        }

        private static void WriteFormattedWithSingleFormatter<T>(string lineSuffix, T format, Formatter formatter, Color defaultColor)
        {
            WriteWithColorMap(
                new TextFormatter(defaultColor).GetFormatMap(
                    format.ToString(),
                    new object[] { formatter.Target },
                    new Color[] { formatter.Color }
                ),
                lineSuffix
            );
        }

        private static void WriteColored3Args<T0, T1>(Action<T0, T1, T1> writeAction, T0 format, T1 arg0, T1 arg1, Color color)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = _colorManager.GetConsoleColor(color);
            writeAction(format, arg0, arg1);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteAlternating3Args<T0, T1>(Action<T0, T1, T1> writeAction, T0 format, T1 arg0, T1 arg1, ColorAlternator alternator)
        {
            string formattedText = string.Format(format.ToString(), arg0, arg1);
            Color nextColor = alternator.GetNextColor(formattedText);
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = _colorManager.GetConsoleColor(nextColor);
            writeAction(format, arg0, arg1);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteStyled3Args<T0, T1>(string lineSuffix, T0 format, T1 arg0, T1 arg1, StyleSheet styleSheet)
        {
            WriteWithColorMap(
                new TextAnnotator(styleSheet).GetAnnotationMap(string.Format(format.ToString(), arg0, arg1)),
                lineSuffix
            );
        }

        
        private static void WriteFormatted3Args<T0, T1>(string lineSuffix, T0 format, T1 arg0, T1 arg1, Color styledColor, Color defaultColor)
        {
            TextFormatter formatter = new TextFormatter(defaultColor);
            //var formatMap = formatter.GetFormatMap(format.ToString(), new T1[] { arg0, arg1 }, new Color[] { styledColor });
            var formatMap = formatter.GetFormatMap(
                format.ToString(),
                new object[] { arg0, arg1 }, // Convertir T1[] a object[]
                new Color[] { styledColor }
            );
            WriteWithColorMap(formatMap, lineSuffix);
        }

        private static void WriteFormattedWith2Formatters<T>(string lineSuffix, T format, Formatter formatter0, Formatter formatter1, Color defaultColor)
        {
            WriteWithColorMap(
                new TextFormatter(defaultColor).GetFormatMap(
                    format.ToString(),
                    new object[] { formatter0.Target, formatter1.Target },
                    new Color[] { formatter0.Color, formatter1.Color }
                ),
                lineSuffix
            );
        }

        private static void WriteColored4Args<T0, T1>(Action<T0, T1, T1, T1> writeAction, T0 format, T1 arg0, T1 arg1, T1 arg2, Color color)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = _colorManager.GetConsoleColor(color);
            writeAction(format, arg0, arg1, arg2);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteAlternating4Args<T0, T1>(Action<T0, T1, T1, T1> writeAction, T0 format, T1 arg0, T1 arg1, T1 arg2, ColorAlternator alternator)
        {
            string formattedText = string.Format(format.ToString(), arg0, arg1, arg2);
            Color nextColor = alternator.GetNextColor(formattedText);
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = _colorManager.GetConsoleColor(nextColor);
            writeAction(format, arg0, arg1, arg2);
            System.Console.ForegroundColor = originalColor;
        }

        private static void WriteStyled4Args<T0, T1>(string lineSuffix, T0 format, T1 arg0, T1 arg1, T1 arg2, StyleSheet styleSheet)
        {
            WriteWithColorMap(
                new TextAnnotator(styleSheet).GetAnnotationMap(string.Format(format.ToString(), arg0, arg1, arg2)),
                lineSuffix
            );
        }

        
        private static void WriteFormatted4Args<T0, T1>(string lineSuffix, T0 format, T1 arg0, T1 arg1, T1 arg2, Color styledColor, Color defaultColor)
        {
            TextFormatter formatter = new TextFormatter(defaultColor);
            //var formatMap = formatter.GetFormatMap(format.ToString(), new T1[] { arg0, arg1, arg2 }, new Color[] { styledColor });
            var formatMap = formatter.GetFormatMap(
                format.ToString(),
                new object[] { arg0, arg1, arg2 }, // Convertir T1[] a object[]
                new Color[] { styledColor }
            );
            WriteWithColorMap(formatMap, lineSuffix);
        }

        private static void WriteFormattedWith3Formatters<T>(string lineSuffix, T format, Formatter formatter0, Formatter formatter1, Formatter formatter2, Color defaultColor)
        {
            WriteWithColorMap(
                new TextFormatter(defaultColor).GetFormatMap(
                    format.ToString(),
                    new object[] { formatter0.Target, formatter1.Target, formatter2.Target },
                    new Color[] { formatter0.Color, formatter1.Color, formatter2.Color }
                ),
                lineSuffix
            );
        }

        private static void WriteFormattedWithFormatterArray<T>(string lineSuffix, T format, Formatter[] formatters, Color defaultColor)
        {
            WriteWithColorMap(
                new TextFormatter(defaultColor).GetFormatMap(
                    format.ToString(),
                    formatters.Select(f => f.Target).ToArray(),
                    formatters.Select(f => f.Color).ToArray()
                ),
                lineSuffix
            );
        }

        private static void WriteWithGradient<T>(Action<object, Color> writeAction, IEnumerable<T> input, Color startColor, Color endColor, int maxColorsInGradient)
        {
            foreach (var styleClass in new GradientGenerator().GenerateGradient<T>(input, startColor, endColor, maxColorsInGradient))
            {
                writeAction(styleClass.Target, styleClass.Color);
            }
        }

        private static Figlet CreateFiglet(FigletFont font = null)
        {
            return font == null ? new Figlet() : new Figlet(font);
        }

        private static ColorStore InitializeColorStore()
        {
            var colorToConsoleColor = new ConcurrentDictionary<Color, ConsoleColor>();
            var consoleColorToColor = new ConcurrentDictionary<ConsoleColor, Color>();

            consoleColorToColor.TryAdd(ConsoleColor.Black, COLOR_BLACK);
            consoleColorToColor.TryAdd(ConsoleColor.Blue, COLOR_BLUE);
            consoleColorToColor.TryAdd(ConsoleColor.Cyan, COLOR_CYAN);
            consoleColorToColor.TryAdd(ConsoleColor.DarkBlue, COLOR_DARK_BLUE);
            consoleColorToColor.TryAdd(ConsoleColor.DarkCyan, COLOR_DARK_CYAN);
            consoleColorToColor.TryAdd(ConsoleColor.DarkGray, COLOR_DARK_GRAY);
            consoleColorToColor.TryAdd(ConsoleColor.DarkGreen, COLOR_DARK_GREEN);
            consoleColorToColor.TryAdd(ConsoleColor.DarkMagenta, COLOR_DARK_MAGENTA);
            consoleColorToColor.TryAdd(ConsoleColor.DarkRed, COLOR_DARK_RED);
            consoleColorToColor.TryAdd(ConsoleColor.DarkYellow, COLOR_DARK_YELLOW);
            consoleColorToColor.TryAdd(ConsoleColor.Gray, COLOR_GRAY);
            consoleColorToColor.TryAdd(ConsoleColor.Green, COLOR_GREEN);
            consoleColorToColor.TryAdd(ConsoleColor.Magenta, COLOR_MAGENTA);
            consoleColorToColor.TryAdd(ConsoleColor.Red, COLOR_RED);
            consoleColorToColor.TryAdd(ConsoleColor.White, COLOR_WHITE);
            consoleColorToColor.TryAdd(ConsoleColor.Yellow, COLOR_YELLOW);

            return new ColorStore(colorToConsoleColor, consoleColorToColor);
        }

        private static void InitializeColorSystem(bool compatibilityMode)
        {
            _colorStore = InitializeColorStore();
            _colorManagerFactory = new ColorManagerFactory();
            _colorManager = _colorManagerFactory.GetManager(_colorStore, MAX_GRADIENT_COLORS, COLOR_DISTANCE, compatibilityMode);

            if (!_colorManager.IsInCompatibilityMode)
                new ColorMapper().SetBatchBufferColors(_bufferColorMap);
        }

        #endregion

        #region Public Properties

        public static Color BackgroundColor
        {
            get => _colorManager.GetColor(System.Console.BackgroundColor);
            set => System.Console.BackgroundColor = _colorManager.GetConsoleColor(value);
        }

        public static int BufferHeight
        {
            get => System.Console.BufferHeight;
            set => System.Console.BufferHeight = value;
        }

        public static int BufferWidth
        {
            get => System.Console.BufferWidth;
            set => System.Console.BufferWidth = value;
        }

        public static bool CapsLock => System.Console.CapsLock;

        public static int CursorLeft
        {
            get => System.Console.CursorLeft;
            set => System.Console.CursorLeft = value;
        }

        public static int CursorSize
        {
            get => System.Console.CursorSize;
            set => System.Console.CursorSize = value;
        }

        public static int CursorTop
        {
            get => System.Console.CursorTop;
            set => System.Console.CursorTop = value;
        }

        public static bool CursorVisible
        {
            get => System.Console.CursorVisible;
            set => System.Console.CursorVisible = value;
        }

        public static TextWriter Error => System.Console.Error;

        public static Color ForegroundColor
        {
            get => _colorManager.GetColor(System.Console.ForegroundColor);
            set => System.Console.ForegroundColor = _colorManager.GetConsoleColor(value);
        }

        public static TextReader In => System.Console.In;

        public static Encoding InputEncoding
        {
            get => System.Console.InputEncoding;
            set => System.Console.InputEncoding = value;
        }

        public static bool IsErrorRedirected => System.Console.IsErrorRedirected;
        public static bool IsInputRedirected => System.Console.IsInputRedirected;
        public static bool IsOutputRedirected => System.Console.IsOutputRedirected;
        public static bool KeyAvailable => System.Console.KeyAvailable;
        public static int LargestWindowHeight => System.Console.LargestWindowHeight;
        public static int LargestWindowWidth => System.Console.LargestWindowWidth;
        public static bool NumberLock => System.Console.NumberLock;
        public static TextWriter Out => System.Console.Out;

        public static Encoding OutputEncoding
        {
            get => System.Console.OutputEncoding;
            set => System.Console.OutputEncoding = value;
        }

        public static string Title
        {
            get => System.Console.Title;
            set => System.Console.Title = value;
        }

        public static bool TreatControlCAsInput
        {
            get => System.Console.TreatControlCAsInput;
            set => System.Console.TreatControlCAsInput = value;
        }

        public static int WindowHeight
        {
            get => System.Console.WindowHeight;
            set => System.Console.WindowHeight = value;
        }

        public static int WindowLeft
        {
            get => System.Console.WindowLeft;
            set => System.Console.WindowLeft = value;
        }

        public static int WindowTop
        {
            get => System.Console.WindowTop;
            set => System.Console.WindowTop = value;
        }

        public static int WindowWidth
        {
            get => System.Console.WindowWidth;
            set => System.Console.WindowWidth = value;
        }

        #endregion

        #region Events

        public static event ConsoleCancelEventHandler CancelKeyPress;

        #endregion

        #region Static Constructor

        
        static Console()
        {
            bool compatibilityMode = !OperatingSystem.IsWindows();

            if (!compatibilityMode)
            {
                try
                {
                    _bufferColorMap = new ColorMapper().GetBufferColors();
                }
                catch
                {
                    compatibilityMode = true;
                }
            }

            InitializeColorSystem(compatibilityMode);
            System.Console.CancelKeyPress += OnCancelKeyPress;
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            CancelKeyPress?.Invoke(sender, e);
        }

        #endregion

        #region Write Methods - bool

        public static void Write(bool value) => System.Console.Write(value);

        public static void Write(bool value, Color color)
        {
            WriteColored<bool>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(bool value, ColorAlternator alternator)
        {
            WriteAlternatingColor<bool>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(bool value, StyleSheet styleSheet)
        {
            WriteStyled<bool>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - char

        public static void Write(char value) => System.Console.Write(value);

        public static void Write(char value, Color color)
        {
            WriteColored<char>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(char value, ColorAlternator alternator)
        {
            WriteAlternatingColor<char>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(char value, StyleSheet styleSheet)
        {
            WriteStyled<char>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - char[]

        public static void Write(char[] value) => System.Console.Write(value);

        public static void Write(char[] value, Color color)
        {
            WriteColored<char[]>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(char[] value, ColorAlternator alternator)
        {
            WriteAlternatingColor<char[]>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(char[] value, StyleSheet styleSheet)
        {
            WriteStyled<char[]>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - decimal

        public static void Write(decimal value) => System.Console.Write(value);

        public static void Write(decimal value, Color color)
        {
            WriteColored<decimal>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(decimal value, ColorAlternator alternator)
        {
            WriteAlternatingColor<decimal>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(decimal value, StyleSheet styleSheet)
        {
            WriteStyled<decimal>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - double

        public static void Write(double value) => System.Console.Write(value);

        public static void Write(double value, Color color)
        {
            WriteColored<double>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(double value, ColorAlternator alternator)
        {
            WriteAlternatingColor<double>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(double value, StyleSheet styleSheet)
        {
            WriteStyled<double>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - float

        public static void Write(float value) => System.Console.Write(value);

        public static void Write(float value, Color color)
        {
            WriteColored<float>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(float value, ColorAlternator alternator)
        {
            WriteAlternatingColor<float>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(float value, StyleSheet styleSheet)
        {
            WriteStyled<float>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - int

        public static void Write(int value) => System.Console.Write(value);

        public static void Write(int value, Color color)
        {
            WriteColored<int>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(int value, ColorAlternator alternator)
        {
            WriteAlternatingColor<int>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(int value, StyleSheet styleSheet)
        {
            WriteStyled<int>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - long

        public static void Write(long value) => System.Console.Write(value);

        public static void Write(long value, Color color)
        {
            WriteColored<long>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(long value, ColorAlternator alternator)
        {
            WriteAlternatingColor<long>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(long value, StyleSheet styleSheet)
        {
            WriteStyled<long>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - object

        public static void Write(object value) => System.Console.Write(value);

        public static void Write(object value, Color color)
        {
            WriteColored<object>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(object value, ColorAlternator alternator)
        {
            WriteAlternatingColor<object>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(object value, StyleSheet styleSheet)
        {
            WriteStyled<object>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - string

        public static void Write(string value) => System.Console.Write(value);

        public static void Write(string value, Color color)
        {
            WriteColored<string>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(string value, ColorAlternator alternator)
        {
            WriteAlternatingColor<string>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(string value, StyleSheet styleSheet)
        {
            WriteStyled<string>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - uint

        public static void Write(uint value) => System.Console.Write(value);

        public static void Write(uint value, Color color)
        {
            WriteColored<uint>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(uint value, ColorAlternator alternator)
        {
            WriteAlternatingColor<uint>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(uint value, StyleSheet styleSheet)
        {
            WriteStyled<uint>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - ulong

        public static void Write(ulong value) => System.Console.Write(value);

        public static void Write(ulong value, Color color)
        {
            WriteColored<ulong>(System.Console.Write, value, color);
        }

        public static void WriteAlternating(ulong value, ColorAlternator alternator)
        {
            WriteAlternatingColor<ulong>(System.Console.Write, value, alternator);
        }

        public static void WriteStyled(ulong value, StyleSheet styleSheet)
        {
            WriteStyled<ulong>(EMPTY_STRING, value, styleSheet);
        }

        #endregion

        #region Write Methods - Format with 1 arg

        public static void Write(string format, object arg0) => System.Console.Write(format, arg0);

        public static void Write(string format, object arg0, Color color)
        {
            WriteColored2Args<string, object>(System.Console.Write, format, arg0, color);
        }

        public static void WriteAlternating(string format, object arg0, ColorAlternator alternator)
        {
            WriteAlternating2Args<string, object>(System.Console.Write, format, arg0, alternator);
        }

        public static void WriteStyled(string format, object arg0, StyleSheet styleSheet)
        {
            WriteStyled2Args<string, object>(EMPTY_STRING, format, arg0, styleSheet);
        }

        public static void WriteFormatted(string format, object arg0, Color styledColor, Color defaultColor)
        {
            WriteFormatted2Args<string, object>(EMPTY_STRING, format, arg0, styledColor, defaultColor);
        }

        public static void WriteFormatted(string format, Formatter arg0, Color defaultColor)
        {
            WriteFormattedWithSingleFormatter<string>(EMPTY_STRING, format, arg0, defaultColor);
        }

        #endregion

        #region Write Methods - Format with params

        public static void Write(string format, params object[] args) => System.Console.Write(format, args);

        public static void Write(string format, Color color, params object[] args)
        {
            WriteColored2Args<string, object[]>(System.Console.Write, format, args, color);
        }

        public static void WriteAlternating(string format, ColorAlternator alternator, params object[] args)
        {
            WriteAlternating2Args<string, object[]>(System.Console.Write, format, args, alternator);
        }

        public static void WriteStyled(StyleSheet styleSheet, string format, params object[] args)
        {
            WriteStyled2Args<string, object[]>(EMPTY_STRING, format, args, styleSheet);
        }

        public static void WriteFormatted(string format, Color styledColor, Color defaultColor, params object[] args)
        {
            WriteFormatted2Args<string, object[]>(EMPTY_STRING, format, args, styledColor, defaultColor);
        }

        public static void WriteFormatted(string format, Color defaultColor, params Formatter[] args)
        {
            WriteFormattedWithFormatterArray<string>(EMPTY_STRING, format, args, defaultColor);
        }

        #endregion

        #region Write Methods - char array with index

        public static void Write(char[] buffer, int index, int count)
        {
            System.Console.Write(buffer, index, count);
        }

        public static void Write(char[] buffer, int index, int count, Color color)
        {
            WriteCharArrayColored(System.Console.Write, buffer, index, count, color);
        }

        public static void WriteAlternating(char[] buffer, int index, int count, ColorAlternator alternator)
        {
            WriteCharArrayAlternating(System.Console.Write, buffer, index, count, alternator);
        }

        public static void WriteStyled(char[] buffer, int index, int count, StyleSheet styleSheet)
        {
            WriteCharArrayStyled(EMPTY_STRING, buffer, index, count, styleSheet);
        }

        #endregion

        #region Write Methods - Format with 2 args

        public static void Write(string format, object arg0, object arg1)
        {
            System.Console.Write(format, arg0, arg1);
        }

        public static void Write(string format, object arg0, object arg1, Color color)
        {
            WriteColored3Args<string, object>(System.Console.Write, format, arg0, arg1, color);
        }

        public static void WriteAlternating(string format, object arg0, object arg1, ColorAlternator alternator)
        {
            WriteAlternating3Args<string, object>(System.Console.Write, format, arg0, arg1, alternator);
        }

        public static void WriteStyled(string format, object arg0, object arg1, StyleSheet styleSheet)
        {
            WriteStyled3Args<string, object>(EMPTY_STRING, format, arg0, arg1, styleSheet);
        }

        public static void WriteFormatted(string format, object arg0, object arg1, Color styledColor, Color defaultColor)
        {
            WriteFormatted3Args<string, object>(EMPTY_STRING, format, arg0, arg1, styledColor, defaultColor);
        }

        public static void WriteFormatted(string format, Formatter arg0, Formatter arg1, Color defaultColor)
        {
            WriteFormattedWith2Formatters<string>(EMPTY_STRING, format, arg0, arg1, defaultColor);
        }

        #endregion

        #region Write Methods - Format with 3 args

        public static void Write(string format, object arg0, object arg1, object arg2)
        {
            System.Console.Write(format, arg0, arg1, arg2);
        }

        public static void Write(string format, object arg0, object arg1, object arg2, Color color)
        {
            WriteColored4Args<string, object>(System.Console.Write, format, arg0, arg1, arg2, color);
        }

        public static void WriteAlternating(string format, object arg0, object arg1, object arg2, ColorAlternator alternator)
        {
            WriteAlternating4Args<string, object>(System.Console.Write, format, arg0, arg1, arg2, alternator);
        }

        public static void WriteStyled(string format, object arg0, object arg1, object arg2, StyleSheet styleSheet)
        {
            WriteStyled4Args<string, object>(EMPTY_STRING, format, arg0, arg1, arg2, styleSheet);
        }

        public static void WriteFormatted(string format, object arg0, object arg1, object arg2, Color styledColor, Color defaultColor)
        {
            WriteFormatted4Args<string, object>(EMPTY_STRING, format, arg0, arg1, arg2, styledColor, defaultColor);
        }

        public static void WriteFormatted(string format, Formatter arg0, Formatter arg1, Formatter arg2, Color defaultColor)
        {
            WriteFormattedWith3Formatters<string>(EMPTY_STRING, format, arg0, arg1, arg2, defaultColor);
        }

        #endregion

        #region Write Methods - Format with 4 args

        public static void Write(string format, object arg0, object arg1, object arg2, object arg3)
        {
            System.Console.Write(format, new object[] { arg0, arg1, arg2, arg3 });
        }

        public static void Write(string format, object arg0, object arg1, object arg2, object arg3, Color color)
        {
            WriteColored2Args<string, object[]>(System.Console.Write, format, new object[] { arg0, arg1, arg2, arg3 }, color);
        }

        public static void WriteAlternating(string format, object arg0, object arg1, object arg2, object arg3, ColorAlternator alternator)
        {
            WriteAlternating2Args<string, object[]>(System.Console.Write, format, new object[] { arg0, arg1, arg2, arg3 }, alternator);
        }

        public static void WriteStyled(string format, object arg0, object arg1, object arg2, object arg3, StyleSheet styleSheet)
        {
            WriteStyled2Args<string, object[]>(EMPTY_STRING, format, new object[] { arg0, arg1, arg2, arg3 }, styleSheet);
        }

        public static void WriteFormatted(string format, object arg0, object arg1, object arg2, object arg3, Color styledColor, Color defaultColor)
        {
            WriteFormatted2Args<string, object[]>(EMPTY_STRING, format, new object[] { arg0, arg1, arg2, arg3 }, styledColor, defaultColor);
        }

        public static void WriteFormatted(string format, Formatter arg0, Formatter arg1, Formatter arg2, Formatter arg3, Color defaultColor)
        {
            WriteFormattedWithFormatterArray<string>(EMPTY_STRING, format, new Formatter[] { arg0, arg1, arg2, arg3 }, defaultColor);
        }

        #endregion

        #region WriteLine Methods - Basic

        public static void WriteLine() => System.Console.WriteLine();

        public static void WriteLineAlternating(ColorAlternator alternator)
        {
            WriteAlternatingColor<string>(System.Console.Write, NEW_LINE, alternator);
        }

        public static void WriteLineStyled(StyleSheet styleSheet)
        {
            WriteStyled<string>(EMPTY_STRING, NEW_LINE, styleSheet);
        }

        #endregion

        #region WriteLine Methods - bool

        public static void WriteLine(bool value) => System.Console.WriteLine(value);

        public static void WriteLine(bool value, Color color)
        {
            WriteColored<bool>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(bool value, ColorAlternator alternator)
        {
            WriteAlternatingColor<bool>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(bool value, StyleSheet styleSheet)
        {
            WriteStyled<bool>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - char

        public static void WriteLine(char value) => System.Console.WriteLine(value);

        public static void WriteLine(char value, Color color)
        {
            WriteColored<char>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(char value, ColorAlternator alternator)
        {
            WriteAlternatingColor<char>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(char value, StyleSheet styleSheet)
        {
            WriteStyled<char>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - char[]

        public static void WriteLine(char[] value) => System.Console.WriteLine(value);

        public static void WriteLine(char[] value, Color color)
        {
            WriteColored<char[]>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(char[] value, ColorAlternator alternator)
        {
            WriteAlternatingColor<char[]>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(char[] value, StyleSheet styleSheet)
        {
            WriteStyled<char[]>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - decimal

        public static void WriteLine(decimal value) => System.Console.WriteLine(value);

        public static void WriteLine(decimal value, Color color)
        {
            WriteColored<decimal>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(decimal value, ColorAlternator alternator)
        {
            WriteAlternatingColor<decimal>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(decimal value, StyleSheet styleSheet)
        {
            WriteStyled<decimal>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - double

        public static void WriteLine(double value) => System.Console.WriteLine(value);

        public static void WriteLine(double value, Color color)
        {
            WriteColored<double>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(double value, ColorAlternator alternator)
        {
            WriteAlternatingColor<double>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(double value, StyleSheet styleSheet)
        {
            WriteStyled<double>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - float

        public static void WriteLine(float value) => System.Console.WriteLine(value);

        public static void WriteLine(float value, Color color)
        {
            WriteColored<float>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(float value, ColorAlternator alternator)
        {
            WriteAlternatingColor<float>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(float value, StyleSheet styleSheet)
        {
            WriteStyled<float>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - int

        public static void WriteLine(int value) => System.Console.WriteLine(value);

        public static void WriteLine(int value, Color color)
        {
            WriteColored<int>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(int value, ColorAlternator alternator)
        {
            WriteAlternatingColor<int>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(int value, StyleSheet styleSheet)
        {
            WriteStyled<int>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - long

        public static void WriteLine(long value) => System.Console.WriteLine(value);

        public static void WriteLine(long value, Color color)
        {
            WriteColored<long>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(long value, ColorAlternator alternator)
        {
            WriteAlternatingColor<long>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(long value, StyleSheet styleSheet)
        {
            WriteStyled<long>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - object

        public static void WriteLine(object value) => System.Console.WriteLine(value);

        public static void WriteLine(object value, Color color)
        {
            WriteColored<object>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(object value, ColorAlternator alternator)
        {
            WriteAlternatingColor<object>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(StyledString value, StyleSheet styleSheet)
        {
            WriteStyledStringWithStyleSheet(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - string

        public static void WriteLine(string value, Color color)
        {
            WriteColored<string>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(string value, ColorAlternator alternator)
        {
            WriteAlternatingColor<string>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(string value, StyleSheet styleSheet)
        {
            WriteStyled<string>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - uint

        public static void WriteLine(uint value) => System.Console.WriteLine(value);

        public static void WriteLine(uint value, Color color)
        {
            WriteColored<uint>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(uint value, ColorAlternator alternator)
        {
            WriteAlternatingColor<uint>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(uint value, StyleSheet styleSheet)
        {
            WriteStyled<uint>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - ulong

        public static void WriteLine(ulong value) => System.Console.WriteLine(value);

        public static void WriteLine(ulong value, Color color)
        {
            WriteColored<ulong>(System.Console.WriteLine, value, color);
        }

        public static void WriteLineAlternating(ulong value, ColorAlternator alternator)
        {
            WriteAlternatingColor<ulong>(System.Console.WriteLine, value, alternator);
        }

        public static void WriteLineStyled(ulong value, StyleSheet styleSheet)
        {
            WriteStyled<ulong>(NEW_LINE, value, styleSheet);
        }

        #endregion

        #region WriteLine Methods - Format with 1 arg

        public static void WriteLine(string format, object arg0) => System.Console.WriteLine(format, arg0);

        public static void WriteLine(string format, object arg0, Color color)
        {
            WriteColored2Args<string, object>(System.Console.WriteLine, format, arg0, color);
        }

        public static void WriteLineAlternating(string format, object arg0, ColorAlternator alternator)
        {
            WriteAlternating2Args<string, object>(System.Console.WriteLine, format, arg0, alternator);
        }

        public static void WriteLineStyled(string format, object arg0, StyleSheet styleSheet)
        {
            WriteStyled2Args<string, object>(NEW_LINE, format, arg0, styleSheet);
        }

        public static void WriteLineFormatted(string format, object arg0, Color styledColor, Color defaultColor)
        {
            WriteFormatted2Args<string, object>(NEW_LINE, format, arg0, styledColor, defaultColor);
        }

        public static void WriteLineFormatted(string format, Formatter arg0, Color defaultColor)
        {
            WriteFormattedWithSingleFormatter<string>(NEW_LINE, format, arg0, defaultColor);
        }

        #endregion

        #region WriteLine Methods - Format with params

        public static void WriteLine(string format, params object[] args)
        {
            System.Console.WriteLine(format, args);
        }

        public static void WriteLine(string format, Color color, params object[] args)
        {
            WriteColored2Args<string, object[]>(System.Console.WriteLine, format, args, color);
        }

        public static void WriteLineAlternating(string format, ColorAlternator alternator, params object[] args)
        {
            WriteAlternating2Args<string, object[]>(System.Console.WriteLine, format, args, alternator);
        }

        public static void WriteLineStyled(StyleSheet styleSheet, string format, params object[] args)
        {
            WriteStyled2Args<string, object[]>(NEW_LINE, format, args, styleSheet);
        }

        public static void WriteLineFormatted(string format, Color styledColor, Color defaultColor, params object[] args)
        {
            WriteFormatted2Args<string, object[]>(NEW_LINE, format, args, styledColor, defaultColor);
        }

        public static void WriteLineFormatted(string format, Color defaultColor, params Formatter[] args)
        {
            WriteFormattedWithFormatterArray<string>(NEW_LINE, format, args, defaultColor);
        }

        #endregion

        #region WriteLine Methods - char array with index

        public static void WriteLine(char[] buffer, int index, int count)
        {
            System.Console.WriteLine(buffer, index, count);
        }

        public static void WriteLine(char[] buffer, int index, int count, Color color)
        {
            WriteCharArrayColored(System.Console.WriteLine, buffer, index, count, color);
        }

        public static void WriteLineAlternating(char[] buffer, int index, int count, ColorAlternator alternator)
        {
            WriteCharArrayAlternating(System.Console.WriteLine, buffer, index, count, alternator);
        }

        public static void WriteLineStyled(char[] buffer, int index, int count, StyleSheet styleSheet)
        {
            WriteCharArrayStyled(NEW_LINE, buffer, index, count, styleSheet);
        }

        #endregion

        #region WriteLine Methods - Format with 2 args

        public static void WriteLine(string format, object arg0, object arg1)
        {
            System.Console.WriteLine(format, arg0, arg1);
        }

        public static void WriteLine(string format, object arg0, object arg1, Color color)
        {
            WriteColored3Args<string, object>(System.Console.WriteLine, format, arg0, arg1, color);
        }

        public static void WriteLineAlternating(string format, object arg0, object arg1, ColorAlternator alternator)
        {
            WriteAlternating3Args<string, object>(System.Console.WriteLine, format, arg0, arg1, alternator);
        }

        public static void WriteLineStyled(string format, object arg0, object arg1, StyleSheet styleSheet)
        {
            WriteStyled3Args<string, object>(NEW_LINE, format, arg0, arg1, styleSheet);
        }

        public static void WriteLineFormatted(string format, object arg0, object arg1, Color styledColor, Color defaultColor)
        {
            WriteFormatted3Args<string, object>(NEW_LINE, format, arg0, arg1, styledColor, defaultColor);
        }

        public static void WriteLineFormatted(string format, Formatter arg0, Formatter arg1, Color defaultColor)
        {
            WriteFormattedWith2Formatters<string>(NEW_LINE, format, arg0, arg1, defaultColor);
        }

        #endregion

        #region WriteLine Methods - Format with 3 args

        public static void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            System.Console.WriteLine(format, arg0, arg1, arg2);
        }

        public static void WriteLine(string format, object arg0, object arg1, object arg2, Color color)
        {
            WriteColored4Args<string, object>(System.Console.WriteLine, format, arg0, arg1, arg2, color);
        }

        public static void WriteLineAlternating(string format, object arg0, object arg1, object arg2, ColorAlternator alternator)
        {
            WriteAlternating4Args<string, object>(System.Console.WriteLine, format, arg0, arg1, arg2, alternator);
        }

        public static void WriteLineStyled(string format, object arg0, object arg1, object arg2, StyleSheet styleSheet)
        {
            WriteStyled4Args<string, object>(NEW_LINE, format, arg0, arg1, arg2, styleSheet);
        }

        public static void WriteLineFormatted(string format, object arg0, object arg1, object arg2, Color styledColor, Color defaultColor)
        {
            WriteFormatted4Args<string, object>(NEW_LINE, format, arg0, arg1, arg2, styledColor, defaultColor);
        }

        public static void WriteLineFormatted(string format, Formatter arg0, Formatter arg1, Formatter arg2, Color defaultColor)
        {
            WriteFormattedWith3Formatters<string>(NEW_LINE, format, arg0, arg1, arg2, defaultColor);
        }

        #endregion

        #region WriteLine Methods - Format with 4 args

        public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3)
        {
            System.Console.WriteLine(format, new object[] { arg0, arg1, arg2, arg3 });
        }

        public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3, Color color)
        {
            WriteColored2Args<string, object[]>(System.Console.WriteLine, format, new object[] { arg0, arg1, arg2, arg3 }, color);
        }

        public static void WriteLineAlternating(string format, object arg0, object arg1, object arg2, object arg3, ColorAlternator alternator)
        {
            WriteAlternating2Args<string, object[]>(System.Console.WriteLine, format, new object[] { arg0, arg1, arg2, arg3 }, alternator);
        }

        public static void WriteLineStyled(string format, object arg0, object arg1, object arg2, object arg3, StyleSheet styleSheet)
        {
            WriteStyled2Args<string, object[]>(NEW_LINE, format, new object[] { arg0, arg1, arg2, arg3 }, styleSheet);
        }

        public static void WriteLineFormatted(string format, object arg0, object arg1, object arg2, object arg3, Color styledColor, Color defaultColor)
        {
            WriteFormatted2Args<string, object[]>(NEW_LINE, format, new object[] { arg0, arg1, arg2, arg3 }, styledColor, defaultColor);
        }

        public static void WriteLineFormatted(string format, Formatter arg0, Formatter arg1, Formatter arg2, Formatter arg3, Color defaultColor)
        {
            WriteFormattedWithFormatterArray<string>(NEW_LINE, format, new Formatter[] { arg0, arg1, arg2, arg3 }, defaultColor);
        }

        #endregion

        #region ASCII Art Methods

        public static void WriteAscii(string value) => WriteAscii(value, null);

        public static void WriteAscii(string value, FigletFont font)
        {
            System.Console.WriteLine(CreateFiglet(font).ToAscii(value).ConcreteValue);
        }

        public static void WriteAscii(string value, Color color)
        {
            WriteAscii(value, null, color);
        }

        public static void WriteAscii(string value, FigletFont font, Color color)
        {
            WriteLine(CreateFiglet(font).ToAscii(value).ConcreteValue, color);
        }

        public static void WriteAsciiAlternating(string value, ColorAlternator alternator)
        {
            WriteAsciiAlternating(value, null, alternator);
        }

        public static void WriteAsciiAlternating(string value, FigletFont font, ColorAlternator alternator)
        {
            string asciiArt = CreateFiglet(font).ToAscii(value).ConcreteValue;
            foreach (string line in asciiArt.Split('\n'))
                WriteLineAlternating(line, alternator);
        }

        public static void WriteAsciiStyled(string value, StyleSheet styleSheet)
        {
            WriteAsciiStyled(value, null, styleSheet);
        }

        public static void WriteAsciiStyled(string value, FigletFont font, StyleSheet styleSheet)
        {
            WriteLineStyled(CreateFiglet(font).ToAscii(value), styleSheet);
        }

        #endregion

        #region Gradient Methods

        public static void WriteWithGradient<T>(IEnumerable<T> input, Color startColor, Color endColor, int maxColorsInGradient = MAX_GRADIENT_COLORS)
        {
            WriteWithGradient(Write, input, startColor, endColor, maxColorsInGradient);
        }

        public static void WriteLineWithGradient<T>(IEnumerable<T> input, Color startColor, Color endColor, int maxColorsInGradient = MAX_GRADIENT_COLORS)
        {
            WriteWithGradient(WriteLine, input, startColor, endColor, maxColorsInGradient);
        }

        #endregion

        #region Standard Console Methods

        public static int Read() => System.Console.Read();
        public static ConsoleKeyInfo ReadKey() => System.Console.ReadKey();
        public static ConsoleKeyInfo ReadKey(bool intercept) => System.Console.ReadKey(intercept);
        public static string ReadLine() => System.Console.ReadLine();
        public static void ResetColor() => System.Console.ResetColor();
        public static void SetBufferSize(int width, int height) => System.Console.SetBufferSize(width, height);
        public static void SetCursorPosition(int left, int top) => System.Console.SetCursorPosition(left, top);
        public static void SetError(TextWriter newError) => System.Console.SetError(newError);
        public static void SetIn(TextReader newIn) => System.Console.SetIn(newIn);
        public static void SetOut(TextWriter newOut) => System.Console.SetOut(newOut);
        public static void SetWindowPosition(int left, int top) => System.Console.SetWindowPosition(left, top);
        public static void SetWindowSize(int width, int height) => System.Console.SetWindowSize(width, height);
        public static Stream OpenStandardError() => System.Console.OpenStandardError();
        public static Stream OpenStandardError(int bufferSize) => System.Console.OpenStandardError(bufferSize);
        public static Stream OpenStandardInput() => System.Console.OpenStandardInput();
        public static Stream OpenStandardInput(int bufferSize) => System.Console.OpenStandardInput(bufferSize);
        public static Stream OpenStandardOutput() => System.Console.OpenStandardOutput();
        public static Stream OpenStandardOutput(int bufferSize) => System.Console.OpenStandardOutput(bufferSize);

        public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
        {
            System.Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);
        }

        public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
        {
            System.Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, sourceChar, sourceForeColor, sourceBackColor);
        }

        public static void Clear() => System.Console.Clear();

        #endregion

        #region Color Management

        public static void ReplaceAllColorsWithDefaults()
        {
            _colorStore = InitializeColorStore();
            _colorManagerFactory = new ColorManagerFactory();
            _colorManager = _colorManagerFactory.GetManager(_colorStore, MAX_GRADIENT_COLORS, COLOR_DISTANCE, _colorManager.IsInCompatibilityMode);

            if (!_colorManager.IsInCompatibilityMode)
                new ColorMapper().SetBatchBufferColors(_bufferColorMap);
        }

        public static void ReplaceColor(Color oldColor, Color newColor)
        {
            _colorManager.ReplaceColor(oldColor, newColor);
        }

        public static void Beep(int frequency, int duration) => System.Console.Beep(frequency, duration);

        #endregion

        #region Helper Classes

        /// <summary>
        /// Clase auxiliar generada por el compilador para escritura asíncrona con mapeo de colores
        /// </summary>
        private class ColorMapWriteTask
        {
            public IEnumerable<KeyValuePair<string, Color>> ColorMap;
            public string LineSuffix;

            public async Task ExecuteAsync()
            {
                await Task.CompletedTask;
            }
        }

        #endregion
    }
}