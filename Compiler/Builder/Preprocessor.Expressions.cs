namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Interpreter.Bytecode;
using Interpreter.Types;
using Data;
using Analysis;

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
        ConstantResult? result = VisitConstant(context.Constant);

        if (result is null) {
            return null;
        }

        context.Result = result;
        context.ExpressionType = result.Type;
        return null;
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

        // get the type of the expression
        TypeIdentifier? sourceType = context.Expression.ExpressionType;

        // stop if an error occured
        if (sourceType is null) {
            return null;
        }

        // true if both expression types are primitive types
        // in this case we can use a simple instruction instead of a function call
        bool isPrimitiveOperation = TypeHandler.PrimitiveConversions.ArePrimitiveTypes(sourceType, targetType);

        // TODO implement non-primitive casting
        if (!isPrimitiveOperation) {
            IssueHandler.Add(Issue.FeatureNotImplemented(context, "non-primitive casting"));
            return null;
        }

        Instruction? instruction = TypeHandler.PrimitiveConversions.GetCastInstruction(sourceType, targetType);

        context.ExpressionType = targetType;
        
        // TODO finish
        Console.WriteLine($"{sourceType} -> {targetType}");

        return null;
    }

    /// <summary>
    /// Visits a nested expression and assigns it the same type as its inner expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitNestedExpression(NestedExpressionContext context) {
        VisitExpression(context.Body);

        context.ExpressionType = context.Body.ExpressionType;

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
    /// Visits a multiplicative expression.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>Always null.</returns>
    public override object? VisitMultiplicativeOperatorExpression(MultiplicativeOperatorExpressionContext context) {
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

    /// <summary>
    /// Visit a type name.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The identifier of the type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitType(TypeContext context) {
        return (TypeIdentifier?)Visit(context);
    }

    /// <summary>
    /// Visit a non-generic type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The identifier of the type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitSimpleType(SimpleTypeContext context) {
        // get the name of the type
        string name = VisitId(context.Name);

        // search the type by name
        TypeInfo? type = TypeHandler.TryGetByName(name);

        // stop if the type does not exist
        if (type is null) {
            IssueHandler.Add(Issue.UnrecognizedType(context, name));
            return null;
        }

        return new TypeIdentifier(type, []);
    }

    /// <summary>
    /// Visit a generic type.
    /// </summary>
    /// <param name="context">The node to visit.</param>
    /// <returns>The identifier of the type if it exists, null otherwise.</returns>
    public override TypeIdentifier? VisitGenericType(GenericTypeContext context) {
        // get the name of the type
        string name = VisitId(context.Name);

        // search the type by name
        TypeInfo? type = TypeHandler.TryGetByName(name);

        // stop if the type does not exist
        if (type is null) {
            IssueHandler.Add(Issue.UnrecognizedType(context, name));
            return null;
        }

        // TODO no checks are performed for number of generic parameters

        // get generic parameter nodes
        TypeContext[] typeContexts = context.type();

        // create an array for results
        TypeIdentifier[] genericParameters = new TypeIdentifier[typeContexts.Length];

        // for every generic parameter
        for (int i = 0; i < typeContexts.Length; i++) {
            // get the type identifier
            TypeIdentifier? typeIdentifier = VisitType(typeContexts[i]);

            // stop if the type does not exist
            if (typeIdentifier is null) {
                return null;
            }

            // assign array element
            genericParameters[i] = typeIdentifier;
        }

        return new TypeIdentifier(type, genericParameters);
    }

    // TODO implement
    /*
    public override ExpressionResult? VisitMemberAccessOperatorExpression(MemberAccessOperatorExpressionContext context) {
        ExpressionResult? left = VisitExpression(context.Type);

        if (left is null) {
            return null;
        }

        string name = VisitId(context.Member);

        IssueHandler.Add(Issue.FeatureNotImplemented(context, "member access"));
        return null;
    }

    public override ExpressionResult? VisitIdentifierExpression(IdentifierExpressionContext context) {
        // we have no way of knowing the actual meaning of this identifier
        // it could be a variable or a member access

        // assume it is a variable and handle other cases elsewhere
        // since this node should not be visited in other cases,
        // we can throw an error if the variable was not found

        string name = VisitId(context.Identifier);

        IssueHandler.Add(Issue.FeatureNotImplemented(context, "identifier expression"));
        return null;
    }

    public override ExpressionResult? VisitFunctionCallExpression(FunctionCallExpressionContext context) {
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

    private ExpressionResult? ResolveUnaryExpression(ExpressionContext context, int operatorType) {
        // resolve the expression
        ExpressionResult? expression = VisitExpression(context);

        // return if an error occured
        if (expression is null) {
            return null;
        }

        // true if the expression type is a primitive type or null
        // in this case we can use a simple instruction instead of a function call
        bool isPrimitiveType = TypeHandler.PrimitiveConversions.IsPrimitiveType(expression.Type);

        // calculate results
        ExpressionResult? result = operatorType switch {
            OP_PLUS => null,
            OP_MINUS => null,
            OP_NOT => null,
            OP_INCREMENT => null,
            OP_DECREMENT => null,
            _ => throw new ArgumentException($"Method cannot handle the provided operator type {operatorType}")
        };

        return result;
    }

     public override ExpressionResult? VisitLeftUnaryOperatorExpression(LeftUnaryOperatorExpressionContext context) {
        return ResolveUnaryExpression(context.Expression, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitRightUnaryOperatorExpression(RightUnaryOperatorExpressionContext context) {
        return ResolveUnaryExpression(context.Expression, context.Operator.start.Type);
    }

    */
}