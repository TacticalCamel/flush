namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using System.Globalization;
using Interpreter.Bytecode;
using Handlers;
using Data;
using Grammar;
using Analysis;

/// <summary>
/// Implements the first pass traversal of the AST. This loads types, methods and
/// determines the actual types of expressions, but not yet emits any instructions.
/// </summary>
/// <remarks>
/// Information will need to be accessible at the time of the second pass,
/// so data is stored directly in the nodes instead of return values.
/// </remarks>
/// <param name="issueHandler">The issue handler to use.</param>
/// <param name="typeHandler">The type handler to use.</param>
/// <param name="dataHandler">The data handler to use.</param>
internal sealed partial class Preprocessor(IssueHandler issueHandler, TypeHandler typeHandler, DataHandler dataHandler) : ScrantonBaseVisitor<object?> {
    private IssueHandler IssueHandler { get; } = issueHandler;
    private TypeHandler TypeHandler { get; } = typeHandler;
    private DataHandler DataHandler { get; } = dataHandler;

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

    /// <summary>
    /// TODO comment
    /// </summary>
    /// <param name="context">The binary expression. Used for error messages.</param>
    /// <param name="leftContext">The expression of the left side.</param>
    /// <param name="rightContext">The expression of the right side.</param>
    /// <param name="operatorType">The identifier of the operator between the operands.</param>
    /// <exception cref="ArgumentException">The provided operator type is not valid for a binary expression.</exception>
     private void ResolveBinaryExpression(ExpressionContext context, ExpressionContext leftContext, ExpressionContext rightContext, int operatorType) {
    /*
        // resolve left and right side
        VisitExpression(leftContext);
        VisitExpression(rightContext);

        // stop if an error occured
        if (leftContext.ExpressionType is not { } left || rightContext.ExpressionType is not { } right) {
            return;
        }

        bool isLeftPrimitive = TypeHandler.PrimitiveConversions.IsPrimitiveType(left);
        bool isRightPrimitive = TypeHandler.PrimitiveConversions.IsPrimitiveType(right);

        // true if both expression types are primitive types
        // in this case we can use a simple instruction instead of a function call
        bool isPrimitiveOperation = isLeftPrimitive && isRightPrimitive;

        // calculate results
        TypeIdentifier? result = operatorType switch {
            // same as addition
            OP_PLUS => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_MINUS => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_MULTIPLY => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_DIVIDE => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_MODULUS => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_EQ => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_NOT_EQ => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_LESS => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_GREATER => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_LESS_EQ => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_GREATER_EQ => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_AND => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_OR => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_SHIFT_LEFT => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_SHIFT_RIGHT => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            // can have different type
            OP_ASSIGN => null,
            OP_MULTIPLY_ASSIGN => null,
            OP_DIVIDE_ASSIGN => null,
            OP_MODULUS_ASSIGN => null,
            OP_PLUS_ASSIGN => null,
            OP_MINUS_ASSIGN => null,
            OP_SHIFT_LEFT_ASSIGN => null,
            OP_SHIFT_RIGHT_ASSIGN => null,
            OP_AND_ASSIGN => null,
            OP_OR_ASSIGN => null,
            _ => throw new ArgumentException($"Method cannot handle the provided operator type {operatorType}")
        };

        // cancel processing if an error occured
        if (result is null) {
            IssueHandler.Add(Issue.InvalidBinaryOperation(context, left.Type, right.Type, DefaultVocabulary.GetDisplayName(operatorType)));
            return;
        }
    */
    }

    // TODO whatever is happening here

/*
    private TypeIdentifier? GetCommonPrimitiveType(ExpressionContext leftContext, ExpressionContext rightContext, ExpressionResult left, ExpressionResult right) {
        PrimitiveCast cast = GetBestCast(left.Type, right.Type, out TypeIdentifier? resultType);

        bool isImplicit = IsImplicitCast(cast);

        if (resultType is null) {
            return null;
        }

        if (!isImplicit) {
            return null;
        }

        Console.WriteLine($"{leftContext.GetText()}:[{left.Type}] + {rightContext.GetText()}:[{right.Type}] -> [{resultType}]");

        // cast left
        if (left.Type != resultType) {
            Instruction? i = GetCastInstruction(left.Type, resultType);

            if (i is not null) {
                Console.WriteLine($"    casting left [{left.Type}] -> [{resultType}]");
                leftContext.EmitAfterVisit.Add(i.Value);
                leftContext.ExpressionType = resultType;
            }
        }

        // cast right
        else if (right.Type != resultType) {
            Instruction? i = GetCastInstruction(right.Type, resultType);

            if (i is not null) {
                Console.WriteLine($"    casting right [{right.Type}] -> [{resultType}]");
                rightContext.EmitAfterVisit.Add(i.Value);
                rightContext.ExpressionType = resultType;
            }
        }

        return resultType;

        Instruction? GetCastInstruction(TypeIdentifier source, TypeIdentifier destination) {
            if (cast == PrimitiveCast.NotRequired) {
                return null;
            }

            if (cast == PrimitiveCast.ResizeImplicit) {
                int difference = destination.Size - source.Size;

                if (difference > 0) {
                    return new Instruction { Code = OperationCode.pshz, Size = difference };
                }

                return new Instruction { Code = OperationCode.pop, Size = -difference };
            }

            return null;
        }
    }

    private PrimitiveCast GetBestCast(TypeIdentifier sourceType, TypeIdentifier targetType, out TypeIdentifier? res) {
        // cast left side to right side
        PrimitiveCast castLeft = TypeHandler.PrimitiveConversions.GetCast(sourceType, targetType);

        // cast right side to left side
        PrimitiveCast castRight = TypeHandler.PrimitiveConversions.GetCast(targetType, sourceType);

        // if casting the right side is easier,
        // the type of the left side will be the result 
        if (castLeft < castRight) {
            res = sourceType;
            return castRight;
        }

        // if casting the left side is harder but possible,
        // the type of the right side will be the result 
        if (PrimitiveCast.None < castLeft) {
            res = targetType;
            return castLeft;
        }

        // casting is not possible
        res = null;
        return PrimitiveCast.None;
    }

    private static bool IsImplicitCast(PrimitiveCast cast) {
        return cast switch {
            PrimitiveCast.FloatToFloatImplicit => true,
            PrimitiveCast.UnsignedToFloatImplicit => true,
            PrimitiveCast.SignedToFloatImplicit => true,
            PrimitiveCast.ResizeImplicit => true,
            PrimitiveCast.NotRequired => true,
            _ => false
        };
    }*/
}