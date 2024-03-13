namespace Compiler.Builder;

using Analysis;
using Data;
using System.Globalization;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override ExpressionResult? VisitConstantExpression(ConstantExpressionContext context) {
        return VisitConstant(context.Constant);
    }

    public override ExpressionResult? VisitConstant(ConstantContext context) {
        // subtypes must be visited
        return (ExpressionResult?)Visit(context);
    }

    public override ExpressionResult? VisitDecimalLiteral(DecimalLiteralContext context) {
        return AddInteger(context, context.start.Text, NumberStyles.Integer);
    }

    public override ExpressionResult? VisitHexadecimalLiteral(HexadecimalLiteralContext context) {
        return AddInteger(context, context.start.Text.AsSpan(2), NumberStyles.HexNumber);
    }

    public override ExpressionResult? VisitBinaryLiteral(BinaryLiteralContext context) {
        return AddInteger(context, context.start.Text.AsSpan(2), NumberStyles.BinaryNumber);
    }

    public override ExpressionResult? VisitDoubleFloat(DoubleFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = double.TryParse(text, out double value);

        if (success) {
            return new ExpressionResult(DataHandler.F64.Add(value), TypeHandler.GetFromType<Runtime.Core.F64>());
        }

        WarningHandler.Add(Warning.InvalidFloatFormat(context));
        return null;
    }

    public override ExpressionResult? VisitSingleFloat(SingleFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = float.TryParse(text, out float value);

        if (success) {
            return new ExpressionResult(DataHandler.F32.Add(value), TypeHandler.GetFromType<Runtime.Core.F32>());
        }

        WarningHandler.Add(Warning.InvalidFloatFormat(context));
        return null;
    }

    public override ExpressionResult? VisitHalfFloat(HalfFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = Half.TryParse(text, out Half value);

        if (success) {
            return new ExpressionResult(DataHandler.F16.Add(value), TypeHandler.GetFromType<Runtime.Core.F16>());
        }

        WarningHandler.Add(Warning.InvalidFloatFormat(context));
        return null;
    }

    public override ExpressionResult? VisitCharLiteral(CharLiteralContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan(1..^1);

        char result = GetFirstCharacter(context, ref text, false);

        if (text.Length > 0) {
            WarningHandler.Add(Warning.InvalidCharFormat(context));
            return null;
        }

        MemoryAddress address = DataHandler.Char.Add(result);
        return new ExpressionResult(address, TypeHandler.GetFromType<Runtime.Core.Char>());
    }

    public override ExpressionResult VisitStringLiteral(StringLiteralContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan(1..^1);

        List<char> characters = [];

        while (text.Length > 0) {
            char c = GetFirstCharacter(context, ref text, true);

            characters.Add(c);
        }

        string result = string.Concat(characters);

        return new ExpressionResult(DataHandler.Str.Add(result), TypeHandler.GetFromType<Runtime.Core.Str>());
    }

    public override ExpressionResult VisitNullKeyword(NullKeywordContext context) {
        return new ExpressionResult(MemoryAddress.NULL, null);
    }

    public override ExpressionResult VisitTrueKeyword(TrueKeywordContext context) {
        return new ExpressionResult(DataHandler.Bool.Add(true), TypeHandler.GetFromType<Runtime.Core.Bool>());
    }

    public override ExpressionResult VisitFalseKeyword(FalseKeywordContext context) {
        return new ExpressionResult(DataHandler.Bool.Add(false), TypeHandler.GetFromType<Runtime.Core.Bool>());
    }

    private ExpressionResult? AddInteger(ConstantContext context, ReadOnlySpan<char> number, NumberStyles styles) {
        bool isValid = long.TryParse(number, styles, CultureInfo.InvariantCulture, out long value);

        if (!isValid) {
            WarningHandler.Add(Warning.IntegerTooLarge(context));
            return null;
        }

        if (value is >= sbyte.MinValue and <= sbyte.MaxValue) {
            return new ExpressionResult(DataHandler.I8.Add((sbyte)value), TypeHandler.GetFromType<Runtime.Core.I8>());
        }

        if (value is >= short.MinValue and <= short.MaxValue) {
            return new ExpressionResult(DataHandler.I16.Add((short)value), TypeHandler.GetFromType<Runtime.Core.I16>());
        }

        if (value is >= int.MinValue and <= int.MaxValue) {
            return new ExpressionResult(DataHandler.I32.Add((int)value), TypeHandler.GetFromType<Runtime.Core.I32>());
        }

        return new ExpressionResult(DataHandler.I64.Add(value), TypeHandler.GetFromType<Runtime.Core.I64>());
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