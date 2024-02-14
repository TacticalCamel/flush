namespace ConsoleInterface.Options;

internal static class ParseFunctions {
    public static bool? ParseBool(string[] values) {
        return true;
    }

    public static LogLevel? ParseLogLevel(string[] values) {
        if (values.Length != 1) return null;

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

    public static int? ParseInteger(string[] values) {
        if (values.Length != 1) return null;

        return int.TryParse(values[0], out int x) ? x : null;
    }

    public static uint[]? ParseUnsignedArray(string[] values) {
        uint[] results = new uint[values.Length];

        for (int i = 0; i < values.Length; i++) {
            bool success = uint.TryParse(values[i], out uint x);
            if (!success) return null;
            results[i] = x;
        }

        return results;
    }

    public static string? ParseString(string[] values) {
        return values.Length == 1 ? values[0] : null;
    }

    public static string[] ParseStringArray(string[] values) {
        return values;
    }
}