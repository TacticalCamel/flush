namespace Compiler.Builder;

using static Grammar.FlushParser;
using System.Globalization;
using Data;
using Analysis;

// ScriptBuilder.Constants: methods related to visiting constants
internal sealed partial class ScriptBuilder {
    /// <summary>
    /// Visits a constant.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The expression result if successful, null otherwise.</returns>
    public override ConstantResult? VisitConstant(ConstantContext context) {
        return (ConstantResult?)Visit(context);
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
    /// Convert a span of characters into an integer and store it in the smallest possible byte width.
    /// </summary>
    /// <param name="context">The context of the number node.</param>
    /// <param name="number">The string representation of the number.</param>
    /// <param name="styles">The style of the number.</param>
    /// <param name="isNegative">True if the number is negative, false otherwise.</param>
    /// <returns>The stored result if successful, null otherwise.</returns>
    private ConstantResult? StoreInteger(ConstantContext context, ReadOnlySpan<char> number, NumberStyles styles, bool isNegative) {
        // convert value and check if too large in absolute value
        bool isValid = UInt128.TryParse(number, styles, null, out UInt128 value);

        // check if it's a too large negative number 
        if (isNegative) {
            isValid &= value <= (UInt128)(-Int128.MinValue);
        }

        // return if failed
        if (!isValid) {
            IssueHandler.Add(Issue.IntegerTooLarge(context));
            return null;
        }

        // determine smallest signed type that can store the value
        if (isNegative) {
            Int128 signedValue = -(Int128)value;

            if (signedValue >= sbyte.MinValue) {
                return new ConstantResult(DataHandler.I8.Add((sbyte)signedValue), TypeHandler.CoreTypes.I8);
            }

            if (signedValue >= short.MinValue) {
                return new ConstantResult(DataHandler.I16.Add((short)signedValue), TypeHandler.CoreTypes.I16);
            }

            if (signedValue >= int.MinValue) {
                return new ConstantResult(DataHandler.I32.Add((int)signedValue), TypeHandler.CoreTypes.I32);
            }

            if (signedValue >= long.MinValue) {
                return new ConstantResult(DataHandler.I64.Add((long)signedValue), TypeHandler.CoreTypes.I64);
            }

            return new ConstantResult(DataHandler.I128.Add(signedValue), TypeHandler.CoreTypes.I128);
        }

        // determine smallest unsigned type that can represent the value
        // also get the smallest signed type
        // for example: 100 -> u8 / i8, but 200 -> u8 / i16

        // later we might need this information
        // since "-100 + 100" and "-100 + 200" are both i8 + u8
        // and the common type is i8 for the first one, but i16 for the second one 

        if (value <= byte.MaxValue) {
            TypeIdentifier signedType = value <= (UInt128)sbyte.MaxValue ? TypeHandler.CoreTypes.I8 : TypeHandler.CoreTypes.I16;

            return new ConstantResult(DataHandler.I8.Add((sbyte)value), TypeHandler.CoreTypes.U8, signedType);
        }

        if (value <= ushort.MaxValue) {
            TypeIdentifier signedType = value <= (UInt128)short.MaxValue ? TypeHandler.CoreTypes.I16 : TypeHandler.CoreTypes.I32;

            return new ConstantResult(DataHandler.I16.Add((short)value), TypeHandler.CoreTypes.U16, signedType);
        }

        if (value <= uint.MaxValue) {
            TypeIdentifier signedType = value <= (UInt128)int.MaxValue ? TypeHandler.CoreTypes.I32 : TypeHandler.CoreTypes.I64;

            return new ConstantResult(DataHandler.I32.Add((int)value), TypeHandler.CoreTypes.U32, signedType);
        }

        if (value <= ulong.MaxValue) {
            TypeIdentifier signedType = value <= (UInt128)long.MaxValue ? TypeHandler.CoreTypes.I64 : TypeHandler.CoreTypes.I128;

            return new ConstantResult(DataHandler.I64.Add((long)value), TypeHandler.CoreTypes.U64, signedType);
        }

        {
            TypeIdentifier? signedType = value <= (UInt128)Int128.MaxValue ? TypeHandler.CoreTypes.I128 : null;

            return new ConstantResult(DataHandler.I128.Add((Int128)value), TypeHandler.CoreTypes.U128, signedType);
        }
    }

    /// <summary>
    /// Remove and return the first unescaped character from a span.
    /// </summary>
    /// <param name="context">The context of the node which contains the characters. Used for error messages.</param>
    /// <param name="characters">The span containing the characters.</param>
    /// <param name="inString">True if the span represents a string, false if it's a char.</param>
    /// <returns>The first unescaped character of the span if successful, null otherwise.</returns>
    private char? TryGetFirstCharacter(ConstantContext context, ref ReadOnlySpan<char> characters, bool inString) {
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
            IssueHandler.Add(Issue.UnclosedEscapeSequence(context));
            return null;
        }

        // get second character and modify span
        char second = characters[0];
        characters = characters[1..];

        // unicode escape sequence
        if (second is 'u' or 'U') {
            if (characters.Length < 4) {
                IssueHandler.Add(Issue.InvalidUnicodeEscape(context, 4));
                return null;
            }

            // must be exactly 4 hexadecimal digits
            ReadOnlySpan<char> number = characters[..4];

            // modify span
            characters = characters[4..];

            ushort unicode = ushort.Parse(number, NumberStyles.HexNumber);

            return (char)unicode;
        }

        // other escape sequence
        // do not allow single quote escapes in strings and double quote escapes in chars
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
        IssueHandler.Add(Issue.UnknownEscapeSequence(context, second));
        return null;
    }
}