namespace Compiler.Builder;

using Data;
using static Grammar.ScrantonParser;

// ScriptBuilder.Statements: methods related to visiting statements
internal sealed partial class ScriptBuilder {
    /// <summary>
    /// Visit the program body.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitProgramBody(ProgramBodyContext context) {
        // separate type definitions from statements
        TypeDefinitionContext[] typeDefinitions = context.typeDefinition();
        StatementContext[] statements = context.statement();

        // visit type definitions first
        ProcessTypeDefinitions(typeDefinitions);

        // enter body scope
        CodeHandler.EnterScope();

        // visit statements after fully loading types
        foreach (StatementContext statement in statements) {
            VisitStatement(statement);
        }

        // exit body scope
        CodeHandler.ExitScope();

        return null;
    }
    
    /// <summary>
    /// Visits a regular statement.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitRegularStatement(RegularStatementContext context) {
        // call other visit method if variable declaration
        if (context.VariableDeclaration is not null) {
            VisitVariableDeclaration(context.VariableDeclaration);
            return null;
        }

        // preprocess expression to resolve types
        PreprocessExpression(context.Expression);
        
        // visit expression
        ExpressionResult? result = VisitExpression(context.Expression);

        if (result is null) {
            return null;
        }

        // discard results
        CodeHandler.PopBytes(result.Type.Size);

        return null;
    }
    
    /// <summary>
    /// Visit a variable declaration.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitVariableDeclaration(VariableDeclarationContext context) {
        // get variable name
        string name = VisitId(context.VariableWithType.Name);

        // preprocess expression to resolve types
        PreprocessExpression(context.Expression);

        // get variable type
        TypeIdentifier? type = VisitType(context.VariableWithType.Type);

        if (type is null) {
            return null;
        }

        // assign final type
        context.Expression.FinalType = type;

        // visit expression
        ExpressionResult? result = VisitExpression(context.Expression);

        if (result is null) {
            return null;
        }

        // define variable
        // do not pop result, it will be on the stack until the variable is out of scope
        CodeHandler.DefineVariable(name, result.Type);

        return null;
    }
}