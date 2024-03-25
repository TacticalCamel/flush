namespace Compiler.Builder;

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
        string number = context.start.Text;

        bool hasNegativeSign = number[0] == '-';
        bool hasPositiveSign = number[0] == '+';

        return AddInteger(context, number.AsSpan(hasNegativeSign || hasPositiveSign ? 1 : 0), NumberStyles.Integer, hasNegativeSign);
    }

    public override ExpressionResult? VisitHexadecimalLiteral(HexadecimalLiteralContext context) {
        string number = context.start.Text;
        
        bool hasNegativeSign = number[0] == '-';
        bool hasPositiveSign = number[0] == '+';
        
        return AddInteger(context, number.AsSpan(hasNegativeSign || hasPositiveSign ? 3 : 2), NumberStyles.HexNumber, hasNegativeSign);
    }

    public override ExpressionResult? VisitBinaryLiteral(BinaryLiteralContext context) {
        string number = context.start.Text;
        
        bool hasNegativeSign = number[0] == '-';
        bool hasPositiveSign = number[0] == '+';
        
        return AddInteger(context, number.AsSpan(hasNegativeSign || hasPositiveSign ? 3 : 2), NumberStyles.BinaryNumber, hasNegativeSign);
    }

    public override ExpressionResult? VisitDoubleFloat(DoubleFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = double.TryParse(text, out double value);

        if (success) {
            return new ExpressionResult(DataHandler.F64.Add(value), TypeHandler.CoreTypes.F64);
        }

        IssueHandler.Add(Issue.InvalidFloatFormat(context));
        return null;
    }

    public override ExpressionResult? VisitSingleFloat(SingleFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = float.TryParse(text, out float value);

        if (success) {
            return new ExpressionResult(DataHandler.F32.Add(value), TypeHandler.CoreTypes.F32);
        }

        IssueHandler.Add(Issue.InvalidFloatFormat(context));
        return null;
    }

    public override ExpressionResult? VisitHalfFloat(HalfFloatContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan();

        if (char.IsAsciiLetter(text[^1])) {
            text = text[..^1];
        }

        bool success = Half.TryParse(text, out Half value);

        if (success) {
            return new ExpressionResult(DataHandler.F16.Add(value), TypeHandler.CoreTypes.F16);
        }

        IssueHandler.Add(Issue.InvalidFloatFormat(context));
        return null;
    }

    public override ExpressionResult? VisitCharLiteral(CharLiteralContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan(1..^1);

        char result = GetFirstCharacter(context, ref text, false);

        if (text.Length > 0) {
            IssueHandler.Add(Issue.InvalidCharFormat(context));
            return null;
        }

        MemoryAddress address = DataHandler.I16.Add((short)result);
        return new ExpressionResult(address, TypeHandler.CoreTypes.Char);
    }

    public override ExpressionResult VisitStringLiteral(StringLiteralContext context) {
        ReadOnlySpan<char> text = context.start.Text.AsSpan(1..^1);
        
        List<char> characters = [];

        while (text.Length > 0) {
            char c = GetFirstCharacter(context, ref text, true);

            characters.Add(c);
        }

        string result = string.Concat(characters);

        return new ExpressionResult(DataHandler.Str.Add(result), TypeHandler.CoreTypes.Str);
    }

    public override ExpressionResult VisitNullKeyword(NullKeywordContext context) {
        return new ExpressionResult(MemoryAddress.Null, TypeHandler.CoreTypes.Null);
    }

    public override ExpressionResult VisitTrueKeyword(TrueKeywordContext context) {
        return new ExpressionResult(DataHandler.Bool.Add(true), TypeHandler.CoreTypes.Bool);
    }

    public override ExpressionResult VisitFalseKeyword(FalseKeywordContext context) {
        return new ExpressionResult(DataHandler.Bool.Add(false), TypeHandler.CoreTypes.Bool);
    }

    private ExpressionResult? AddInteger(ConstantContext context, ReadOnlySpan<char> number, NumberStyles styles, bool isNegative) {
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
                return new ExpressionResult(DataHandler.I8.Add((sbyte)signedValue), TypeHandler.CoreTypes.I8);
            }
            
            if (signedValue >= short.MinValue) {
                return new ExpressionResult(DataHandler.I16.Add((short)signedValue), TypeHandler.CoreTypes.I16);
            }
            
            if (signedValue >= int.MinValue) {
                return new ExpressionResult(DataHandler.I32.Add((int)signedValue), TypeHandler.CoreTypes.I32);
            }
            
            if (signedValue >= long.MinValue) {
                return new ExpressionResult(DataHandler.I64.Add((long)signedValue), TypeHandler.CoreTypes.I64);
            }

            return new ExpressionResult(DataHandler.I128.Add(signedValue), TypeHandler.CoreTypes.I128);
        }
        
        // determine smallest unsigned type that can represent the value
        // also get the smallest signed type
        // for example: 100 -> u8 / i8, but 200 -> u8 / i16
        
        // later we might need this information
        // since "-100 + 100" and "-100 + 200" are both i8 + u8
        // and the common type is i8 for the first one, but i16 for the second one 
        
        if (value <= byte.MaxValue) {
            TypeIdentifier signedType = value <= (UInt128)sbyte.MaxValue ? TypeHandler.CoreTypes.I8 : TypeHandler.CoreTypes.I16;

            return new ExpressionResult(DataHandler.I8.Add((sbyte)value), TypeHandler.CoreTypes.U8, signedType);
        }
        
        if (value <= ushort.MaxValue) {
            TypeIdentifier signedType = value <= (UInt128)short.MaxValue ? TypeHandler.CoreTypes.I16 : TypeHandler.CoreTypes.I32;

            return new ExpressionResult(DataHandler.I16.Add((short)value), TypeHandler.CoreTypes.U16, signedType);
        }
        
        if (value <= uint.MaxValue) {
            TypeIdentifier signedType = value <= (UInt128)int.MaxValue ? TypeHandler.CoreTypes.I32 : TypeHandler.CoreTypes.I64;

            return new ExpressionResult(DataHandler.I32.Add((int)value), TypeHandler.CoreTypes.U32, signedType);
        }
        
        if (value <= ulong.MaxValue) {
            TypeIdentifier signedType = value <= (UInt128)long.MaxValue ? TypeHandler.CoreTypes.I64 : TypeHandler.CoreTypes.I128;

            return new ExpressionResult(DataHandler.I64.Add((long)value), TypeHandler.CoreTypes.U64, signedType);
        }

        {
            TypeIdentifier? signedType = value <= (UInt128)Int128.MaxValue ? TypeHandler.CoreTypes.I128 : null;
            
            return new ExpressionResult(DataHandler.I128.Add((Int128)value), TypeHandler.CoreTypes.U128, signedType);
        }
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
            IssueHandler.Add(Issue.UnclosedEscapeSequence(context));
            return '\0';
        }

        // get second character and modify span
        char second = characters[0];
        characters = characters[1..];

        // unicode escape sequence
        if (second is 'u' or 'U') {
            if (characters.Length < 4) {
                IssueHandler.Add(Issue.InvalidUnicodeEscape(context, 4));
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
        return '\0';
    }
}