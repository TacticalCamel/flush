namespace Compiler.Builder;

using Data;
using Analysis;
using Interpreter.Types;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override object? VisitVariableDeclaration(VariableDeclarationContext context) {
        VariableIdentifier? variable = VisitVariableWithType(context.VariableWithType);
        
        ExpressionResult? result = VisitExpression(context.Expression);
        
        if (variable is null) {
            return null;
        }
        
        Logger.Debug($"{variable} = {result?.ToString() ?? "null"}");

        return null;
    }

    public override VariableIdentifier? VisitVariableWithType(VariableWithTypeContext context) {
        TypeIdentifier? type = VisitType(context.Type);
        string name = VisitId(context.Name);

        if (type is null) {
            return null;
        }
        
        return new VariableIdentifier(type, name);
    }

    public override TypeIdentifier? VisitType(TypeContext context) {
        // subtypes must be visited
        return (TypeIdentifier?)Visit(context);
    }

    public override TypeIdentifier? VisitSimpleType(SimpleTypeContext context) {
        string name = VisitId(context.Name);
        
        TypeInfo? type = TypeHandler.TryGetByName(name);

        if (type is null) {
            IssueHandler.Add(Issue.UnknownVariableType(context, name));
            return null;
        }
        
        return new TypeIdentifier(type, []);
    }

    public override TypeIdentifier? VisitGenericType(GenericTypeContext context) {
        string name = VisitId(context.Name);
        
        TypeInfo? type = TypeHandler.TryGetByName(name);
        
        if (type is null) {
            IssueHandler.Add(Issue.UnknownVariableType(context, name));
            return null;
        }

        TypeContext[] typeContexts = context.type();

        TypeIdentifier[] genericParameters = new TypeIdentifier[typeContexts.Length];

        for (int i = 0; i < typeContexts.Length; i++) {
            TypeIdentifier? typeIdentifier = VisitType(typeContexts[i]);

            if (typeIdentifier is null) {
                return null;
            }

            genericParameters[i] = typeIdentifier;
        }
        
        return new TypeIdentifier(type, genericParameters);
    }
    
    public override ExpressionResult? VisitExpression(ExpressionContext context) {
        // subtypes must be visited
        return (ExpressionResult?)Visit(context);
    }
}