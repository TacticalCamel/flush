namespace Compiler.Builder;

using Data;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override object? VisitProgramBody(ProgramBodyContext context) {
        StatementContext[] statements = context.statement();
        TypeDefinitionContext[] typeDefinitions = context.typeDefinition();

        // visit type definitions
        foreach (TypeDefinitionContext typeDefinition in typeDefinitions) {
            VisitTypeDefinition(typeDefinition);
        }
        
        CodeHandler.EnterScope();
        
        // visit statements
        foreach (StatementContext statement in statements) {
            VisitStatement(statement);
        }
        
        CodeHandler.ExitScope();

        return null;
    }

    public override object? VisitVariableDeclaration(VariableDeclarationContext context) {
        ExpressionResult? result = VisitExpression(context.Expression);
        string name = VisitId(context.VariableWithType.Name);

        if (result is null) {
            return null;
        }

        // do not pop result
        // it is stored on the stack until the variable is out of scope
        CodeHandler.DefineVariable(name, result);

        return null;
    }

    /*
    public override VariableIdentifier? VisitVariableWithType(VariableWithTypeContext context) {
        TypeIdentifier? type = VisitType(context.Type);
        string name = VisitId(context.Name);

        if (type is null) {
            return null;
        }

        return new VariableIdentifier(type, name);
    }
    */

    public override string VisitId(IdContext context) {
        return context.Start.Text;
    }

    public override object? VisitRegularStatement(RegularStatementContext context) {
        if (context.Expression is not null) {
            ExpressionResult? result = VisitExpression(context.Expression);

            if (result is null) {
                return null;
            }

            CodeHandler.Pop(result.Type.Size);
        }
        else {
            VisitChildren(context);
        }

        return null;
    }
}