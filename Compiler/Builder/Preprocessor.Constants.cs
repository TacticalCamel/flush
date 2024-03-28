namespace Compiler.Builder;

using Data;
using Analysis;
using System.Globalization;
using static Grammar.ScrantonParser;

internal sealed partial class Preprocessor {
    public override ExpressionResult? VisitConstantExpression(ConstantExpressionContext context) {
        return VisitConstant(context.Constant);
    }

    public override ConstantResult? VisitConstant(ConstantContext context) {
        return (ConstantResult?)Visit(context);
    }

    public override ConstantResult? VisitDecimalLiteral(DecimalLiteralContext context) {
        string number = context.start.Text;

        bool hasNegativeSign = number[0] == '-';
        bool hasPositiveSign = number[0] == '+';

        return StoreInteger(context, number.AsSpan(hasNegativeSign || hasPositiveSign ? 1 : 0), NumberStyles.Integer, hasNegativeSign);
    }

    public override ConstantResult? VisitHexadecimalLiteral(HexadecimalLiteralContext context) {
        string number = context.start.Text;

        bool hasNegativeSign = number[0] == '-';
        bool hasPositiveSign = number[0] == '+';

        return StoreInteger(context, number.AsSpan(hasNegativeSign || hasPositiveSign ? 3 : 2), NumberStyles.HexNumber, hasNegativeSign);
    }

    public override ConstantResult? VisitBinaryLiteral(BinaryLiteralContext context) {
        string number = context.start.Text;

        bool hasNegativeSign = number[0] == '-';
        bool hasPositiveSign = number[0] == '+';

        return StoreInteger(context, number.AsSpan(hasNegativeSign || hasPositiveSign ? 3 : 2), NumberStyles.BinaryNumber, hasNegativeSign);
    }

    public override ConstantResult? VisitDoubleFloat(DoubleFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = double.TryParse(text, out double value);

        if (success) {
            return new ConstantResult(DataHandler.F64.Add(value), TypeHandler.CoreTypes.F64);
        }

        IssueHandler.Add(Issue.InvalidFloatFormat(context));
        return null;
    }

    public override ConstantResult? VisitSingleFloat(SingleFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = float.TryParse(text, out float value);

        if (success) {
            return new ConstantResult(DataHandler.F32.Add(value), TypeHandler.CoreTypes.F32);
        }

        IssueHandler.Add(Issue.InvalidFloatFormat(context));
        return null;
    }

    public override ConstantResult? VisitHalfFloat(HalfFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = Half.TryParse(text, out Half value);

        if (success) {
            return new ConstantResult(DataHandler.F16.Add(value), TypeHandler.CoreTypes.F16);
        }

        IssueHandler.Add(Issue.InvalidFloatFormat(context));
        return null;
    }

    public override ConstantResult? VisitCharLiteral(CharLiteralContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan(1..^1);

        char? result = GetFirstCharacter(context, ref text, false);

        if (text.Length > 0 || result is null) {
            IssueHandler.Add(Issue.InvalidCharFormat(context));
            return null;
        }

        return new ConstantResult(DataHandler.I16.Add((short)result.Value), TypeHandler.CoreTypes.Char);
    }

    public override ConstantResult? VisitStringLiteral(StringLiteralContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan(1..^1);

        List<char> characters = [];

        while (text.Length > 0) {
            char? character = GetFirstCharacter(context, ref text, true);

            if (character is null) {
                return null;
            }

            characters.Add(character.Value);
        }

        string result = string.Concat(characters);

        return new ConstantResult(DataHandler.Str.Add(result), TypeHandler.CoreTypes.Str);
    }

    public override ConstantResult VisitNullKeyword(NullKeywordContext context) {
        return new ConstantResult(MemoryAddress.Null, TypeHandler.CoreTypes.Null);
    }

    public override ConstantResult VisitTrueKeyword(TrueKeywordContext context) {
        return new ConstantResult(DataHandler.Bool.Add(true), TypeHandler.CoreTypes.Bool);
    }

    public override ConstantResult VisitFalseKeyword(FalseKeywordContext context) {
        return new ConstantResult(DataHandler.Bool.Add(false), TypeHandler.CoreTypes.Bool);
    }
}