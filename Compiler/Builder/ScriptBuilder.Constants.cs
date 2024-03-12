namespace Compiler.Builder;

using Analysis;
using Data;
using System.Globalization;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override object? VisitConstantExpression(ConstantExpressionContext context) {
        return Visit(context.Constant);
    }

    public override object? VisitConstant(ConstantContext context) {
        // subtypes must be visited
        return Visit(context);
    }

    public override object? VisitDecimalLiteral(DecimalLiteralContext context) {
        return AddInteger(context, context.start.Text, NumberStyles.Integer);
    }

    public override object? VisitHexadecimalLiteral(HexadecimalLiteralContext context) {
        return AddInteger(context, context.start.Text.AsSpan(2), NumberStyles.HexNumber);
    }

    public override object? VisitBinaryLiteral(BinaryLiteralContext context) {
        return AddInteger(context, context.start.Text.AsSpan(2), NumberStyles.BinaryNumber);
    }

    public override object? VisitDoubleFloat(DoubleFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = double.TryParse(text, out double value);

        if (success) {
            return DataHandler.F64.Add(value);
        }

        WarningHandler.Add(Warning.InvalidFloatFormat(context));
        return null;
    }

    public override object? VisitSingleFloat(SingleFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = float.TryParse(text, out float value);

        if (success) {
            return DataHandler.F32.Add(value);
        }

        WarningHandler.Add(Warning.InvalidFloatFormat(context));
        return null;
    }

    public override object? VisitHalfFloat(HalfFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = Half.TryParse(text, out Half value);

        if (success) {
            return DataHandler.F16.Add(value);
        }

        WarningHandler.Add(Warning.InvalidFloatFormat(context));
        return null;
    }

    public override object? VisitCharLiteral(CharLiteralContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan(1..^1);

        char result = GetFirstCharacter(context, ref text, false);

        if (text.Length > 0) {
            WarningHandler.Add(Warning.InvalidCharFormat(context));
            return null;
        }

        return result;
    }

    public override object VisitStringLiteral(StringLiteralContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan(1..^1);

        List<char> characters = [];

        while (text.Length > 0) {
            char c = GetFirstCharacter(context, ref text, true);

            characters.Add(c);
        }

        string result = string.Concat(characters);

        return DataHandler.Str.Add(result);
    }

    public override object VisitNullKeyword(NullKeywordContext context) {
        return MemoryAddress.NULL;
    }

    public override object VisitTrueKeyword(TrueKeywordContext context) {
        return DataHandler.Bool.Add(true);
    }

    public override object VisitFalseKeyword(FalseKeywordContext context) {
        return DataHandler.Bool.Add(false);
    }

    private MemoryAddress? AddInteger(ConstantContext context, ReadOnlySpan<char> number, NumberStyles styles) {
        bool isValid = long.TryParse(number, styles, CultureInfo.InvariantCulture, out long value);

        if (!isValid) {
            WarningHandler.Add(Warning.IntegerTooLarge(context));
            return null;
        }

        if (value is >= sbyte.MinValue and <= sbyte.MaxValue) {
            return DataHandler.I8.Add((sbyte)value);
        }

        if (value is >= short.MinValue and <= short.MaxValue) {
            return DataHandler.I16.Add((short)value);
        }

        if (value is >= int.MinValue and <= int.MaxValue) {
            return DataHandler.I32.Add((int)value);
        }

        return DataHandler.I64.Add(value);
    }

    private char GetFirstCharacter(ConstantContext context, ref ReadOnlySpan<char> characters, bool inString) {
        // must contain at least 1 character
        if (characters.Length == 0) {
            return '\0';
        }

        // get first character and modify span
        char first = characters[0];
        characters = characters[1..];


        // not an escape sequence, simply return the character
        if (first != '\\') {
            return first;
        }

        // no character is escaped
        if (characters.Length == 0) {
            WarningHandler.Add(Warning.UnclosedEscapeSequence(context));
            return '\0';
        }

        // get second character and modify span
        char second = characters[0];
        characters = characters[1..];

        // unicode escape sequence
        if (second is 'u' or 'U') {
            if (characters.Length < 4) {
                WarningHandler.Add(Warning.InvalidUnicodeEscape(context, 4));
                return '\0';
            }

            // must be exactly 4 hexadecimal digits
            ReadOnlySpan<char> number = characters[..4];

            // modify span
            characters = characters[4..];

            ushort unicode = ushort.Parse(number, NumberStyles.HexNumber);

            return (char)unicode;
        }

        // other escape sequence
        char? result = second switch {
            'b' => '\b',
            'f' => '\f',
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            '\\' => '\\',
            '\'' => inString ? null : second,
            '"' => inString ? second : null,
            _ => null
        };

        // valid escape sequence
        if (result is not null) {
            return result.Value;
        }

        // invalid escape sequence
        WarningHandler.Add(Warning.UnknownEscapeSequence(context, second));
        return '\0';
    }
}