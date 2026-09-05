using Plugin.Core.Enums;
using System;
using System.Globalization;

namespace Plugin.Core.Utility
{
    public class DateTimeUtil
    {
        public static DateTime Now()
        {
            try
            {
                DateTime BaseDT = DateTime.Now;
                return ConfigLoader.CustomYear ? BaseDT.AddYears(-ConfigLoader.BackYear) : BaseDT;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return new DateTime();
            }
        }

        public static string Now(string Format) => Now().ToString(Format, CultureInfo.InvariantCulture);

        public static DateTime Convert(string Now)
        {
            string[] Formats = new string[2]
            {
                "yyMMddHHmm",
                "yyMMdd"
            };

            try
            {
                // Log the input for debugging
                CLogger.Print($"Attempting to parse: '{Now}' (Length: {Now?.Length})", LoggerType.Debug);

                if (string.IsNullOrEmpty(Now) || Now.Length < 6)
                {
                    Now = "101010";
                    CLogger.Print($"Input too short, using default: '{Now}'", LoggerType.Debug);
                }

                // Try each format individually for better error reporting
                foreach (string format in Formats)
                {
                    if (Now.Length == format.Length)
                    {
                        try
                        {
                            DateTime result = DateTime.ParseExact(Now, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
                            CLogger.Print($"Successfully parsed '{Now}' using format '{format}'", LoggerType.Debug);
                            return result;
                        }
                        catch (FormatException ex)
                        {
                            CLogger.Print($"Failed to parse '{Now}' with format '{format}': {ex.Message}", LoggerType.Debug);
                        }
                    }
                }

                // If we get here, none of the formats worked
                CLogger.Print($"No format matched for input '{Now}'. Expected formats: {string.Join(", ", Formats)}", LoggerType.Error);
                return new DateTime();
            }
            catch (Exception Ex)
            {
                CLogger.Print($"Unexpected error in Convert method with input '{Now}': {Ex.Message}", LoggerType.Error, Ex);
                return new DateTime();
            }
        }

        // Alternative method with more flexible parsing
        public static DateTime ConvertFlexible(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return new DateTime();

            // Extended list of possible formats
            string[] formats = new string[]
            {
                "yyMMddHHmm",   // Original format 1
                "yyMMdd",       // Original format 2
                "yyyyMMddHHmm", // 4-digit year variants
                "yyyyMMdd",
                "yy-MM-dd",     // With separators
                "yyyy-MM-dd",
                "MM/dd/yy",
                "MM/dd/yyyy",
                "dd/MM/yy",
                "dd/MM/yyyy"
            };

            foreach (string format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }

            // Last resort: try general parsing
            if (DateTime.TryParse(dateString, out DateTime generalResult))
            {
                return generalResult;
            }

            CLogger.Print($"Could not parse date string: '{dateString}'", LoggerType.Error);
            return new DateTime();
        }

        public static DateTime Real(string Now)
        {
            try
            {
                DateTime BaseDT = Convert(Now);
                return ConfigLoader.CustomYear ? BaseDT.AddYears(ConfigLoader.BackYear) : BaseDT;
            }
            catch (Exception Ex)
            {
                CLogger.Print(Ex.Message, LoggerType.Error, Ex);
                return new DateTime();
            }
        }
    }
}