namespace Compiler.Builder;

using Data;
using Analysis;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    /// <summary>
    /// Visit an expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitExpression(ExpressionContext context) {
        // visit expression subtype
        ExpressionResult? result = (ExpressionResult?)Visit(context);

        return result;
    }

    /// <summary>
    /// Visit a constant expression.
    /// Push the value to the stack and convert it to the desired type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitConstantExpression(ConstantExpressionContext context) {
        // error during first pass, should never happen
        if (context.Address is null || context.OriginalType is null || context.FinalType is null) {
            return null;
        }

        // push the constant to the stack if it's stored in the data section
        if (context.Address.Value.Location == MemoryLocation.Data) {
            CodeHandler.PushFromData(context.Address.Value, context.OriginalType.Size);
        }

        bool success = CastExpression(context);

        return success ? new ExpressionResult(context.Address.Value, context.FinalType) : null;
    }

    public override ExpressionResult? VisitIdentifierExpression(IdentifierExpressionContext context) {
        string name = VisitId(context.Identifier);

        ExpressionResult? expressionResult = CodeHandler.GetVariableAddress(name);

        if (expressionResult is null) {
            IssueHandler.Add(Issue.UnknownVariable(context, name));
            return null;
        }

        return expressionResult;
    }

    public override ExpressionResult? VisitMemberAccessOperatorExpression(MemberAccessOperatorExpressionContext context) {
        throw new NotImplementedException();

        /*
        ExpressionResult? left = VisitExpression(context.Type);

        if (left is null) {
            return null;
        }

        string name = VisitId(context.Member);

        IssueHandler.Add(Issue.FeatureNotImplemented(context, "member access"));
        return null;
        */
    }

    /// <summary>
    /// Visit a cast expression.
    /// Convert the value to the desired type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitCastExpression(CastExpressionContext context) {
        ExpressionResult? result = VisitExpression(context.Expression);

        if (result is null) {
            return null;
        }

        bool success = CastExpression(context);

        return success ? new ExpressionResult(result.Address, context.FinalType!) : null;
    }

    /// <summary>
    /// Visit a nested expression.
    /// Convert the value to the desired type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The address and type of the expression.</returns>
    public override ExpressionResult? VisitNestedExpression(NestedExpressionContext context) {
        ExpressionResult? result = VisitExpression(context.Body);

        if (result is null) {
            return null;
        }

        bool success = CastExpression(context);

        return success ? new ExpressionResult(result.Address, context.FinalType!) : null;
    }

    public override ExpressionResult? VisitFunctionCallExpression(FunctionCallExpressionContext context) {
        throw new NotImplementedException();

        /*
        ExpressionResult? callerExpression = VisitExpression(context.Caller);

        // get parameter expressions
        ExpressionContext[] expressions = context.ExpressionList.expression();

        // create an array for results
        ExpressionResult[] results = new ExpressionResult[expressions.Length];

        // resolve parameters and return if any of them was null
        for (int i = 0; i < expressions.Length; i++) {
            ExpressionResult? result = VisitExpression(expressions[i]);

            if (result is null) {
                return null;
            }

            results[i] = result;
        }

        IssueHandler.Add(Issue.FeatureNotImplemented(context, "function call"));
        return null;
        */
    }

    public override ExpressionResult? VisitLeftUnaryOperatorExpression(LeftUnaryOperatorExpressionContext context) {
        return ResolveUnaryExpression(context, context.Expression, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitRightUnaryOperatorExpression(RightUnaryOperatorExpressionContext context) {
        return ResolveUnaryExpression(context, context.Expression, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitMultiplicativeOperatorExpression(MultiplicativeOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitShiftOperatorExpression(ShiftOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitComparisonOperatorExpression(ComparisonOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitLogicalOperatorExpression(LogicalOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitAssigmentOperatorExpression(AssigmentOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }
}