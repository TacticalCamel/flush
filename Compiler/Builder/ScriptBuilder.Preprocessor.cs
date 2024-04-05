namespace Compiler.Builder;

using Analysis;
using Data;
using static Grammar.ScrantonParser;

// ScriptBuilder.Preprocessor: methods related to preprocessing expressions
internal sealed partial class ScriptBuilder {
    

    
    /// <summary>
    /// Visits a constant and assigns the result to the visited expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessConstantExpression(ConstantExpressionContext context) {
        // get expression result
        ExpressionResult? result = VisitConstant(context.Constant);

        if (result is null) {
            return null;
        }

        // assign address and type
        context.Address = result.Address;
        context.OriginalType = result.Type;

        // assign secondary type if it's a constant result
        if (result is ConstantResult constantResult) {
            context.AlternativeType = constantResult.SecondaryType;
        }

        return null;
    }

    /// <summary>
    /// Visits an identifier.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessIdentifierExpression(IdentifierExpressionContext context) {
        // get identifier name
        string name = VisitId(context.Identifier);

        ExpressionResult? expressionResult = CodeHandler.GetVariable(name);

        if (expressionResult is null) {
            return null;
        }
        
        context.OriginalType = expressionResult.Type;
        
        return null;
    }

    /// <summary>
    /// Visits a member access.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object PreprocessMemberAccessOperatorExpression(MemberAccessOperatorExpressionContext context) {
        throw new NotImplementedException();

        ExpressionResult? left = VisitExpression(context.Type);

        if (left is null) {
            return null;
        }

        string name = VisitId(context.Member);

        IssueHandler.Add(Issue.FeatureNotImplemented(context, "member access"));
        return null;
    }

    /// <summary>
    /// Visits an expression cast.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessCastExpression(CastExpressionContext context) {
        // get the target type
        TypeIdentifier? targetType = VisitType(context.Type);

        // stop if the type does not exist
        if (targetType is null) {
            return null;
        }

        // resolve the expression
        VisitExpression(context.Expression);

        // get the source type
        TypeIdentifier? sourceType = context.Expression.OriginalType;

        // stop if an error occured
        if (sourceType is null) {
            return null;
        }

        // check if the cast exists
        // allow both explicit and implicit
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
    public object? PreprocessNestedExpression(NestedExpressionContext context) {
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
    public object PreprocessFunctionCallExpression(FunctionCallExpressionContext context) {
        throw new NotImplementedException();

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
    }

    /// <summary>
    /// Visits a left unary expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessLeftUnaryOperatorExpression(LeftUnaryOperatorExpressionContext context) {
        ResolveUnaryExpression(context, context.Expression, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a right unary expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessRightUnaryOperatorExpression(RightUnaryOperatorExpressionContext context) {
        ResolveUnaryExpression(context, context.Expression, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a multiplicative expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessMultiplicativeOperatorExpression(MultiplicativeOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a additive expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a shift expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessShiftOperatorExpression(ShiftOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a comparison expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessComparisonOperatorExpression(ComparisonOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits a logical expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessLogicalOperatorExpression(LogicalOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }

    /// <summary>
    /// Visits an assignment expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public object? PreprocessAssigmentOperatorExpression(AssigmentOperatorExpressionContext context) {
        ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);

        return null;
    }
}