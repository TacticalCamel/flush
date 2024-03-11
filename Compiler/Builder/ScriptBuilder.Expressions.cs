namespace Compiler.Builder;

using Analysis;
using Data;
using System.Globalization;
using System.Text.RegularExpressions;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override object? VisitExpression(ExpressionContext context) {
        // subtypes must be visited
        return Visit(context);
    }

    #region Constants

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
        string text = Regex.Unescape(context.start.Text[1..^1]);

        bool success = char.TryParse(text, out char value);

        if (success) {
            return DataHandler.Char.Add(value);
        }

        WarningHandler.Add(Warning.IncorrectCharFormat(context));
        return null;
    }

    public override object VisitStringLiteral(StringLiteralContext context) {
        string text = Regex.Unescape(context.start.Text[1..^1]);

        return DataHandler.Str.Add(text);
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

    #endregion

    public override object? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        // get operator name
        object? op = Visit(context.AdditiveOperator);

        // should never happen, but throw an error if it does
        if (op is not string methodName) {
            return null;
        }

        // TODO
        // find matching method

        /*object? left =*/ Visit(context.Left);
        /*object? right =*/ Visit(context.Right);
        
        // TODO
        // method call
        Console.WriteLine($"call {methodName}<{2}>");

        return null;
    }

    public override object? VisitOpAdditive(OpAdditiveContext context) {
        if (context.OP_PLUS() != null) {
            return "op_Addition";
        }

        if (context.OP_MINUS() != null) {
            return "op_Subtraction";
        }

        WarningHandler.Add(Warning.UnrecognizedOperator(context, context.start.Text));
        return null;
    }
}