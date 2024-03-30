namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Analysis;
using Data;

internal sealed partial class Preprocessor {
    /// <summary>
    /// Visit an expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitExpression(ExpressionContext context) {
        Visit(context);

        return null;
    }

    /// <summary>
    /// Visits a constant and assigns the result to the visited expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitConstantExpression(ConstantExpressionContext context) {
        // get constant result
        ConstantResult? result = VisitConstant(context.Constant);

        // assign address and type
        context.Address = result?.Address;
        context.OriginalType = result?.Type;
        context.AlternativeType = result?.SecondaryType;

        return null;
    }

    /// <summary>
    /// Visits an identifier.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object VisitIdentifierExpression(IdentifierExpressionContext context) {
        throw new NotImplementedException();

        /*
        // we have no way of knowing the actual meaning of this identifier
        // it could be a variable or a member access

        // assume it is a variable and handle other cases elsewhere
        // since this node should not be visited in other cases,
        // we can throw an error if the variable was not found

        string name = VisitId(context.Identifier);

        IssueHandler.Add(Issue.FeatureNotImplemented(context, "identifier expression"));
        return null;*/
    }

    /// <summary>
    /// Visits a member access.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object VisitMemberAccessOperatorExpression(MemberAccessOperatorExpressionContext context) {
        throw new NotImplementedException();

        /*ExpressionResult? left = VisitExpression(context.Type);

        if (left is null) {
            return null;
        }

        string name = VisitId(context.Member);

        IssueHandler.Add(Issue.FeatureNotImplemented(context, "member access"));
        return null;*/
    }

    /// <summary>
    /// Visits an expression cast.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitCastExpression(CastExpressionContext context) {
        // get the target type
        TypeIdentifier? targetType = VisitType(context.Type);

        // stop if the type does not exist
        if (targetType is null) {
            return null;
        }

        // resolve the expression
        VisitExpression(context.Expression);

        TypeIdentifier? sourceType = context.Expression.OriginalType;

        // stop if an error occured
        if (sourceType is null) {
            return null;
        }

        // check if the cast exists
        if (TypeHandler.Casts.ArePrimitiveTypes(sourceType, targetType)) {
            PrimitiveCast cast = TypeHandler.Casts.GetPrimitiveCast(sourceType, targetType);

            if (cast == PrimitiveCast.None) {
                IssueHandler.Add(Issue.InvalidCast(context, sourceType, targetType));
                return null;
            }
        }

        context.Expression.FinalType = targetType;
        context.OriginalType = targetType;

        return null;
    }

    /// <summary>
    /// Visits a nested expression and assigns it the same type as its inner expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitNestedExpression(NestedExpressionContext context) {
        VisitExpression(context.Body);

        // assign types
        context.Body.FinalType = context.Body.OriginalType;
        context.OriginalType = context.Body.OriginalType;

        return null;
    }

    /// <summary>
    /// Visits a function call.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object VisitFunctionCallExpression(FunctionCallExpressionContext context) {
        throw new NotImplementedException();

        /*ExpressionResult? callerExpression = VisitExpression(context.Caller);

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
        return null;*/
    }

    /// <summary>
    /// Visits a left unary expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitLeftUnaryOperatorExpression(LeftUnaryOperatorExpressionContext context) {
        ResolveUnaryExpression(context, context.Expression, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a right unary expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitRightUnaryOperatorExpression(RightUnaryOperatorExpressionContext context) {
        ResolveUnaryExpression(context, context.Expression, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a multiplicative expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitMultiplicativeOperatorExpression(MultiplicativeOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a additive expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a shift expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitShiftOperatorExpression(ShiftOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a comparison expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitComparisonOperatorExpression(ComparisonOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a logical expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitLogicalOperatorExpression(LogicalOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits an assignment expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitAssigmentOperatorExpression(AssigmentOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }
}