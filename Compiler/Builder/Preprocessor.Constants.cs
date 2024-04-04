namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using System.Globalization;
using Data;
using Analysis;

internal sealed partial class Preprocessor {
    /// <summary>
    /// Visits a constant.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The expression result if successful, null otherwise.</returns>
    public override ExpressionResult? VisitConstant(ConstantContext context) {
        return (ExpressionResult?)Visit(context);
    }

    /// <summary>
    /// Visits and stores an integer which is in the decimal format.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A constant result with an integer type if successful, null otherwise.</returns>
    public override ConstantResult? VisitDecimalLiteral(DecimalLiteralContext context) {
        // the string value of the number
        ReadOnlySpan<char> number = context.start.Text.AsSpan();

        // check if the number has a sign prefix
        bool hasNegativeSign = number[0] == '-';
        bool hasPositiveSign = number[0] == '+';

        // calculate the index of the first digit
        int prefixLength = hasNegativeSign || hasPositiveSign ? 1 : 0;

        // remove the prefix
        number = number[prefixLength..];

        // call the common method for storing numbers,
        // with the decimal format
        return StoreInteger(context, number, NumberStyles.Integer, hasNegativeSign);
    }

    /// <summary>
    /// Visits and stores an integer which is in the hexadecimal format.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A constant result with an integer type if successful, null otherwise.</returns>
    public override ConstantResult? VisitHexadecimalLiteral(HexadecimalLiteralContext context) {
        // the string value of the number
        ReadOnlySpan<char> number = context.start.Text.AsSpan();

        // check if the number has a sign prefix
        bool hasNegativeSign = number[0] == '-';
        bool hasPositiveSign = number[0] == '+';

        // calculate the index of the first digit
        int prefixLength = hasNegativeSign || hasPositiveSign ? 3 : 2;

        // remove the prefix
        number = number[prefixLength..];

        // call the common method for storing numbers,
        // with the hex format
        return StoreInteger(context, number, NumberStyles.HexNumber, hasNegativeSign);
    }

    /// <summary>
    /// Visits and stores an integer which is in the binary format.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A constant result with an integer type if successful, null otherwise.</returns>
    public override ConstantResult? VisitBinaryLiteral(BinaryLiteralContext context) {
        // the string value of the number
        ReadOnlySpan<char> number = context.start.Text.AsSpan();

        // check if the number has a sign prefix
        bool hasNegativeSign = number[0] == '-';
        bool hasPositiveSign = number[0] == '+';

        // calculate the index of the first digit
        int prefixLength = hasNegativeSign || hasPositiveSign ? 3 : 2;

        // remove the prefix
        number = number[prefixLength..];

        // call the common method for storing numbers,
        // with the binary format
        return StoreInteger(context, number, NumberStyles.BinaryNumber, hasNegativeSign);
    }

    /// <summary>
    /// Visits and stores a 16-bit float.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A constant result with the type of 16-bit float if successful, null otherwise.</returns>
    public override ConstantResult? VisitHalfFloat(HalfFloatContext context) {
        // the string value of the number
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        // remove the float suffix, if present
        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        // try to convert the string to a number
        bool success = Half.TryParse(text, out Half value);

        // stop and throw an error if the format is invalid
        if (!success) {
            IssueHandler.Add(Issue.InvalidFloatFormat(context));
            return null;
        }

        // store the number
        int address = DataHandler.F16.Add(value);

        return new ConstantResult(address, TypeHandler.CoreTypes.F16);
    }

    /// <summary>
    /// Visits and stores a 32-bit float.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A constant result with the type of 32-bit float if successful, null otherwise.</returns>
    public override ConstantResult? VisitSingleFloat(SingleFloatContext context) {
        // the string value of the number
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        // remove the float suffix, if present
        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        // try to convert the string to a number
        bool success = float.TryParse(text, out float value);

        // stop and throw an error if the format is invalid
        if (!success) {
            IssueHandler.Add(Issue.InvalidFloatFormat(context));
            return null;
        }

        // store the number
        int address = DataHandler.F32.Add(value);

        return new ConstantResult(address, TypeHandler.CoreTypes.F32);
    }

    /// <summary>
    /// Visits and stores a 64-bit float.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A constant result with the type of 64-bit float if successful, null otherwise.</returns>
    public override ConstantResult? VisitDoubleFloat(DoubleFloatContext context) {
        // the string value of the number
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        // remove the float suffix, if present
        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        // try to convert the string to a number
        bool success = double.TryParse(text, out double value);

        // stop and throw an error if the format is invalid
        if (!success) {
            IssueHandler.Add(Issue.InvalidFloatFormat(context));
            return null;
        }

        // store the number
        int address = DataHandler.F64.Add(value);

        return new ConstantResult(address, TypeHandler.CoreTypes.F64);
    }

    /// <summary>
    /// Visits, unescapes then stores a char literal.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A constant result with the type of char if successful, null otherwise.</returns>
    public override ConstantResult? VisitCharLiteral(CharLiteralContext context) {
        // the char value without quotes
        ReadOnlySpan<char> text = context.start.Text.AsSpan(1..^1);

        // try to get the first char
        char? result = TryGetFirstCharacter(context, ref text, false);

        // stop if an error occured
        // or the literal is longer that 1 characters
        if (text.Length > 0 || result is null) {
            IssueHandler.Add(Issue.InvalidCharFormat(context));
            return null;
        }

        // store the character as a 16-bit integer
        int address = DataHandler.I16.Add((short)result.Value);

        return new ConstantResult(address, TypeHandler.CoreTypes.Char);
    }

    /// <summary>
    /// Visits, unescapes then stores a string literal.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A constant result with the type of string if successful, null otherwise.</returns>
    public override ConstantResult? VisitStringLiteral(StringLiteralContext context) {
        // the string value without quotes
        ReadOnlySpan<char> text = context.start.Text.AsSpan(1..^1);

        // the resulting array of characters
        List<char> characters = [];

        // while there is a next character
        while (text.Length > 0) {
            // try to get the first char
            char? character = TryGetFirstCharacter(context, ref text, true);

            // stop if an error occured
            if (character is null) {
                return null;
            }

            // add the character to the end of the list
            characters.Add(character.Value);
        }

        // create a string from the character
        string result = string.Concat(characters);

        // store the string
        int address = DataHandler.Str.Add(result);

        return new ConstantResult(address, TypeHandler.CoreTypes.Str);
    }

    /// <summary>
    /// Visits and stores the true keyword.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A constant result with the type of bool.</returns>
    public override ConstantResult VisitTrueKeyword(TrueKeywordContext context) {
        // store the value
        int address = DataHandler.Bool.Add(true);

        return new ConstantResult(address, TypeHandler.CoreTypes.Bool);
    }

    /// <summary>
    /// Visits and stores the false keyword.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>A constant result with the type of bool.</returns>
    public override ConstantResult VisitFalseKeyword(FalseKeywordContext context) {
        // store the value
        int address = DataHandler.Bool.Add(false);

        return new ConstantResult(address, TypeHandler.CoreTypes.Bool);
    }

    /// <summary>
    /// Visits the null keyword.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>An expression result with the type of void.</returns>
    public override ExpressionResult VisitNullKeyword(NullKeywordContext context) {
        return new ExpressionResult(MemoryAddress.Null, TypeHandler.CoreTypes.Void);
    }
}