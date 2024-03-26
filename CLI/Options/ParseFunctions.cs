// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

namespace CLI.Options;

/// <summary>
/// A class containing method definitions for command line argument parsing.
/// Methods must be public, static, have exactly 1 parameter which is string array,
/// and return the parsed type or its nullable version.
/// </summary>
internal static class ParseFunctions {
    /// <summary>
    /// Parse an array of strings as a bool.
    /// </summary>
    /// <param name="values">The input string array.</param>
    /// <returns>True if no values were provided, null otherwise.</returns>
    public static bool? ParseBool(string[] values) {
        return values.Length == 0 ? true : null;
    }

    /// <summary>
    /// Parse an array of strings as a log level.
    /// </summary>
    /// <param name="values">The input string array.</param>
    /// <returns>The log level if exactly 1 valid value was provided, null otherwise.</returns>
    public static LogLevel? ParseLogLevel(string[] values) {
        if (values.Length != 1) {
            return null;
        }

        return values[0] switch {
            "t" => LogLevel.Trace,
            "trace" => LogLevel.Trace,
            "d" => LogLevel.Debug,
            "debug" => LogLevel.Debug,
            "i" => LogLevel.Information,
            "information" => LogLevel.Information,
            "w" => LogLevel.Warning,
            "warning" => LogLevel.Warning,
            "e" => LogLevel.Error,
            "error" => LogLevel.Error,
            "c" => LogLevel.Critical,
            "critical" => LogLevel.Critical,
            "n" => LogLevel.None,
            "none" => LogLevel.None,
            _ => null
        };
    }

    /// <summary>
    /// Parse an array of strings as a signed 32-bit integer.
    /// </summary>
    /// <param name="values">The input string array.</param>
    /// <returns>The integer if exactly 1 valid value was provided, null otherwise.</returns>
    public static int? ParseInteger(string[] values) {
        if (values.Length != 1) {
            return null;
        }

        return int.TryParse(values[0], out int result) ? result : null;
    }

    /// <summary>
    /// Parse an array of strings as an array of unsigned 32-bit integers.
    /// </summary>
    /// <param name="values">The input string array.</param>
    /// <returns>An array of unsigned integers if every value was in the valid format, null otherwise.</returns>
    public static uint[]? ParseUnsignedArray(string[] values) {
        uint[] results = new uint[values.Length];

        for (int i = 0; i < values.Length; i++) {
            bool success = uint.TryParse(values[i], out uint x);
            if (!success) return null;
            results[i] = x;
        }

        return results;
    }

    /// <summary>
    /// Parse an array of strings as a string.
    /// </summary>
    /// <param name="values">The input string array.</param>
    /// <returns>The first element if exactly 1 value was provided, null otherwise.</returns>
    public static string? ParseString(string[] values) {
        return values.Length == 1 ? values[0] : null;
    }

    /// <summary>
    /// Parse an array of strings as an array of strings.
    /// Yes this is necessary.
    /// </summary>
    /// <param name="values">The input string array.</param>
    /// <returns>The input array without changes.</returns>
    public static string[] ParseStringArray(string[] values) {
        return values;
    }
}