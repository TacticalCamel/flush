namespace Compiler.Builder;

using static Grammar.ScrantonParser;
using Interpreter.Bytecode;
using Data;
using Analysis;

internal sealed partial class Preprocessor {
    public override ExpressionResult? VisitExpression(ExpressionContext context) {
        return (ExpressionResult?)Visit(context);
    }

    public override ExpressionResult? VisitNestedExpression(NestedExpressionContext context) {
        return VisitExpression(context.Body);
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

    private ExpressionResult? ResolveBinaryExpression(ExpressionContext context, ExpressionContext leftContext, ExpressionContext rightContext, int operatorType) {
        // resolve both sides
        ExpressionResult? left = VisitExpression(leftContext);
        ExpressionResult? right = VisitExpression(rightContext);

        // cancel processing if an error occured
        if (left is null || right is null) {
            return null;
        }

        bool isLeftPrimitive = TypeHandler.PrimitiveConversions.IsPrimitiveType(left.Type);
        bool isRightPrimitive = TypeHandler.PrimitiveConversions.IsPrimitiveType(right.Type);

        // true if both expression types are primitive types
        // in this case we can use a simple instruction instead of a function call
        bool isPrimitiveOperation = isLeftPrimitive && isRightPrimitive;
        
        // calculate results
        TypeIdentifier? result = operatorType switch {
            // same as addition
            OP_PLUS => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_MINUS => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_MULTIPLY => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_DIVIDE => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_MODULUS => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_EQ => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_NOT_EQ => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_LESS => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_GREATER => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_LESS_EQ => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_GREATER_EQ => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_AND => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_OR => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_SHIFT_LEFT => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            OP_SHIFT_RIGHT => isPrimitiveOperation ? GetCommonPrimitiveType(leftContext, rightContext, left, right) : null,
            // can have different type
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

        // cancel processing if an error occured
        if (result is null) {
            IssueHandler.Add(Issue.InvalidBinaryOperation(context, left.Type, right.Type, DefaultVocabulary.GetDisplayName(operatorType)));
            return null;
        }

        return new ExpressionResult(MemoryAddress.Null, result);
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

    #region Operator methods

    private TypeIdentifier? GetCommonPrimitiveType(ExpressionContext leftContext, ExpressionContext rightContext, ExpressionResult left, ExpressionResult right) {
        PrimitiveCast cast = GetBestCast(left.Type, right.Type, out TypeIdentifier? resultType);

        bool isImplicit = IsImplicitCast(cast);

        if (resultType is null) {
            return null;
        }

        if (!isImplicit) {
            return null;
        }
        
        Console.WriteLine($"{leftContext.GetText()}:[{left.Type}] + {rightContext.GetText()}:[{right.Type}] -> [{resultType}]");
        
        // cast left
        if (left.Type != resultType) {
            Instruction? i = GetCastInstruction(left.Type, resultType);

            if (i is not null) {
                Console.WriteLine($"    casting left [{left.Type}] -> [{resultType}]");
                leftContext.InstructionsAfterTraversal.Add(i.Value);
                leftContext.OverrideType = resultType;
            }
        }

        // cast right
        else if (right.Type != resultType) {
            Instruction? i = GetCastInstruction(right.Type, resultType);

            if (i is not null) {
                Console.WriteLine($"    casting right [{right.Type}] -> [{resultType}]");
                rightContext.InstructionsAfterTraversal.Add(i.Value);
                rightContext.OverrideType = resultType;
            }
        }

        return resultType;

        Instruction? GetCastInstruction(TypeIdentifier source, TypeIdentifier destination) {
            if (cast == PrimitiveCast.NotRequired) {
                return null;
            }

            if (cast == PrimitiveCast.ResizeImplicit) {
                int difference = destination.Size - source.Size;

                if (difference > 0) {
                    return new Instruction { Code = OperationCode.pshz, Size = difference };
                }

                return new Instruction { Code = OperationCode.pop, Size = -difference };
            }

            return null;
        }
    }

    #endregion

    private PrimitiveCast GetBestCast(TypeIdentifier leftType, TypeIdentifier rightType, out TypeIdentifier? resultType) {
        // cast left side to right side
        PrimitiveCast castLeft = TypeHandler.PrimitiveConversions.GetCast(leftType, rightType);

        // cast right side to left side
        PrimitiveCast castRight = TypeHandler.PrimitiveConversions.GetCast(rightType, leftType);

        // if casting the right side is easier,
        // the type of the left side will be the result 
        if (castLeft < castRight) {
            resultType = leftType;
            return castRight;
        }

        // if casting the left side is harder but possible,
        // the type of the right side will be the result 
        if (PrimitiveCast.None < castLeft) {
            resultType = rightType;
            return castLeft;
        }

        // casting is not possible
        resultType = null;
        return PrimitiveCast.None;
    }

    private static bool IsImplicitCast(PrimitiveCast cast) {
        return cast switch {
            PrimitiveCast.FloatToFloatImplicit => true,
            PrimitiveCast.UnsignedToFloatImplicit => true,
            PrimitiveCast.SignedToFloatImplicit => true,
            PrimitiveCast.ResizeImplicit => true,
            PrimitiveCast.NotRequired => true,
            _ => false
        };
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
}