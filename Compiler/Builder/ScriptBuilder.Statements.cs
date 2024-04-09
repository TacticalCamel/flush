namespace Compiler.Builder;

using Data;
using Analysis;
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
    /// Visits a statement.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitStatement(StatementContext context) {
        // visit statement subtype
        VisitChildren(context);

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
    /// Visits a control statement.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitControlStatement(ControlStatementContext context) {
        // visit control statement subtype
        Visit(context);

        return null;
    }

    /// <summary>
    /// Visits a block statement.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitBlockStatement(BlockStatementContext context) {
        // visit block statement subtype
        VisitChildren(context);

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
        bool success = CodeHandler.DefineVariable(name, result.Type);

        if (!success) {
            IssueHandler.Add(Issue.VariableAlreadyDeclared(context, name));
        }

        return null;
    }

    /// <summary>
    /// Visit a block.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitBlock(BlockContext context) {
        // begin new scope
        CodeHandler.EnterScope();

        // visit statements inside
        VisitChildren(context);
        
        // exit scope
        CodeHandler.ExitScope();
        
        return null;
    }

    /// <summary>
    /// Visit an if block.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitIfBlock(IfBlockContext context) {
        // the collection of jumps that target the end of the entire if block
        // these are places to the end of every branch
        List<JumpHandle> endJumps = [];

        // visit every 
        foreach (IfBlockBodyContext bodyContext in context.ifBlockBody()) {
            // preprocess expression to resolve types
            PreprocessExpression(bodyContext.Condition);
            
            // expression must evaluate to bool
            bodyContext.Condition.FinalType = TypeHandler.CoreTypes.Bool;
            
            // visit condition
            ExpressionResult? result = VisitExpression(bodyContext.Condition);

            // return if an error occured
            if (result is null) {
                return null;
            }
            
            // the jump targeting the next if branch
            JumpHandle nextJump = CodeHandler.CreateJumpPlaceholder();
            
            // visit contents
            VisitStatement(bodyContext.Statement);

            // the jump targeting the end of the entire if block
            JumpHandle endJump = CodeHandler.CreateJumpPlaceholder();
            
            // add the the jump list
            endJumps.Add(endJump);
            
            // place next jump immediately before the next branch
            CodeHandler.ConditionalJump(nextJump);
        }

        // if has an else branch
        if (context.ElseStatement is not null) {
            // visit contents
            VisitStatement(context.ElseStatement);
        }
        
        // resolve end jumps
        foreach (JumpHandle endJump in endJumps) {
            CodeHandler.Jump(endJump);
        }
        
        return null;
    }

    public override object? VisitDebugStatement(DebugStatementContext context) {
        CodeHandler.StopDebug();
        return null;
    }
}