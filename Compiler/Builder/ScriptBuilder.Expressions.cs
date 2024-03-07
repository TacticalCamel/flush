namespace Compiler.Builder;

using Analysis;
using Data;
using Interpreter.Types;
using System.Globalization;
using System.Text.RegularExpressions;
using Runtime.Core;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    private TypeInfo? SuggestedType { get; set; }

    #region Constants

    public override object? VisitConstantExpression(ConstantExpressionContext context) {
        object? address = Visit(context.Constant);

        return address;
    }

    public override object? VisitDecimalLiteral(DecimalLiteralContext context) {
        string text = context.start.Text;

        int? address = AddInteger(text, NumberStyles.Integer);

        if (address is null) {
            Warnings.Add(Warning.IncorrectNumberFormat(context));
            return null;
        }

        return address.Value;
    }

    public override object? VisitHexadecimalLiteral(HexadecimalLiteralContext context) {
        string text = context.start.Text[2..];
        
        int? address = AddInteger(text, NumberStyles.HexNumber);

        if (address is null) {
            Warnings.Add(Warning.IncorrectNumberFormat(context));
            return null;
        }

        return address.Value;
    }

    public override object? VisitBinaryLiteral(BinaryLiteralContext context) {
        string text = context.start.Text[2..];
        
        int? address = AddInteger(text, NumberStyles.BinaryNumber);

        if (address is null) {
            Warnings.Add(Warning.IncorrectNumberFormat(context));
            return null;
        }

        return address.Value;
    }

    public override object? VisitFloatLiteral(FloatLiteralContext context) {
        string text = context.start.Text;
        
        int? address = AddFloat(text);

        if (address is null) {
            Warnings.Add(Warning.IncorrectNumberFormat(context));
            return null;
        }

        return address.Value;
    }
    
    private int? AddInteger(string text, NumberStyles format) {
        Type? type = SuggestedType?.Type;
        
        if (type == typeof(I8)) {
            bool success = sbyte.TryParse(text, format, CultureInfo.InvariantCulture, out sbyte value);

            if (success) {
                //Console.WriteLine($"{typeof(I8).FullName}: {value}");

                int address = DataHandler.I8.Add(value);
                return address;
            }

            return null;
        }

        if (type == typeof(I16)) {
            bool success = short.TryParse(text, format, CultureInfo.InvariantCulture, out short value);

            if (success) {
                //Console.WriteLine($"{typeof(I16).FullName}: {value}");

                int address = DataHandler.I16.Add(value);
                return address;
            }

            return null;
        }

        if (type == typeof(I32)) {
            bool success = int.TryParse(text, format, CultureInfo.InvariantCulture, out int value);

            if (success) {
                //Console.WriteLine($"{typeof(I32).FullName}: {value}");

                int address = DataHandler.I32.Add(value);
                return address;
            }

            return null;
        }

        if (type == typeof(I64)) {
            bool success = long.TryParse(text, format, CultureInfo.InvariantCulture, out long value);

            if (success) {
                //Console.WriteLine($"{typeof(I64).FullName}: {value}");

                int address = DataHandler.I64.Add(value);
                return address;
            }

            return null;
        }

        return null;
    }
    
    private int? AddFloat(string text) {
        Type? type = SuggestedType?.Type;
        
        if (type == typeof(F16)) {
            bool success = Half.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out Half value);

            if (success) {
                //Console.WriteLine($"{typeof(F16).FullName}: {value}");

                int address = DataHandler.F16.Add(value);
                return address;
            }

            return null;
        }

        if (type == typeof(F32)) {
            bool success = float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);

            if (success) {
                //Console.WriteLine($"{typeof(F32).FullName}: {value}");

                int address = DataHandler.F32.Add(value);
                return address;
            }

            return null;
        }

        if (type == typeof(F64)) {
            bool success = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value);

            if (success) {
                //Console.WriteLine($"{typeof(F64).FullName}: {value}");

                int address = DataHandler.F64.Add(value);
                return address;
            }

            return null;
        }

        return null;
    }

    public override object? VisitCharLiteral(CharLiteralContext context) {
        string text = Regex.Unescape(context.start.Text[1..^1]);

        bool success = char.TryParse(text, out char value);

        if (success) {
            int address = DataHandler.Char.Add(value);
            return address;
        }

        Warnings.Add(Warning.IncorrectCharFormat(context));
        return null;
    }

    public override object? VisitStringLiteral(StringLiteralContext context) {
        string value = Regex.Unescape(context.start.Text[1..^1]);

        int address = DataHandler.Str.Add(value);

        return address;
    }

    public override object? VisitNullKeyword(NullKeywordContext context) {
        return null;
    }

    public override object? VisitTrueKeyword(TrueKeywordContext context) {
        return DataHandler.Bool.Add(true);
    }

    public override object? VisitFalseKeyword(FalseKeywordContext context) {
        return DataHandler.Bool.Add(false);
    }

    #endregion

    public override object? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        object? left = Visit(context.Left);
        object? right = Visit(context.Right);


        return null;
    }

    public override object? VisitOpAdditive(OpAdditiveContext context) {
        if (context.OP_PLUS() != null) { }
        else if (context.OP_MINUS() != null) { }

        return base.VisitOpAdditive(context);
    }

    public override object? VisitVariableDeclaration(VariableDeclarationContext context) {
        TypeTemplate template = VisitTypeName(context.varWithType().Type)!;

        TypeInfo? type = ClassLoader.GetTypeByName(template.TypeName);

        if (type is null) {
            Warnings.Add(Warning.UnknownVariableType(context.varWithType().Type, template.TypeName));
            return null;
        }

        SuggestedType = type;
        VisitExpression(context.Expression);

        return null;
    }


    public override TypeTemplate VisitTypeName(TypeNameContext context) {
        return (TypeTemplate)Visit(context)!;
    }

    public override TypeTemplate VisitSimpleType(SimpleTypeContext context) {
        return new TypeTemplate(VisitId(context.Name), Array.Empty<TypeTemplate>());
    }

    public override TypeTemplate VisitGenericType(GenericTypeContext context) {
        return new TypeTemplate(VisitId(context.Name), context.typeName().Select(x => (TypeTemplate)VisitTypeName(x)!).ToArray());
    }

    public override string VisitId(IdContext context) {
        return context.Start.Text;
    }
}