namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using System.Globalization;
using Handlers;
using Data;
using Grammar;
using Analysis;

/// <summary>
/// Implements the first pass traversal of the syntax tree with the visitor pattern.
/// This loads types, methods and determines the actual types of expressions,
/// but not yet emits any instructions.
/// </summary>
/// <remarks>
/// Information will need to be accessible at the time of the second pass,
/// so data is stored directly in the nodes instead of return values.
/// </remarks>
/// <param name="issueHandler">The issue handler to use.</param>
/// <param name="typeHandler">The type handler to use.</param>
/// <param name="dataHandler">The data handler to use.</param>
internal sealed partial class Preprocessor(IssueHandler issueHandler, TypeHandler typeHandler, DataHandler dataHandler) : ScrantonBaseVisitor<object?> {
    /// <summary>
    /// The issue handler to use.
    /// </summary>
    private IssueHandler IssueHandler { get; } = issueHandler;

    /// <summary>
    /// The type handler to use.
    /// </summary>
    private TypeHandler TypeHandler { get; } = typeHandler;

    /// <summary>
    /// The data handler to use.
    /// </summary>
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
    /// Resolves a unary expression.
    /// </summary>
    /// <param name="context">The unary expression.</param>
    /// <param name="innerContext">The inner expression.</param>
    /// <param name="operatorType">The identifier of the unary operator.</param>
    private void ResolveUnaryExpression(ExpressionContext context, ExpressionContext innerContext, int operatorType) {
        // resolve the inner expression
        VisitExpression(innerContext);

        // stop if an error occured
        if (innerContext.OriginalType is null) {
            return;
        }

        // unary operators do not change the type
        innerContext.FinalType = innerContext.OriginalType;
        context.OriginalType = innerContext.OriginalType;
    }

    /// <summary>
    /// Resolves a binary expression.
    /// </summary>
    /// <param name="context">The binary expression.</param>
    /// <param name="left">The expression of the left side.</param>
    /// <param name="right">The expression of the right side.</param>
    /// <param name="operatorType">The identifier of the binary operator.</param>
    private void ResolveBinaryExpression(ExpressionContext context, ExpressionContext left, ExpressionContext right, int operatorType) {
        // resolve left and right side
        VisitExpression(left);
        VisitExpression(right);

        // stop if an error occured
        if (left.OriginalType is null || right.OriginalType is null) {
            return;
        }

        // casting the left side is not valid for assignment operators
        bool allowLeftCast = operatorType switch {
            OP_PLUS => true,
            OP_MINUS => true,
            OP_MULTIPLY => true,
            OP_DIVIDE => true,
            OP_MODULUS => true,
            OP_EQ => true,
            OP_NOT_EQ => true,
            OP_LESS => true,
            OP_GREATER => true,
            OP_LESS_EQ => true,
            OP_GREATER_EQ => true,
            OP_AND => true,
            OP_OR => true,
            OP_XOR => true,
            OP_SHIFT_LEFT => true,
            OP_SHIFT_RIGHT => true,
            _ => false
        };

        // calculate results
        // we do not care if the operation for the common type exists or not
        TypeIdentifier? commonType = FindCommonType(left, right, allowLeftCast, true);

        // stop if no valid common type exists
        if (commonType is null) {
            IssueHandler.Add(Issue.InvalidBinaryOperation(context, left.OriginalType, right.OriginalType, DefaultVocabulary.GetDisplayName(operatorType)));
        }

        // return type is bool for comparison operators
        context.OriginalType = operatorType switch {
            OP_EQ => TypeHandler.CoreTypes.Bool,
            OP_NOT_EQ => TypeHandler.CoreTypes.Bool,
            OP_LESS => TypeHandler.CoreTypes.Bool,
            OP_GREATER => TypeHandler.CoreTypes.Bool,
            OP_LESS_EQ => TypeHandler.CoreTypes.Bool,
            OP_GREATER_EQ => TypeHandler.CoreTypes.Bool,
            _ => commonType
        };

        left.FinalType = commonType;
        right.FinalType = commonType;
    }

    /// <summary>
    /// Finds the best common type for a binary expression.
    /// </summary>
    /// <param name="left">The expression of the left side.</param>
    /// <param name="right">The expression of the right side.</param>
    /// <param name="allowLeftCast">Allow changing the type of the left side.</param>
    /// <param name="allowRightCast">Allow changing the type of the right side.</param>
    /// <returns></returns>
    private TypeIdentifier? FindCommonType(ExpressionContext left, ExpressionContext right, bool allowLeftCast, bool allowRightCast) {
        TypeIdentifier?[] leftTypes = left is ConstantExpressionContext leftConstant ? [leftConstant.OriginalType, leftConstant.AlternativeType] : [left.OriginalType];
        TypeIdentifier?[] rightTypes = right is ConstantExpressionContext rightConstant ? [rightConstant.OriginalType, rightConstant.AlternativeType] : [right.OriginalType];

        PrimitiveCast bestCast = PrimitiveCast.None;
        TypeIdentifier? bestType = null;

        // cast left to right
        if (allowLeftCast) {
            foreach (TypeIdentifier? sourceType in leftTypes) {
                foreach (TypeIdentifier? targetType in rightTypes) {
                    if (sourceType is null || targetType is null) {
                        continue;
                    }

                    // TODO implement non-primitive cast
                    if (!TypeHandler.Casts.ArePrimitiveTypes(sourceType, targetType)) {
                        continue;
                    }

                    PrimitiveCast cast = TypeHandler.Casts.GetPrimitiveCast(sourceType, targetType);

                    if (bestCast < cast) {
                        bestCast = cast;
                        bestType = targetType;
                    }
                }
            }
        }

        // cast right to left
        if (allowRightCast) {
            foreach (TypeIdentifier? sourceType in rightTypes) {
                foreach (TypeIdentifier? targetType in leftTypes) {
                    if (sourceType is null || targetType is null) {
                        continue;
                    }

                    // TODO implement non-primitive cast
                    if (!TypeHandler.Casts.ArePrimitiveTypes(sourceType, targetType)) {
                        continue;
                    }

                    PrimitiveCast cast = TypeHandler.Casts.GetPrimitiveCast(sourceType, targetType);

                    if (bestCast < cast) {
                        bestCast = cast;
                        bestType = targetType;
                    }
                }
            }
        }

        return bestType;
    }
}