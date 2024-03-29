namespace Compiler.Builder;

using Data;
using Analysis;
using Interpreter.Bytecode;
using static Grammar.ScrantonParser;

internal sealed partial class ScriptBuilder {
    public override ExpressionResult? VisitExpression(ExpressionContext context) {
        // subtypes must be visited
        ExpressionResult? result = (ExpressionResult?)Visit(context);

        return result;
    }

    public override ExpressionResult? VisitConstantExpression(ConstantExpressionContext context) {
        return context.Result;
    }

    public override ExpressionResult? VisitNestedExpression(NestedExpressionContext context) {
        return VisitExpression(context.Body);
    }

    public override ExpressionResult? VisitCastExpression(CastExpressionContext context) {
        ExpressionResult? result = VisitExpression(context.Expression);

        return result;
    }

    public override ExpressionResult? VisitAdditiveOperatorExpression(AdditiveOperatorExpressionContext context) {
        return ResolveBinaryExpression(context, context.Left, context.Right, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitMultiplicativeOperatorExpression(MultiplicativeOperatorExpressionContext context) {
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

    public override ExpressionResult? VisitLeftUnaryOperatorExpression(LeftUnaryOperatorExpressionContext context) {
        return ResolveUnaryExpression(context.Expression, context.Operator.start.Type);
    }

    public override ExpressionResult? VisitRightUnaryOperatorExpression(RightUnaryOperatorExpressionContext context) {
        return ResolveUnaryExpression(context.Expression, context.Operator.start.Type);
    }

    // TODO implement
    public override ExpressionResult? VisitMemberAccessOperatorExpression(MemberAccessOperatorExpressionContext context) {
        ExpressionResult? left = VisitExpression(context.Type);

        if (left is null) {
            return null;
        }

        string name = VisitId(context.Member);

        IssueHandler.Add(Issue.FeatureNotImplemented(context, "member access"));
        return null;
    }

    // TODO: implement
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

    private ExpressionResult? ResolveBinaryExpression(ExpressionContext context, ExpressionContext leftContext, ExpressionContext rightContext, int operatorType) {
        // resolve left side
        ExpressionResult? left = VisitExpression(leftContext);

        // return if there was an error
        if (left is null) {
            return null;
        }

        // move object to stack if it's in the data section
        if (left.Address.Location == MemoryLocation.Data) {
            Instructions.PushFromData(left, left.Type.Size);
        }

        // emit instructions
        foreach (Instruction i in leftContext.EmitOnVisit) {
            Instructions.Add(i);
        }

        // get actual type
        TypeIdentifier leftType = leftContext.ExpressionType ?? left.Type;

        // resolve right side
        ExpressionResult? right = VisitExpression(rightContext);

        // return if there was an error
        if (right is null) {
            return null;
        }

        // move object to stack if it's in the data section
        if (right.Address.Location == MemoryLocation.Data) {
            Instructions.PushFromData(right, right.Type.Size);
        }

        // emit instructions
        foreach (Instruction i in rightContext.EmitOnVisit) {
            Instructions.Add(i);
        }

        // get actual type
        TypeIdentifier rightType = rightContext.ExpressionType ?? right.Type;

        bool isLeftPrimitive = TypeHandler.Casts.IsPrimitiveType(leftType);
        bool isRightPrimitive = TypeHandler.Casts.IsPrimitiveType(rightType);

        // true if both expression types are primitive types or null
        // in this case we can use a simple instruction instead of a function call
        bool isPrimitiveType = isLeftPrimitive && isRightPrimitive;

        // calculate results
        // ah sweet, man-made horrors beyond my comprehension
        ExpressionResult? result = operatorType switch {
            OP_PLUS => isPrimitiveType ? PrimitiveAddition(leftType, rightType) : null,
            OP_MINUS => null,
            OP_MULTIPLY => null,
            OP_DIVIDE => null,
            OP_MODULUS => null,
            OP_SHIFT_LEFT => null,
            OP_SHIFT_RIGHT => null,
            OP_EQ => null,
            OP_NOT_EQ => null,
            OP_LESS => null,
            OP_GREATER => null,
            OP_LESS_EQ => null,
            OP_GREATER_EQ => null,
            OP_AND => null,
            OP_OR => null,
            OP_ASSIGN => null,
            OP_MULTIPLY_ASSIGN => null,
            OP_DIVIDE_ASSIGN => null,
            OP_MODULUS_ASSIGN => null,
            OP_PLUS_ASSIGN => null,
            OP_MINUS_ASSIGN => null,
            OP_SHIFT_LEFT_ASSIGN => null,
            OP_SHIFT_RIGHT_ASSIGN => null,
            OP_AND_ASSIGN => null,
            OP_OR_ASSIGN => null,
            _ => throw new ArgumentException($"Method cannot handle the provided operator type {operatorType}")
        };

        return result;
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
        bool isPrimitiveType = TypeHandler.Casts.IsPrimitiveType(expression.Type);

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

    // TODO: implement
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

    #region Operator methods

    private ExpressionResult? PrimitiveAddition(TypeIdentifier leftType, TypeIdentifier rightType) {
        if (leftType != rightType) {
            return null;
        }

        MemoryAddress address = Instructions.AddInt(leftType.Size);

        return new ExpressionResult(address, leftType);
    }

    #endregion

    public override string VisitId(IdContext context) {
        return context.Start.Text;
    }

    public override string VisitContextualKeyword(ContextualKeywordContext context) {
        return context.start.Text;
    }

    #region Unused visit methods

    public override object? VisitOpLeftUnary(OpLeftUnaryContext context) {
        return null;
    }

    public override object? VisitOpRightUnary(OpRightUnaryContext context) {
        return null;
    }

    public override object? VisitOpMultiplicative(OpMultiplicativeContext context) {
        return null;
    }

    public override object? VisitOpAdditive(OpAdditiveContext context) {
        return null;
    }

    public override object? VisitOpShift(OpShiftContext context) {
        return null;
    }

    public override object? VisitOpComparison(OpComparisonContext context) {
        return null;
    }

    public override object? VisitOpLogical(OpLogicalContext context) {
        return null;
    }

    public override object? VisitOpAssignment(OpAssignmentContext context) {
        return null;
    }

    public override object? VisitExpressionList(ExpressionListContext context) {
        return null;
    }

    #endregion
}