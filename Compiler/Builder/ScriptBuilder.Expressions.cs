namespace Compiler.Builder;

using Analysis;
using System.Globalization;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override object? VisitConstantExpression(ConstantExpressionContext context) {
        return VisitChildren(context);
    }

    public override object? VisitDecimalLiteral(DecimalLiteralContext context) {
        string text = context.start.Text;
        bool success = int.TryParse(text, out int value);

        if (success) {
            // TODO: not always 32-bit
            int address = DataHandler.I32.Add(value);
            return address;
        }
        
        AddWarning(WarningType.IncorrectNumberFormat, context);
        return null;
    }
    
    public override object? VisitHexadecimalLiteral(HexadecimalLiteralContext context) {
        string text = context.start.Text[2..];
        bool success = int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value);
        
        if (success) {
            // TODO: not always 32-bit
            int address = DataHandler.I32.Add(value);
            return address;
        }
        
        AddWarning(WarningType.IncorrectNumberFormat, context);
        return null;
    }

    public override object? VisitBinaryLiteral(BinaryLiteralContext context) {
        string text = context.start.Text[2..];
        bool success = int.TryParse(text,NumberStyles.BinaryNumber, CultureInfo.InvariantCulture,  out int value);

        if (success) {
            // TODO: not always 32-bit
            int address = DataHandler.I32.Add(value);
            return address;
        }
        
        AddWarning(WarningType.IncorrectNumberFormat, context);
        return null;
    }

    public override object? VisitFloatLiteral(FloatLiteralContext context) {
        string text = context.start.Text;
        bool success = float.TryParse(text, out float value);

        if (success) {
            // TODO: not always 32-bit
            int address = DataHandler.F32.Add(value);
            return address;
        }
            
        AddWarning(WarningType.IncorrectNumberFormat, context);
        return null;
    }

    public override object? VisitCharLiteral(CharLiteralContext context) {
        string text = context.start.Text[1..^1];
        bool success = char.TryParse(text, out char value);
        
        if (success) {
            // TODO: escaped char not handled
            int address = DataHandler.Char.Add(value);
            return address;
        }
        
        AddWarning(WarningType.IncorrectCharFormat, context);
        return null;
    }

    public override object? VisitStringLiteral(StringLiteralContext context) {
        string value = context.start.Text[1..^1];
        
        // TODO: escaped characters not handled
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

    /*
    public override object? VisitVariableDeclaration(VariableDeclarationContext context) {
        TypeInfo typeInfo = (TypeInfo)VisitTypeName(context.varWithType().Type)!;

        ClassLoader.Types.TryGetValue(typeInfo.TypeName, out Type? type);
        Console.WriteLine($"Var : {type?.FullName ?? "null"}");
        return base.VisitVariableDeclaration(context);
    }

    public override object? VisitTypeName(TypeNameContext context) {
        return Visit(context);
    }

    public override TypeInfo VisitSimpleType(SimpleTypeContext context) {
        return new TypeInfo(VisitId(context.Name), Array.Empty<TypeInfo>());
    }

    public override TypeInfo VisitGenericType(GenericTypeContext context) {
        return new TypeInfo(VisitId(context.Name), context.typeName().Select(x => (TypeInfo)VisitTypeName(x)!).ToArray());
    }

    public override string VisitId(IdContext context) {
        return context.Start.Text;
    }*/
}