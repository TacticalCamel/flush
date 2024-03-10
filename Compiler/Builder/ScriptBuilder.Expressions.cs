namespace Compiler.Builder;

using Analysis;
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
        // forward expected type
        context.Constant.ExpectedType = context.ExpectedType;
        
        object? address = Visit(context.Constant);
        
        return address;
    }

    public override object? VisitConstant(ConstantContext context) {
        // subtypes must be visited
        return Visit(context);
    }

    public override object? VisitDecimalLiteral(DecimalLiteralContext context) {
        string text = context.start.Text;

        return AddInteger(context, text, NumberStyles.Integer);
    }

    public override object? VisitHexadecimalLiteral(HexadecimalLiteralContext context) {
        string text = context.start.Text[2..];
        
        return AddInteger(context, text, NumberStyles.HexNumber);
    }

    public override object? VisitBinaryLiteral(BinaryLiteralContext context) {
        string text = context.start.Text[2..];

        return AddInteger(context, text, NumberStyles.BinaryNumber);
    }

    public override object? VisitFloatLiteral(FloatLiteralContext context) {
        string text = context.start.Text;

        return AddFloat(context, text);
    }

    private int? AddInteger(ConstantContext context, string text, NumberStyles format) {
        string? type = context.ExpectedType?.Name;

        switch (type) {
            case "i8": {
                bool success = sbyte.TryParse(text, format, CultureInfo.InvariantCulture, out sbyte value);

                if (success) {
                    int address = DataHandler.I8.Add(value);
                    return address;
                }

                break;
            }
            case "i16": {
                bool success = short.TryParse(text, format, CultureInfo.InvariantCulture, out short value);

                if (success) {
                    int address = DataHandler.I16.Add(value);
                    return address;
                }
                
                break;
            }
            case "i32": {
                bool success = int.TryParse(text, format, CultureInfo.InvariantCulture, out int value);

                if (success) {
                    int address = DataHandler.I32.Add(value);
                    return address;
                }
                
                break;
            }
            case "i64": {
                bool success = long.TryParse(text, format, CultureInfo.InvariantCulture, out long value);

                if (success) {
                    int address = DataHandler.I64.Add(value);
                    return address;
                }
                
                break;
            }
            case null: {
                context.ExpectedType = ClassLoader.GetTypeByName("i32");
                return AddInteger(context, text, format);
            }
        }
        
        WarningHandler.Add(Warning.IntegerOutOfRange(context, text, type));
        return null;
    }

    private int? AddFloat(ConstantContext context, string text) {
        const NumberStyles FORMAT = NumberStyles.Float;
        
        string? type = context.ExpectedType?.Name;

        switch (type) {
            case "f16": {
                bool success = Half.TryParse(text, FORMAT, CultureInfo.InvariantCulture, out Half value);

                if (success) {
                    int address = DataHandler.F16.Add(value);
                    return address;
                }
                
                break;
            }
            case "f32": {
                bool success = float.TryParse(text, FORMAT, CultureInfo.InvariantCulture, out float value);

                if (success) {
                    int address = DataHandler.F32.Add(value);
                    return address;
                }
                
                break;
            }
            case "f64": {
                bool success = double.TryParse(text, FORMAT, CultureInfo.InvariantCulture, out double value);

                if (success) {
                    int address = DataHandler.F64.Add(value);
                    return address;
                }
                
                break;
            }
            case null: {
                context.ExpectedType = ClassLoader.GetTypeByName("f32");
                return AddInteger(context, text, FORMAT);
            }
        }
        
        WarningHandler.Add(Warning.IntegerOutOfRange(context, text, type));
        return null;
    }

    public override object? VisitCharLiteral(CharLiteralContext context) {
        string text = Regex.Unescape(context.start.Text[1..^1]);

        bool success = char.TryParse(text, out char value);

        if (success) {
            int address = DataHandler.Char.Add(value);
            return address;
        }

        WarningHandler.Add(Warning.IncorrectCharFormat(context));
        return null;
    }

    public override object VisitStringLiteral(StringLiteralContext context) {
        string value = Regex.Unescape(context.start.Text[1..^1]);

        int address = DataHandler.Str.Add(value);

        return address;
    }

    public override object? VisitNullKeyword(NullKeywordContext context) {
        return null;
    }

    public override object VisitTrueKeyword(TrueKeywordContext context) {
        return DataHandler.Bool.Add(true);
    }

    public override object VisitFalseKeyword(FalseKeywordContext context) {
        return DataHandler.Bool.Add(false);
    }

    #endregion

    public override object? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        // TODO
        // get actual operator type
        object? methodName = Visit(context.AdditiveOperator);
        
        // TODO
        // find matching method and forward parameter types
        // currently keeping excepted type, fine for most primitive operations
        context.Left.ExpectedType = context.ExpectedType;
        context.Right.ExpectedType = context.ExpectedType;
        
        object? left = Visit(context.Left);
        object? right = Visit(context.Right);
        
        // TODO
        // call operation for operands
        // probably working with instructions already

        return null;
    }

    public override object? VisitOpAdditive(OpAdditiveContext context) {
        if (context.OP_PLUS() != null) {
            return "op_Addition";
        }
        
        if (context.OP_MINUS() != null) {
            return "op_Subtraction";
        }
        
        return null;
    }
}