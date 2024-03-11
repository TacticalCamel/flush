namespace Compiler.Builder;

using Analysis;
using Data;
using Interpreter.Types;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override object? VisitVariableDeclaration(VariableDeclarationContext context) {
        TypeTemplate template = VisitTypeName(context.varWithType().Type);

        TypeInfo? type = ClassLoader.GetTypeByName(template.TypeName);

        if (type is null) {
            WarningHandler.Add(Warning.UnknownVariableType(context.varWithType().Type, template.TypeName));
            return null;
        }

        ExpressionContext expression = context.Expression;

        VisitExpression(expression);

        return null;
    }
    
    public override TypeTemplate VisitTypeName(TypeNameContext context) {
        return (TypeTemplate)Visit(context)!;
    }

    public override TypeTemplate VisitSimpleType(SimpleTypeContext context) {
        return new TypeTemplate(VisitId(context.Name), Array.Empty<TypeTemplate>());
    }

    public override TypeTemplate VisitGenericType(GenericTypeContext context) {
        return new TypeTemplate(VisitId(context.Name), context.typeName().Select(VisitTypeName).ToArray());
    }

    public override string VisitId(IdContext context) {
        return context.Start.Text;
    }
}